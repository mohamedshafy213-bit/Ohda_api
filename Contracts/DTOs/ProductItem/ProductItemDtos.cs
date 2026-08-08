using Contracts.BaseDtos;
using Entities.Models.Enums;

namespace Contracts.DTOs.ProductItem;

public class ProductItemDto : BaseDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductBarcode { get; set; } = string.Empty;

    public string SerialNumber { get; set; } = string.Empty;
    public string QRCode { get; set; } = string.Empty;

    public ProductItemStatus Status { get; set; }
    public string StatusName => Status.ToString();

    public string? RecipientName { get; set; }
    public string? Place { get; set; }
    public DateTime? ExitDate { get; set; }
    public int? ProductExitRequestId { get; set; }
    public string? Notes { get; set; }
}

public class ProductItemCreateDto : BaseCreateDto
{
    public int ProductId { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string QRCode { get; set; } = string.Empty;
    public ProductItemStatus Status { get; set; } = ProductItemStatus.InStock;
    public string? RecipientName { get; set; }
    public string? Place { get; set; }
    public string? Notes { get; set; }
}

public class ProductItemUpdateDto : BaseUpdateDto
{
    public int ProductId { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string QRCode { get; set; } = string.Empty;
    public ProductItemStatus Status { get; set; }
    public string? RecipientName { get; set; }
    public string? Place { get; set; }
    public DateTime? ExitDate { get; set; }
    public int? ProductExitRequestId { get; set; }
    public string? Notes { get; set; }
}
