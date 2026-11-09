using Entities.Models.BaseTables;

namespace Entities.Models.Tables;

public class UntrackedItem : TenantBaseTable
{
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

