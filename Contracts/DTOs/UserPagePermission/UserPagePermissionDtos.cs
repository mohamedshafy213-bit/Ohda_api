using Contracts.BaseDtos;

namespace Contracts.DTOs.UserPagePermission;

public class UserPagePermissionDto : BaseDto
{
    public int UserId { get; set; }
    public string? Username { get; set; }
    public int PageId { get; set; }
    public string? PageTitle { get; set; }
    public string? PagePath { get; set; }
    public string? PageIcon { get; set; }
    public int? GrantedByUserId { get; set; }
    public string? GrantedByUsername { get; set; }
}

public class GrantPermissionDto : BaseCreateDto
{
    public int UserId { get; set; }
    public int PageId { get; set; }
}

public class UserPagePermissionUpdateDto : BaseUpdateDto
{
    public int UserId { get; set; }
    public int PageId { get; set; }
}
