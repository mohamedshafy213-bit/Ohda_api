using Contracts.DTOs.Category;
using Contracts.Interfaces.Repository;
using Entities.Models.Databases;
using Entities.Models.Tables;
using LoggerService;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.Repositories;

namespace Repositories.Models;

public class CategoryRepository
    : RepositoryBase<Category, CategoryDto, CategoryCreateDto, CategoryUpdateDto>, ICategoryRepository
{
    public CategoryRepository(
        ILoggerManager logger,
        RepositoryContext repositoryContext,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
        : base(logger, repositoryContext, httpContextAccessor, mapper)
    {
    }

    public async Task<Category?> GetByNameAsync(string name)
    {
        return await RepositoryContext.Categories
            .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower() && !c.IsDeleted);
    }
}
