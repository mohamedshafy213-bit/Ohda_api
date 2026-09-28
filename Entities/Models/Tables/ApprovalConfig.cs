using Entities.Models.BaseTables;
using Entities.Models.Enums;

namespace Entities.Models.Tables;

public class ApprovalConfig : TenantBaseTable
{
    public RequestType RequestType { get; set; }
    public int UserGroupId { get; set; }
    public UserGroup? UserGroup { get; set; }
    public WorkflowRole WorkflowRole { get; set; }
    public int StepOrder { get; set; } = 1;
    public string? StepName { get; set; }
    public string? ColorHex { get; set; } = "#3B82F6";
    public bool IsActive { get; set; } = true;
}
