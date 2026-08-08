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
            SerialNumber = pi.SerialNumber,
            QRCode = pi.QRCode,
            Status = pi.Status,
            RecipientName = pi.RecipientName,
            Place = pi.Place,
            ExitDate = pi.ExitDate,
            ProductExitRequestId = pi.ProductExitRequestId,
            Notes = pi.Notes
        }).ToList();

        return Ok(new SingleObjectResponseModel<List<ProductItemDto>>
        {
            IsDone = true,
            ReturnMessage = "Product items retrieved successfully.",
            SingleObject = dtos
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
            SerialNumber = pi.SerialNumber,
            QRCode = pi.QRCode,
            Status = pi.Status,
            RecipientName = pi.RecipientName,
            Place = pi.Place,
            ExitDate = pi.ExitDate,
            ProductExitRequestId = pi.ProductExitRequestId,
            Notes = pi.Notes
        }).ToList();

        return Ok(new SingleObjectResponseModel<List<ProductItemDto>>
        {
            IsDone = true,
            ReturnMessage = "Available in-stock items retrieved successfully.",
            SingleObject = dtos
        });
    }
}
