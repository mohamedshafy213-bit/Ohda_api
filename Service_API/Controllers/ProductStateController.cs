using Contracts.DTOs.ProductState;
using Contracts.interfaces.Repository;
using Entities.Models.Tables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service_API.BaseControllers;

namespace Service_API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProductStateController : BaseController<ProductState, ProductStateDto, ProductStateCreateDto, ProductStateUpdateDto>
{
    public ProductStateController(IRepositoryWrapper repositoryWrapper)
    {
        _repository = repositoryWrapper.ProductStates;
    }
}
