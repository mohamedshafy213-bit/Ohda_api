using Contracts.DTOs.Page;
using Contracts.DTOs.UserPagePermission;
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
public class UserPagePermissionController : BaseController<UserPagePermission, UserPagePermissionDto, GrantPermissionDto, UserPagePermissionUpdateDto>
{
    private readonly IRepositoryWrapper _repositoryWrapper;

    public UserPagePermissionController(IRepositoryWrapper repositoryWrapper)
    {
        _repositoryWrapper = repositoryWrapper;
        _repository = repositoryWrapper.UserPagePermissions;
    }

    [HttpGet("my-pages")]
    public async Task<IActionResult> GetMyPages()
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

        var allowedPages = await _repositoryWrapper.UserPagePermissions.GetAllowedPagesForUserAsync(userId);
        return Ok(new SingleObjectResponseModel<List<PageDto>>
        {
            IsDone = true,
            ReturnMessage = "User allowed pages retrieved successfully.",
            SingleObject = allowedPages
        });
    }

    [HttpGet("user/{userId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUserPages([FromRoute] int userId)
    {
        var allowedPages = await _repositoryWrapper.UserPagePermissions.GetAllowedPagesForUserAsync(userId);
        return Ok(new SingleObjectResponseModel<List<PageDto>>
        {
            IsDone = true,
            ReturnMessage = $"Allowed pages retrieved for user ID {userId}",
            SingleObject = allowedPages
        });
    }

    [HttpPost("grant")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GrantPermission([FromBody] GrantPermissionDto grantDto)
    {
        int adminUserId = GetCurrentUserId();
        bool success = await _repositoryWrapper.UserPagePermissions.GrantPermissionAsync(grantDto.UserId, grantDto.PageId, adminUserId);

        return Ok(new SingleObjectResponseModel
        {
            IsDone = success,
            ReturnMessage = success ? "Page permission granted successfully." : "Failed to grant page permission."
        });
    }

    [HttpDelete("revoke/{userId}/{pageId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RevokePermission([FromRoute] int userId, [FromRoute] int pageId)
    {
        bool success = await _repositoryWrapper.UserPagePermissions.RevokePermissionAsync(userId, pageId);

        return Ok(new SingleObjectResponseModel
        {
            IsDone = success,
            ReturnMessage = success ? "Page permission revoked successfully." : "Permission not found or already revoked."
        });
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        return claim != null && int.TryParse(claim.Value, out int userId) ? userId : 0;
    }
}
