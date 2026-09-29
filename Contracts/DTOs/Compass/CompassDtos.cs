using Contracts.BaseDtos;
using Entities.Models.Enums;

namespace Contracts.DTOs.Compass;

public class CompassDto : BaseDto
{
    public string SerialNumber { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public string? DelivererName { get; set; }
    public string? SupervisorName { get; set; }
    public string? ManagerName { get; set; }
    public string? ApprovalTrail { get; set; }
    public DateTime? RequesterConfirmedDate { get; set; }
    public string? Purpose { get; set; }
    public string Place { get; set; } = string.Empty;
    public DateTime ExitDate { get; set; }
    public CompassType Type { get; set; } = CompassType.Exit;
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public int? ProductStateId { get; set; }
    public string? ProductStateName { get; set; }
    public int? ProductExitRequestId { get; set; }
    public int? ProductEntryRequestId { get; set; }
    public string? Notes { get; set; }
}

public class CompassCreateDto : BaseCreateDto
{
    public string SerialNumber { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public string Place { get; set; } = string.Empty;
    public DateTime ExitDate { get; set; } = DateTime.UtcNow;
    public CompassType Type { get; set; } = CompassType.Exit;
    public int? DepartmentId { get; set; }
    public int? ProductStateId { get; set; }
    public int? ProductExitRequestId { get; set; }
    public int? ProductEntryRequestId { get; set; }
    public string? Notes { get; set; }
}

public class CompassUpdateDto : BaseUpdateDto
{
    public string SerialNumber { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public string Place { get; set; } = string.Empty;
    public DateTime ExitDate { get; set; }
    public CompassType Type { get; set; }
    public int? DepartmentId { get; set; }
    public int? ProductStateId { get; set; }
    public int? ProductExitRequestId { get; set; }
    public int? ProductEntryRequestId { get; set; }
    public string? Notes { get; set; }
}
