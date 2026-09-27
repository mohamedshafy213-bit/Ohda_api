using Contracts.DTOs.Inventory;
using Contracts.Interfaces.Repository;
using Entities.Models.Databases;
using Entities.Models.Tables;
using LoggerService;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.Repositories;

namespace Repositories.Models;

public class InventoryRepository
    : RepositoryBase<Inventory, InventoryDto, InventoryCreateDto, InventoryUpdateDto>, IInventoryRepository
{
    public InventoryRepository(
        ILoggerManager logger,
        RepositoryContext repositoryContext,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
        : base(logger, repositoryContext, httpContextAccessor, mapper)
    {
    }

    public async Task<Inventory?> GetByProductIdAsync(int productId)
    {
        return await RepositoryContext.Inventories
            .Include(i => i.Product)
            .FirstOrDefaultAsync(i => i.ProductId == productId && !i.IsDeleted);
    }

    public async Task<Dictionary<int, int>> GetQuantitiesByProductIdsAsync(IEnumerable<int> productIds)
    {
        var ids = productIds.Distinct().ToList();
        if (!ids.Any())
            return new Dictionary<int, int>();

        return await RepositoryContext.Inventories
            .AsNoTracking()
            .Where(i => ids.Contains(i.ProductId) && !i.IsDeleted)
            .GroupBy(i => i.ProductId)
            .Select(g => new { ProductId = g.Key, Quantity = g.Sum(x => x.Quantity) })
            .ToDictionaryAsync(x => x.ProductId, x => x.Quantity);
    }
}
