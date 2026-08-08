using Contracts.BaseDtos;

namespace Contracts.DTOs.Order;

public class OrderDetailDto : BaseDto
{
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public string? ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class OrderDetailCreateDto : BaseCreateDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class OrderDto : BaseDto
{
    public int UserId { get; set; }
    public string? Username { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderDetailDto> OrderDetails { get; set; } = new();
}

public class OrderCreateDto : BaseCreateDto
{
    public int UserId { get; set; }
    public List<OrderDetailCreateDto> OrderDetails { get; set; } = new();
}

public class OrderUpdateDto : BaseUpdateDto
{
    public int UserId { get; set; }
    public decimal TotalAmount { get; set; }
}
