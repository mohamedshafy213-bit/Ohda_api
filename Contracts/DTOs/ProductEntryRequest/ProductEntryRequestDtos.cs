using Contracts.BaseDtos;
using Entities.Models.Enums;

namespace Contracts.DTOs.ProductEntryRequest;

public class ProductEntryRequestDto : BaseDto
{
    public int ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductSKU { get; set; }
    public int EnteredQuantity { get; set; }
    public RequestStatus Status { get; set; }

    public string FromSource { get; set; } = string.Empty;
    public string? InvoiceNumber { get; set; }
    public string? Notes { get; set; }

    public int ReceivedByUserId { get; set; }
    public string? ReceivedByUsername { get; set; }

    public int? ManagerId { get; set; }
    public string? ManagerUsername { get; set; }
    public bool ManagerApprove => ManagerId.HasValue && ManagerId > 0;

    public int? SupervisorId { get; set; }
    public string? SupervisorUsername { get; set; }
    public bool SupervisorApprove => SupervisorId.HasValue && SupervisorId > 0;

    public string? RejectionReason { get; set; }
    public DateTime? InsertDate { get; set; }
}

public class ProductEntryCreateDto : BaseCreateDto
{
    public int ProductId { get; set; }
    public int EnteredQuantity { get; set; }
    public string FromSource { get; set; } = string.Empty;
    public string? InvoiceNumber { get; set; }
    public string? Notes { get; set; }
    public int ReceivedByUserId { get; set; }
}

public class ProductEntryUpdateDto : BaseUpdateDto
{
    public int ProductId { get; set; }
    public int EnteredQuantity { get; set; }
    public RequestStatus Status { get; set; }
    public string FromSource { get; set; } = string.Empty;
    public string? InvoiceNumber { get; set; }
    public string? Notes { get; set; }
    public int ReceivedByUserId { get; set; }
    public int? ManagerId { get; set; }
    public int? SupervisorId { get; set; }
    public string? RejectionReason { get; set; }
}
