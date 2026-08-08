using Contracts.BaseDtos;
using Entities.Models.Enums;

namespace Contracts.DTOs.ScanTransaction;

public class ScanTransactionDto : BaseDto
{
    public string BarcodeScanned { get; set; } = string.Empty;
    public TransactionType TransactionType { get; set; }
    public int Quantity { get; set; }
    public string? ScannerDeviceId { get; set; }
    public int? ProductId { get; set; }
    public string? ProductName { get; set; }
    public DateTime? InsertDate { get; set; }
}

public class ScanTransactionCreateDto : BaseCreateDto
{
    public string BarcodeScanned { get; set; } = string.Empty;
    public TransactionType TransactionType { get; set; }
    public int Quantity { get; set; }
    public string? ScannerDeviceId { get; set; }
    public int? ProductId { get; set; }
}

public class ScanTransactionUpdateDto : BaseUpdateDto
{
    public string BarcodeScanned { get; set; } = string.Empty;
    public TransactionType TransactionType { get; set; }
    public int Quantity { get; set; }
    public string? ScannerDeviceId { get; set; }
}

public class ScanRequestDto
{
    public string BarcodeScanned { get; set; } = string.Empty;
    public TransactionType TransactionType { get; set; }
    public int Quantity { get; set; }
    public string? ScannerDeviceId { get; set; }
}
