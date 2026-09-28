using Contracts.DTOs.UserGroup;
using Contracts.interfaces.Repository;
using Entities.Models.Tables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service_API.BaseControllers;

namespace Service_API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserGroupController : BaseController<UserGroup, UserGroupDto, UserGroupCreateDto, UserGroupUpdateDto>
{
    private readonly IRepositoryWrapper _repositoryWrapper;

    public UserGroupController(IRepositoryWrapper repositoryWrapper)
    {
        _repositoryWrapper = repositoryWrapper;
        _repository = repositoryWrapper.UserGroups;
    }

    [HttpGet]
    public override async Task<IActionResult> GetAll([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null)
    {
        try
        {
            int? branchId = null;
            if (HttpContext.Request.Query.TryGetValue("branchId", out var branchIdVal) && int.TryParse(branchIdVal, out var parsedBranchId))
            {
                branchId = parsedBranchId;
            }

            var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            bool isSuperAdmin = roleClaim == "SuperAdmin" || User.IsInRole("SuperAdmin");
            var branchClaim = User.FindFirst("branch_id")?.Value ?? User.FindFirst("BranchId")?.Value;
            int? userBranchId = int.TryParse(branchClaim, out var bId) && bId > 0 ? bId : null;

            var groups = await _repositoryWrapper.UserGroups.GetGroupsFilteredAsync(branchId, isSuperAdmin, userBranchId);

            return Ok(new Contracts.Responses.ListOfObjectsResponseModel<UserGroupDto>
            {
                IsDone = true,
                ErrorCode = Contracts.enums.ErrorCatalog.noError,
                ReturnMessage = "User groups loaded successfully",
                Objects = groups,
                TotalCount = groups.Count
            });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new Contracts.Responses.SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "حدث خطأ أثناء جلب مجموعات الصلاحيات."
            });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public override async Task<IActionResult> Create([FromBody] UserGroupCreateDto createDto)
    {
        return await base.Create(createDto);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public override async Task<IActionResult> Update([FromRoute] string id, [FromBody] UserGroupUpdateDto updateDto)
    {
        return await base.Update(id, updateDto);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public override async Task<IActionResult> Delete([FromRoute] string id)
    {
        return await base.Delete(id);
    }
}
