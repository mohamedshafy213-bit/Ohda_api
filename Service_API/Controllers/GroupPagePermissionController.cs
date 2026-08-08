using Contracts.DTOs.GroupPagePermission;
using Contracts.DTOs.Page;
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
public class GroupPagePermissionController : BaseController<GroupPagePermission, GroupPagePermissionDto, GrantGroupPermissionDto, GroupPagePermissionUpdateDto>
{
    private readonly IRepositoryWrapper _repositoryWrapper;

    public GroupPagePermissionController(IRepositoryWrapper repositoryWrapper)
    {
        _repositoryWrapper = repositoryWrapper;
        _repository = repositoryWrapper.GroupPagePermissions;
    }

    [HttpGet("group/{groupId}")]
    public async Task<IActionResult> GetAllowedPagesForGroup([FromRoute] int groupId)
    {
        var allowedPages = await _repositoryWrapper.GroupPagePermissions.GetAllowedPagesForGroupAsync(groupId);

        return Ok(new SingleObjectResponseModel<List<PageDto>>
        {
            IsDone = true,
            ReturnMessage = $"Allowed pages retrieved successfully for group ID: {groupId}",
            SingleObject = allowedPages
        });
    }

    [HttpPost("grant")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GrantPermission([FromBody] GrantGroupPermissionDto grantDto)
    {
        int grantedByUserId = GetCurrentUserId();

        var success = await _repositoryWrapper.GroupPagePermissions.GrantPermissionAsync(
            grantDto.UserGroupId,
            grantDto.PageId,
            grantedByUserId
        );

        if (!success)
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "Failed to grant permission. Verify if Group and Page exist."
            });
        }

        return Ok(new SingleObjectResponseModel
        {
            IsDone = true,
            ReturnMessage = "Page permission granted to group successfully."
        });
    }

    [HttpDelete("revoke/{groupId}/{pageId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RevokePermission([FromRoute] int groupId, [FromRoute] int pageId)
    {
        var success = await _repositoryWrapper.GroupPagePermissions.RevokePermissionAsync(groupId, pageId);

        if (!success)
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "Failed to revoke permission. Permission record not found."
            });
        }

        return Ok(new SingleObjectResponseModel
        {
            IsDone = true,
            ReturnMessage = "Page permission revoked from group successfully."
        });
    }

    [HttpPost("grant-all/{groupId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GrantAllPages([FromRoute] int groupId)
    {
        int grantedByUserId = GetCurrentUserId();
        await _repositoryWrapper.GroupPagePermissions.GrantAllPagesToGroupAsync(groupId, grantedByUserId);

        return Ok(new SingleObjectResponseModel
        {
            IsDone = true,
            ReturnMessage = "All active pages granted to group successfully."
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
}
