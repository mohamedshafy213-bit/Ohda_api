using Contracts.DTOs.ProductItem;
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
public class ProductItemController : BaseController<ProductItem, ProductItemDto, ProductItemCreateDto, ProductItemUpdateDto>
{
    private readonly IRepositoryWrapper _repositoryWrapper;

    public ProductItemController(IRepositoryWrapper repositoryWrapper)
    {
        _repositoryWrapper = repositoryWrapper;
        _repository = repositoryWrapper.ProductItems;
    }

    [HttpGet("product/{productId}")]
    public async Task<IActionResult> GetByProductId([FromRoute] int productId)
    {
        var items = await _repositoryWrapper.ProductItems.GetByProductIdAsync(productId);
        var dtos = items.Select(pi => new ProductItemDto
        {
            Id = pi.Id,
            ProductId = pi.ProductId,
            ProductName = pi.Product?.Name ?? string.Empty,
            ProductBarcode = pi.Product?.Barcode ?? string.Empty,
            ProductSKU = pi.Product?.SKU ?? string.Empty,
            SerialNumber = pi.SerialNumber,
            QRCode = pi.QRCode,
            Status = pi.Status,
            RecipientName = pi.RecipientName,
            Place = pi.Place,
            ExitDate = pi.ExitDate,
            ProductExitRequestId = pi.ProductExitRequestId,
            Notes = pi.Notes
        }).ToList();

        return Ok(new ListOfObjectsResponseModel<ProductItemDto>
        {
            IsDone = true,
            ReturnMessage = "Product items retrieved successfully.",
            Objects = dtos
        });
    }

    [HttpGet("product/{productId}/instock")]
    public async Task<IActionResult> GetInStockByProductId([FromRoute] int productId)
    {
        var items = await _repositoryWrapper.ProductItems.GetInStockByProductIdAsync(productId);
        var dtos = items.Select(pi => new ProductItemDto
        {
            Id = pi.Id,
            ProductId = pi.ProductId,
            ProductName = pi.Product?.Name ?? string.Empty,
            ProductBarcode = pi.Product?.Barcode ?? string.Empty,
            ProductSKU = pi.Product?.SKU ?? string.Empty,
            SerialNumber = pi.SerialNumber,
            QRCode = pi.QRCode,
            Status = pi.Status,
            RecipientName = pi.RecipientName,
            Place = pi.Place,
            ExitDate = pi.ExitDate,
            ProductExitRequestId = pi.ProductExitRequestId,
            Notes = pi.Notes
        }).ToList();

        return Ok(new ListOfObjectsResponseModel<ProductItemDto>
        {
            IsDone = true,
            ReturnMessage = "Available in-stock items retrieved successfully.",
            Objects = dtos
        });
    }

    [HttpGet("department/{departmentId}")]
    public async Task<IActionResult> GetByDepartmentId([FromRoute] int departmentId)
    {
        var items = await _repositoryWrapper.ProductItems.GetByDepartmentIdAsync(departmentId);
        var dtos = items.Select(pi => new ProductItemDto
        {
            Id = pi.Id,
            ProductId = pi.ProductId,
            ProductName = pi.Product?.Name ?? string.Empty,
            ProductBarcode = pi.Product?.Barcode ?? string.Empty,
            ProductSKU = pi.Product?.SKU ?? string.Empty,
            SerialNumber = pi.SerialNumber,
            QRCode = pi.QRCode,
            Status = pi.Status,
            RecipientName = pi.RecipientName,
            Place = pi.Place,
            ExitDate = pi.ExitDate,
            ProductExitRequestId = pi.ProductExitRequestId,
            Notes = pi.Notes
        }).ToList();

        return Ok(new ListOfObjectsResponseModel<ProductItemDto>
        {
            IsDone = true,
            ReturnMessage = "Department product items retrieved successfully.",
            Objects = dtos
        });
    }

    [HttpGet("serial/{serialNumber}")]
    public async Task<IActionResult> GetBySerialNumber([FromRoute] string serialNumber)
    {
        var item = await _repositoryWrapper.ProductItems.GetBySerialNumberAsync(serialNumber);
        if (item == null)
        {
            return Ok(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = $"Product item with serial number '{serialNumber}' not found."
            });
        }

        var dto = new ProductItemDto
        {
            Id = item.Id,
            ProductId = item.ProductId,
            ProductName = item.Product?.Name ?? string.Empty,
            ProductBarcode = item.Product?.Barcode ?? string.Empty,
            ProductSKU = item.Product?.SKU ?? string.Empty,
            SerialNumber = item.SerialNumber,
            QRCode = item.QRCode,
            Status = item.Status,
            RecipientName = item.RecipientName,
            Place = item.Place,
            ExitDate = item.ExitDate,
            ProductExitRequestId = item.ProductExitRequestId,
            Notes = item.Notes
        };

        return Ok(new SingleObjectResponseModel<ProductItemDto>
        {
            IsDone = true,
            ReturnMessage = "Product item retrieved successfully.",
            SingleObject = dto
        });
    }

    [HttpGet("exit-request/{exitRequestId}")]
    public async Task<IActionResult> GetByExitRequestId([FromRoute] int exitRequestId)
    {
        var items = await _repositoryWrapper.ProductItems.GetByExitRequestIdAsync(exitRequestId);
        var dtos = items.Select(pi => new ProductItemDto
        {
            Id = pi.Id,
            ProductId = pi.ProductId,
            ProductName = pi.Product?.Name ?? string.Empty,
            ProductBarcode = pi.Product?.Barcode ?? string.Empty,
            ProductSKU = pi.Product?.SKU ?? string.Empty,
            SerialNumber = pi.SerialNumber,
            QRCode = pi.QRCode,
            Status = pi.Status,
            RecipientName = pi.RecipientName,
            Place = pi.Place,
            ExitDate = pi.ExitDate,
            ProductExitRequestId = pi.ProductExitRequestId,
            Notes = pi.Notes
        }).ToList();

        return Ok(new ListOfObjectsResponseModel<ProductItemDto>
        {
            IsDone = true,
            ReturnMessage = "Exit request product items retrieved successfully.",
            Objects = dtos
        });
    }
}
