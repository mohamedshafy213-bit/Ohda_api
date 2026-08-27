using Entities.Models.BaseTables;

namespace Entities.Models.Tables;

public class ProductState : BaseTable
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
}
