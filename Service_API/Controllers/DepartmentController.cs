using Contracts.DTOs.Department;
using Contracts.interfaces.Repository;
using Entities.Models.Tables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service_API.BaseControllers;

namespace Service_API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DepartmentController : BaseController<Department, DepartmentDto, DepartmentCreateDto, DepartmentUpdateDto>
{
    public DepartmentController(IRepositoryWrapper repositoryWrapper)
    {
        _repository = repositoryWrapper.Departments;
    }
}
