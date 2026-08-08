using Contracts.DTOs.Order;
using Contracts.interfaces.Repository;
using Entities.Models.Tables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service_API.BaseControllers;

namespace Service_API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OrderController : BaseController<Order, OrderDto, OrderCreateDto, OrderUpdateDto>
{
    public OrderController(IRepositoryWrapper repositoryWrapper)
    {
        _repository = repositoryWrapper.Orders;
    }
}
