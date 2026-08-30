using System;
using System.Collections.Generic;
using Contracts.BaseDtos;
using Entities.Models.Enums;

namespace Contracts.DTOs.ProductEntryRequest
{
    public class ProductEntryRequestItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductSKU { get; set; }
        public int Quantity { get; set; }
        public int? ProductStateId { get; set; }
        public string? ProductStateName { get; set; }
        public RequestStatus Status { get; set; }
        public string? Notes { get; set; }
    }

    public class ProductEntryRequestItemCreateDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public int? ProductStateId { get; set; }
        public string? Notes { get; set; }
    }

    public class ProductEntryRequestItemUpdateDto
    {
        public int Id { get; set; }
        public RequestStatus Status { get; set; }
    }

    public class ProductEntryRequestDto : BaseDto
    {
        public List<ProductEntryRequestItemDto> Items { get; set; } = new();
        public RequestStatus Status { get; set; }

        public string FromSource { get; set; } = string.Empty;
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
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
        public List<ProductEntryRequestItemCreateDto> Items { get; set; } = new();
        public string FromSource { get; set; } = string.Empty;
        public int? DepartmentId { get; set; }
        public string? InvoiceNumber { get; set; }
        public string? Notes { get; set; }
        public int ReceivedByUserId { get; set; }
    }

    public class ProductEntryUpdateDto : BaseUpdateDto
    {
        public List<ProductEntryRequestItemUpdateDto> Items { get; set; } = new();
        public RequestStatus Status { get; set; }
        public string FromSource { get; set; } = string.Empty;
        public int? DepartmentId { get; set; }
        public string? InvoiceNumber { get; set; }
        public string? Notes { get; set; }
        public int ReceivedByUserId { get; set; }
        public int? ManagerId { get; set; }
        public int? SupervisorId { get; set; }
        public string? RejectionReason { get; set; }
    }
}
