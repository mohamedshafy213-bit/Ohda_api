namespace Contracts.DTOs.UntrackedItem;

public class UntrackedItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int BranchId { get; set; }
}

public class UntrackedItemCreateDto
{
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int? BranchId { get; set; }
}

public class UntrackedItemUpdateDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

