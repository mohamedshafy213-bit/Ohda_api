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
        var bins = await _repositoryWrapper.WarehouseBins.GetAllWithDetailsAsync(departmentId);
        
        // Fetch count of items per bin
        var binDtos = new List<WarehouseBinDto>();
        foreach (var b in bins)
        {
            var items = await _repositoryWrapper.WarehouseBins.GetItemsByBinIdAsync(b.Id);
            binDtos.Add(new WarehouseBinDto
            {
                Id = b.Id,
                Code = b.Code,
                Name = b.Name,
                Aisle = b.Aisle,
                Shelf = b.Shelf,
                Capacity = b.Capacity,
                Description = b.Description,
                IsActive = b.IsActive,
                DepartmentId = b.DepartmentId,
                DepartmentName = b.Department?.Name ?? "غير محدد",
                ItemsCount = items.Count(),
                InsertDate = b.InsertDate,
                LastUpdate = b.LastUpdate
            });
        }

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
        var bin = await _repositoryWrapper.WarehouseBins.GetByIdAsync(id);
        if (bin == null)
        {
            return NotFound(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = $"Warehouse bin with ID {id} not found."
            });
        }

        var items = await _repositoryWrapper.WarehouseBins.GetItemsByBinIdAsync(id);
        var result = items.Select(pi => new WarehouseBinItemDto
        {
            Id = pi.Id,
            ProductId = pi.ProductId,
            ProductName = pi.Product?.Name ?? "Unknown",
            ProductSKU = pi.Product?.SKU ?? "N/A",
            SerialNumber = pi.SerialNumber,
            QRCode = pi.QRCode,
            Status = (int)pi.Status,
            StatusLabel = pi.Status switch
            {
                Entities.Models.Enums.ProductItemStatus.InStock => "متوفر بالمستودع",
                Entities.Models.Enums.ProductItemStatus.Exited => "منصرف كعهدة",
                Entities.Models.Enums.ProductItemStatus.Damaged => "تالف",
                Entities.Models.Enums.ProductItemStatus.InMaintenance => "تحت الصيانة",
                _ => pi.Status.ToString()
            },
            Place = pi.Place,
            InsertDate = pi.InsertDate
        }).ToList();

        return Ok(new SingleObjectResponseModel<List<WarehouseBinItemDto>>
        {
            IsDone = true,
            ReturnMessage = $"Items for bin {bin.Code} retrieved successfully.",
            SingleObject = result
        });
    }
}
