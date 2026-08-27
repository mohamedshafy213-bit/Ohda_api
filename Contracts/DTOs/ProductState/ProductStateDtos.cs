using Contracts.BaseDtos;

namespace Contracts.DTOs.ProductState;

public class ProductStateDto : BaseDto
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
}

public class ProductStateCreateDto : BaseCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
}

public class ProductStateUpdateDto : BaseUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
}
