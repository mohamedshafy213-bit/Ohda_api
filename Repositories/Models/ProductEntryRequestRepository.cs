using Contracts.DTOs.ProductEntryRequest;
using Contracts.Interfaces.Repository;
using Entities.Models.Databases;
using Entities.Models.Tables;
using LoggerService;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.Repositories;

namespace Repositories.Models;

public class ProductEntryRequestRepository
    : RepositoryBase<ProductEntryRequest, ProductEntryRequestDto, ProductEntryCreateDto, ProductEntryUpdateDto>, IProductEntryRequestRepository
{
    public ProductEntryRequestRepository(
        ILoggerManager logger,
        RepositoryContext repositoryContext,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
        : base(logger, repositoryContext, httpContextAccessor, mapper)
    {
    }

    public async Task<ProductEntryRequest?> GetByIdAsync(int id)
    {
        return await RepositoryContext.ProductEntryRequests
            .Include(r => r.Product)
            .Include(r => r.ReceivedByUser)
            .Include(r => r.Supervisor)
            .Include(r => r.Manager)
            .Include(r => r.Department)
            .Include(r => r.ProductState)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
    }
}
