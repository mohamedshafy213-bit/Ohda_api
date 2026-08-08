namespace Contracts.DTOs.DashBoard;

public class ProductDashboardDto
{
    public int TotalProducts { get; set; }
    public int TotalCategories { get; set; }
    public int TotalSuppliers { get; set; }
    public int TotalStockQuantity { get; set; }
    public decimal TotalStockValue { get; set; }
    public int LowStockProductsCount { get; set; }
    public int OutOfStockProductsCount { get; set; }
    public int PendingExitRequestsCount { get; set; }

    public List<CategoryCountDto> ProductsByCategory { get; set; } = new();
    public List<SupplierCountDto> ProductsBySupplier { get; set; } = new();
    public List<LowStockItemDto> LowStockAlerts { get; set; } = new();
}

public class CategoryCountDto
{
    public string CategoryName { get; set; } = string.Empty;
    public int ProductCount { get; set; }
}

public class SupplierCountDto
{
    public string SupplierName { get; set; } = string.Empty;
    public int ProductCount { get; set; }
}

public class LowStockItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public int CurrentQuantity { get; set; }
    public int MinStock { get; set; }
}
