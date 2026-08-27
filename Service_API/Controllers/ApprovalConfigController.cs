using Contracts.DTOs.ApprovalConfig;
using Contracts.interfaces.Repository;
using Entities.Models.Tables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service_API.BaseControllers;

namespace Service_API.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class ApprovalConfigController : BaseController<ApprovalConfig, ApprovalConfigDto, ApprovalConfigCreateDto, ApprovalConfigUpdateDto>
{
    public ApprovalConfigController(IRepositoryWrapper repositoryWrapper)
    {
        _repository = repositoryWrapper.ApprovalConfigs;
    }
}
