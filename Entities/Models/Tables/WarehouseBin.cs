using Entities.Models.BaseTables;

namespace Entities.Models.Tables;

public class WarehouseBin : TenantBaseTable
{
    public string Code { get; set; } = string.Empty; // e.g. "A-01-B03"
    public string Name { get; set; } = string.Empty; // e.g. "رف أجهزة الحواسب واللابتوبات"
    public string? Aisle { get; set; } // e.g. "A"
    public string? Shelf { get; set; } // e.g. "01"
    public int? Capacity { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public int DepartmentId { get; set; }
    public Department? Department { get; set; }
}
