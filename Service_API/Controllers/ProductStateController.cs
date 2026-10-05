using Contracts.DTOs.ProductState;
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
public class ProductStateController : BaseController<ProductState, ProductStateDto, ProductStateCreateDto, ProductStateUpdateDto>
{
    private readonly RepositoryContext _context;

    public ProductStateController(IRepositoryWrapper repositoryWrapper, RepositoryContext context)
    {
        _repository = repositoryWrapper.ProductStates;
        _context = context;
    }

    [HttpDelete("{id}")]
    public override async Task<IActionResult> Delete([FromRoute] string id)
    {
        if (!int.TryParse(id, out var stateId))
            return BadRequest(new SingleObjectResponseModel { IsDone = false, ReturnMessage = "معرف الحالة غير صالح." });

        var existingState = await _context.ProductStates.FirstOrDefaultAsync(s => s.Id == stateId);
        if (existingState == null)
            return NotFound(new SingleObjectResponseModel { IsDone = false, ReturnMessage = "الحالة غير موجودة." });

        var linkedEntryRequestsCount = await _context.ProductEntryRequests.CountAsync(r => r.ProductStateId == stateId && !r.IsDeleted);
        if (linkedEntryRequestsCount > 0)
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = $"لا يمكن حذف هذه الحالة لأنها مسندة لـ ({linkedEntryRequestsCount}) طلبات إدخال في النظام."
            });
        }

        return await base.Delete(id);
    }
}
