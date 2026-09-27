using Contracts.DTOs.Branch;
using Contracts.enums;
using Contracts.interfaces.Repository;
using Contracts.Responses;
using Entities.Models.Tables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service_API.BaseControllers;

namespace Service_API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BranchesController : BaseController<Branch, BranchDto, BranchCreateDto, BranchUpdateDto>
{
    private readonly IRepositoryWrapper _repositoryWrapper;

    public BranchesController(IRepositoryWrapper repositoryWrapper)
    {
        _repositoryWrapper = repositoryWrapper;
        _repository = repositoryWrapper.Branches;
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin")]
    public override async Task<IActionResult> GetAll([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null)
    {
        var response = await _repositoryWrapper.Branches.FindAll(pageNumber, pageSize);
        var dtos = (response as ListOfObjectsResponseModel<BranchDto>)?.Objects;
        if (dtos != null && dtos.Any())
        {
            var branchIds = dtos.Select(dto => dto.Id).ToList();
            var userCounts = await _repositoryWrapper.Branches.GetActiveUserCountsByBranchIdsAsync(branchIds);
            var productCounts = await _repositoryWrapper.Branches.GetActiveProductCountsByBranchIdsAsync(branchIds);

            foreach (var dto in dtos)
            {
                dto.CurrentUserCount = userCounts.TryGetValue(dto.Id, out int uc) ? uc : 0;
                dto.CurrentProductCount = productCounts.TryGetValue(dto.Id, out int pc) ? pc : 0;
            }
        }
        return HandleResponse(response);
    }

    [HttpGet("my-quota")]
    public async Task<IActionResult> GetMyBranchQuota()
    {
        var branchClaim = User.FindFirst("branch_id")?.Value ?? User.FindFirst("BranchId")?.Value;
        int.TryParse(branchClaim, out var userBranchId);
        if (userBranchId <= 0)
        {
            userBranchId = 1;
        }

        var branch = await _repositoryWrapper.Branches.GetByIdAsync(userBranchId);
        if (branch == null)
        {
            return NotFound(new SingleObjectResponseModel
            {
                ErrorCode = ErrorCatalog.ObjectNotFound,
                IsDone = false,
                ReturnMessage = "Branch not found"
            });
        }

        var userCount = await _repositoryWrapper.Branches.GetActiveUserCountAsync(userBranchId);
        var productCount = await _repositoryWrapper.Branches.GetActiveProductCountAsync(userBranchId);

        return Ok(new SingleObjectResponseModel<object>
        {
            ErrorCode = ErrorCatalog.noError,
            IsDone = true,
            ReturnMessage = "Quota retrieved successfully",
            SingleObject = new
            {
                BranchId = branch.Id,
                BranchName = branch.Name,
                BranchCode = branch.Code,
                MaxUsers = branch.MaxUsers,
                CurrentUserCount = userCount,
                RemainingUsers = Math.Max(0, branch.MaxUsers - userCount),
                MaxProducts = branch.MaxProducts,
                CurrentProductCount = productCount,
                RemainingProducts = Math.Max(0, branch.MaxProducts - productCount),
                IsUserQuotaExceeded = userCount >= branch.MaxUsers,
                IsProductQuotaExceeded = productCount >= branch.MaxProducts
            }
        });
    }

    [HttpGet("stats")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetAllBranchStats()
    {
        var result = await _repositoryWrapper.Branches.GetAllBranchStatsAsync();
        return Ok(result);
    }

    [HttpGet("{id}/stats")]
    public async Task<IActionResult> GetBranchStats(int id)
    {
        var isSuperAdmin = User.IsInRole("SuperAdmin");
        var branchClaim = User.FindFirst("branch_id")?.Value ?? User.FindFirst("BranchId")?.Value;
        int.TryParse(branchClaim, out var userBranchId);

        if (!isSuperAdmin && userBranchId != id)
        {
            return NotFound(new SingleObjectResponseModel
            {
                ErrorCode = ErrorCatalog.ObjectNotFound,
                IsDone = false,
                ReturnMessage = "Branch not found"
            });
        }

        var result = await _repositoryWrapper.Branches.GetStatsAsync(id);
        if (!result.IsDone)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPost("create-branch")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> CreateBranch([FromBody] BranchCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _repositoryWrapper.Branches.CreateWithInitialAdminAsync(dto);
        if (!result.IsDone)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin")]
    public override async Task<IActionResult> Update([FromRoute] string id, [FromBody] BranchUpdateDto updateDto)
    {
        return await base.Update(id, updateDto);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin")]
    public override async Task<IActionResult> Delete([FromRoute] string id)
    {
        return await base.Delete(id);
    }

    [HttpPost("{id}/toggle-status")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ToggleBranchStatus(int id)
    {
        var branch = await _repositoryWrapper.Branches.GetByIdAsync(id);
        if (branch == null)
        {
            return NotFound(new SingleObjectResponseModel
            {
                ErrorCode = ErrorCatalog.ObjectNotFound,
                IsDone = false,
                ReturnMessage = "Branch not found"
            });
        }

        branch.IsActive = !branch.IsActive;
        branch.SuspendedDate = branch.IsActive ? null : DateTime.UtcNow;

        await _repositoryWrapper.SaveAsync();

        return Ok(new SingleObjectResponseModel
        {
            ErrorCode = ErrorCatalog.noError,
            IsDone = true,
            ReturnMessage = branch.IsActive ? "Branch activated successfully" : "Branch suspended successfully"
        });
    }
}
