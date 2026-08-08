using Contracts.BaseDtos;

namespace Contracts.DTOs.Compass;

public class CompassDto : BaseDto
{
    public string SerialNumber { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public string Place { get; set; } = string.Empty;
    public DateTime ExitDate { get; set; }
    public int? ProductExitRequestId { get; set; }
    public string? Notes { get; set; }
}

public class CompassCreateDto : BaseCreateDto
{
    public string SerialNumber { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public string Place { get; set; } = string.Empty;
    public DateTime ExitDate { get; set; } = DateTime.UtcNow;
    public int? ProductExitRequestId { get; set; }
    public string? Notes { get; set; }
}

public class CompassUpdateDto : BaseUpdateDto
{
    public string SerialNumber { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public string Place { get; set; } = string.Empty;
    public DateTime ExitDate { get; set; }
    public int? ProductExitRequestId { get; set; }
    public string? Notes { get; set; }
}
