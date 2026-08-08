using Contracts.DTOs.Inventory;
using Contracts.DTOs.Notification;
using Contracts.DTOs.ProductEntryRequest;
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
public class ProductEntryRequestController : BaseController<ProductEntryRequest, ProductEntryRequestDto, ProductEntryCreateDto, ProductEntryUpdateDto>
{
    private readonly IRepositoryWrapper _repositoryWrapper;

    public ProductEntryRequestController(IRepositoryWrapper repositoryWrapper)
    {
        _repositoryWrapper = repositoryWrapper;
        _repository = repositoryWrapper.ProductEntryRequests;
    }

    [HttpPost("request")]
    [Authorize(Roles = "Employee, Admin")]
    public async Task<IActionResult> CreateEntryRequest([FromBody] ProductEntryCreateDto requestDto)
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

        if (string.IsNullOrWhiteSpace(requestDto.FromSource))
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "FromSource is required."
            });
        }

        var response = await _repositoryWrapper.ProductEntryRequests.Create(new ProductEntryCreateDto
        {
            ProductId = requestDto.ProductId,
            EnteredQuantity = requestDto.EnteredQuantity,
            FromSource = requestDto.FromSource,
            InvoiceNumber = requestDto.InvoiceNumber,
            Notes = requestDto.Notes,
            ReceivedByUserId = userId
        });

        if (response.IsDone)
        {
            // Step 1: Notify Managers about new entry request pending Manager approval
            await _repositoryWrapper.Notifications.Create(new NotificationCreateDto
            {
                UserId = userId,
                Title = "New Product Entry Request",
                Message = $"Stock-In request created for product ID {requestDto.ProductId} (Qty: {requestDto.EnteredQuantity}) from {requestDto.FromSource}. Awaiting Manager approval.",
                Type = NotificationType.EntryRequest,
                ReferenceType = "ProductEntryRequest"
            });
        }

        return HandleResponse(response);
    }

    [HttpPut("{id}/manager-approve")]
    [Authorize(Roles = "Manager, Admin")]
    public async Task<IActionResult> ManagerApprove([FromRoute] int id)
    {
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

        var updateDto = new ProductEntryUpdateDto
        {
            Id = request.Id,
            ProductId = request.ProductId,
            EnteredQuantity = request.EnteredQuantity,
            FromSource = request.FromSource,
            InvoiceNumber = request.InvoiceNumber,
            Notes = request.Notes,
            ReceivedByUserId = request.ReceivedByUserId,
            ManagerId = request.ManagerId,
            SupervisorId = request.SupervisorId,
            Status = request.Status,
            RejectionReason = request.RejectionReason
        };

        var response = await _repositoryWrapper.ProductEntryRequests.Update(id.ToString(), updateDto);

        if (response.IsDone)
        {
            // Step 1 Complete: Notify Supervisors that Manager has approved and Supervisor approval is now required
            await _repositoryWrapper.Notifications.Create(new NotificationCreateDto
            {
                UserId = request.ReceivedByUserId,
                Title = "Entry Request Manager Approved (Step 1 Complete)",
                Message = $"Product entry request (ID: {id}) has received Manager approval. Awaiting final Supervisor approval.",
                Type = NotificationType.EntryRequest,
                ReferenceId = id,
                ReferenceType = "ProductEntryRequest"
            });
        }

        return HandleResponse(response);
    }

    [HttpPut("{id}/supervisor-approve")]
    [Authorize(Roles = "Supervisor, Admin")]
    public async Task<IActionResult> SupervisorApprove([FromRoute] int id)
    {
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

        if (request.Status != RequestStatus.ManagerApproved && request.Status != RequestStatus.Pending)
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = $"Cannot approve request in status '{request.Status}'. Request must be ManagerApproved first."
            });
        }

        // INCREMENT inventory quantity on final Supervisor approval
        var inventory = await _repositoryWrapper.Inventories.GetByProductIdAsync(request.ProductId);
        if (inventory == null)
        {
            await _repositoryWrapper.Inventories.Create(new InventoryCreateDto
            {
                ProductId = request.ProductId,
                Quantity = request.EnteredQuantity,
                MinStock = 5,
                MaxStock = 100
            });
        }
        else
        {
            int newStock = inventory.Quantity + request.EnteredQuantity;
            await _repositoryWrapper.Inventories.Update(inventory.Id.ToString(), new InventoryUpdateDto
            {
                Id = inventory.Id,
                ProductId = inventory.ProductId,
                Quantity = newStock,
                MinStock = inventory.MinStock,
                MaxStock = inventory.MaxStock
            });
        }

        request.Status = RequestStatus.SupervisorApproved;
        request.SupervisorId = supervisorId;
        request.LastUpdate = DateTime.UtcNow;

        var updateDto = new ProductEntryUpdateDto
        {
            Id = request.Id,
            ProductId = request.ProductId,
            EnteredQuantity = request.EnteredQuantity,
            FromSource = request.FromSource,
            InvoiceNumber = request.InvoiceNumber,
            Notes = request.Notes,
            ReceivedByUserId = request.ReceivedByUserId,
            ManagerId = request.ManagerId,
            SupervisorId = request.SupervisorId,
            Status = request.Status,
            RejectionReason = request.RejectionReason
        };

        var response = await _repositoryWrapper.ProductEntryRequests.Update(id.ToString(), updateDto);

        if (response.IsDone)
        {
            // Step 2 Complete: Notify employee that request is fully approved
            await _repositoryWrapper.Notifications.Create(new NotificationCreateDto
            {
                UserId = request.ReceivedByUserId,
                Title = "Entry Request Fully Approved",
                Message = $"Product entry request (ID: {id}) for '{request.Product?.Name}' has received both Manager & Supervisor approvals. Inventory stock increased by {request.EnteredQuantity}.",
                Type = NotificationType.EntryRequest,
                ReferenceId = id,
                ReferenceType = "ProductEntryRequest"
            });
        }

        return HandleResponse(response);
    }

    [HttpPut("{id}/reject")]
    [Authorize(Roles = "Supervisor, Manager, Admin")]
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

        request.Status = RequestStatus.Rejected;
        request.RejectionReason = rejectDto.RejectionReason;
        request.LastUpdate = DateTime.UtcNow;

        var updateDto = new ProductEntryUpdateDto
        {
            Id = request.Id,
            ProductId = request.ProductId,
            EnteredQuantity = request.EnteredQuantity,
            FromSource = request.FromSource,
            InvoiceNumber = request.InvoiceNumber,
            Notes = request.Notes,
            ReceivedByUserId = request.ReceivedByUserId,
            ManagerId = request.ManagerId,
            SupervisorId = request.SupervisorId,
            Status = request.Status,
            RejectionReason = rejectDto.RejectionReason
        };

        var response = await _repositoryWrapper.ProductEntryRequests.Update(id.ToString(), updateDto);

        if (response.IsDone)
        {
            // Notify employee of rejection
            await _repositoryWrapper.Notifications.Create(new NotificationCreateDto
            {
                UserId = request.ReceivedByUserId,
                Title = "Product Entry Request Rejected",
                Message = $"Product entry request (ID: {id}) was rejected. Reason: {rejectDto.RejectionReason}",
                Type = NotificationType.Warning,
                ReferenceId = id,
                ReferenceType = "ProductEntryRequest"
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
