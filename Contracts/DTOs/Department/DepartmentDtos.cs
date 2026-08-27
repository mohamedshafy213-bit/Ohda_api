using Contracts.BaseDtos;

namespace Contracts.DTOs.Department;

public class DepartmentDto : BaseDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class DepartmentCreateDto : BaseCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class DepartmentUpdateDto : BaseUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
