using Contracts.DTOs.Order;
using Contracts.interfaces.Repository;
using Entities.Models.Tables;

namespace Contracts.Interfaces.Repository;

public interface IOrderRepository : IRepositoryBase<Order, OrderDto, OrderCreateDto, OrderUpdateDto>
{
}
