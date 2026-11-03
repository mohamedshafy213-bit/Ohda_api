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
                ReturnMessage = "مجموعتك الحالية غير مصرح لها بتقديم طلبات صرف العهدة."
            });
        }

        long userId = GetCurrentUserId();
        if (userId <= 0)
        {
            return Unauthorized(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "بيانات الجلسة غير صالحة."
            });
        }

        if (string.IsNullOrWhiteSpace(requestDto.RecipientName))
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "اسم المستلم مطلوب."
            });
        }

        if (requestDto.Items == null || !requestDto.Items.Any())
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "يجب أن يحتوي الطلب على صنف واحد على الأقل."
            });
        }

        // Validate that approval workflow is configured with valid assigned groups
        var configs = await _context.ApprovalConfigs
            .AsNoTracking()
            .Include(c => c.UserGroup)
            .Where(c => c.RequestType == RequestType.Exit && c.IsActive && !c.IsDeleted)
            .OrderBy(c => c.StepOrder)
            .ToListAsync();

        var approvalSteps = configs.Where(c => c.StepOrder > 1 || c.WorkflowRole != WorkflowRole.Requester).ToList();
        if (!approvalSteps.Any())
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "لا يمكن تقديم الطلب: مسار الاعتماد والموافقات غير مهيأ (لم يتم تعيين خطوات موافقة في إعدادات الموافقات لهذا النوع من الطلبات)."
            });
        }

        foreach (var step in approvalSteps)
        {
            if (step.UserGroupId <= 0 || step.UserGroup == null)
            {
                return BadRequest(new SingleObjectResponseModel
                {
                    IsDone = false,
                    ReturnMessage = $"لا يمكن تقديم الطلب: خطوة الاعتماد '{step.StepName ?? $"المرحلة {step.StepOrder}"}' لم يتم إسناد مجموعة مستخدمين لها في إعدادات الموافقات."
                });
            }
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
                    ReturnMessage = $"الرصيد غير كافٍ في المستودع للصنف '{prodName}'. المتوفر: {inventory?.Quantity ?? 0}، المطلوب: {item.Quantity}."
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
                var personName = User.FindFirst("person_name")?.Value ?? userName;

                var firstApprovalStep = approvalSteps.OrderBy(c => c.StepOrder).First();
                createdReq.CurrentStep = firstApprovalStep.StepOrder;

                var step1Config = configs.FirstOrDefault(c => c.StepOrder == 1);
                createdReq.ApprovalTrail = WorkflowHelper.AppendTrail(null, new WorkflowStepRecord
                {
                    StepOrder = 1,
                    StepName = step1Config?.StepName ?? "مقدم طلب الصرف",
                    Role = "Requester",
                    Action = "Created",
                    UserId = userId,
                    UserName = userName,
                    PersonName = personName,
                    MilitaryNumber = userId,
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
                    $"تم تسجيل طلب صرف العهدة #{newExitReqId} بنجاح وهو الآن في مسار الاعتماد.",
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
        return await ExecuteExitStepApprovalAsync(id, approvalDto);
    }

    [HttpPut("{id}/supervisor-approve")]
    [Authorize]
    public async Task<IActionResult> SupervisorApprove([FromRoute] int id, [FromBody] RequestApprovalDto? approvalDto = null)
    {
        return await ExecuteExitStepApprovalAsync(id, approvalDto);
    }

    [HttpPut("{id}/approve")]
    [Authorize]
    public async Task<IActionResult> Approve([FromRoute] int id, [FromBody] RequestApprovalDto? approvalDto = null)
    {
        return await ExecuteExitStepApprovalAsync(id, approvalDto);
    }

    private async Task<IActionResult> ExecuteExitStepApprovalAsync(int id, RequestApprovalDto? approvalDto)
    {
        var request = await _repositoryWrapper.ProductExitRequests.GetByIdAsync(id, trackChanges: true);
        if (request == null)
        {
            return NotFound(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = $"طلب صرف العهدة رقم {id} غير موجود."
            });
        }

        if (request.Status == RequestStatus.Approved || (request.Status == RequestStatus.SupervisorApproved && request.IsRequesterConfirmed))
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "تم اعتماد هذا الطلب وصرف الأصناف بالكامل مسبقاً."
            });
        }

        if (request.Status == RequestStatus.Rejected)
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "لا يمكن اعتماد طلب مرفوض."
            });
        }

        // Fetch active configs for Exit Flow
        var configs = await _context.ApprovalConfigs
            .AsNoTracking()
            .Include(c => c.UserGroup)
            .Where(c => c.RequestType == RequestType.Exit && c.IsActive && !c.IsDeleted)
            .OrderBy(c => c.StepOrder)
            .ToListAsync();

        var currentStepConfig = configs.FirstOrDefault(c => c.StepOrder == request.CurrentStep)
                             ?? configs.FirstOrDefault(c => c.WorkflowRole != WorkflowRole.Requester);

        // Check permission against the group assigned to the current step
        var (allowed, assignedGroupName, stepName) = await CheckStepPermissionAsync(RequestType.Exit, request.CurrentStep);
        if (!allowed)
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = $"غير مصرح لك باعتماد هذا الطلب في مرحلة ({stepName ?? currentStepConfig?.StepName ?? $"المرحلة {request.CurrentStep}"}). هذه الصلاحية مسندة حصراً لمجموعة: '{assignedGroupName ?? currentStepConfig?.UserGroup?.Name ?? "المعتمدين"}'."
            });
        }

        long currentUserId = GetCurrentUserId();
        var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? $"User #{currentUserId}";
        var personName = User.FindFirst("person_name")?.Value ?? userName;

        // Apply item decisions if provided
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

        // If all items were rejected, reject the request
        if (request.Items.All(i => i.Status == RequestStatus.Rejected))
        {
            request.Status = RequestStatus.Rejected;
            request.RejectionReason = "تم رفض جميع الأصناف من قبل المعتمد.";
            request.ApprovalTrail = WorkflowHelper.AppendTrail(request.ApprovalTrail, new WorkflowStepRecord
            {
                StepOrder = request.CurrentStep,
                StepName = currentStepConfig?.StepName ?? $"المرحلة {request.CurrentStep}",
                Role = currentStepConfig?.WorkflowRole.ToString() ?? "Approver",
                Action = "Rejected",
                UserId = currentUserId,
                UserName = userName,
                PersonName = personName,
                MilitaryNumber = currentUserId,
                Date = DateTime.UtcNow,
                Notes = request.RejectionReason
            });
            await _repositoryWrapper.SaveAsync();

            var rejReqId = id;
            var rejUserId = request.RequestedByUserId;
            var rejReason = request.RejectionReason;
            await _backgroundQueue.QueueBackgroundWorkItemAsync(async (sp, ct) =>
            {
                var repo = sp.GetRequiredService<IRepositoryWrapper>();
                await NotificationHelper.SendNotificationToUserAsync(
                    repo,
                    rejUserId,
                    "تم رفض طلب صرف العهدة",
                    $"تم رفض طلب صرف العهدة #{rejReqId}. السبب: {rejReason}",
                    NotificationType.Warning,
                    rejReqId,
                    "ProductExitRequest"
                );
            });

            return Ok(new SingleObjectResponseModel { IsDone = true, ReturnMessage = "تم رفض الطلب لرفض كافة الأصناف." });
        }

        // Determine if there is a next approval step in the configured workflow
        int currentStepOrder = currentStepConfig?.StepOrder ?? request.CurrentStep;
        var nextStepConfig = configs.FirstOrDefault(c => c.StepOrder > currentStepOrder);

        if (nextStepConfig != null)
        {
            // Intermediate step approved: advance to the next step
            request.Status = RequestStatus.ManagerApproved;
            request.ManagerId = currentUserId;
            request.CurrentStep = nextStepConfig.StepOrder;
            request.LastUpdate = DateTime.UtcNow;
            request.ApprovalTrail = WorkflowHelper.AppendTrail(request.ApprovalTrail, new WorkflowStepRecord
            {
                StepOrder = currentStepOrder,
                StepName = currentStepConfig?.StepName ?? $"المرحلة {currentStepOrder}",
                Role = currentStepConfig?.WorkflowRole.ToString() ?? "Reviewer",
                Action = "Approved",
                UserId = currentUserId,
                UserName = userName,
                PersonName = personName,
                MilitaryNumber = currentUserId,
                Date = DateTime.UtcNow,
                Notes = "تمت الموافقة والتصديق بنجاح"
            });

            await _repositoryWrapper.SaveAsync();

            var reqId = id;
            var reqUserId = request.RequestedByUserId;
            var nextStepName = nextStepConfig.StepName;
            var curStepName = currentStepConfig?.StepName ?? $"المرحلة {currentStepOrder}";

            await _backgroundQueue.QueueBackgroundWorkItemAsync(async (sp, ct) =>
            {
                var repo = sp.GetRequiredService<IRepositoryWrapper>();
                var db = sp.GetRequiredService<RepositoryContext>();

                await NotificationHelper.NotifyWorkflowStepUsersAsync(
                    repo,
                    db,
                    RequestType.Exit,
                    WorkflowRole.Approver,
                    $"طلب صرف عهدة معتمد في مرحلة ({curStepName})",
                    $"تم اعتماد طلب صرف العهدة #{reqId} في مرحلة ({curStepName}) وهو الآن بانتظار الاعتماد في مرحلة ({nextStepName}).",
                    reqId,
                    "ProductExitRequest"
                );

                await NotificationHelper.SendNotificationToUserAsync(
                    repo,
                    reqUserId,
                    "تحديث مسار طلب صرف العهدة",
                    $"تم اعتماد طلب صرف العهدة #{reqId} في مرحلة ({curStepName}) وانتقل إلى مرحلة ({nextStepName}).",
                    NotificationType.ExitRequest,
                    reqId,
                    "ProductExitRequest"
                );
            });

            return Ok(new SingleObjectResponseModel
            {
                IsDone = true,
                ReturnMessage = $"تم تصديق واعتماد الطلب في مرحلة ({curStepName}) بنجاح، وانتقل إلى مرحلة ({nextStepName})."
            });
        }
        else
        {
            // FINAL STEP: Execute final approval, decrement stock, and record Compass entries
            var (stockSuccess, stockError) = await FinalizeExitStockAndCompassAsync(request, id);
            if (!stockSuccess)
            {
                return BadRequest(new SingleObjectResponseModel
                {
                    IsDone = false,
                    ReturnMessage = stockError ?? "فشل تنفيذ حسم المخزون وتوثيق الأجهزة."
                });
            }

            request.SupervisorId = currentUserId;
            request.Status = RequestStatus.Approved;
            request.IsRequesterConfirmed = true;
            request.RequesterConfirmedDate = DateTime.UtcNow;
            request.LastUpdate = DateTime.UtcNow;
            request.ApprovalTrail = WorkflowHelper.AppendTrail(request.ApprovalTrail, new WorkflowStepRecord
            {
                StepOrder = currentStepOrder,
                StepName = currentStepConfig?.StepName ?? "الاعتماد النهائي والتوثيق المستودعي",
                Role = currentStepConfig?.WorkflowRole.ToString() ?? "Approver",
                Action = "Approved",
                UserId = currentUserId,
                UserName = userName,
                PersonName = personName,
                MilitaryNumber = currentUserId,
                Date = DateTime.UtcNow,
                Notes = "تم الاعتماد النهائي، وتوثيق استلام وصرف الأجهزة في بوصلة العهد والمخزون"
            });

            await _repositoryWrapper.SaveAsync();

            var reqId = id;
            var reqUserId = request.RequestedByUserId;
            await _backgroundQueue.QueueBackgroundWorkItemAsync(async (sp, ct) =>
            {
                var repo = sp.GetRequiredService<IRepositoryWrapper>();
                await NotificationHelper.SendNotificationToUserAsync(
                    repo,
                    reqUserId,
                    "تم الاعتماد النهائي لطلب صرف العهدة",
                    $"تم اعتماد طلب صرف العهدة #{reqId} بالكامل وتوثيق خروج الأصناف في بوصلة العهد والمستودع بنجاح.",
                    NotificationType.ExitRequest,
                    reqId,
                    "ProductExitRequest"
                );
            });

            return Ok(new SingleObjectResponseModel
            {
                IsDone = true,
                ReturnMessage = "تم الاعتماد النهائي وتوثيق صرف العهدة في البوصلة والمخزون بنجاح!"
            });
        }
    }

    [HttpPost("{id}/requester-confirm")]
    [Authorize]
    public async Task<IActionResult> RequesterConfirm([FromRoute] int id)
    {
        long userId = GetCurrentUserId();
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

        var (stockSuccess, stockError) = await FinalizeExitStockAndCompassAsync(request, id);
        if (!stockSuccess)
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = stockError ?? "فشل تنفيذ حسم المخزون."
            });
        }

        var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? $"User #{userId}";
        var personName = User.FindFirst("person_name")?.Value ?? userName;
        request.IsRequesterConfirmed = true;
        request.RequesterConfirmedDate = DateTime.UtcNow;
        request.Status = RequestStatus.Approved;
        request.LastUpdate = DateTime.UtcNow;
        request.ApprovalTrail = WorkflowHelper.AppendTrail(request.ApprovalTrail, new WorkflowStepRecord
        {
            StepOrder = Math.Max(request.CurrentStep, 2),
            StepName = "تأكيد واستلام العهدة",
            Role = "Requester",
            Action = "Confirmed",
            UserId = userId,
            UserName = userName,
            PersonName = personName,
            MilitaryNumber = userId,
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

    private async Task<(bool Success, string? ErrorMessage)> FinalizeExitStockAndCompassAsync(ProductExitRequest request, int id)
    {
        var approvedItems = request.Items.Where(i => i.Status == RequestStatus.Approved || i.Status == RequestStatus.Pending).ToList();

        // 1. Validate stock levels
        foreach (var item in approvedItems)
        {
            var inventory = await _repositoryWrapper.Inventories.GetByProductIdAsync(item.ProductId);
            if (inventory == null || inventory.Quantity < item.Quantity)
            {
                var product = await _repositoryWrapper.Products.GetByIdAsync(item.ProductId);
                string prodName = product?.Name ?? $"ID {item.ProductId}";
                return (false, $"الرصيد غير كافٍ في المخزون للصنف '{prodName}'. المتوفر: {inventory?.Quantity ?? 0}، المطلوب: {item.Quantity}.");
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

        return (true, null);
    }

    [HttpPost("{id}/reject")]
    [HttpPut("{id}/reject")]
    [Authorize]
    public async Task<IActionResult> Reject([FromRoute] int id, [FromBody] RejectRequestDto rejectDto)
    {
        var request = await _repositoryWrapper.ProductExitRequests.GetByIdAsync(id, trackChanges: true);
        if (request == null)
        {
            return NotFound(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = $"طلب صرف العهدة رقم {id} غير موجود."
            });
        }

        // Check permission against the group assigned to the current step
        var (allowed, assignedGroupName, stepName) = await CheckStepPermissionAsync(RequestType.Exit, request.CurrentStep);
        if (!allowed)
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = $"غير مصرح لك برفض هذا الطلب في مرحلة ({stepName ?? $"المرحلة {request.CurrentStep}"}). هذه الصلاحية مسندة لمجموعة: '{assignedGroupName}'."
            });
        }

        long userId = GetCurrentUserId();
        var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? $"User #{userId}";
        var personName = User.FindFirst("person_name")?.Value ?? userName;
        string reason = string.IsNullOrWhiteSpace(rejectDto.RejectionReason) ? "تم رفض الطلب" : rejectDto.RejectionReason.Trim();

        request.Status = RequestStatus.Rejected;
        request.RejectionReason = reason;
        request.LastUpdate = DateTime.UtcNow;
        request.ApprovalTrail = WorkflowHelper.AppendTrail(request.ApprovalTrail, new WorkflowStepRecord
        {
            StepOrder = request.CurrentStep,
            StepName = stepName ?? $"المرحلة {request.CurrentStep}",
            Role = "Approver",
            Action = "Rejected",
            UserId = userId,
            UserName = userName,
            PersonName = personName,
            MilitaryNumber = userId,
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

        return Ok(new SingleObjectResponseModel { IsDone = true, ReturnMessage = "تم رفض الطلب بنجاح." });
    }

    [HttpGet]
    public override async Task<IActionResult> GetAll([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null)
    {
        var response = await _repositoryWrapper.ProductExitRequests.FindAll(pageNumber, pageSize);
        var dtos = (response as ListOfObjectsResponseModel<ProductExitRequestDto>)?.Objects;
        if (dtos != null && dtos.Any())
        {
            // All requests belonging to the active branch are returned so users can follow requests in Acceptance center
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
            return NotFound(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = $"Product exit request with ID {id} not found."
            });
        }

        var dto = new ProductExitRequestDto
        {
            Id = request.Id,
            BranchId = request.BranchId,
            Status = request.Status,
            RecipientName = request.RecipientName,
            DepartmentId = request.DepartmentId,
            DepartmentName = request.Department?.Name,
            Purpose = request.Purpose,
            RequestedByUserId = request.RequestedByUserId,
            RequestedByUsername = request.RequestedByUser?.Username,
            RequestedByPersonName = request.RequestedByUser?.PersonName,
            ManagerId = request.ManagerId,
            ManagerUsername = request.Manager?.Username,
            ManagerPersonName = request.Manager?.PersonName,
            SupervisorId = request.SupervisorId,
            SupervisorUsername = request.Supervisor?.Username,
            SupervisorPersonName = request.Supervisor?.PersonName,
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

    private long GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                 ?? User.FindFirst("sub")
                 ?? User.FindFirst("id")
                 ?? User.FindFirst("nameid");

        if (claim != null && long.TryParse(claim.Value, out long userId) && userId > 0)
        {
            return userId;
        }

        return 1L;
    }

    private async Task<bool> CheckPermissionAsync(RequestType type, WorkflowRole role)
    {
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
        if (roleClaim == "Admin" || roleClaim == "SuperAdmin" || User.IsInRole("Admin") || User.IsInRole("SuperAdmin"))
            return true;

        int? userGroupId = int.TryParse(User.FindFirst("user_group_id")?.Value, out var gid) && gid > 0 ? gid : null;
        if (userGroupId == null)
        {
            long userId = GetCurrentUserId();
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

    private async Task<(bool Allowed, string? GroupName, string? StepName)> CheckStepPermissionAsync(RequestType type, int stepOrder)
    {
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
        if (roleClaim == "Admin" || roleClaim == "SuperAdmin" || User.IsInRole("Admin") || User.IsInRole("SuperAdmin"))
            return (true, null, null);

        long userId = GetCurrentUserId();
        int? userGroupId = int.TryParse(User.FindFirst("user_group_id")?.Value, out var gid) && gid > 0 ? gid : null;
        if (userGroupId == null)
        {
            var user = await _repositoryWrapper.Users.GetByIdWithGroupAsync(userId);
            if (user == null) return (false, null, null);
            if (user.Role == UserRole.Admin || user.Role == UserRole.SuperAdmin) return (true, null, null);
            userGroupId = user.UserGroupId;
        }

        var config = await _context.ApprovalConfigs
            .AsNoTracking()
            .Include(c => c.UserGroup)
            .Where(c => c.RequestType == type && c.StepOrder == stepOrder && c.IsActive && !c.IsDeleted)
            .FirstOrDefaultAsync();

        if (config == null)
        {
            // If no step with exact stepOrder, fallback to check if user has Approver or Reviewer role
            var anyAllowed = userGroupId.HasValue && (
                await _repositoryWrapper.ApprovalConfigs.IsActionAllowedAsync(type, userGroupId.Value, WorkflowRole.Approver) ||
                await _repositoryWrapper.ApprovalConfigs.IsActionAllowedAsync(type, userGroupId.Value, WorkflowRole.Reviewer));
            return (anyAllowed, null, null);
        }

        bool match = userGroupId.HasValue && userGroupId.Value == config.UserGroupId;
        return (match, config.UserGroup?.Name, config.StepName);
    }
}
