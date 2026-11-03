using Contracts.BaseDtos;
using Entities.Models.Enums;

namespace Contracts.DTOs.ApprovalConfig;

public class ApprovalUserSummaryDto
{
    public long MilitaryNumber { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? PersonName { get; set; }
}

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
    public List<ApprovalUserSummaryDto> EligibleUsers { get; set; } = new();
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
