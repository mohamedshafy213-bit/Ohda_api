using Contracts.DTOs.Category;
using Contracts.interfaces.Repository;
using Entities.Models.Tables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service_API.BaseControllers;

namespace Service_API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CategoryController : BaseController<Category, CategoryDto, CategoryCreateDto, CategoryUpdateDto>
{
    public CategoryController(IRepositoryWrapper repositoryWrapper)
    {
        _repository = repositoryWrapper.Categories;
    }
}
