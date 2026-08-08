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

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public override async Task<IActionResult> Create([FromBody] UserGroupCreateDto createDto)
    {
        return await base.Create(createDto);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public override async Task<IActionResult> Update([FromRoute] string id, [FromBody] UserGroupUpdateDto updateDto)
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
