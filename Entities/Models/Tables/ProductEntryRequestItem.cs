using Entities.Models.BaseTables;
using Entities.Models.Enums;

namespace Entities.Models.Tables
{
    public class ProductEntryRequestItem : BaseTable
    {
        public int ProductEntryRequestId { get; set; }
        public ProductEntryRequest? ProductEntryRequest { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int Quantity { get; set; }
        public int? ProductStateId { get; set; }
        public ProductState? ProductState { get; set; }

        public RequestStatus Status { get; set; } = RequestStatus.Pending;
        public string? Notes { get; set; }
    }
}
