using Contracts.BaseDtos;

namespace Contracts.DTOs.Category;

public class CategoryDto : BaseDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class CategoryCreateDto : BaseCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class CategoryUpdateDto : BaseUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
