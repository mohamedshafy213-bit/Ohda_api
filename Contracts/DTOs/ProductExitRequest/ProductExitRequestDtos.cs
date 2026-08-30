using System;
using System.Collections.Generic;
using Contracts.BaseDtos;
using Entities.Models.Enums;

namespace Contracts.DTOs.ProductExitRequest
{
    public class ProductExitRequestItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductSKU { get; set; }
        public int Quantity { get; set; }
        public RequestStatus Status { get; set; }
        public string? Notes { get; set; }
    }

    public class ProductExitRequestItemCreateDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string? Notes { get; set; }
    }

    public class ProductExitRequestItemUpdateDto
    {
        public int Id { get; set; }
        public RequestStatus Status { get; set; }
    }

    public class ProductExitRequestDto : BaseDto
    {
        public List<ProductExitRequestItemDto> Items { get; set; } = new();
        public RequestStatus Status { get; set; }

        public string RecipientName { get; set; } = string.Empty;
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
        public List<ProductExitRequestItemCreateDto> Items { get; set; } = new();
        public string RecipientName { get; set; } = string.Empty;
        public int? DepartmentId { get; set; }
        public string? Purpose { get; set; }
        public int RequestedByUserId { get; set; }
        public List<int> SelectedProductItemIds { get; set; } = new();
    }

    public class ProductExitUpdateDto : BaseUpdateDto
    {
        public List<ProductExitRequestItemUpdateDto> Items { get; set; } = new();
        public RequestStatus Status { get; set; }
        public string RecipientName { get; set; } = string.Empty;
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

    public class RequestApprovalDto
    {
        public List<ItemApprovalDto> Items { get; set; } = new();
    }

    public class ItemApprovalDto
    {
        public int ItemId { get; set; }
        public RequestStatus Status { get; set; }
    }
}
