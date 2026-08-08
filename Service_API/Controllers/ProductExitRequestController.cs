using Contracts.DTOs.Inventory;
using Contracts.DTOs.Notification;
using Contracts.DTOs.ProductExitRequest;
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
    [Authorize(Roles = "Employee, Admin")]
    public async Task<IActionResult> CreateExitRequest([FromBody] ProductExitCreateDto requestDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

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

        var response = await _repositoryWrapper.ProductExitRequests.Create(new ProductExitCreateDto
        {
            ProductId = requestDto.ProductId,
            RequestedQuantity = requestDto.RequestedQuantity,
            RecipientName = requestDto.RecipientName,
            RecipientDepartment = requestDto.RecipientDepartment,
            Purpose = requestDto.Purpose,
            RequestedByUserId = userId
        });

        if (response.IsDone)
        {
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
    [Authorize(Roles = "Manager, Admin")]
    public async Task<IActionResult> ManagerApprove([FromRoute] int id)
    {
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
    [Authorize(Roles = "Supervisor, Admin")]
    public async Task<IActionResult> SupervisorApprove([FromRoute] int id)
    {
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

        if (request.Status != RequestStatus.ManagerApproved && request.Status != RequestStatus.Pending)
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
    [Authorize(Roles = "Supervisor, Manager, Admin")]
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
}
