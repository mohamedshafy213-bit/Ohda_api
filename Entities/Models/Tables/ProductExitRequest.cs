using Entities.Models.BaseTables;
using Entities.Models.Enums;

namespace Entities.Models.Tables;

public class ProductExitRequest : BaseTable
{
    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int RequestedQuantity { get; set; }
    public RequestStatus Status { get; set; } = RequestStatus.Pending;

    public string RecipientName { get; set; } = string.Empty;
    public string? RecipientDepartment { get; set; }
    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }
    public string? Purpose { get; set; }

    public int RequestedByUserId { get; set; }
    public User? RequestedByUser { get; set; }

    public int? SupervisorId { get; set; }
    public User? Supervisor { get; set; }

    public int? ManagerId { get; set; }
    public User? Manager { get; set; }

    public string? RejectionReason { get; set; }
}
