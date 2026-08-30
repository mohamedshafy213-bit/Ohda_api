using Entities.Models.BaseTables;
using Entities.Models.Enums;

namespace Entities.Models.Tables
{
    public class ProductExitRequestItem : BaseTable
    {
        public int ProductExitRequestId { get; set; }
        public ProductExitRequest? ProductExitRequest { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int Quantity { get; set; }
        public RequestStatus Status { get; set; } = RequestStatus.Pending;
        public string? Notes { get; set; }
    }
}
