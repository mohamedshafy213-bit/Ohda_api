using Entities.Models.BaseTables;
using Entities.Models.Enums;

namespace Entities.Models.Tables;

public class ApprovalConfig : BaseTable
{
    public RequestType RequestType { get; set; }
    public int UserGroupId { get; set; }
    public UserGroup? UserGroup { get; set; }
    public WorkflowRole WorkflowRole { get; set; }
    public bool IsActive { get; set; } = true;
}
