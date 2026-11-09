using System.Collections.Generic;

namespace Contracts.DTOs.Package;

public class PackageItemDto
{
    public int Id { get; set; }
    public int PackageId { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int? TrackedProductId { get; set; }
    public string? TrackedProductName { get; set; }
    public string? TrackedProductSKU { get; set; }
    public int? UntrackedItemId { get; set; }
    public string? UntrackedItemName { get; set; }
    public int Quantity { get; set; } = 1;
    public string ItemType => UntrackedItemId.HasValue ? "Untracked" : (CategoryId.HasValue ? "Category" : "Tracked");
}

public class PackageItemCreateDto
{
    public int? CategoryId { get; set; }
    public int? TrackedProductId { get; set; }
    public int? UntrackedItemId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class PackageDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int BranchId { get; set; }
    public List<PackageItemDto> Items { get; set; } = new();
}

public class PackageCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int? BranchId { get; set; }
    public List<PackageItemCreateDto> Items { get; set; } = new();
}

public class PackageUpdateDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public List<PackageItemCreateDto> Items { get; set; } = new();
}

