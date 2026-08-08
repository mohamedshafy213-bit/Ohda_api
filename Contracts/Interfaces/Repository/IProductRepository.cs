using Contracts.DTOs.Product;
using Contracts.interfaces.Repository;
using Entities.Models.Tables;

namespace Contracts.Interfaces.Repository;

public interface IProductRepository : IRepositoryBase<Product, ProductDto, ProductCreateDto, ProductUpdateDto>
{
    Task<Product?> GetByBarcodeAsync(string barcode);
    Task<Product?> GetBySKUAsync(string sku);
    Task<Product?> GetByNameAsync(string name);
}

