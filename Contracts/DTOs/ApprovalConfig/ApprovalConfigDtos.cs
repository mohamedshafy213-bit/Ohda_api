using Contracts.BaseDtos;
using Entities.Models.Enums;

namespace Contracts.DTOs.ApprovalConfig;

public class ApprovalConfigDto : BaseDto
{
    public RequestType RequestType { get; set; }
    public int UserGroupId { get; set; }
    public string? UserGroupName { get; set; }
    public WorkflowRole WorkflowRole { get; set; }
    public bool IsActive { get; set; }
}

public class ApprovalConfigCreateDto : BaseCreateDto
{
    public RequestType RequestType { get; set; }
    public int UserGroupId { get; set; }
    public WorkflowRole WorkflowRole { get; set; }
    public bool IsActive { get; set; } = true;
}

public class ApprovalConfigUpdateDto : BaseUpdateDto
{
    public RequestType RequestType { get; set; }
    public int UserGroupId { get; set; }
    public WorkflowRole WorkflowRole { get; set; }
    public bool IsActive { get; set; }
}
