using Entities.Models.BaseTables;

namespace Entities.Models.Tables;

public class Page : BaseTable
{
    public string Title { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int SortOrder { get; set; }
    public string AllowedRoles { get; set; } = string.Empty;
}
