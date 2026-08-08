using Contracts.DTOs.Inventory;
using Contracts.interfaces.Repository;
using Entities.Models.Tables;

namespace Contracts.Interfaces.Repository;

public interface IInventoryRepository : IRepositoryBase<Inventory, InventoryDto, InventoryCreateDto, InventoryUpdateDto>
{
    Task<Inventory?> GetByProductIdAsync(int productId);
}
