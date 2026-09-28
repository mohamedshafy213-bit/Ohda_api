using Contracts.DTOs.Page;
using Contracts.interfaces.Repository;
using Entities.Models.Tables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service_API.BaseControllers;

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

    [HttpGet]
    public override async Task<IActionResult> GetAll([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null)
    {
        try
        {
            var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            bool isSuperAdmin = roleClaim == "SuperAdmin" || User.IsInRole("SuperAdmin");

            var pages = await _repositoryWrapper.Pages.GetPagesFilteredAsync(isSuperAdmin);

            return Ok(new Contracts.Responses.ListOfObjectsResponseModel<PageDto>
            {
                IsDone = true,
                ErrorCode = Contracts.enums.ErrorCatalog.noError,
                ReturnMessage = "Pages loaded successfully",
                Objects = pages,
                TotalCount = pages.Count
            });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new Contracts.Responses.SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "حدث خطأ أثناء جلب صفحات النظام."
            });
        }
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

        updateDto.Id = Convert.ToInt32(id); // Ensure the ID from the route is set in the DTO
        return await base.Update(id, updateDto);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public override async Task<IActionResult> Delete([FromRoute] string id)
    {
        return await base.Delete(id);
    }
}
