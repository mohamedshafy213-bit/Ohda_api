using Contracts.DTOs.Product;
using Contracts.Interfaces.Repository;
using Entities.Models.Databases;
using Entities.Models.Tables;
using LoggerService;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.Repositories;

namespace Repositories.Models;

public class ProductRepository
    : RepositoryBase<Product, ProductDto, ProductCreateDto, ProductUpdateDto>, IProductRepository
{
    public ProductRepository(
        ILoggerManager logger,
        RepositoryContext repositoryContext,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
        : base(logger, repositoryContext, httpContextAccessor, mapper)
    {
    }

    public async Task<Product?> GetByBarcodeAsync(string barcode)
    {
        return await RepositoryContext.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(p => p.Barcode == barcode && !p.IsDeleted);
    }

    public async Task<Product?> GetBySKUAsync(string sku)
    {
        return await RepositoryContext.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(p => p.SKU == sku && !p.IsDeleted);
    }
}
