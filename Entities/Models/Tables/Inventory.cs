using Entities.Models.BaseTables;

namespace Entities.Models.Tables;

public class Inventory : TenantBaseTable
{
    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int Quantity { get; set; }
    public int MinStock { get; set; }
    public int MaxStock { get; set; }

    public int? BinId { get; set; }
    public WarehouseBin? Bin { get; set; }
}
