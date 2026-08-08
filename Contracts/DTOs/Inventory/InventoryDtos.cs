using Contracts.BaseDtos;

namespace Contracts.DTOs.Inventory;

public class InventoryDto : BaseDto
{
    public int ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductSKU { get; set; }
    public string? ProductBarcode { get; set; }
    public int Quantity { get; set; }
    public int MinStock { get; set; }
    public int MaxStock { get; set; }
}

public class InventoryCreateDto : BaseCreateDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public int MinStock { get; set; }
    public int MaxStock { get; set; }
}

public class InventoryUpdateDto : BaseUpdateDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public int MinStock { get; set; }
    public int MaxStock { get; set; }
}
