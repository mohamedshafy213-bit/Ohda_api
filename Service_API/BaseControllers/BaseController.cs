using Contracts.BaseDtos;
using Contracts.enums;
using Contracts.interfaces.Repository;
using Contracts.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySqlX.XDevAPI.Common;
namespace Service_API.BaseControllers;




[ApiController]
[Route("api/[controller]")]

public abstract class BaseController<T, TDto, TCreateDto, TUpdateDto> : ControllerBase
        where T : Entities.Models.BaseTables.BaseTable
        where TDto : BaseDto
        where TCreateDto : BaseCreateDto
        where TUpdateDto : BaseUpdateDto
{
    protected  IRepositoryBase<T, TDto, TCreateDto, TUpdateDto> _repository;


    [HttpGet]
    public virtual async Task<IActionResult> GetAll()
    {
        var response = await _repository.FindAll();
        return HandleResponse(response);
    }


    [HttpPost]
    public virtual async Task<IActionResult> Create([FromBody] TCreateDto createDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var response = await _repository.Create(createDto);
        return HandleResponse(response);
    }

    [HttpPut("{id}")]
    public virtual async Task<IActionResult> Update([FromRoute] string id, [FromBody] TUpdateDto updateDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest("Route parameter 'id' is required.");
        }

        var response = await _repository.Update(id, updateDto);
        return HandleResponse(response);
    }

    [HttpDelete("{id}")]
    public virtual async Task<IActionResult> Delete([FromRoute] string id)
    {
        if ( string.IsNullOrWhiteSpace(id))
        {
            return BadRequest("Route parameter 'id' is required.");
        }

        var response = await _repository.Delete(id);
        return HandleResponse(response);
    }


    protected IActionResult HandleResponse(SingleObjectResponseModel response)
    {
        if (response.IsDone)
        {
            return Ok(response);
        }

        return response.ErrorCode switch
        {
            ErrorCatalog.ObjectNotFound => Ok(response), // 404 Not Found
            //ErrorCatalog.DataBaseFauiler => Ok(500, response), // 500 Internal Server Error
            //ErrorCatalog.missingValues => Ok(response), // 400 Bad Request
            //ErrorCatalog.ConnectionLost => Ok(503, response), // 503 Service Unavailable
            _ => Ok(response) // Default to 400 Bad Request for unhandled cases
        };
    }
    protected ActionResult HandleResponse<T>(T response) where T : SingleObjectResponseModel
    {
        if (response.IsDone)
        {
            return Ok(response);
        }
        if (!response.IsDone)
            response.StatusCode = 404;
        response.StatusCode = response.ErrorCode switch
        {
            ErrorCatalog.ObjectNotFound => 404, // Not Found
            ErrorCatalog.DataBaseFauiler => 500, // Internal Server Error
            ErrorCatalog.missingValues => 400, // Bad Request
            ErrorCatalog.ConnectionLost => 503, // Service Unavailable
            _ => 400  // Default
        };
        return Ok(response);


    }
}
