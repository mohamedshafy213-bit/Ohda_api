using System;
using System.Threading.Tasks;
using Contracts.DTOs.UntrackedItem;
using Contracts.interfaces.Repository;
using Contracts.Responses;
using Entities.Models.Databases;
using Entities.Models.Tables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_API.BaseControllers;

namespace Service_API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UntrackedItemController : BaseController<UntrackedItem, UntrackedItemDto, UntrackedItemCreateDto, UntrackedItemUpdateDto>
{
    private readonly RepositoryContext _context;
    private readonly IRepositoryWrapper _repositoryWrapper;

    public UntrackedItemController(IRepositoryWrapper repositoryWrapper, RepositoryContext context)
    {
        _repositoryWrapper = repositoryWrapper;
        _repository = repositoryWrapper.UntrackedItems;
        _context = context;
    }

    [HttpGet]
    public override async Task<IActionResult> GetAll([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null)
    {
        var effectivePageSize = pageSize ?? 1000;
        var response = await _repository.FindAll(pageNumber, effectivePageSize);
        return HandleResponse(response);
    }

    [HttpPut("{id}/toggle-active")]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var item = await _context.UntrackedItems.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (item == null)
        {
            return NotFound(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "الصنف غير موجود"
            });
        }

        item.IsActive = !item.IsActive;
        await _context.SaveChangesAsync();

        return Ok(new SingleObjectResponseModel<bool>
        {
            IsDone = true,
            ReturnMessage = item.IsActive ? "تم تفعيل الصنف بنجاح" : "تم إلغاء تفعيل الصنف بنجاح",
            SingleObject = item.IsActive
        });
    }
}

