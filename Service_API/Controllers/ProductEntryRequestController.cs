using Contracts.DTOs.Compass;
using Contracts.DTOs.Inventory;
using Contracts.DTOs.Notification;
using Contracts.DTOs.ProductEntryRequest;
using Entities.Models.Databases;
using Contracts.DTOs.ProductExitRequest;
using Contracts.DTOs.ProductItem;
using Contracts.interfaces.Repository;
using Contracts.Responses;
using Entities.Models.Enums;
using Entities.Models.Tables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_API.BaseControllers;
using Service_API.Helpers;
using System.Security.Claims;

namespace Service_API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProductEntryRequestController : BaseController<ProductEntryRequest, ProductEntryRequestDto, ProductEntryCreateDto, ProductEntryUpdateDto>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly RepositoryContext _context;

    public ProductEntryRequestController(IRepositoryWrapper repositoryWrapper, RepositoryContext context)
    {
        _repositoryWrapper = repositoryWrapper;
        _context = context;
        _repository = repositoryWrapper.ProductEntryRequests;
    }

    [HttpPost("request")]
    [Authorize]
    public async Task<IActionResult> CreateEntryRequest([FromBody] ProductEntryCreateDto requestDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!await CheckPermissionAsync(RequestType.Entry, WorkflowRole.Requester))
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "Your user group is not authorized to submit entry requests."
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

        if (string.IsNullOrWhiteSpace(requestDto.FromSource))
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "FromSource is required."
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

        requestDto.ReceivedByUserId = userId;
        var response = await _repositoryWrapper.ProductEntryRequests.Create(requestDto);

        if (response.IsDone)
        {
            int newReqId = 0;
            if (response is SingleObjectResponseModel<ProductEntryRequestDto> typedResp && typedResp.SingleObject != null)
            {
                newReqId = typedResp.SingleObject.Id;
            }

            // Step 1 -> Step 2: Notify Reviewers / Managers about new entry request
            await NotificationHelper.NotifyWorkflowStepUsersAsync(
                _repositoryWrapper,
                _context,
                RequestType.Entry,
                WorkflowRole.Reviewer,
                "طلب توريد جديد بانتظار المراجعة والتدقيق",
                $"تم تقديم طلب توريد مخزون جديد برقم #{newReqId} من قبل المستخدم #{userId}. يرجى المراجعة والتدقيق.",
                newReqId,
                "ProductEntryRequest"
            );

            // Notify Requester (confirmation)
            await NotificationHelper.SendNotificationToUserAsync(
                _repositoryWrapper,
                userId,
                "تم تسجيل طلب التوريد بنجاح",
                $"تم تسجيل طلب التوريد #{newReqId} بنجاح وهو الآن في مرحلة مراجعة وتدقيق المدير.",
                NotificationType.EntryRequest,
                newReqId,
                "ProductEntryRequest"
            );
        }

        return HandleResponse(response);
    }

    [HttpPut("{id}/manager-approve")]
    [Authorize]
    public async Task<IActionResult> ManagerApprove([FromRoute] int id, [FromBody] RequestApprovalDto? approvalDto = null)
    {
        if (!await CheckPermissionAsync(RequestType.Entry, WorkflowRole.Reviewer))
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "Your user group is not authorized to review/approve requests at this stage."
            });
        }

        int managerId = GetCurrentUserId();
        var request = await _repositoryWrapper.ProductEntryRequests.GetByIdAsync(id);
        if (request == null)
        {
            return Ok(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = $"Product entry request with ID {id} not found."
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
        if (request.Status == RequestStatus.Rejected)
        {
            await NotificationHelper.SendNotificationToUserAsync(
                _repositoryWrapper,
                request.ReceivedByUserId,
                "تم رفض طلب التوريد",
                $"تم رفض طلب التوريد #{id} من قبل المدير. السبب: {request.RejectionReason}",
                NotificationType.Warning,
                id,
                "ProductEntryRequest"
            );
        }
        else
        {
            // Step 2 -> Step 3: Notify Approvers / Supervisors
            await NotificationHelper.NotifyWorkflowStepUsersAsync(
                _repositoryWrapper,
                _context,
                RequestType.Entry,
                WorkflowRole.Approver,
                "طلب توريد معتمد من المدير بانتظار التوثيق النهائي",
                $"تمت موافقة المدير على طلب التوريد #{id}. يرجى توثيق الطلب وإدخال الأصناف للمخزن.",
                id,
                "ProductEntryRequest"
            );

            // Notify Requester
            await NotificationHelper.SendNotificationToUserAsync(
                _repositoryWrapper,
                request.ReceivedByUserId,
                "موافقة المدير على طلب التوريد",
                $"تمت مراجعة واعتماد طلب التوريد #{id} من قبل المدير. بانتظار التوثيق والاعتماد النهائي من المشرف.",
                NotificationType.EntryRequest,
                id,
                "ProductEntryRequest"
            );
        }

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
        if (!await CheckPermissionAsync(RequestType.Entry, WorkflowRole.Approver))
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "Your user group is not authorized to approve requests."
            });
        }

        int supervisorId = GetCurrentUserId();
        var request = await _repositoryWrapper.ProductEntryRequests.GetByIdAsync(id);
        if (request == null)
        {
            return Ok(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = $"Product entry request with ID {id} not found."
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
                UserId = request.ReceivedByUserId,
                Title = "Entry Request Rejected",
                Message = $"Your entry request (ID: {id}) was rejected by the Supervisor.",
                Type = NotificationType.Warning,
                ReferenceId = id,
                ReferenceType = "ProductEntryRequest"
            });

            return Ok(new SingleObjectResponseModel { IsDone = true, ReturnMessage = "Request rejected because all items were rejected." });
        }

        var approvedItems = request.Items.Where(i => i.Status == RequestStatus.Approved).ToList();

        // Increment inventory and generate ProductItems atomically
        foreach (var item in approvedItems)
        {
            var inventory = await _repositoryWrapper.Inventories.GetByProductIdAsync(item.ProductId);
            if (inventory == null)
            {
                var newInventory = new Inventory
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    MinStock = 5,
                    MaxStock = 100,
                    InsertDate = DateTime.UtcNow,
                    IsDeleted = false
                };
                await _repositoryWrapper.Inventories.CreateDirectAsync(newInventory);
            }
            else
            {
                inventory.Quantity += item.Quantity;
                inventory.LastUpdate = DateTime.UtcNow;
            }

            // Extract returned serial numbers if present in item.Notes (sent as [SN-1, SN-2])
            List<string> returnedSerials = new();
            if (!string.IsNullOrWhiteSpace(item.Notes) && item.Notes.Contains("[") && item.Notes.Contains("]"))
            {
                int start = item.Notes.IndexOf("[") + 1;
                int end = item.Notes.IndexOf("]");
                if (end > start)
                {
                    string serialsStr = item.Notes.Substring(start, end - start);
                    var splitSerials = serialsStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var s in splitSerials)
                    {
                        returnedSerials.Add(s.Trim());
                    }
                }
            }

            var product = await _repositoryWrapper.Products.GetByIdAsync(item.ProductId);
            string sku = product?.SKU ?? $"P{item.ProductId}";

            // Resolve item state to determine if it has problems (Damaged / InMaintenance)
            ProductItemStatus itemStatus = ProductItemStatus.InStock;
            string stateLabel = "";
            if (item.ProductStateId.HasValue)
            {
                var pState = await _repositoryWrapper.ProductStates.GetByIdAsync(item.ProductStateId.Value);
                if (pState != null)
                {
                    stateLabel = pState.Name ?? "";
                    string code = (pState.Code ?? "").ToUpper();
                    if (code == "DAMAGED" || stateLabel.Contains("تالف"))
                    {
                        itemStatus = ProductItemStatus.Damaged;
                    }
                    else if (code == "MAINT" || stateLabel.Contains("صيانة"))
                    {
                        itemStatus = ProductItemStatus.InMaintenance;
                    }
                }
            }

            // Process returned serial numbers (change status from Exited to InStock or Damaged/Maintenance)
            int processedQty = 0;
            foreach (var serial in returnedSerials)
            {
                var productItem = await _repositoryWrapper.ProductItems.GetBySerialNumberAsync(serial);
                if (productItem != null)
                {
                    productItem.Status = itemStatus;
                    productItem.BinId = item.BinId;
                    productItem.RecipientName = null;
                    productItem.Place = null;
                    productItem.ExitDate = null;
                    productItem.ProductExitRequestId = null;
                    productItem.Notes = string.IsNullOrWhiteSpace(stateLabel) 
                        ? $"Returned to stock via Request #{request.Id}" 
                        : $"Returned via Request #{request.Id} (حالة الصنف: {stateLabel})";

                    // Create Compass log entry for return
                    var compassCreate = new CompassCreateDto
                    {
                        SerialNumber = serial,
                        ProductName = product?.Name ?? "Unknown Product",
                        RecipientName = request.ReceivedByUser?.PersonName ?? request.ReceivedByUser?.Username ?? "System",
                        Place = request.Department?.Name ?? request.FromSource ?? "N/A",
                        ExitDate = DateTime.UtcNow,
                        Type = CompassType.Entry,
                        DepartmentId = request.DepartmentId,
                        ProductStateId = item.ProductStateId,
                        ProductEntryRequestId = request.Id,
                        Notes = "Returned from department: " + (item.Notes ?? request.Notes)
                    };
                    await _repositoryWrapper.Compasses.Create(compassCreate);
                    processedQty++;
                }
            }

            // For the remaining quantity, generate new serial numbers (purchased/new items)
            int remainingQty = item.Quantity - processedQty;
            for (int u = 1; u <= remainingQty; u++)
            {
                string guidSuffix = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
                string serialNumber = $"SN-{sku}-{u}-{guidSuffix}";
                string qrCode = $"QR-{serialNumber}";

                var productItem = new ProductItem
                {
                    ProductId = item.ProductId,
                    SerialNumber = serialNumber,
                    QRCode = qrCode,
                    Status = itemStatus,
                    BinId = item.BinId,
                    Notes = string.IsNullOrWhiteSpace(stateLabel) ? null : $"حالة الصنف عند التوريد: {stateLabel}",
                    InsertDate = DateTime.UtcNow,
                    IsDeleted = false
                };
                await _repositoryWrapper.ProductItems.CreateDirectAsync(productItem);

                // Create Compass log entry for EACH ProductItem
                var compassCreate = new CompassCreateDto
                {
                    SerialNumber = serialNumber,
                    ProductName = product?.Name ?? "Unknown Product",
                    RecipientName = request.ReceivedByUser?.PersonName ?? request.ReceivedByUser?.Username ?? "System",
                    Place = request.Department?.Name ?? request.FromSource ?? "N/A",
                    ExitDate = DateTime.UtcNow,
                    Type = CompassType.Entry,
                    DepartmentId = request.DepartmentId,
                    ProductStateId = item.ProductStateId,
                    ProductEntryRequestId = request.Id,
                    Notes = item.Notes ?? request.Notes
                };
                await _repositoryWrapper.Compasses.Create(compassCreate);
            }
        }

        request.Status = RequestStatus.SupervisorApproved;
        request.SupervisorId = supervisorId;
        request.LastUpdate = DateTime.UtcNow;

        await _repositoryWrapper.SaveAsync();

        // Notify Requester that request is fully approved and stock added
        await NotificationHelper.SendNotificationToUserAsync(
            _repositoryWrapper,
            request.ReceivedByUserId,
            "اكتمال وتوثيق طلب التوريد بنجاح",
            $"تم توثيق طلب التوريد #{id} من قبل المشرف وإضافة الأصناف إلى رصيد المخزن بنجاح.",
            NotificationType.EntryRequest,
            id,
            "ProductEntryRequest"
        );

        return Ok(new SingleObjectResponseModel { IsDone = true, ReturnMessage = "Request approved by Supervisor." });
    }

    [HttpPut("{id}/reject")]
    [Authorize]
    public async Task<IActionResult> Reject([FromRoute] int id, [FromBody] RejectRequestDto rejectDto)
    {
        var request = await _repositoryWrapper.ProductEntryRequests.GetByIdAsync(id);
        if (request == null)
        {
            return Ok(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = $"Product entry request with ID {id} not found."
            });
        }

        WorkflowRole requiredRole = request.Status == RequestStatus.ManagerApproved ? WorkflowRole.Approver : WorkflowRole.Reviewer;
        if (!await CheckPermissionAsync(RequestType.Entry, requiredRole))
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
            UserId = request.ReceivedByUserId,
            Title = $"تم إرجاع / رفض طلب إدخال المخزون (رقم #{id})",
            Message = $"تم إرجاع طلب التوريد الخاص بك. سبب الإرجاع: {rejectDto.RejectionReason}",
            Type = NotificationType.Warning,
            ReferenceId = id,
            ReferenceType = "ProductEntryRequest"
        });

        return Ok(new SingleObjectResponseModel { IsDone = true, ReturnMessage = "Request rejected." });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var request = await _repositoryWrapper.ProductEntryRequests.GetByIdAsync(id);
        if (request == null)
        {
            return Ok(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = $"Product entry request with ID {id} not found."
            });
        }

        var dto = new ProductEntryRequestDto
        {
            Id = request.Id,
            Status = request.Status,
            FromSource = request.FromSource,
            DepartmentId = request.DepartmentId,
            DepartmentName = request.Department?.Name,
            InvoiceNumber = request.InvoiceNumber,
            Notes = request.Notes,
            ReceivedByUserId = request.ReceivedByUserId,
            ReceivedByUsername = request.ReceivedByUser?.Username,
            ManagerId = request.ManagerId,
            ManagerUsername = request.Manager?.Username,
            SupervisorId = request.SupervisorId,
            SupervisorUsername = request.Supervisor?.Username,
            RejectionReason = request.RejectionReason,
            InsertDate = request.InsertDate,
            Items = request.Items.Select(i => new ProductEntryRequestItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.Product?.Name,
                ProductSKU = i.Product?.SKU,
                Quantity = i.Quantity,
                ProductStateId = i.ProductStateId,
                ProductStateName = i.ProductState?.Name,
                BinId = i.BinId,
                BinCode = i.Bin?.Code,
                Status = i.Status,
                Notes = i.Notes
            }).ToList()
        };

        return Ok(new SingleObjectResponseModel<ProductEntryRequestDto>
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
        if (user.Role == UserRole.Admin || user.Role == UserRole.SuperAdmin) return true;
        if (user.UserGroupId == null) return false;

        if (role == WorkflowRole.Reviewer)
        {
            return await _repositoryWrapper.ApprovalConfigs.IsActionAllowedAsync(type, user.UserGroupId.Value, WorkflowRole.Reviewer) ||
                   await _repositoryWrapper.ApprovalConfigs.IsActionAllowedAsync(type, user.UserGroupId.Value, WorkflowRole.Approver);
        }

        return await _repositoryWrapper.ApprovalConfigs.IsActionAllowedAsync(type, user.UserGroupId.Value, role);
    }
}
