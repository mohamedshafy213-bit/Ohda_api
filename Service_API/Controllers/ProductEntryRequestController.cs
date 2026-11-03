using Contracts.DTOs.Compass;
using Contracts.DTOs.Inventory;
using Contracts.DTOs.Notification;
using Contracts.DTOs.ProductEntryRequest;
using Contracts.DTOs.ProductExitRequest;
using Entities.Models.Databases;
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
using Service_API.Services;

namespace Service_API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProductEntryRequestController : BaseController<ProductEntryRequest, ProductEntryRequestDto, ProductEntryCreateDto, ProductEntryUpdateDto>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly RepositoryContext _context;
    private readonly IBackgroundTaskQueue _backgroundQueue;

    public ProductEntryRequestController(
        IRepositoryWrapper repositoryWrapper,
        RepositoryContext context,
        IBackgroundTaskQueue backgroundQueue)
    {
        _repositoryWrapper = repositoryWrapper;
        _context = context;
        _repository = repositoryWrapper.ProductEntryRequests;
        _backgroundQueue = backgroundQueue;
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
                ReturnMessage = "مجموعتك الحالية غير مصرح لها بتقديم طلبات توريد وإدخال المخزون."
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

        if (string.IsNullOrWhiteSpace(requestDto.FromSource))
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "الجهة الموردة أو المصدر مطلوب."
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

        // Validate approval workflow configuration
        var configs = await _context.ApprovalConfigs
            .AsNoTracking()
            .Include(c => c.UserGroup)
            .Where(c => c.RequestType == RequestType.Entry && c.IsActive && !c.IsDeleted)
            .OrderBy(c => c.StepOrder)
            .ToListAsync();

        var approvalSteps = configs.Where(c => c.StepOrder > 1 || c.WorkflowRole != WorkflowRole.Requester).ToList();
        if (!approvalSteps.Any())
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "لا يمكن تقديم الطلب: مسار الاعتماد والموافقات لطلبات التوريد غير مهيأ في النظام. يرجى ضبط الخطوات في إعدادات الموافقات أولاً."
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

        requestDto.ReceivedByUserId = userId;
        var response = await _repositoryWrapper.ProductEntryRequests.Create(requestDto);

        if (response.IsDone)
        {
            int newReqId = 0;
            if (response is SingleObjectResponseModel<ProductEntryRequestDto> typedResp && typedResp.SingleObject != null)
            {
                newReqId = typedResp.SingleObject.Id;
            }

            var createdReq = await _repositoryWrapper.ProductEntryRequests.GetByIdAsync(newReqId, trackChanges: true);
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
                    StepName = step1Config?.StepName ?? "مقدم الطلب والتوريد",
                    Role = "Requester",
                    Action = "Created",
                    UserId = userId,
                    UserName = userName,
                    PersonName = personName,
                    MilitaryNumber = userId,
                    Date = DateTime.UtcNow,
                    Notes = requestDto.Notes
                });
                await _repositoryWrapper.SaveAsync();
            }

            var newEntryReqId = newReqId;
            var currentUserId = userId;
            await _backgroundQueue.QueueBackgroundWorkItemAsync(async (sp, ct) =>
            {
                var repo = sp.GetRequiredService<IRepositoryWrapper>();
                var db = sp.GetRequiredService<RepositoryContext>();

                await NotificationHelper.NotifyWorkflowStepUsersAsync(
                    repo,
                    db,
                    RequestType.Entry,
                    WorkflowRole.Reviewer,
                    "طلب توريد جديد بانتظار المراجعة والتدقيق",
                    $"تم تقديم طلب توريد مخزون جديد برقم #{newEntryReqId} من قبل المستخدم #{currentUserId}. يرجى المراجعة والتدقيق.",
                    newEntryReqId,
                    "ProductEntryRequest"
                );

                await NotificationHelper.SendNotificationToUserAsync(
                    repo,
                    currentUserId,
                    "تم تسجيل طلب التوريد بنجاح",
                    $"تم تسجيل طلب التوريد #{newEntryReqId} بنجاح وهو الآن في مسار الاعتماد.",
                    NotificationType.EntryRequest,
                    newEntryReqId,
                    "ProductEntryRequest"
                );
            });
        }

        return HandleResponse(response);
    }

    [HttpPut("{id}/manager-approve")]
    [Authorize]
    public async Task<IActionResult> ManagerApprove([FromRoute] int id, [FromBody] RequestApprovalDto? approvalDto = null)
    {
        return await ExecuteEntryStepApprovalAsync(id, approvalDto);
    }

    [HttpPut("{id}/supervisor-approve")]
    [Authorize]
    public async Task<IActionResult> SupervisorApprove([FromRoute] int id, [FromBody] RequestApprovalDto? approvalDto = null)
    {
        return await ExecuteEntryStepApprovalAsync(id, approvalDto);
    }

    [HttpPut("{id}/approve")]
    [Authorize]
    public async Task<IActionResult> Approve([FromRoute] int id, [FromBody] RequestApprovalDto? approvalDto = null)
    {
        return await ExecuteEntryStepApprovalAsync(id, approvalDto);
    }

    private async Task<IActionResult> ExecuteEntryStepApprovalAsync(int id, RequestApprovalDto? approvalDto)
    {
        var request = await _repositoryWrapper.ProductEntryRequests.GetByIdAsync(id, trackChanges: true);
        if (request == null)
        {
            return NotFound(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = $"طلب التوريد رقم {id} غير موجود."
            });
        }

        if (request.Status == RequestStatus.Approved || (request.Status == RequestStatus.SupervisorApproved && request.IsRequesterConfirmed))
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "تم اعتماد هذا الطلب وإدخال الأصناف للمخزون بالكامل مسبقاً."
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

        // Fetch active configs for Entry Flow
        var configs = await _context.ApprovalConfigs
            .AsNoTracking()
            .Include(c => c.UserGroup)
            .Where(c => c.RequestType == RequestType.Entry && c.IsActive && !c.IsDeleted)
            .OrderBy(c => c.StepOrder)
            .ToListAsync();

        var currentStepConfig = configs.FirstOrDefault(c => c.StepOrder == request.CurrentStep)
                             ?? configs.FirstOrDefault(c => c.WorkflowRole != WorkflowRole.Requester);

        // Check permission against the group assigned to the current step
        var (allowed, assignedGroupName, stepName) = await CheckStepPermissionAsync(RequestType.Entry, request.CurrentStep);
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
            var rejUserId = request.ReceivedByUserId;
            var rejReason = request.RejectionReason;

            await _backgroundQueue.QueueBackgroundWorkItemAsync(async (sp, ct) =>
            {
                var repo = sp.GetRequiredService<IRepositoryWrapper>();
                await NotificationHelper.SendNotificationToUserAsync(
                    repo,
                    rejUserId,
                    "تم رفض طلب التوريد",
                    $"تم رفض طلب التوريد #{rejReqId}. السبب: {rejReason}",
                    NotificationType.Warning,
                    rejReqId,
                    "ProductEntryRequest"
                );
            });

            return Ok(new SingleObjectResponseModel { IsDone = true, ReturnMessage = "تم رفض طلب التوريد لرفض كافة الأصناف." });
        }

        int currentStepOrder = currentStepConfig?.StepOrder ?? request.CurrentStep;
        var nextStepConfig = configs.FirstOrDefault(c => c.StepOrder > currentStepOrder);

        if (nextStepConfig != null)
        {
            // Intermediate approval
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
                Notes = "تمت المراجعة والفحص الفني بنجاح"
            });

            await _repositoryWrapper.SaveAsync();

            var reqId = id;
            var reqUserId = request.ReceivedByUserId;
            var nextStepName = nextStepConfig.StepName;
            var curStepName = currentStepConfig?.StepName ?? $"المرحلة {currentStepOrder}";

            await _backgroundQueue.QueueBackgroundWorkItemAsync(async (sp, ct) =>
            {
                var repo = sp.GetRequiredService<IRepositoryWrapper>();
                var db = sp.GetRequiredService<RepositoryContext>();

                await NotificationHelper.NotifyWorkflowStepUsersAsync(
                    repo,
                    db,
                    RequestType.Entry,
                    WorkflowRole.Approver,
                    $"طلب توريد معتمد في مرحلة ({curStepName})",
                    $"تمت مراجعة طلب التوريد #{reqId} في مرحلة ({curStepName}) وهو الآن بانتظار الاعتماد في مرحلة ({nextStepName}).",
                    reqId,
                    "ProductEntryRequest"
                );

                await NotificationHelper.SendNotificationToUserAsync(
                    repo,
                    reqUserId,
                    "تحديث مسار طلب التوريد",
                    $"تم اعتماد طلب التوريد #{reqId} في مرحلة ({curStepName}) وانتقل إلى مرحلة ({nextStepName}).",
                    NotificationType.EntryRequest,
                    reqId,
                    "ProductEntryRequest"
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
            // FINAL STEP: Execute final approval, increase inventory, generate serials/items, and log to Compass
            await FinalizeEntryStockAndCompassAsync(request, id);

            request.SupervisorId = currentUserId;
            request.Status = RequestStatus.Approved;
            request.IsRequesterConfirmed = true;
            request.RequesterConfirmedDate = DateTime.UtcNow;
            request.LastUpdate = DateTime.UtcNow;
            request.ApprovalTrail = WorkflowHelper.AppendTrail(request.ApprovalTrail, new WorkflowStepRecord
            {
                StepOrder = currentStepOrder,
                StepName = currentStepConfig?.StepName ?? "الاعتماد النهائي والتسكين",
                Role = currentStepConfig?.WorkflowRole.ToString() ?? "Approver",
                Action = "Approved",
                UserId = currentUserId,
                UserName = userName,
                PersonName = personName,
                MilitaryNumber = currentUserId,
                Date = DateTime.UtcNow,
                Notes = "تم الاعتماد النهائي وإدخال الأصناف للمخزون والبوصلة بنجاح"
            });

            await _repositoryWrapper.SaveAsync();

            var reqId = id;
            var reqUserId = request.ReceivedByUserId;
            await _backgroundQueue.QueueBackgroundWorkItemAsync(async (sp, ct) =>
            {
                var repo = sp.GetRequiredService<IRepositoryWrapper>();
                await NotificationHelper.SendNotificationToUserAsync(
                    repo,
                    reqUserId,
                    "تم الاعتماد النهائي لطلب التوريد",
                    $"تم اعتماد طلب التوريد #{reqId} بالكامل وإضافة الأصناف إلى رصيد المستودع والبوصلة بنجاح.",
                    NotificationType.EntryRequest,
                    reqId,
                    "ProductEntryRequest"
                );
            });

            return Ok(new SingleObjectResponseModel
            {
                IsDone = true,
                ReturnMessage = "تم الاعتماد النهائي وإدخال الأصناف للمخزون والبوصلة بنجاح!"
            });
        }
    }

    [HttpPost("{id}/requester-confirm")]
    [Authorize]
    public async Task<IActionResult> RequesterConfirm([FromRoute] int id)
    {
        long userId = GetCurrentUserId();
        var request = await _repositoryWrapper.ProductEntryRequests.GetByIdAsync(id, trackChanges: true);
        if (request == null)
            return NotFound(new SingleObjectResponseModel { IsDone = false, ReturnMessage = "الطلب غير موجود." });

        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
        bool isAdmin = roleClaim == "Admin" || roleClaim == "SuperAdmin" || User.IsInRole("Admin") || User.IsInRole("SuperAdmin");
        if (!isAdmin && request.ReceivedByUserId != userId)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "فقط مقدم الطلب أو المسؤول يمكنه تأكيد الاستلام وإتمام التوريد."
            });
        }

        if (request.IsRequesterConfirmed)
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "تم تأكيد هذا الطلب وإدخال الأصناف للمخزون مسبقاً."
            });
        }

        await FinalizeEntryStockAndCompassAsync(request, id);

        var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? $"User #{userId}";
        var personName = User.FindFirst("person_name")?.Value ?? userName;
        request.IsRequesterConfirmed = true;
        request.RequesterConfirmedDate = DateTime.UtcNow;
        request.Status = RequestStatus.Approved;
        request.LastUpdate = DateTime.UtcNow;
        request.ApprovalTrail = WorkflowHelper.AppendTrail(request.ApprovalTrail, new WorkflowStepRecord
        {
            StepOrder = Math.Max(request.CurrentStep, 2),
            StepName = "تأكيد واستلام التوريد",
            Role = "Requester",
            Action = "Confirmed",
            UserId = userId,
            UserName = userName,
            PersonName = personName,
            MilitaryNumber = userId,
            Date = DateTime.UtcNow,
            Notes = "تم تأكيد الاستلام وتفعيل الأصناف في المخزون وبوصلة العهد"
        });

        await _repositoryWrapper.SaveAsync();

        return Ok(new SingleObjectResponseModel
        {
            IsDone = true,
            ReturnMessage = "تم تأكيد الاستلام وإدخال الأصناف للمخزون وتسجيلها في البوصلة بنجاح!"
        });
    }

    private async Task FinalizeEntryStockAndCompassAsync(ProductEntryRequest request, int id)
    {
        var approvedItems = request.Items.Where(i => i.Status == RequestStatus.Approved || i.Status == RequestStatus.Pending).ToList();

        foreach (var item in approvedItems)
        {
            var product = await _repositoryWrapper.Products.GetByIdAsync(item.ProductId);
            string sku = product?.SKU ?? $"P{item.ProductId}";

            // 1. Update Inventory stock
            var inventory = await _repositoryWrapper.Inventories.GetByProductIdAsync(item.ProductId);
            if (inventory != null)
            {
                inventory.Quantity += item.Quantity;
                inventory.LastUpdate = DateTime.UtcNow;
            }
            else
            {
                var newInventory = new InventoryCreateDto
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    MinStock = 5,
                    MaxStock = 100
                };
                await _repositoryWrapper.Inventories.Create(newInventory);
            }

            // 2. Process serials if returned or generate new
            List<string> returnedSerials = new();
            if (!string.IsNullOrWhiteSpace(item.Notes))
            {
                var parts = item.Notes.Split(new[] { ',', ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(s => s.Trim())
                                      .Where(s => s.StartsWith("SN-", StringComparison.OrdinalIgnoreCase))
                                      .ToList();
                returnedSerials.AddRange(parts);
            }

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
    }

    [HttpPost("{id}/reject")]
    [HttpPut("{id}/reject")]
    [Authorize]
    public async Task<IActionResult> Reject([FromRoute] int id, [FromBody] RejectRequestDto rejectDto)
    {
        var request = await _repositoryWrapper.ProductEntryRequests.GetByIdAsync(id, trackChanges: true);
        if (request == null)
        {
            return NotFound(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = $"طلب التوريد رقم {id} غير موجود."
            });
        }

        var (allowed, assignedGroupName, stepName) = await CheckStepPermissionAsync(RequestType.Entry, request.CurrentStep);
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

        var rejReqId = id;
        var rejUserId = request.ReceivedByUserId;

        await _backgroundQueue.QueueBackgroundWorkItemAsync(async (sp, ct) =>
            {
                var repo = sp.GetRequiredService<IRepositoryWrapper>();
                await NotificationHelper.SendNotificationToUserAsync(
                    repo,
                    rejUserId,
                    "تم رفض طلب التوريد",
                    $"تم رفض طلب التوريد #{rejReqId}. سبب الرفض: {reason}",
                    NotificationType.Warning,
                    rejReqId,
                    "ProductEntryRequest"
                );
            });

        return Ok(new SingleObjectResponseModel { IsDone = true, ReturnMessage = "تم رفض الطلب بنجاح." });
    }

    [HttpGet]
    public override async Task<IActionResult> GetAll([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null)
    {
        var response = await _repositoryWrapper.ProductEntryRequests.FindAll(pageNumber, pageSize);
        var dtos = (response as ListOfObjectsResponseModel<ProductEntryRequestDto>)?.Objects;
        if (dtos != null && dtos.Any())
        {
            // All requests belonging to the active branch are returned so users can follow requests in Acceptance center
            if (response is ListOfObjectsResponseModel<ProductEntryRequestDto> listResponse)
            {
                listResponse.Objects = dtos;
            }
        }
        return HandleResponse(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var request = await _repositoryWrapper.ProductEntryRequests.GetByIdAsync(id);
        if (request == null)
        {
            return NotFound(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = $"Product entry request with ID {id} not found."
            });
        }

        var dto = new ProductEntryRequestDto
        {
            Id = request.Id,
            BranchId = request.BranchId,
            Status = request.Status,
            FromSource = request.FromSource,
            DepartmentId = request.DepartmentId,
            DepartmentName = request.Department?.Name,
            InvoiceNumber = request.InvoiceNumber,
            Notes = request.Notes,
            ReceivedByUserId = request.ReceivedByUserId,
            ReceivedByUsername = request.ReceivedByUser?.Username,
            ReceivedByPersonName = request.ReceivedByUser?.PersonName,
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
            var anyAllowed = userGroupId.HasValue && (
                await _repositoryWrapper.ApprovalConfigs.IsActionAllowedAsync(type, userGroupId.Value, WorkflowRole.Approver) ||
                await _repositoryWrapper.ApprovalConfigs.IsActionAllowedAsync(type, userGroupId.Value, WorkflowRole.Reviewer));
            return (anyAllowed, null, null);
        }

        bool match = userGroupId.HasValue && userGroupId.Value == config.UserGroupId;
        return (match, config.UserGroup?.Name, config.StepName);
    }
}
