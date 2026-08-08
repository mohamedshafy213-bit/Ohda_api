using Entities.Models.BaseTables;

namespace Entities.Models.Tables;

public class UserPagePermission : BaseTable
{
    public int UserId { get; set; }
    public User? User { get; set; }

    public int PageId { get; set; }
    public Page? Page { get; set; }

    public int? GrantedByUserId { get; set; }
    public User? GrantedByUser { get; set; }
}
