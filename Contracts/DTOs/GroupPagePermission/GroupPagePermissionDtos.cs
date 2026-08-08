using Contracts.BaseDtos;

namespace Contracts.DTOs.GroupPagePermission;

public class GroupPagePermissionDto : BaseDto
{
    public int UserGroupId { get; set; }
    public string UserGroupName { get; set; } = string.Empty;

    public int PageId { get; set; }
    public string PageTitle { get; set; } = string.Empty;
    public string PagePath { get; set; } = string.Empty;

    public int? GrantedByUserId { get; set; }
    public string? GrantedByUsername { get; set; }
}

public class GrantGroupPermissionDto : BaseCreateDto
{
    public int UserGroupId { get; set; }
    public int PageId { get; set; }
}

public class GroupPagePermissionUpdateDto : BaseUpdateDto
{
    public int UserGroupId { get; set; }
    public int PageId { get; set; }
}
