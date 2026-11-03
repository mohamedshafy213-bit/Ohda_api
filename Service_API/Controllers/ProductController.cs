using Contracts.DTOs.Product;
using Contracts.interfaces.Repository;
using Contracts.Responses;
using Entities.Models.Databases;
using Entities.Models.Tables;
using Entities.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_API.BaseControllers;

namespace Service_API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProductController : BaseController<Product, ProductDto, ProductCreateDto, ProductUpdateDto>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly RepositoryContext _context;

    public ProductController(IRepositoryWrapper repositoryWrapper, RepositoryContext context)
    {
        _repositoryWrapper = repositoryWrapper;
        _repository = repositoryWrapper.Products;
        _context = context;
    }

    [HttpDelete("{id}")]
    public override async Task<IActionResult> Delete([FromRoute] string id)
    {
        if (!int.TryParse(id, out var productId))
            return BadRequest(new SingleObjectResponseModel { IsDone = false, ReturnMessage = "معرف المنتج غير صالح." });

        var existingProduct = await _context.Products.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == productId);
        if (existingProduct == null)
        {
            _repositoryWrapper.Products.InvalidateCache();
            return Ok(new SingleObjectResponseModel
            {
                IsDone = true,
                ReturnMessage = "تم حذف الصنف وتحديث القائمة بنجاح."
            });
        }

        if (existingProduct.IsDeleted)
        {
            _repositoryWrapper.Products.InvalidateCache();
            _repositoryWrapper.Inventories.InvalidateCache();
            return Ok(new SingleObjectResponseModel
            {
                IsDone = true,
                ReturnMessage = "الصنف محذوف بالفعل، وتم تحديث القائمة بنجاح."
            });
        }

        // Check if product is involved in active or historical exit requests
        var exitRequestsCount = await _context.ProductExitRequestItems
            .Include(i => i.ProductExitRequest)
            .CountAsync(i => i.ProductId == productId && !i.IsDeleted && i.ProductExitRequest != null && !i.ProductExitRequest.IsDeleted);

        // Check if product is involved in entry requests
        var entryRequestsCount = await _context.ProductEntryRequestItems
            .Include(i => i.ProductEntryRequest)
            .CountAsync(i => i.ProductId == productId && !i.IsDeleted && i.ProductEntryRequest != null && !i.ProductEntryRequest.IsDeleted);

        // Check if product is involved in orders
        var ordersCount = await _context.OrderDetails
            .CountAsync(o => o.ProductId == productId && !o.IsDeleted);

        // Check if any product item is actively assigned to an employee or in custody (not InStock)
        var assignedItemsCount = await _context.ProductItems
            .CountAsync(pi => pi.ProductId == productId && !pi.IsDeleted && pi.Status != ProductItemStatus.InStock);

        if (exitRequestsCount > 0 || entryRequestsCount > 0 || ordersCount > 0 || assignedItemsCount > 0)
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = $"لا يمكن حذف هذا الصنف لوجود ارتباطات سارية أو تاريخية به (طلبات صرف: {exitRequestsCount}، طلبات إدخال: {entryRequestsCount}، عهد مستلمة: {assignedItemsCount}). يرجى إلغاء أو معالجة تلك الارتباطات أولاً."
            });
        }

        // Safe cascade cleanup for unused inventory and in-stock items
        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync<IActionResult>(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var productItems = await _context.ProductItems.Where(pi => pi.ProductId == productId && !pi.IsDeleted).ToListAsync();
                foreach (var item in productItems)
                {
                    item.IsDeleted = true;
                    item.DeleteDate = DateTime.UtcNow;
                }

                var inventories = await _context.Inventories.Where(i => i.ProductId == productId && !i.IsDeleted).ToListAsync();
                foreach (var inv in inventories)
                {
                    inv.IsDeleted = true;
                    inv.DeleteDate = DateTime.UtcNow;
                    inv.Quantity = 0;
                }

                existingProduct.IsDeleted = true;
                existingProduct.DeleteDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _repositoryWrapper.Products.InvalidateCache();
                _repositoryWrapper.Inventories.InvalidateCache();

                return Ok(new SingleObjectResponseModel
                {
                    IsDone = true,
                    ReturnMessage = "تم حذف الصنف ورصيده في المخزن بنجاح."
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new SingleObjectResponseModel
                {
                    IsDone = false,
                    ReturnMessage = $"فشل حذف الصنف: {ex.Message}"
                });
            }
        });
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

        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync<IActionResult>(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var response = await _repositoryWrapper.Products.Create(createDto);
                if (!response.IsDone)
                {
                    await transaction.RollbackAsync();
                    return HandleResponse(response);
                }

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

                await transaction.CommitAsync();
                _repositoryWrapper.Products.InvalidateCache();
                _repositoryWrapper.Inventories.InvalidateCache();
                return HandleResponse(response);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();
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
                await transaction.RollbackAsync();
                return BadRequest(new SingleObjectResponseModel
                {
                    ErrorCode = Contracts.enums.ErrorCatalog.missingValues,
                    IsDone = false,
                    ReturnMessage = $"حدث خطأ أثناء حفظ المنتج: {ex.Message}"
                });
            }
        });
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
            _repositoryWrapper.Products.InvalidateCache();
            _repositoryWrapper.Inventories.InvalidateCache();
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
