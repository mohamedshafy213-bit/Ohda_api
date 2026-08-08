using Contracts.DTOs.ProductItem;
using Contracts.Interfaces.Repository;
using Entities.Models.Databases;
using Entities.Models.Tables;
using LoggerService;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.Repositories;

namespace Repositories.Models;

public class ProductItemRepository
    : RepositoryBase<ProductItem, ProductItemDto, ProductItemCreateDto, ProductItemUpdateDto>, IProductItemRepository
{
    public ProductItemRepository(
        ILoggerManager logger,
        RepositoryContext repositoryContext,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
        : base(logger, repositoryContext, httpContextAccessor, mapper)
    {
    }

    public async Task<ProductItem?> GetBySerialNumberAsync(string serialNumber)
    {
        return await RepositoryContext.ProductItems
            .Include(pi => pi.Product)
            .FirstOrDefaultAsync(pi => pi.SerialNumber == serialNumber && !pi.IsDeleted);
    }

    public async Task<IEnumerable<ProductItem>> GetByProductIdAsync(int productId)
    {
        return await RepositoryContext.ProductItems
            .Include(pi => pi.Product)
            .Where(pi => pi.ProductId == productId && !pi.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProductItem>> GetInStockByProductIdAsync(int productId)
    {
        return await RepositoryContext.ProductItems
            .Include(pi => pi.Product)
            .Where(pi => pi.ProductId == productId && pi.Status == Entities.Models.Enums.ProductItemStatus.InStock && !pi.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProductItem>> GetByExitRequestIdAsync(int exitRequestId)
    {
        return await RepositoryContext.ProductItems
            .Include(pi => pi.Product)
            .Where(pi => pi.ProductExitRequestId == exitRequestId && !pi.IsDeleted)
            .ToListAsync();
    }

    public async Task<ProductItem?> GetByIdAsync(int id)
    {
        return await RepositoryContext.ProductItems
            .Include(pi => pi.Product)
            .FirstOrDefaultAsync(pi => pi.Id == id && !pi.IsDeleted);
    }
}


