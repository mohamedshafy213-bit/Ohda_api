using Contracts.DTOs.Category;
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
public class CategoryController : BaseController<Category, CategoryDto, CategoryCreateDto, CategoryUpdateDto>
{
    private readonly RepositoryContext _context;

    public CategoryController(IRepositoryWrapper repositoryWrapper, RepositoryContext context)
    {
        _repository = repositoryWrapper.Categories;
        _context = context;
    }

    [HttpDelete("{id}")]
    public override async Task<IActionResult> Delete([FromRoute] string id)
    {
        if (!int.TryParse(id, out var categoryId))
            return BadRequest(new SingleObjectResponseModel { IsDone = false, ReturnMessage = "معرف التصنيف غير صالح." });

        var existingCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Id == categoryId);
        if (existingCategory == null)
            return NotFound(new SingleObjectResponseModel { IsDone = false, ReturnMessage = "التصنيف غير موجود." });

        var linkedProductsCount = await _context.Products.CountAsync(p => p.CategoryId == categoryId && !p.IsDeleted);
        if (linkedProductsCount > 0)
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = $"لا يمكن حذف هذا التصنيف لوجود ({linkedProductsCount}) أصناف/أجهزة مرتبطة به حالياً. يرجى نقل أو حذف الأصناف التابعة له أولاً."
            });
        }

        return await base.Delete(id);
    }
}
