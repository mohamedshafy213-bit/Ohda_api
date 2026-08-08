using Entities.Models.BaseTables;
using Entities.Models.Enums;

namespace Entities.Models.Tables;

public class Product : BaseTable
{
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    public decimal UnitPrice { get; set; }
    public InventoryType InventoryType { get; set; } = InventoryType.Purchased;
    public decimal? PurchasePrice { get; set; }
    public decimal? AssetValue { get; set; }
}
