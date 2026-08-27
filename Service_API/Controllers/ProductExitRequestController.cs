using Contracts.DTOs.Inventory;
using Contracts.DTOs.Notification;
using Contracts.DTOs.ProductExitRequest;
using Contracts.DTOs.ProductItem;
using Contracts.DTOs.Compass;
using Contracts.interfaces.Repository;
using Contracts.Responses;
using Entities.Models.Enums;
using Entities.Models.Tables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service_API.BaseControllers;
using System.Security.Claims;


namespace Service_API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProductExitRequestController : BaseController<ProductExitRequest, ProductExitRequestDto, ProductExitCreateDto, ProductExitUpdateDto>
{
    private readonly IRepositoryWrapper _repositoryWrapper;

    public ProductExitRequestController(IRepositoryWrapper repositoryWrapper)
    {
        _repositoryWrapper = repositoryWrapper;
        _repository = repositoryWrapper.ProductExitRequests;
    }

    [HttpPost("request")]
    [Authorize]
    public async Task<IActionResult> CreateExitRequest([FromBody] ProductExitCreateDto requestDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!await CheckPermissionAsync(RequestType.Exit, WorkflowRole.Requester))
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "Your user group is not authorized to submit exit requests."
            });
        }

        int userId = GetCurrentUserId();
        if (userId <= 0)
        {
            return Unauthorized(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "Invalid user token claims."
            });
        }

        if (string.IsNullOrWhiteSpace(requestDto.RecipientName))
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "RecipientName is required."
            });
        }

        requestDto.RequestedByUserId = userId;
        var response = await _repositoryWrapper.ProductExitRequests.Create(requestDto);

        if (response.IsDone)
        {
            var createdDto = (response as SingleObjectResponseModel<ProductExitRequestDto>)?.SingleObject;
            if (createdDto != null && requestDto.SelectedProductItemIds != null && requestDto.SelectedProductItemIds.Any())
            {
                foreach (var itemId in requestDto.SelectedProductItemIds)
                {
                    var item = await _repositoryWrapper.ProductItems.GetByIdAsync(itemId);
                    if (item != null)
                    {
                        var updateDto = new ProductItemUpdateDto
                        {
                            Id = item.Id,
                            ProductId = item.ProductId,
                            SerialNumber = item.SerialNumber,
                            QRCode = item.QRCode,
                            Status = item.Status,
                            ProductExitRequestId = createdDto.Id,
                            RecipientName = requestDto.RecipientName,
                            Place = requestDto.RecipientDepartment,
                            Notes = requestDto.Purpose
                        };
                        await _repositoryWrapper.ProductItems.Update(item.Id.ToString(), updateDto);
                    }

                }
                await _repositoryWrapper.SaveAsync();
            }

            // Step 1: Notify Managers about new exit request pending Manager approval
            await _repositoryWrapper.Notifications.Create(new NotificationCreateDto
            {
                UserId = userId,
                Title = "New Product Exit Request",
                Message = $"Exit request created for product ID {requestDto.ProductId} (Qty: {requestDto.RequestedQuantity}) to {requestDto.RecipientName}. Awaiting Manager approval.",
                Type = NotificationType.ExitRequest,
                ReferenceType = "ProductExitRequest"
            });
        }

        return HandleResponse(response);
    }


    [HttpPut("{id}/manager-approve")]
    [Authorize]
    public async Task<IActionResult> ManagerApprove([FromRoute] int id)
    {
        if (!await CheckPermissionAsync(RequestType.Exit, WorkflowRole.Reviewer))
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "Your user group is not authorized to review/approve requests at this stage."
            });
        }

        int managerId = GetCurrentUserId();
        var request = await _repositoryWrapper.ProductExitRequests.GetByIdAsync(id);
        if (request == null)
        {
            return Ok(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = $"Product exit request with ID {id} not found."
            });
        }

        if (request.Status != RequestStatus.Pending)
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = $"Cannot approve request in status '{request.Status}'. Status must be Pending."
            });
        }

        request.Status = RequestStatus.ManagerApproved;
        request.ManagerId = managerId;
        request.LastUpdate = DateTime.UtcNow;

        var updateDto = new ProductExitUpdateDto
        {
            Id = request.Id,
            ProductId = request.ProductId,
            RequestedQuantity = request.RequestedQuantity,
            RecipientName = request.RecipientName,
            RecipientDepartment = request.RecipientDepartment,
            DepartmentId = request.DepartmentId,
            Purpose = request.Purpose,
            RequestedByUserId = request.RequestedByUserId,
            ManagerId = request.ManagerId,
            SupervisorId = request.SupervisorId,
            Status = request.Status,
            RejectionReason = request.RejectionReason
        };

        var response = await _repositoryWrapper.ProductExitRequests.Update(id.ToString(), updateDto);

        if (response.IsDone)
        {
            // Step 1 Complete: Notify Supervisors that Manager has approved and Supervisor approval is now required
            await _repositoryWrapper.Notifications.Create(new NotificationCreateDto
            {
                UserId = request.RequestedByUserId,
                Title = "Exit Request Manager Approved (Step 1 Complete)",
                Message = $"Product exit request (ID: {id}) has received Manager approval. Awaiting final Supervisor approval.",
                Type = NotificationType.ExitRequest,
                ReferenceId = id,
                ReferenceType = "ProductExitRequest"
            });
        }

        return HandleResponse(response);
    }

    [HttpPut("{id}/supervisor-approve")]
    [Authorize]
    public async Task<IActionResult> SupervisorApprove([FromRoute] int id)
    {
        if (!await CheckPermissionAsync(RequestType.Exit, WorkflowRole.Approver))
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "Your user group is not authorized to approve requests."
            });
        }

        int supervisorId = GetCurrentUserId();
        var request = await _repositoryWrapper.ProductExitRequests.GetByIdAsync(id);
        if (request == null)
        {
            return Ok(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = $"Product exit request with ID {id} not found."
            });
        }

        if (request.Status != RequestStatus.ManagerApproved)
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = $"Cannot approve request in status '{request.Status}'. Request must be ManagerApproved first."
            });
        }

        // Deduct inventory quantity on final Supervisor approval
        var inventory = await _repositoryWrapper.Inventories.GetByProductIdAsync(request.ProductId);
        if (inventory == null || inventory.Quantity < request.RequestedQuantity)
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = $"Insufficient inventory stock. Available: {inventory?.Quantity ?? 0}, Requested: {request.RequestedQuantity}."
            });
        }

        int newStock = inventory.Quantity - request.RequestedQuantity;
        await _repositoryWrapper.Inventories.Update(inventory.Id.ToString(), new InventoryUpdateDto
        {
            Id = inventory.Id,
            ProductId = inventory.ProductId,
            Quantity = newStock,
            MinStock = inventory.MinStock,
            MaxStock = inventory.MaxStock
        });

        // Mark associated product items as Exited and create Compass entries
        var productItems = await _repositoryWrapper.ProductItems.GetByExitRequestIdAsync(id);
        foreach (var item in productItems)
        {
            var itemUpdate = new ProductItemUpdateDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                SerialNumber = item.SerialNumber,
                QRCode = item.QRCode,
                Status = ProductItemStatus.Exited,
                ProductExitRequestId = item.ProductExitRequestId,
                RecipientName = request.RecipientName,
                Place = request.RecipientDepartment,
                ExitDate = DateTime.UtcNow,
                Notes = request.Purpose
            };
            await _repositoryWrapper.ProductItems.Update(item.Id.ToString(), itemUpdate);

            var compassCreate = new CompassCreateDto
            {
                SerialNumber = item.SerialNumber,
                ProductName = request.Product?.Name ?? item.Product?.Name ?? "Unknown Product",
                RecipientName = request.RecipientName,
                Place = request.RecipientDepartment ?? "N/A",
                ExitDate = DateTime.UtcNow,
                Type = CompassType.Exit,
                DepartmentId = request.DepartmentId,
                ProductExitRequestId = request.Id,
                Notes = request.Purpose
            };
            await _repositoryWrapper.Compasses.Create(compassCreate);
        }

        request.Status = RequestStatus.SupervisorApproved;
        request.SupervisorId = supervisorId;
        request.LastUpdate = DateTime.UtcNow;

        var updateDto = new ProductExitUpdateDto
        {
            Id = request.Id,
            ProductId = request.ProductId,
            RequestedQuantity = request.RequestedQuantity,
            RecipientName = request.RecipientName,
            RecipientDepartment = request.RecipientDepartment,
            DepartmentId = request.DepartmentId,
            Purpose = request.Purpose,
            RequestedByUserId = request.RequestedByUserId,
            ManagerId = request.ManagerId,
            SupervisorId = request.SupervisorId,
            Status = request.Status,
            RejectionReason = request.RejectionReason
        };

        var response = await _repositoryWrapper.ProductExitRequests.Update(id.ToString(), updateDto);

        if (response.IsDone)
        {
            // Step 2 Complete: Notify employee that request is fully approved
            await _repositoryWrapper.Notifications.Create(new NotificationCreateDto
            {
                UserId = request.RequestedByUserId,
                Title = "Exit Request Fully Approved",
                Message = $"Your product exit request (ID: {id}) for '{request.Product?.Name}' has received both Manager & Supervisor approvals. Inventory stock updated.",
                Type = NotificationType.ExitRequest,
                ReferenceId = id,
                ReferenceType = "ProductExitRequest"
            });
        }

        return HandleResponse(response);
    }


    [HttpPut("{id}/reject")]
    [Authorize]
    public async Task<IActionResult> Reject([FromRoute] int id, [FromBody] RejectRequestDto rejectDto)
    {
        var request = await _repositoryWrapper.ProductExitRequests.GetByIdAsync(id);
        if (request == null)
        {
            return Ok(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = $"Product exit request with ID {id} not found."
            });
        }

        WorkflowRole requiredRole = request.Status == RequestStatus.ManagerApproved ? WorkflowRole.Approver : WorkflowRole.Reviewer;
        if (!await CheckPermissionAsync(RequestType.Exit, requiredRole))
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "Your user group is not authorized to reject this request."
            });
        }

        request.Status = RequestStatus.Rejected;
        request.RejectionReason = rejectDto.RejectionReason;
        request.LastUpdate = DateTime.UtcNow;

        var updateDto = new ProductExitUpdateDto
        {
            Id = request.Id,
            ProductId = request.ProductId,
            RequestedQuantity = request.RequestedQuantity,
            RecipientName = request.RecipientName,
            RecipientDepartment = request.RecipientDepartment,
            DepartmentId = request.DepartmentId,
            Purpose = request.Purpose,
            RequestedByUserId = request.RequestedByUserId,
            ManagerId = request.ManagerId,
            SupervisorId = request.SupervisorId,
            Status = request.Status,
            RejectionReason = rejectDto.RejectionReason
        };

        var response = await _repositoryWrapper.ProductExitRequests.Update(id.ToString(), updateDto);

        if (response.IsDone)
        {
            // Notify employee of rejection
            await _repositoryWrapper.Notifications.Create(new NotificationCreateDto
            {
                UserId = request.RequestedByUserId,
                Title = "Exit Request Rejected",
                Message = $"Your exit request (ID: {id}) was rejected. Reason: {rejectDto.RejectionReason}",
                Type = NotificationType.Warning,
                ReferenceId = id,
                ReferenceType = "ProductExitRequest"
            });
        }

        return HandleResponse(response);
    }

    [HttpGet]
    public override async Task<IActionResult> GetAll()
    {
        var response = await _repositoryWrapper.ProductExitRequests.FindAll();
        var dtos = (response as ListOfObjectsResponseModel<ProductExitRequestDto>)?.Objects;
        if (dtos != null)
        {
            int userId = GetCurrentUserId();
            var user = await _repositoryWrapper.Users.GetByIdWithGroupAsync(userId);
            if (user != null && user.Role != UserRole.Admin)
            {
                if (user.UserGroup?.Name == "Supervisors" || user.Role == UserRole.Supervisor)
                {
                    dtos = dtos.Where(d => d.Status != RequestStatus.Pending).ToList();
                }
                else if (user.UserGroup?.Name == "Employees" || user.Role == UserRole.Employee)
                {
                    dtos = dtos.Where(d => d.RequestedByUserId == userId).ToList();
                }
            }

            foreach (var dto in dtos)
            {
                var items = await _repositoryWrapper.ProductItems.GetByExitRequestIdAsync(dto.Id);
                dto.SelectedProductItemIds = items.Select(i => i.Id).ToList();
                dto.SelectedSerials = items.Select(i => i.SerialNumber).ToList();
            }

            if (response is ListOfObjectsResponseModel<ProductExitRequestDto> listResponse)
            {
                listResponse.Objects = dtos;
            }
        }
        return HandleResponse(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var request = await _repositoryWrapper.ProductExitRequests.GetByIdAsync(id);
        if (request == null)
        {
            return Ok(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = $"Product exit request with ID {id} not found."
            });
        }

        var dto = new ProductExitRequestDto
        {
            Id = request.Id,
            ProductId = request.ProductId,
            ProductName = request.Product?.Name,
            ProductSKU = request.Product?.SKU,
            RequestedQuantity = request.RequestedQuantity,
            Status = request.Status,
            RecipientName = request.RecipientName,
            RecipientDepartment = request.RecipientDepartment,
            DepartmentId = request.DepartmentId,
            DepartmentName = request.Department?.Name,
            Purpose = request.Purpose,
            RequestedByUserId = request.RequestedByUserId,
            RequestedByUsername = request.RequestedByUser?.Username,
            ManagerId = request.ManagerId,
            ManagerUsername = request.Manager?.Username,
            SupervisorId = request.SupervisorId,
            SupervisorUsername = request.Supervisor?.Username,
            RejectionReason = request.RejectionReason,
            InsertDate = request.InsertDate
        };

        var items = await _repositoryWrapper.ProductItems.GetByExitRequestIdAsync(id);
        dto.SelectedProductItemIds = items.Select(i => i.Id).ToList();
        dto.SelectedSerials = items.Select(i => i.SerialNumber).ToList();

        return Ok(new SingleObjectResponseModel<ProductExitRequestDto>
        {
            IsDone = true,
            ReturnMessage = "Request retrieved successfully.",
            SingleObject = dto
        });
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                 ?? User.FindFirst("sub")
                 ?? User.FindFirst("id")
                 ?? User.FindFirst("nameid");

        if (claim != null && int.TryParse(claim.Value, out int userId) && userId > 0)
        {
            return userId;
        }

        return 1;
    }

    private async Task<bool> CheckPermissionAsync(RequestType type, WorkflowRole role)
    {
        int userId = GetCurrentUserId();
        var user = await _repositoryWrapper.Users.GetByIdWithGroupAsync(userId);
        if (user == null) return false;
        if (user.Role == UserRole.Admin) return true;
        if (user.UserGroupId == null) return false;

        if (role == WorkflowRole.Reviewer)
        {
            return await _repositoryWrapper.ApprovalConfigs.IsActionAllowedAsync(type, user.UserGroupId.Value, WorkflowRole.Reviewer) ||
                   await _repositoryWrapper.ApprovalConfigs.IsActionAllowedAsync(type, user.UserGroupId.Value, WorkflowRole.Approver);
        }

        return await _repositoryWrapper.ApprovalConfigs.IsActionAllowedAsync(type, user.UserGroupId.Value, role);
    }
}

