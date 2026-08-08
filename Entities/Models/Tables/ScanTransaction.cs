using Entities.Models.BaseTables;
using Entities.Models.Enums;

namespace Entities.Models.Tables;

public class ScanTransaction : BaseTable
{
    public string BarcodeScanned { get; set; } = string.Empty;
    public TransactionType TransactionType { get; set; }
    public int Quantity { get; set; }
    public string? ScannerDeviceId { get; set; }

    public int? ProductId { get; set; }
    public Product? Product { get; set; }
}
