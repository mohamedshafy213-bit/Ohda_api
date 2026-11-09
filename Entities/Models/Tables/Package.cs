using System.Collections.Generic;
using Entities.Models.BaseTables;

namespace Entities.Models.Tables;

public class Package : TenantBaseTable
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<PackageItem> Items { get; set; } = new List<PackageItem>();
}

