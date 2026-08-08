using Contracts.DTOs.Order;
using Contracts.Interfaces.Repository;
using Entities.Models.Databases;
using Entities.Models.Tables;
using LoggerService;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Repositories.Repositories;

namespace Repositories.Models;

public class OrderRepository
    : RepositoryBase<Order, OrderDto, OrderCreateDto, OrderUpdateDto>, IOrderRepository
{
    public OrderRepository(
        ILoggerManager logger,
        RepositoryContext repositoryContext,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
        : base(logger, repositoryContext, httpContextAccessor, mapper)
    {
    }
}
