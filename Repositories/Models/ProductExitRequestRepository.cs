using Contracts.DTOs.ProductExitRequest;
using Contracts.Interfaces.Repository;
using Entities.Models.Databases;
using Entities.Models.Tables;
using LoggerService;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.Repositories;

namespace Repositories.Models;

public class ProductExitRequestRepository
    : RepositoryBase<ProductExitRequest, ProductExitRequestDto, ProductExitCreateDto, ProductExitUpdateDto>, IProductExitRequestRepository
{
    public ProductExitRequestRepository(
        ILoggerManager logger,
        RepositoryContext repositoryContext,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
        : base(logger, repositoryContext, httpContextAccessor, mapper)
    {
    }

    public async Task<ProductExitRequest?> GetByIdAsync(int id)
    {
        return await RepositoryContext.ProductExitRequests
            .Include(r => r.Product)
            .Include(r => r.RequestedByUser)
            .Include(r => r.Supervisor)
            .Include(r => r.Manager)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
    }
}
