using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.DTOs.WarehouseBin;
using Contracts.interfaces.Repository;
using Contracts.Responses;
using Entities.Models.Tables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service_API.BaseControllers;

namespace Service_API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class WarehouseBinController : BaseController<WarehouseBin, WarehouseBinDto, WarehouseBinCreateDto, WarehouseBinUpdateDto>
{
    private readonly IRepositoryWrapper _repositoryWrapper;

    public WarehouseBinController(IRepositoryWrapper repositoryWrapper)
    {
        _repositoryWrapper = repositoryWrapper;
        _repository = repositoryWrapper.WarehouseBins;
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetBins([FromQuery] int? departmentId = null)
    {
        var binDtos = await _repositoryWrapper.WarehouseBins.GetBinsWithCountsAsync(departmentId);
        return Ok(new SingleObjectResponseModel<List<WarehouseBinDto>>
        {
            IsDone = true,
            ReturnMessage = "Warehouse bins retrieved successfully.",
            SingleObject = binDtos
        });
    }

    [HttpGet("{id}/items")]
    public async Task<IActionResult> GetBinItems([FromRoute] int id)
    {
        var result = await _repositoryWrapper.WarehouseBins.GetBinItemsDtoAsync(id);
        return Ok(new SingleObjectResponseModel<List<WarehouseBinItemDto>>
        {
            IsDone = true,
            ReturnMessage = $"Items for bin ID {id} retrieved successfully.",
            SingleObject = result
        });
    }
}
