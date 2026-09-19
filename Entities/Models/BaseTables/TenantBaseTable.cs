using Entities.Models.Interfaces;
using Entities.Models.Tables;

namespace Entities.Models.BaseTables;

public abstract class TenantBaseTable : BaseTable, ITenantEntity
{
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }
}
