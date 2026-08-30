using Contracts.DTOs.Inventory;
using Contracts.DTOs.Notification;
using Contracts.DTOs.ProductExitRequest;
using Contracts.DTOs.ProductItem;
using Contracts.DTOs.Compass;
using Entities.Models.Databases;
using Contracts.interfaces.Repository;
using Contracts.Responses;
using Entities.Models.Enums;
using Entities.Models.Tables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_API.BaseControllers;
using System.Security.Claims;


namespace Service_API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProductExitRequestController : BaseController<ProductExitRequest, ProductExitRequestDto, ProductExitCreateDto, ProductExitUpdateDto>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly RepositoryContext _context;

    public ProductExitRequestController(IRepositoryWrapper repositoryWrapper, RepositoryContext context)
    {
        _repositoryWrapper = repositoryWrapper;
        _context = context;
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

        if (requestDto.Items == null || !requestDto.Items.Any())
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "Request must contain at least one item."
            });
        }

        // Validate stock for all items
        foreach (var item in requestDto.Items)
        {
            var inventory = await _repositoryWrapper.Inventories.GetByProductIdAsync(item.ProductId);
            if (inventory == null || inventory.Quantity < item.Quantity)
            {
                var product = await _repositoryWrapper.Products.GetByIdAsync(item.ProductId);
                string prodName = product?.Name ?? $"ID {item.ProductId}";
                return BadRequest(new SingleObjectResponseModel
                {
                    IsDone = false,
                    ReturnMessage = $"Insufficient inventory stock for '{prodName}'. Available: {inventory?.Quantity ?? 0}, Requested: {item.Quantity}."
                });
            }
        }

        requestDto.RequestedByUserId = userId;
        var response = await _repositoryWrapper.ProductExitRequests.Create(requestDto);

        if (response.IsDone)
        {
            var createdDto = (response as SingleObjectResponseModel<ProductExitRequestDto>)?.SingleObject;
            if (createdDto != null && requestDto.SelectedProductItemIds != null && requestDto.SelectedProductItemIds.Any())
            {
                string departmentName = "N/A";
                if (requestDto.DepartmentId.HasValue)
                {
                    var dep = await _context.Departments.FirstOrDefaultAsync(d => d.Id == requestDto.DepartmentId.Value && !d.IsDeleted);
                    if (dep != null)
                    {
                        departmentName = dep.Name;
                    }
                }

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
                            Place = departmentName,
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
                Message = $"Exit request created containing {requestDto.Items.Count} item(s) to {requestDto.RecipientName}. Awaiting Manager approval.",
                Type = NotificationType.ExitRequest,
                ReferenceType = "ProductExitRequest"
            });
        }

        return HandleResponse(response);
    }


    [HttpPut("{id}/manager-approve")]
    [Authorize]
    public async Task<IActionResult> ManagerApprove([FromRoute] int id, [FromBody] RequestApprovalDto? approvalDto = null)
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

        if (approvalDto != null && approvalDto.Items != null && approvalDto.Items.Any())
        {
            foreach (var itemUpdate in approvalDto.Items)
            {
                var item = request.Items.FirstOrDefault(i => i.Id == itemUpdate.ItemId);
                if (item != null)
                {
                    item.Status = itemUpdate.Status;
                    item.LastUpdate = DateTime.UtcNow;
                }
            }
        }
        else
        {
            foreach (var item in request.Items)
            {
                item.Status = RequestStatus.Approved;
                item.LastUpdate = DateTime.UtcNow;
            }
        }

        if (request.Items.All(i => i.Status == RequestStatus.Rejected))
        {
            request.Status = RequestStatus.Rejected;
            request.RejectionReason = "All items rejected by Manager.";
        }

        await _repositoryWrapper.SaveAsync();



        // Notify
        await _repositoryWrapper.Notifications.Create(new NotificationCreateDto
        {
            UserId = request.RequestedByUserId,
            Title = request.Status == RequestStatus.Rejected ? "Exit Request Rejected" : "Exit Request Manager Approved (Step 1 Complete)",
            Message = request.Status == RequestStatus.Rejected 
                ? $"Your exit request (ID: {id}) was rejected by the Manager." 
                : $"Product exit request (ID: {id}) has received Manager approval. Awaiting final Supervisor approval.",
            Type = request.Status == RequestStatus.Rejected ? NotificationType.Warning : NotificationType.ExitRequest,
            ReferenceId = id,
            ReferenceType = "ProductExitRequest"
        });

        return Ok(new SingleObjectResponseModel
        {
            IsDone = true,
            ReturnMessage = request.Status == RequestStatus.Rejected ? "Request rejected." : "Request approved by Manager."
        });
    }

    [HttpPut("{id}/supervisor-approve")]
    [Authorize]
    public async Task<IActionResult> SupervisorApprove([FromRoute] int id, [FromBody] RequestApprovalDto? approvalDto = null)
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

        // Apply item status updates
        if (approvalDto != null && approvalDto.Items != null && approvalDto.Items.Any())
        {
            foreach (var itemUpdate in approvalDto.Items)
            {
                var item = request.Items.FirstOrDefault(i => i.Id == itemUpdate.ItemId);
                if (item != null)
                {
                    item.Status = itemUpdate.Status;
                    item.LastUpdate = DateTime.UtcNow;
                }
            }
        }
        else
        {
            foreach (var item in request.Items)
            {
                if (item.Status == RequestStatus.Pending)
                {
                    item.Status = RequestStatus.Approved;
                }
                item.LastUpdate = DateTime.UtcNow;
            }
        }

        if (request.Items.All(i => i.Status == RequestStatus.Rejected))
        {
            request.Status = RequestStatus.Rejected;
            request.RejectionReason = "All items rejected by Supervisor.";
            await _repositoryWrapper.SaveAsync();

            await _repositoryWrapper.Notifications.Create(new NotificationCreateDto
            {
                UserId = request.RequestedByUserId,
                Title = "Exit Request Rejected",
                Message = $"Your exit request (ID: {id}) was rejected by the Supervisor.",
                Type = NotificationType.Warning,
                ReferenceId = id,
                ReferenceType = "ProductExitRequest"
            });

            return Ok(new SingleObjectResponseModel { IsDone = true, ReturnMessage = "Request rejected because all items were rejected." });
        }

        var approvedItems = request.Items.Where(i => i.Status == RequestStatus.Approved).ToList();

        // 1. Validate stock levels for all approved items atomically
        foreach (var item in approvedItems)
        {
            var inventory = await _repositoryWrapper.Inventories.GetByProductIdAsync(item.ProductId);
            if (inventory == null || inventory.Quantity < item.Quantity)
            {
                var product = await _repositoryWrapper.Products.GetByIdAsync(item.ProductId);
                string prodName = product?.Name ?? $"ID {item.ProductId}";
                return BadRequest(new SingleObjectResponseModel
                {
                    IsDone = false,
                    ReturnMessage = $"Insufficient inventory stock for '{prodName}'. Available: {inventory?.Quantity ?? 0}, Requested: {item.Quantity}."
                });
            }
        }

        // 2. Decrement stock atomically
        foreach (var item in approvedItems)
        {
            var inventory = await _repositoryWrapper.Inventories.GetByProductIdAsync(item.ProductId);
            if (inventory != null)
            {
                inventory.Quantity -= item.Quantity;
                inventory.LastUpdate = DateTime.UtcNow;
            }
        }

        // 3. Mark associated product items as Exited and create Compass entries
        var productItems = await _repositoryWrapper.ProductItems.GetByExitRequestIdAsync(id);
        var productItemsGrouped = productItems.GroupBy(pi => pi.ProductId).ToDictionary(g => g.Key, g => g.ToList());

        foreach (var item in approvedItems)
        {
            List<ProductItem> itemsToExit = new();
            if (productItemsGrouped.TryGetValue(item.ProductId, out var itemsForProduct))
            {
                itemsToExit = itemsForProduct.Take(item.Quantity).ToList();
            }

            int currentCount = itemsToExit.Count;
            if (currentCount < item.Quantity)
            {
                // We need more items than currently linked. Let's find InStock items for this product.
                int extraNeeded = item.Quantity - currentCount;
                var extraItems = await _context.ProductItems
                    .Where(pi => pi.ProductId == item.ProductId && pi.Status == ProductItemStatus.InStock && !pi.IsDeleted && pi.ProductExitRequestId == null)
                    .Take(extraNeeded)
                    .ToListAsync();

                foreach (var extraPi in extraItems)
                {
                    extraPi.ProductExitRequestId = id;
                    itemsToExit.Add(extraPi);
                }

                // If we STILL don't have enough items, let's generate new ProductItem records.
                int stillNeeded = item.Quantity - itemsToExit.Count;
                if (stillNeeded > 0)
                {
                    var product = await _repositoryWrapper.Products.GetByIdAsync(item.ProductId);
                    string sku = product?.SKU ?? $"P{item.ProductId}";
                    for (int u = 1; u <= stillNeeded; u++)
                    {
                        string guidSuffix = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
                        string serialNumber = $"SN-{sku}-{u}-{guidSuffix}";
                        string qrCode = $"QR-{serialNumber}";

                        var productItem = new ProductItem
                        {
                            ProductId = item.ProductId,
                            SerialNumber = serialNumber,
                            QRCode = qrCode,
                            Status = ProductItemStatus.InStock,
                            InsertDate = DateTime.UtcNow,
                            IsDeleted = false,
                            ProductExitRequestId = id
                        };
                        await _repositoryWrapper.ProductItems.CreateDirectAsync(productItem);
                        itemsToExit.Add(productItem);
                    }
                }
            }

            // Now exit all of them and log to Compass
            foreach (var pi in itemsToExit)
            {
                pi.Status = ProductItemStatus.Exited;
                pi.ExitDate = DateTime.UtcNow;
                pi.RecipientName = request.RecipientName;
                pi.Place = request.Department?.Name ?? "N/A";
                pi.Notes = item.Notes ?? request.Purpose;

                // Create Compass log entry
                var compassCreate = new CompassCreateDto
                {
                    SerialNumber = pi.SerialNumber,
                    ProductName = item.Product?.Name ?? pi.Product?.Name ?? "Unknown Product",
                    RecipientName = request.RecipientName,
                    Place = request.Department?.Name ?? "N/A",
                    ExitDate = DateTime.UtcNow,
                    Type = CompassType.Exit,
                    DepartmentId = request.DepartmentId,
                    ProductExitRequestId = request.Id,
                    Notes = item.Notes ?? request.Purpose
                };
                await _repositoryWrapper.Compasses.Create(compassCreate);
            }
        }

        request.Status = RequestStatus.SupervisorApproved;
        request.SupervisorId = supervisorId;
        request.LastUpdate = DateTime.UtcNow;

        await _repositoryWrapper.SaveAsync();



        // Notify
        await _repositoryWrapper.Notifications.Create(new NotificationCreateDto
        {
            UserId = request.RequestedByUserId,
            Title = "Exit Request Fully Approved",
            Message = $"Your product exit request (ID: {id}) has received both Manager & Supervisor approvals. Inventory stock updated.",
            Type = NotificationType.ExitRequest,
            ReferenceId = id,
            ReferenceType = "ProductExitRequest"
        });

        return Ok(new SingleObjectResponseModel { IsDone = true, ReturnMessage = "Request approved by Supervisor." });
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

        foreach (var item in request.Items)
        {
            item.Status = RequestStatus.Rejected;
            item.LastUpdate = DateTime.UtcNow;
        }

        await _repositoryWrapper.SaveAsync();



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

        return Ok(new SingleObjectResponseModel { IsDone = true, ReturnMessage = "Request rejected." });
    }

    [HttpGet]
    public override async Task<IActionResult> GetAll([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null)
    {
        var response = await _repositoryWrapper.ProductExitRequests.FindAll(pageNumber, pageSize);
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
            Status = request.Status,
            RecipientName = request.RecipientName,
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
            InsertDate = request.InsertDate,
            Items = request.Items.Select(i => new ProductExitRequestItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.Product?.Name,
                ProductSKU = i.Product?.SKU,
                Quantity = i.Quantity,
                Status = i.Status,
                Notes = i.Notes
            }).ToList()
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

