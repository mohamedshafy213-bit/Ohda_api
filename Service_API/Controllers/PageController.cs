using Contracts.DTOs.Page;
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
public class PageController : BaseController<Page, PageDto, PageCreateDto, PageUpdateDto>
{
    private readonly IRepositoryWrapper _repositoryWrapper;

    public PageController(IRepositoryWrapper repositoryWrapper)
    {
        _repositoryWrapper = repositoryWrapper;
        _repository = repositoryWrapper.Pages;
    }

    [HttpGet("allowed-pages")]
    public async Task<IActionResult> GetAllowedPages()
    {
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(roleClaim) || !Enum.TryParse<UserRole>(roleClaim, true, out var userRole))
        {
            return Unauthorized(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "Invalid or missing user role claim."
            });
        }

        var allowedPages = await _repositoryWrapper.Pages.GetAllowedPagesForRoleAsync(userRole);

        return Ok(new SingleObjectResponseModel<List<PageDto>>
        {
            IsDone = true,
            ReturnMessage = $"Allowed pages retrieved successfully for role '{userRole}'",
            SingleObject = allowedPages
        });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public override async Task<IActionResult> Create([FromBody] PageCreateDto createDto)
    {
        return await base.Create(createDto);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public override async Task<IActionResult> Update([FromRoute] string id, [FromBody] PageUpdateDto updateDto)
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
