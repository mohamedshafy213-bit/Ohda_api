using Entities.Models.BaseTables;

namespace Entities.Models.Tables;

public class Supplier : BaseTable
{
    public string CompanyName { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
}
