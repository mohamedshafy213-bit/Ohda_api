using Contracts.DTOs.ProductState;
using Contracts.Interfaces.Repository;
using Entities.Models.Databases;
using Entities.Models.Tables;
using LoggerService;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Repositories.Repositories;

using Microsoft.EntityFrameworkCore;

namespace Repositories.Models;

public class ProductStateRepository
    : RepositoryBase<ProductState, ProductStateDto, ProductStateCreateDto, ProductStateUpdateDto>, IProductStateRepository
{
    public ProductStateRepository(
        ILoggerManager logger,
        RepositoryContext repositoryContext,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
        : base(logger, repositoryContext, httpContextAccessor, mapper)
    {
    }

    public async Task<ProductState?> GetByIdAsync(int id)
    {
        return await RepositoryContext.ProductStates.FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
    }
}
