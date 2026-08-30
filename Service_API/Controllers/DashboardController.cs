using Contracts.DTOs.DashBoard;
using Contracts.Responses;
using Entities.Models.Databases;
using Entities.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Service_API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly RepositoryContext _context;

    public DashboardController(RepositoryContext context)
    {
        _context = context;
    }

    [HttpGet]
    [HttpGet("product-status")]
    public async Task<IActionResult> GetProductStatusDashboard()
    {
        var totalProducts = await _context.Products.CountAsync(p => !p.IsDeleted);
        var totalCategories = await _context.Categories.CountAsync(c => !c.IsDeleted);
        var totalSuppliers = await _context.Suppliers.CountAsync(s => !s.IsDeleted);

        var inventories = await _context.Inventories
            .Include(i => i.Product)
            .Where(i => !i.IsDeleted && i.Product != null && !i.Product.IsDeleted)
            .ToListAsync();

        int totalStockQuantity = inventories.Sum(i => i.Quantity);
        decimal totalStockValue = inventories.Sum(i => i.Quantity * (i.Product!.UnitPrice ?? 0));

        int lowStockCount = inventories.Count(i => i.Quantity <= i.MinStock && i.Quantity > 0);
        int outOfStockCount = inventories.Count(i => i.Quantity == 0);

        int pendingExitRequestsCount = await _context.ProductExitRequests
            .CountAsync(r => !r.IsDeleted && r.Status == RequestStatus.Pending);

        var categoryDistribution = await _context.Products
            .Where(p => !p.IsDeleted && p.Category != null)
            .GroupBy(p => p.Category!.Name)
            .Select(g => new CategoryCountDto
            {
                CategoryName = g.Key,
                ProductCount = g.Count()
            })
            .ToListAsync();

        var supplierDistribution = await _context.Products
            .Where(p => !p.IsDeleted && p.Supplier != null)
            .GroupBy(p => p.Supplier!.CompanyName)
            .Select(g => new SupplierCountDto
            {
                SupplierName = g.Key,
                ProductCount = g.Count()
            })
            .ToListAsync();

        var lowStockAlerts = inventories
            .Where(i => i.Quantity <= i.MinStock)
            .Select(i => new LowStockItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.Product!.Name,
                SKU = i.Product.SKU,
                Barcode = i.Product.Barcode,
                CurrentQuantity = i.Quantity,
                MinStock = i.MinStock
            })
            .OrderBy(i => i.CurrentQuantity)
            .ToList();

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

        return Ok(new SingleObjectResponseModel<ProductDashboardDto>
        {
            IsDone = true,
            ReturnMessage = "Product and inventory status dashboard retrieved successfully.",
            SingleObject = dashboardDto
        });
    }
}
