using Contracts.DTOs.Product;
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
public class ProductController : BaseController<Product, ProductDto, ProductCreateDto, ProductUpdateDto>
{
    private readonly IRepositoryWrapper _repositoryWrapper;

    public ProductController(IRepositoryWrapper repositoryWrapper)
    {
        _repositoryWrapper = repositoryWrapper;
        _repository = repositoryWrapper.Products;
    }

    [HttpGet("barcode/{barcode}")]
    public async Task<IActionResult> GetByBarcode([FromRoute] string barcode)
    {
        var product = await _repositoryWrapper.Products.GetByBarcodeAsync(barcode);
        if (product == null)
        {
            return Ok(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = $"Product with barcode '{barcode}' not found."
            });
        }

        var productDto = new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            SKU = product.SKU,
            Barcode = product.Barcode,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name,
            SupplierId = product.SupplierId,
            SupplierName = product.Supplier?.CompanyName,
            UnitPrice = product.UnitPrice,
            InventoryType = product.InventoryType,
            PurchasePrice = product.PurchasePrice,
            AssetValue = product.AssetValue
        };

        return Ok(new SingleObjectResponseModel<ProductDto>
        {
            IsDone = true,
            ReturnMessage = "Product retrieved successfully",
            SingleObject = productDto
        });
    }
}
