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
using Service_API.Helpers;
using System.Security.Claims;

using Service_API.Services;

namespace Service_API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProductExitRequestController : BaseController<ProductExitRequest, ProductExitRequestDto, ProductExitCreateDto, ProductExitUpdateDto>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly RepositoryContext _context;
    private readonly IBackgroundTaskQueue _backgroundQueue;

    public ProductExitRequestController(
        IRepositoryWrapper repositoryWrapper,
        RepositoryContext context,
        IBackgroundTaskQueue backgroundQueue)
    {
        _repositoryWrapper = repositoryWrapper;
        _context = context;
        _repository = repositoryWrapper.ProductExitRequests;
        _backgroundQueue = backgroundQueue;
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

            int newReqId = 0;
            if (response is SingleObjectResponseModel<ProductExitRequestDto> typedResp && typedResp.SingleObject != null)
            {
                newReqId = typedResp.SingleObject.Id;
            }

            var createdReq = await _repositoryWrapper.ProductExitRequests.GetByIdAsync(newReqId, trackChanges: true);
            if (createdReq != null)
            {
                var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? $"User #{userId}";
                createdReq.CurrentStep = 2;
                createdReq.ApprovalTrail = WorkflowHelper.AppendTrail(null, new WorkflowStepRecord
                {
                    StepOrder = 1,
                    StepName = "مقدم الطلب",
                    Role = "Requester",
                    Action = "Created",
                    UserId = userId,
                    UserName = userName,
                    Date = DateTime.UtcNow,
                    Notes = requestDto.Purpose
                });
                await _repositoryWrapper.SaveAsync();
            }

            var newExitReqId = newReqId;
            var currentUserId = userId;
            await _backgroundQueue.QueueBackgroundWorkItemAsync(async (sp, ct) =>
            {
                var repo = sp.GetRequiredService<IRepositoryWrapper>();
                var db = sp.GetRequiredService<RepositoryContext>();

                await NotificationHelper.NotifyWorkflowStepUsersAsync(
                    repo,
                    db,
                    RequestType.Exit,
                    WorkflowRole.Reviewer,
                    "طلب صرف عهدة جديد بانتظار المراجعة والتدقيق",
                    $"تم تقديم طلب صرف عهدة جديد برقم #{newExitReqId} من قبل المستخدم #{currentUserId}. يرجى مراجعة الطلب.",
                    newExitReqId,
                    "ProductExitRequest"
                );

                await NotificationHelper.SendNotificationToUserAsync(
                    repo,
                    currentUserId,
                    "تم تسجيل طلب صرف العهدة بنجاح",
                    $"تم تسجيل طلب صرف العهدة #{newExitReqId} بنجاح وهو الآن في مرحلة مراجعة وتدقيق المدير.",
                    NotificationType.ExitRequest,
                    newExitReqId,
                    "ProductExitRequest"
                );
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
        var request = await _repositoryWrapper.ProductExitRequests.GetByIdAsync(id, trackChanges: true);
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

        var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? $"Manager #{managerId}";
        request.Status = RequestStatus.ManagerApproved;
        request.ManagerId = managerId;
        request.CurrentStep = 3;
        request.LastUpdate = DateTime.UtcNow;
        request.ApprovalTrail = WorkflowHelper.AppendTrail(request.ApprovalTrail, new WorkflowStepRecord
        {
            StepOrder = 2,
            StepName = "المراجعة والفحص الفني",
            Role = "Reviewer",
            Action = "Approved",
            UserId = managerId,
            UserName = userName,
            Date = DateTime.UtcNow,
            Notes = "تمت المراجعة والموافقة الفنية على بنود الطلب"
        });

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

        var reqId = id;
        var reqUserId = request.RequestedByUserId;
        var reqStatus = request.Status;
        var rejReason = request.RejectionReason;

        await _backgroundQueue.QueueBackgroundWorkItemAsync(async (sp, ct) =>
        {
            var repo = sp.GetRequiredService<IRepositoryWrapper>();
            var db = sp.GetRequiredService<RepositoryContext>();

            if (reqStatus == RequestStatus.Rejected)
            {
                await NotificationHelper.SendNotificationToUserAsync(
                    repo,
                    reqUserId,
                    "تم رفض طلب صرف العهدة",
                    $"تم رفض طلب صرف العهدة #{reqId} من قبل المدير. السبب: {rejReason}",
                    NotificationType.Warning,
                    reqId,
                    "ProductExitRequest"
                );
            }
            else
            {
                await NotificationHelper.NotifyWorkflowStepUsersAsync(
                    repo,
                    db,
                    RequestType.Exit,
                    WorkflowRole.Approver,
                    "طلب صرف معتمد من المدير بانتظار الاعتماد النهائي",
                    $"تمت موافقة المدير على طلب صرف العهدة #{reqId}. بانتظار الاعتماد النهائي.",
                    reqId,
                    "ProductExitRequest"
                );

                await NotificationHelper.SendNotificationToUserAsync(
                    repo,
                    reqUserId,
                    "موافقة المدير على طلب صرف العهدة",
                    $"تمت مراجعة واعتماد طلب صرف العهدة #{reqId} من قبل المدير. بانتظار الاعتماد النهائي.",
                    NotificationType.ExitRequest,
                    reqId,
                    "ProductExitRequest"
                );
            }
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
        var request = await _repositoryWrapper.ProductExitRequests.GetByIdAsync(id, trackChanges: true);
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

        var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? $"Supervisor #{supervisorId}";
        request.SupervisorId = supervisorId;
        request.LastUpdate = DateTime.UtcNow;

        if (request.Items.All(i => i.Status == RequestStatus.Rejected))
        {
            request.Status = RequestStatus.Rejected;
            request.RejectionReason = "All items rejected by Supervisor.";
            request.ApprovalTrail = WorkflowHelper.AppendTrail(request.ApprovalTrail, new WorkflowStepRecord
            {
                StepOrder = 3,
                StepName = "الاعتماد النهائي",
                Role = "Approver",
                Action = "Rejected",
                UserId = supervisorId,
                UserName = userName,
                Date = DateTime.UtcNow,
                Notes = request.RejectionReason
            });
            await _repositoryWrapper.SaveAsync();

            var reqId = id;
            var reqUserId = request.RequestedByUserId;
            await _backgroundQueue.QueueBackgroundWorkItemAsync(async (sp, ct) =>
            {
                var repo = sp.GetRequiredService<IRepositoryWrapper>();
                await repo.Notifications.Create(new NotificationCreateDto
                {
                    UserId = reqUserId,
                    Title = "Exit Request Rejected",
                    Message = $"Your exit request (ID: {reqId}) was rejected by the Supervisor.",
                    Type = NotificationType.Warning,
                    ReferenceId = reqId,
                    ReferenceType = "ProductExitRequest"
                });
            });

            return Ok(new SingleObjectResponseModel { IsDone = true, ReturnMessage = "Request rejected because all items were rejected." });
        }

        // Fully approved by final step, now ready for Requester confirmation / release
        request.Status = RequestStatus.SupervisorApproved;
        request.CurrentStep = 4; // Ready for Requester Confirmation
        request.ApprovalTrail = WorkflowHelper.AppendTrail(request.ApprovalTrail, new WorkflowStepRecord
        {
            StepOrder = 3,
            StepName = "الاعتماد النهائي",
            Role = "Approver",
            Action = "Approved",
            UserId = supervisorId,
            UserName = userName,
            Date = DateTime.UtcNow,
            Notes = "تم الاعتماد النهائي، بانتظار تأكيد واستلام صاحب الطلب"
        });

        await _repositoryWrapper.SaveAsync();

        var completedReqId = id;
        var completedUserId = request.RequestedByUserId;

        await _backgroundQueue.QueueBackgroundWorkItemAsync(async (sp, ct) =>
        {
            var repo = sp.GetRequiredService<IRepositoryWrapper>();
            await NotificationHelper.SendNotificationToUserAsync(
                repo,
                completedUserId,
                "تم اعتماد طلب صرف العهدة بالكامل",
                $"تم اعتماد طلب صرف العهدة #{completedReqId} رسمياً من جميع المعتمدين. يرجى الدخول لتأكيد الاستلام والصرف.",
                NotificationType.ExitRequest,
                completedReqId,
                "ProductExitRequest"
            );
        });

        return Ok(new SingleObjectResponseModel { IsDone = true, ReturnMessage = "Request approved by Supervisor. Ready for requester confirmation." });
    }

    [HttpPost("{id}/requester-confirm")]
    [Authorize]
    public async Task<IActionResult> RequesterConfirm([FromRoute] int id)
    {
        int userId = GetCurrentUserId();
        var request = await _repositoryWrapper.ProductExitRequests.GetByIdAsync(id, trackChanges: true);
        if (request == null)
            return NotFound(new SingleObjectResponseModel { IsDone = false, ReturnMessage = "الطلب غير موجود." });

        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
        bool isAdmin = roleClaim == "Admin" || roleClaim == "SuperAdmin" || User.IsInRole("Admin") || User.IsInRole("SuperAdmin");
        if (!isAdmin && request.RequestedByUserId != userId)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "فقط مقدم الطلب أو المسؤول يمكنه تأكيد الاستلام وإتمام الصرف."
            });
        }

        if (request.IsRequesterConfirmed)
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "تم تأكيد هذا الطلب وصرف العهدة مسبقاً."
            });
        }

        var approvedItems = request.Items.Where(i => i.Status == RequestStatus.Approved || i.Status == RequestStatus.Pending).ToList();

        // 1. Validate stock levels
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

        // 2. Decrement stock
        foreach (var item in approvedItems)
        {
            var inventory = await _repositoryWrapper.Inventories.GetByProductIdAsync(item.ProductId);
            if (inventory != null)
            {
                inventory.Quantity = Math.Max(0, inventory.Quantity - item.Quantity);
                inventory.LastUpdate = DateTime.UtcNow;
            }
        }

        // 3. Mark associated product items as Exited and log to Compass
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

        var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? $"User #{userId}";
        request.IsRequesterConfirmed = true;
        request.RequesterConfirmedDate = DateTime.UtcNow;
        request.Status = RequestStatus.Approved;
        request.LastUpdate = DateTime.UtcNow;
        request.ApprovalTrail = WorkflowHelper.AppendTrail(request.ApprovalTrail, new WorkflowStepRecord
        {
            StepOrder = 4,
            StepName = "تأكيد واستلام العهدة",
            Role = "Requester",
            Action = "Confirmed",
            UserId = userId,
            UserName = userName,
            Date = DateTime.UtcNow,
            Notes = "تم تأكيد استلام وصرف الأجهزة وتسجيلها في بوصلة العهد"
        });

        await _repositoryWrapper.SaveAsync();

        return Ok(new SingleObjectResponseModel
        {
            IsDone = true,
            ReturnMessage = "تم تأكيد الاستلام وصرف العهدة وتسجيل الأجهزة في البوصلة بنجاح!"
        });
    }

    [HttpPost("{id}/reject")]
    [HttpPut("{id}/reject")]
    [Authorize]
    public async Task<IActionResult> Reject([FromRoute] int id, [FromBody] RejectRequestDto rejectDto)
    {
        var request = await _repositoryWrapper.ProductExitRequests.GetByIdAsync(id, trackChanges: true);
        if (request == null)
        {
            return Ok(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = $"Product exit request with ID {id} not found."
            });
        }

        int userId = GetCurrentUserId();
        var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? $"User #{userId}";
        string reason = string.IsNullOrWhiteSpace(rejectDto.RejectionReason) ? "تم رفض الطلب" : rejectDto.RejectionReason.Trim();

        request.Status = RequestStatus.Rejected;
        request.RejectionReason = reason;
        request.LastUpdate = DateTime.UtcNow;
        request.ApprovalTrail = WorkflowHelper.AppendTrail(request.ApprovalTrail, new WorkflowStepRecord
        {
            StepOrder = request.CurrentStep,
            StepName = $"المرحلة {request.CurrentStep}",
            Role = "Approver",
            Action = "Rejected",
            UserId = userId,
            UserName = userName,
            Date = DateTime.UtcNow,
            Notes = reason
        });

        foreach (var item in request.Items)
        {
            item.Status = RequestStatus.Rejected;
            item.LastUpdate = DateTime.UtcNow;
        }

        await _repositoryWrapper.SaveAsync();

        var rejectedReqId = id;
        var rejectedUserId = request.RequestedByUserId;

        await _backgroundQueue.QueueBackgroundWorkItemAsync(async (sp, ct) =>
        {
            var repo = sp.GetRequiredService<IRepositoryWrapper>();
            await NotificationHelper.SendNotificationToUserAsync(
                repo,
                rejectedUserId,
                "تم رفض طلب صرف العهدة",
                $"تم رفض طلب صرف العهدة #{rejectedReqId}. سبب الرفض: {reason}",
                NotificationType.Warning,
                rejectedReqId,
                "ProductExitRequest"
            );
        });

        return Ok(new SingleObjectResponseModel { IsDone = true, ReturnMessage = "Request rejected." });
    }

    [HttpGet]
    public override async Task<IActionResult> GetAll([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null)
    {
        var response = await _repositoryWrapper.ProductExitRequests.FindAll(pageNumber, pageSize);
        var dtos = (response as ListOfObjectsResponseModel<ProductExitRequestDto>)?.Objects;
        if (dtos != null && dtos.Any())
        {
            int userId = GetCurrentUserId();

            // Read role and user group from JWT claims — avoids a DB roundtrip on every poll
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            var userRole = Enum.TryParse<UserRole>(roleClaim, out var parsedRole) ? parsedRole : UserRole.Employee;
            int? userGroupId = int.TryParse(User.FindFirst("user_group_id")?.Value, out var gid) && gid > 0 ? gid : null;

            if (userRole != UserRole.Admin && userRole != UserRole.SuperAdmin)
            {
                bool isReviewer = userGroupId != null && await _repositoryWrapper.ApprovalConfigs.IsActionAllowedAsync(RequestType.Exit, userGroupId.Value, WorkflowRole.Reviewer);
                bool isApprover = userGroupId != null && await _repositoryWrapper.ApprovalConfigs.IsActionAllowedAsync(RequestType.Exit, userGroupId.Value, WorkflowRole.Approver);

                if (isReviewer)
                {
                    // Manager / Reviewer: Can see all requests submitted in their branch (including Pending)
                }
                else if (isApprover)
                {
                    // Supervisor / Approver: Can see requests that passed Manager stage (or all if both)
                    dtos = dtos.Where(d => d.Status != RequestStatus.Pending).ToList();
                }
                else
                {
                    // Regular Requester: Can only see their own requests
                    dtos = dtos.Where(d => d.RequestedByUserId == userId).ToList();
                }
            }

            // High-performance batch loading of associated ProductItem IDs & Serials (1 query instead of N queries)
            var requestIds = dtos.Select(d => d.Id).ToList();
            if (requestIds.Any())
            {
                var allItems = await _context.ProductItems
                    .AsNoTracking()
                    .Where(pi => pi.ProductExitRequestId != null && requestIds.Contains(pi.ProductExitRequestId.Value) && !pi.IsDeleted)
                    .Select(pi => new { pi.ProductExitRequestId, pi.Id, pi.SerialNumber })
                    .ToListAsync();

                var groupedItems = allItems.GroupBy(pi => pi.ProductExitRequestId!.Value)
                    .ToDictionary(g => g.Key, g => g.ToList());

                foreach (var dto in dtos)
                {
                    if (groupedItems.TryGetValue(dto.Id, out var items))
                    {
                        dto.SelectedProductItemIds = items.Select(i => i.Id).ToList();
                        dto.SelectedSerials = items.Select(i => i.SerialNumber).ToList();
                    }
                    else
                    {
                        dto.SelectedProductItemIds = new List<int>();
                        dto.SelectedSerials = new List<string>();
                    }
                }
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
            CurrentStep = request.CurrentStep,
            IsRequesterConfirmed = request.IsRequesterConfirmed,
            RequesterConfirmedDate = request.RequesterConfirmedDate,
            ApprovalTrail = request.ApprovalTrail,
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

        var missingProductIds = dto.Items
            .Where(itm => !items.Any(x => x.ProductId == itm.ProductId && x.BinId != null))
            .Select(itm => itm.ProductId)
            .Distinct()
            .ToList();

        Dictionary<int, (int? BinId, string? BinCode, string? BinName)> inStockBins = new();
        if (missingProductIds.Any())
        {
            var stockItems = await _context.ProductItems
                .AsNoTracking()
                .Include(pi => pi.Bin)
                .Where(pi => missingProductIds.Contains(pi.ProductId) && pi.Status == ProductItemStatus.InStock && pi.BinId != null && !pi.IsDeleted)
                .ToListAsync();

            inStockBins = stockItems
                .GroupBy(pi => pi.ProductId)
                .ToDictionary(g => g.Key, g => (g.First().BinId, g.First().Bin?.Code, g.First().Bin?.Name));
        }

        foreach (var itm in dto.Items)
        {
            var assigned = items.FirstOrDefault(x => x.ProductId == itm.ProductId && x.BinId != null);
            if (assigned != null)
            {
                itm.BinId = assigned.BinId;
                itm.BinCode = assigned.Bin?.Code;
                itm.BinName = assigned.Bin?.Name;
            }
            else if (inStockBins.TryGetValue(itm.ProductId, out var binInfo))
            {
                itm.BinId = binInfo.BinId;
                itm.BinCode = binInfo.BinCode;
                itm.BinName = binInfo.BinName;
            }
        }

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
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
        if (roleClaim == "Admin" || roleClaim == "SuperAdmin" || User.IsInRole("Admin") || User.IsInRole("SuperAdmin"))
            return true;

        int? userGroupId = int.TryParse(User.FindFirst("user_group_id")?.Value, out var gid) && gid > 0 ? gid : null;
        if (userGroupId == null)
        {
            int userId = GetCurrentUserId();
            var user = await _repositoryWrapper.Users.GetByIdWithGroupAsync(userId);
            if (user == null) return false;
            if (user.Role == UserRole.Admin || user.Role == UserRole.SuperAdmin) return true;
            if (user.UserGroupId == null) return false;
            userGroupId = user.UserGroupId;
        }

        if (role == WorkflowRole.Reviewer)
        {
            return await _repositoryWrapper.ApprovalConfigs.IsActionAllowedAsync(type, userGroupId.Value, WorkflowRole.Reviewer) ||
                   await _repositoryWrapper.ApprovalConfigs.IsActionAllowedAsync(type, userGroupId.Value, WorkflowRole.Approver);
        }

        return await _repositoryWrapper.ApprovalConfigs.IsActionAllowedAsync(type, userGroupId.Value, role);
    }
}

