using Contracts.BaseDtos;
using System;

namespace Contracts.DTOs.WarehouseBin;

public class WarehouseBinDto : BaseDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Aisle { get; set; }
    public string? Shelf { get; set; }
    public int? Capacity { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public int ItemsCount { get; set; }
    public DateTime? InsertDate { get; set; }
    public DateTime? LastUpdate { get; set; }
}

public class WarehouseBinCreateDto : BaseCreateDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Aisle { get; set; }
    public string? Shelf { get; set; }
    public int? Capacity { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int? DepartmentId { get; set; }
}

public class WarehouseBinUpdateDto : BaseUpdateDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Aisle { get; set; }
    public string? Shelf { get; set; }
    public int? Capacity { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int? DepartmentId { get; set; }
}

public class WarehouseBinItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSKU { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string QRCode { get; set; } = string.Empty;
    public int Status { get; set; }
    public string? StatusLabel { get; set; }
    public string? Place { get; set; }
    public DateTime? InsertDate { get; set; }
}

public class AssignProductToBinDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; } = 1;
}

