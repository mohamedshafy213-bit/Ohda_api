using Entities.Models.BaseTables;

namespace Entities.Models.Tables;

public class Category : BaseTable
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
