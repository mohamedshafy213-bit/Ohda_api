using Contracts.DTOs.Supplier;
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
public class SupplierController : BaseController<Supplier, SupplierDto, SupplierCreateDto, SupplierUpdateDto>
{
    private readonly RepositoryContext _context;

    public SupplierController(IRepositoryWrapper repositoryWrapper, RepositoryContext context)
    {
        _repository = repositoryWrapper.Suppliers;
        _context = context;
    }

    [HttpDelete("{id}")]
    public override async Task<IActionResult> Delete([FromRoute] string id)
    {
        if (!int.TryParse(id, out var supplierId))
            return BadRequest(new SingleObjectResponseModel { IsDone = false, ReturnMessage = "معرف المورد غير صالح." });

        var existingSupplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.Id == supplierId);
        if (existingSupplier == null)
            return NotFound(new SingleObjectResponseModel { IsDone = false, ReturnMessage = "المورد غير موجود." });

        var linkedProductsCount = await _context.Products.CountAsync(p => p.SupplierId == supplierId && !p.IsDeleted);
        if (linkedProductsCount > 0)
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = $"لا يمكن حذف هذا المورد لأنه مسجل لـ ({linkedProductsCount}) أصناف في المخزون. يرجى تعديل الأصناف أولاً."
            });
        }

        return await base.Delete(id);
    }
}
