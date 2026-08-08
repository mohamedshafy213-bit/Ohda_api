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
}
