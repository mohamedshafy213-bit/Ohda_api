using Contracts.DTOs.ApprovalConfig;
using Contracts.interfaces.Repository;
using Entities.Models.Tables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service_API.BaseControllers;

namespace Service_API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ApprovalConfigController : BaseController<ApprovalConfig, ApprovalConfigDto, ApprovalConfigCreateDto, ApprovalConfigUpdateDto>
{
    public ApprovalConfigController(IRepositoryWrapper repositoryWrapper)
    {
        _repository = repositoryWrapper.ApprovalConfigs;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public override async Task<IActionResult> Create([FromBody] ApprovalConfigCreateDto createDto)
    {
        return await base.Create(createDto);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public override async Task<IActionResult> Update([FromRoute] string id, [FromBody] ApprovalConfigUpdateDto updateDto)
    {
        return await base.Update(id, updateDto);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public override async Task<IActionResult> Delete([FromRoute] string id)
    {
        return await base.Delete(id);
    }
}
