using Entities.Models.BaseTables;

namespace Entities.Models.Tables;

public class ProductState : TenantBaseTable
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
}
