using Contracts.BaseDtos;
using Entities.Models.Enums;

namespace Contracts.DTOs.ProductExitRequest;

public class ProductExitRequestDto : BaseDto
{
    public int ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductSKU { get; set; }
    public int RequestedQuantity { get; set; }
    public RequestStatus Status { get; set; }

    public string RecipientName { get; set; } = string.Empty;
    public string? RecipientDepartment { get; set; }
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public string? Purpose { get; set; }

    public int RequestedByUserId { get; set; }
    public string? RequestedByUsername { get; set; }

    public int? ManagerId { get; set; }
    public string? ManagerUsername { get; set; }
    public bool ManagerApprove => ManagerId.HasValue && ManagerId > 0;

    public int? SupervisorId { get; set; }
    public string? SupervisorUsername { get; set; }
    public bool SupervisorApprove => SupervisorId.HasValue && SupervisorId > 0;

    public string? RejectionReason { get; set; }
    public DateTime? InsertDate { get; set; }

    public List<int> SelectedProductItemIds { get; set; } = new();
    public List<string> SelectedSerials { get; set; } = new();
}

public class ProductExitCreateDto : BaseCreateDto
{
    public int ProductId { get; set; }
    public int RequestedQuantity { get; set; }
    public string RecipientName { get; set; } = string.Empty;
    public string? RecipientDepartment { get; set; }
    public int? DepartmentId { get; set; }
    public string? Purpose { get; set; }
    public int RequestedByUserId { get; set; }
    public List<int> SelectedProductItemIds { get; set; } = new();
}

public class ProductExitUpdateDto : BaseUpdateDto
{
    public int ProductId { get; set; }
    public int RequestedQuantity { get; set; }
    public RequestStatus Status { get; set; }
    public string RecipientName { get; set; } = string.Empty;
    public string? RecipientDepartment { get; set; }
    public int? DepartmentId { get; set; }
    public string? Purpose { get; set; }
    public int RequestedByUserId { get; set; }
    public int? ManagerId { get; set; }
    public int? SupervisorId { get; set; }
    public string? RejectionReason { get; set; }
    public List<int> SelectedProductItemIds { get; set; } = new();
}


public class RejectRequestDto
{
    public string RejectionReason { get; set; } = string.Empty;
}
