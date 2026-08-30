using Contracts.BaseDtos;
using Entities.Models.Enums;

namespace Contracts.DTOs.Product;

public class ProductDto : BaseDto
{
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public decimal? UnitPrice { get; set; }
    public InventoryType InventoryType { get; set; }
    public decimal? PurchasePrice { get; set; }
    public decimal? AssetValue { get; set; }
    public int Amount { get; set; }
    public int Quantity { get; set; }
}

public class ProductCreateDto : BaseCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public int? SupplierId { get; set; }
    public decimal? UnitPrice { get; set; }
    public InventoryType InventoryType { get; set; } = InventoryType.Purchased;
    public decimal? PurchasePrice { get; set; }
    public decimal? AssetValue { get; set; }
    public int Amount { get; set; }
    public int Quantity { get; set; }
}

public class ProductUpdateDto : BaseUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public int? SupplierId { get; set; }
    public decimal? UnitPrice { get; set; }
    public InventoryType InventoryType { get; set; }
    public decimal? PurchasePrice { get; set; }
    public decimal? AssetValue { get; set; }
    public int Amount { get; set; }
    public int Quantity { get; set; }
}
