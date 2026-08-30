using Entities.Models.BaseTables;
using Entities.Models.Enums;

namespace Entities.Models.Tables;

public class ProductEntryRequest : BaseTable
{
    public ICollection<ProductEntryRequestItem> Items { get; set; } = new List<ProductEntryRequestItem>();
    public RequestStatus Status { get; set; } = RequestStatus.Pending;

    public string FromSource { get; set; } = string.Empty;
    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }
    public int? ProductStateId { get; set; }
    public ProductState? ProductState { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? Notes { get; set; }

    public int ReceivedByUserId { get; set; }
    public User? ReceivedByUser { get; set; }

    public int? SupervisorId { get; set; }
    public User? Supervisor { get; set; }

    public int? ManagerId { get; set; }
    public User? Manager { get; set; }

    public string? RejectionReason { get; set; }
}
