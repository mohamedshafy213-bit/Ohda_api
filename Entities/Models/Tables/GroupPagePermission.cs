using Entities.Models.BaseTables;

namespace Entities.Models.Tables;

public class GroupPagePermission : BaseTable
{
    public int UserGroupId { get; set; }
    public UserGroup? UserGroup { get; set; }

    public int PageId { get; set; }
    public Page? Page { get; set; }

    public int? GrantedByUserId { get; set; }
    public User? GrantedByUser { get; set; }
}
