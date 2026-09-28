using Contracts.DTOs.DashBoard;
using Contracts.Responses;
using Entities.Models.Databases;
using Entities.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Service_API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly RepositoryContext _context;
    private readonly IMemoryCache _memoryCache;

    public DashboardController(RepositoryContext context, IMemoryCache memoryCache)
    {
        _context = context;
        _memoryCache = memoryCache;
    }

    [HttpGet]
    [HttpGet("product-status")]
    public async Task<IActionResult> GetProductStatusDashboard()
    {
        var branchId = _context.CurrentBranchId ?? 0;
        var cacheKey = $"Dashboard_ProductStatus_{branchId}";

        if (_memoryCache.TryGetValue(cacheKey, out ProductDashboardDto? cachedDto) && cachedDto != null)
        {
            return Ok(new SingleObjectResponseModel<ProductDashboardDto>
            {
                IsDone = true,
                ReturnMessage = "Product and inventory status dashboard retrieved successfully.",
                SingleObject = cachedDto
            });
        }

        var totalCategories = await _context.Categories.AsNoTracking().CountAsync(c => !c.IsDeleted);
        var totalSuppliers = await _context.Suppliers.AsNoTracking().CountAsync(s => !s.IsDeleted);
        var totalProducts = await _context.Products.AsNoTracking().CountAsync(p => !p.IsDeleted);

        var inventoryStats = await _context.Inventories
            .AsNoTracking()
            .Where(i => !i.IsDeleted)
            .GroupBy(i => 1)
            .Select(g => new
            {
                TotalStockQuantity = g.Sum(i => i.Quantity),
                TotalStockValue = g.Sum(i => (decimal?)i.Quantity * (i.Product != null ? i.Product.UnitPrice ?? 0 : 0)) ?? 0,
                LowStockCount = g.Count(i => i.Quantity <= i.MinStock && i.Quantity > 0),
                OutOfStockCount = g.Count(i => i.Quantity == 0)
            })
            .FirstOrDefaultAsync();

        int totalStockQuantity = inventoryStats?.TotalStockQuantity ?? 0;
        decimal totalStockValue = inventoryStats?.TotalStockValue ?? 0;
        int lowStockCount = inventoryStats?.LowStockCount ?? 0;
        int outOfStockCount = inventoryStats?.OutOfStockCount ?? 0;

        int pendingExitRequestsCount = await _context.ProductExitRequests
            .AsNoTracking()
            .CountAsync(r => !r.IsDeleted && r.Status == RequestStatus.Pending);

        var categoryDistribution = await _context.Products
            .AsNoTracking()
            .Where(p => !p.IsDeleted && p.Category != null)
            .GroupBy(p => p.Category!.Name)
            .Select(g => new CategoryCountDto
            {
                CategoryName = g.Key,
                ProductCount = g.Count()
            })
            .ToListAsync();

        var supplierDistribution = await _context.Products
            .AsNoTracking()
            .Where(p => !p.IsDeleted && p.Supplier != null)
            .GroupBy(p => p.Supplier!.CompanyName)
            .Select(g => new SupplierCountDto
            {
                SupplierName = g.Key,
                ProductCount = g.Count()
            })
            .ToListAsync();

        var lowStockAlerts = await _context.Inventories
            .AsNoTracking()
            .Where(i => !i.IsDeleted && i.Quantity <= i.MinStock && i.Product != null && !i.Product.IsDeleted)
            .OrderBy(i => i.Quantity)
            .Take(50)
            .Select(i => new LowStockItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.Product!.Name,
                SKU = i.Product.SKU,
                Barcode = i.Product.Barcode,
                CurrentQuantity = i.Quantity,
                MinStock = i.MinStock
            })
            .ToListAsync();

        var dashboardDto = new ProductDashboardDto
        {
            TotalProducts = totalProducts,
            TotalCategories = totalCategories,
            TotalSuppliers = totalSuppliers,
            TotalStockQuantity = totalStockQuantity,
            TotalStockValue = totalStockValue,
            LowStockProductsCount = lowStockCount,
            OutOfStockProductsCount = outOfStockCount,
            PendingExitRequestsCount = pendingExitRequestsCount,
            ProductsByCategory = categoryDistribution,
            ProductsBySupplier = supplierDistribution,
            LowStockAlerts = lowStockAlerts
        };

        _memoryCache.Set(cacheKey, dashboardDto, TimeSpan.FromMinutes(2));

        return Ok(new SingleObjectResponseModel<ProductDashboardDto>
        {
            IsDone = true,
            ReturnMessage = "Product and inventory status dashboard retrieved successfully.",
            SingleObject = dashboardDto
        });
    }
}
