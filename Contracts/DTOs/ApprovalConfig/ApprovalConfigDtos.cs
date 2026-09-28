using Contracts.BaseDtos;
using Entities.Models.Enums;

namespace Contracts.DTOs.ApprovalConfig;

public class ApprovalConfigDto : BaseDto
{
    public RequestType RequestType { get; set; }
    public int UserGroupId { get; set; }
    public string? UserGroupName { get; set; }
    public WorkflowRole WorkflowRole { get; set; }
    public int StepOrder { get; set; } = 1;
    public string? StepName { get; set; }
    public string? ColorHex { get; set; } = "#3B82F6";
    public bool IsActive { get; set; }
}

public class ApprovalConfigCreateDto : BaseCreateDto
{
    public RequestType RequestType { get; set; }
    public int UserGroupId { get; set; }
    public WorkflowRole WorkflowRole { get; set; }
    public int StepOrder { get; set; } = 1;
    public string? StepName { get; set; }
    public string? ColorHex { get; set; } = "#3B82F6";
    public bool IsActive { get; set; } = true;
}

public class ApprovalConfigUpdateDto : BaseUpdateDto
{
    public RequestType RequestType { get; set; }
    public int UserGroupId { get; set; }
    public WorkflowRole WorkflowRole { get; set; }
    public int StepOrder { get; set; } = 1;
    public string? StepName { get; set; }
    public string? ColorHex { get; set; } = "#3B82F6";
    public bool IsActive { get; set; }
}
