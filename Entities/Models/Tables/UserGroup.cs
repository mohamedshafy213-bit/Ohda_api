using Entities.Models.BaseTables;

namespace Entities.Models.Tables;

public class UserGroup : BaseTable
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<GroupPagePermission> GroupPagePermissions { get; set; } = new List<GroupPagePermission>();
}
