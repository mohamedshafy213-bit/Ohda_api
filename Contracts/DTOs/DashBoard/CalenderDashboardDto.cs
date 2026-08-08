using Contracts.BaseDtos;

namespace Contracts.DTOs.DashBoard;

public class CalenderDashboardDto : BaseDto
{
    public string? HallName { get; set; }
    public string? FirmName { get; set; }
    public DateTime? StDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsConfirmed { get; set; }
    public bool IsWaiting { get; set; }
    public bool IsCanceled { get; set; }
}
