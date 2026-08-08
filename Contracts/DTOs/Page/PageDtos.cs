using Contracts.BaseDtos;

namespace Contracts.DTOs.Page;

public class PageDto : BaseDto
{
    public string Title { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int SortOrder { get; set; }
  
}

public class PageCreateDto : BaseCreateDto
{
    public string Title { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int SortOrder { get; set; }
    
}

public class PageUpdateDto : BaseUpdateDto
{
    public string Title { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int SortOrder { get; set; }

}
