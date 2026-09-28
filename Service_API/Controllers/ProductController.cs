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
    public override async Task<IActionResult> GetAll([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null)
    {
        var response = await _repositoryWrapper.Products.FindAll(pageNumber, pageSize);
        var dtos = (response as ListOfObjectsResponseModel<ProductDto>)?.Objects;
        if (dtos != null && dtos.Any())
        {
            var productIds = dtos.Select(d => d.Id).ToList();
            var quantities = await _repositoryWrapper.Inventories.GetQuantitiesByProductIdsAsync(productIds);
            foreach (var dto in dtos)
            {
                int qty = quantities.TryGetValue(dto.Id, out int q) ? q : 0;
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

        if (string.IsNullOrWhiteSpace(createDto.Name))
        {
            return BadRequest(new SingleObjectResponseModel
            {
                ErrorCode = Contracts.enums.ErrorCatalog.missingValues,
                IsDone = false,
                ReturnMessage = "اسم المنتج / الصنف مطلوب."
            });
        }

        if (string.IsNullOrWhiteSpace(createDto.SKU))
        {
            return BadRequest(new SingleObjectResponseModel
            {
                ErrorCode = Contracts.enums.ErrorCatalog.missingValues,
                IsDone = false,
                ReturnMessage = "رمز الصنف (SKU) مطلوب."
            });
        }

        if (createDto.CategoryId <= 0)
        {
            return BadRequest(new SingleObjectResponseModel
            {
                ErrorCode = Contracts.enums.ErrorCatalog.missingValues,
                IsDone = false,
                ReturnMessage = "يرجى اختيار تصنيف / فئة صالحة للمنتج."
            });
        }

        // Check if SKU already exists
        var existingSku = await _repositoryWrapper.Products.GetBySKUAsync(createDto.SKU.Trim());
        if (existingSku != null)
        {
            return BadRequest(new SingleObjectResponseModel
            {
                ErrorCode = Contracts.enums.ErrorCatalog.missingValues,
                IsDone = false,
                ReturnMessage = $"رمز الصنف (SKU) '{createDto.SKU}' مستخدم بالفعل لمنتج آخر."
            });
        }

        // Check if Barcode already exists (if provided)
        if (!string.IsNullOrWhiteSpace(createDto.Barcode))
        {
            var existingBarcode = await _repositoryWrapper.Products.GetByBarcodeAsync(createDto.Barcode.Trim());
            if (existingBarcode != null)
            {
                return BadRequest(new SingleObjectResponseModel
                {
                    ErrorCode = Contracts.enums.ErrorCatalog.missingValues,
                    IsDone = false,
                    ReturnMessage = $"الباركود '{createDto.Barcode}' مسجل مسبقاً لصنف آخر ({existingBarcode.Name})."
                });
            }
        }

        // Enforce MaxProducts quota configured by SuperAdmin
        var branchClaim = User.FindFirst("branch_id")?.Value ?? User.FindFirst("BranchId")?.Value;
        bool isSuperAdmin = User.IsInRole("SuperAdmin");
        if (int.TryParse(branchClaim, out var userBranchId) && userBranchId > 0 && !isSuperAdmin)
        {
            var branch = await _repositoryWrapper.Branches.GetByIdAsync(userBranchId);
            if (branch != null && branch.MaxProducts > 0)
            {
                var currentProductCount = await _repositoryWrapper.Branches.GetActiveProductCountAsync(userBranchId);
                if (currentProductCount >= branch.MaxProducts)
                {
                    return BadRequest(new SingleObjectResponseModel
                    {
                        ErrorCode = Contracts.enums.ErrorCatalog.missingValues,
                        IsDone = false,
                        ReturnMessage = $"لقد تم استهلاك الحد الأقصى للأصناف والمنتجات المسموح بها لهذا الفرع ({branch.MaxProducts} صنف). تم تعيين هذا الحد بواسطة مدير المنصة (SuperAdmin)."
                    });
                }
            }
        }

        try
        {
            var response = await _repositoryWrapper.Products.Create(createDto);
            if (response.IsDone)
            {
                var createdProduct = (response as SingleObjectResponseModel<ProductDto>)?.SingleObject;
                if (createdProduct != null)
                {
                    int initialAmount = createDto.Amount > 0 ? createDto.Amount : createDto.Quantity;
                    if (initialAmount <= 0) initialAmount = 1;

                    // Create corresponding Inventory record with initial amount
                    await _repositoryWrapper.Inventories.Create(new Contracts.DTOs.Inventory.InventoryCreateDto
                    {
                        ProductId = createdProduct.Id,
                        Quantity = initialAmount,
                        MinStock = 5,
                        MaxStock = 500
                    });

                    // Generate or assign individual ProductItems
                    var customSerials = createDto.SerialNumbers?
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Select(s => s.Trim())
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList() ?? new List<string>();
                    
                    int assignedCount = 0;

                    foreach (var customSerial in customSerials)
                    {
                        assignedCount++;
                        string qrCode = $"QR-{customSerial}";
                        await _repositoryWrapper.ProductItems.Create(new Contracts.DTOs.ProductItem.ProductItemCreateDto
                        {
                            ProductId = createdProduct.Id,
                            SerialNumber = customSerial,
                            QRCode = qrCode,
                            Status = ProductItemStatus.InStock
                        });
                    }

                    for (int u = assignedCount + 1; u <= initialAmount; u++)
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
        catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
        {
            var innerMsg = dbEx.InnerException?.Message ?? dbEx.Message;
            return BadRequest(new SingleObjectResponseModel
            {
                ErrorCode = Contracts.enums.ErrorCatalog.DataBaseFauiler,
                IsDone = false,
                ReturnMessage = $"تعذر حفظ الصنف: تأكد من عدم تكرار الباركود أو السيريال وصحة البيانات المدخلة ({innerMsg})"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new SingleObjectResponseModel
            {
                ErrorCode = Contracts.enums.ErrorCatalog.missingValues,
                IsDone = false,
                ReturnMessage = $"حدث خطأ أثناء حفظ المنتج: {ex.Message}"
            });
        }
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
