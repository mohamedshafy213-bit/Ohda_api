using Contracts.DTOs.Supplier;
using Contracts.interfaces.Repository;
using Entities.Models.Tables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service_API.BaseControllers;

namespace Service_API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SupplierController : BaseController<Supplier, SupplierDto, SupplierCreateDto, SupplierUpdateDto>
{
    public SupplierController(IRepositoryWrapper repositoryWrapper)
    {
        _repository = repositoryWrapper.Suppliers;
    }
}
