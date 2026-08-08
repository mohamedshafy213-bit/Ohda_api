using Entities.Models.BaseTables;
using Entities.Models.Enums;

namespace Entities.Models.Tables;

public class ProductItem : BaseTable
{
    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public string SerialNumber { get; set; } = string.Empty;
    public string QRCode { get; set; } = string.Empty;

    public ProductItemStatus Status { get; set; } = ProductItemStatus.InStock;

    // Exit details / current custody
    public string? RecipientName { get; set; }
    public string? Place { get; set; } // department or location
    public DateTime? ExitDate { get; set; }
    public int? ProductExitRequestId { get; set; }
    public ProductExitRequest? ProductExitRequest { get; set; }
    public string? Notes { get; set; }
}
