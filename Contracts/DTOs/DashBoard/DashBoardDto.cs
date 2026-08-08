using System.Collections.Generic;

namespace Contracts.DTOs.DashBoard;

public class DashBoardDto
{
    public int TotalHalls { get; set; }
    public int TotalReservations { get; set; }
    public int ConfirmedReservations { get; set; }
    public int DeclinedReservations { get; set; }
    public int WaitingReservations { get; set; }
    
    public List<ReservationByUsageDto> ReservationsByUsage { get; set; } = new List<ReservationByUsageDto>();
    public List<ReservationByHallTypeDto> ReservationsByHallType { get; set; } = new List<ReservationByHallTypeDto>();
}

public class ReservationByUsageDto
{
    public string? UsageName { get; set; }
    public int Count { get; set; }
}

public class ReservationByHallTypeDto
{
    public string? TypeName { get; set; }
    public int Count { get; set; }
}
