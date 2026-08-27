using Contracts.DTOs.Product;
using Contracts.interfaces.Repository;
using Contracts.Responses;
using Entities.Models.Tables;
using Entities.Models.Enums;
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

    [HttpGet]
    public override async Task<IActionResult> GetAll()
    {
        var response = await _repositoryWrapper.Products.FindAll();
        var dtos = (response as ListOfObjectsResponseModel<ProductDto>)?.Objects;
        if (dtos != null)
        {
            foreach (var dto in dtos)
            {
                var inventory = await _repositoryWrapper.Inventories.GetByProductIdAsync(dto.Id);
                int qty = inventory?.Quantity ?? 0;
                dto.Amount = qty;
                dto.Quantity = qty;
            }
        }
        return HandleResponse(response);
    }

    [HttpPost]
    public override async Task<IActionResult> Create([FromBody] ProductCreateDto createDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await _repositoryWrapper.Products.Create(createDto);
        if (response.IsDone)
        {
            var createdProduct = (response as SingleObjectResponseModel<ProductDto>)?.SingleObject;
            if (createdProduct != null)
            {
                int initialAmount = createDto.Amount > 0 ? createDto.Amount : createDto.Quantity;
                // Create corresponding Inventory record with initial amount
                await _repositoryWrapper.Inventories.Create(new Contracts.DTOs.Inventory.InventoryCreateDto
                {
                    ProductId = createdProduct.Id,
                    Quantity = initialAmount,
                    MinStock = 5,
                    MaxStock = 500
                });

                // Generate individual ProductItems
                for (int u = 1; u <= initialAmount; u++)
                {
                    string guidSuffix = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
                    string serialNumber = $"SN-{createdProduct.SKU}-{u}-{guidSuffix}";
                    string qrCode = $"QR-{serialNumber}";

                    await _repositoryWrapper.ProductItems.Create(new Contracts.DTOs.ProductItem.ProductItemCreateDto
                    {
                        ProductId = createdProduct.Id,
                        SerialNumber = serialNumber,
                        QRCode = qrCode,
                        Status = ProductItemStatus.InStock
                    });
                }

                await _repositoryWrapper.SaveAsync();
                createdProduct.Amount = initialAmount;
                createdProduct.Quantity = initialAmount;
            }
        }
        return HandleResponse(response);
    }

    [HttpPut("{id}")]
    public override async Task<IActionResult> Update([FromRoute] string id, [FromBody] ProductUpdateDto updateDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await _repositoryWrapper.Products.Update(id, updateDto);
        if (response.IsDone)
        {
            int productId = Convert.ToInt32(id);
            var inventory = await _repositoryWrapper.Inventories.GetByProductIdAsync(productId);
            int updatedAmount = updateDto.Amount > 0 ? updateDto.Amount : updateDto.Quantity;
            if (inventory != null)
            {
                int difference = updatedAmount - inventory.Quantity;
                if (difference != 0)
                {
                    await _repositoryWrapper.Inventories.Update(inventory.Id.ToString(), new Contracts.DTOs.Inventory.InventoryUpdateDto
                    {
                        Id = inventory.Id,
                        ProductId = inventory.ProductId,
                        Quantity = updatedAmount,
                        MinStock = inventory.MinStock,
                        MaxStock = inventory.MaxStock
                    });

                    if (difference > 0)
                    {
                        for (int u = 1; u <= difference; u++)
                        {
                            string guidSuffix = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
                            string serialNumber = $"SN-{updateDto.SKU}-{inventory.Quantity + u}-{guidSuffix}";
                            string qrCode = $"QR-{serialNumber}";

                            await _repositoryWrapper.ProductItems.Create(new Contracts.DTOs.ProductItem.ProductItemCreateDto
                            {
                                ProductId = productId,
                                SerialNumber = serialNumber,
                                QRCode = qrCode,
                                Status = ProductItemStatus.InStock
                            });
                        }
                    }

                    await _repositoryWrapper.SaveAsync();
                }
            }
        }
        return HandleResponse(response);
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

        var inventory = await _repositoryWrapper.Inventories.GetByProductIdAsync(product.Id);
        int qty = inventory?.Quantity ?? 0;
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
            AssetValue = product.AssetValue,
            Amount = qty,
            Quantity = qty
        };

        return Ok(new SingleObjectResponseModel<ProductDto>
        {
            IsDone = true,
            ReturnMessage = "Product retrieved successfully",
            SingleObject = productDto
        });
    }
}
