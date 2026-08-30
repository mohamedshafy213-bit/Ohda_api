using Contracts.DTOs.ProductItem;
using Contracts.interfaces.Repository;
using Entities.Models.Tables;

namespace Contracts.Interfaces.Repository;

public interface IProductItemRepository : IRepositoryBase<ProductItem, ProductItemDto, ProductItemCreateDto, ProductItemUpdateDto>
{
    Task<ProductItem?> GetBySerialNumberAsync(string serialNumber);
    Task<IEnumerable<ProductItem>> GetByProductIdAsync(int productId);
    Task<IEnumerable<ProductItem>> GetInStockByProductIdAsync(int productId);
    Task<IEnumerable<ProductItem>> GetByExitRequestIdAsync(int exitRequestId);
    Task<IEnumerable<ProductItem>> GetByDepartmentIdAsync(int departmentId);
    Task<ProductItem?> GetByIdAsync(int id);
}


