using Contracts.BaseDtos;

namespace Contracts.DTOs.UserGroup;

public class UserGroupDto : BaseDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UserGroupCreateDto : BaseCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UserGroupUpdateDto : BaseUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
