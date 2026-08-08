using Contracts.DTOs.Notification;
using Contracts.interfaces.Repository;
using Contracts.Responses;
using Entities.Models.Tables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service_API.BaseControllers;
using System.Security.Claims;

namespace Service_API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotificationController : BaseController<Notification, NotificationDto, NotificationCreateDto, NotificationUpdateDto>
{
    private readonly IRepositoryWrapper _repositoryWrapper;

    public NotificationController(IRepositoryWrapper repositoryWrapper)
    {
        _repositoryWrapper = repositoryWrapper;
        _repository = repositoryWrapper.Notifications;
    }

    [HttpGet("my-notifications")]
    public async Task<IActionResult> GetMyNotifications([FromQuery] bool unreadOnly = false)
    {
        int userId = GetCurrentUserId();
        if (userId <= 0)
        {
            return Unauthorized(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "Invalid user claims."
            });
        }

        var notifications = await _repositoryWrapper.Notifications.GetNotificationsForUserAsync(userId, unreadOnly);
        return Ok(new SingleObjectResponseModel<List<NotificationDto>>
        {
            IsDone = true,
            ReturnMessage = "Notifications retrieved successfully.",
            SingleObject = notifications
        });
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        int userId = GetCurrentUserId();
        int count = await _repositoryWrapper.Notifications.GetUnreadCountAsync(userId);

        return Ok(new SingleObjectResponseModel<int>
        {
            IsDone = true,
            ReturnMessage = "Unread notifications count retrieved.",
            SingleObject = count
        });
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead([FromRoute] int id)
    {
        int userId = GetCurrentUserId();
        await _repositoryWrapper.Notifications.MarkAsReadAsync(id, userId);

        return Ok(new SingleObjectResponseModel
        {
            IsDone = true,
            ReturnMessage = "Notification marked as read."
        });
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        int userId = GetCurrentUserId();
        await _repositoryWrapper.Notifications.MarkAllAsReadAsync(userId);

        return Ok(new SingleObjectResponseModel
        {
            IsDone = true,
            ReturnMessage = "All notifications marked as read."
        });
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        return claim != null && int.TryParse(claim.Value, out int userId) ? userId : 0;
    }
}
