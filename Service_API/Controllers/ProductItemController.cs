using Contracts.DTOs.ProductItem;
using Contracts.interfaces.Repository;
using Contracts.Responses;
using Entities.Models.Databases;
using Entities.Models.Enums;
using Entities.Models.Tables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_API.BaseControllers;
using System.Security.Claims;

namespace Service_API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProductItemController : BaseController<ProductItem, ProductItemDto, ProductItemCreateDto, ProductItemUpdateDto>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly RepositoryContext _context;

    public ProductItemController(IRepositoryWrapper repositoryWrapper, RepositoryContext context)
    {
        _repositoryWrapper = repositoryWrapper;
        _context = context;
        _repository = repositoryWrapper.ProductItems;
    }

    [HttpGet("product/{productId}")]
    public async Task<IActionResult> GetByProductId([FromRoute] int productId)
    {
        await AutoBackfillProductSerialsAsync(productId);

        var items = await _repositoryWrapper.ProductItems.GetByProductIdAsync(productId);
        var dtos = items.Select(pi => new ProductItemDto
        {
            Id = pi.Id,
            ProductId = pi.ProductId,
            ProductName = pi.Product?.Name ?? string.Empty,
            ProductBarcode = pi.Product?.Barcode ?? string.Empty,
            ProductSKU = pi.Product?.SKU ?? string.Empty,
            CategoryName = pi.Product?.Category?.Name,
            SerialNumber = pi.SerialNumber,
            QRCode = pi.QRCode,
            Status = pi.Status,
            BinId = pi.BinId,
            BinName = pi.Bin?.Name,
            BinCode = pi.Bin?.Code,
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
        await AutoBackfillProductSerialsAsync(productId);

        var items = await _repositoryWrapper.ProductItems.GetInStockByProductIdAsync(productId);
        var dtos = items.Select(pi => new ProductItemDto
        {
            Id = pi.Id,
            ProductId = pi.ProductId,
            ProductName = pi.Product?.Name ?? string.Empty,
            ProductBarcode = pi.Product?.Barcode ?? string.Empty,
            ProductSKU = pi.Product?.SKU ?? string.Empty,
            CategoryName = pi.Product?.Category?.Name,
            SerialNumber = pi.SerialNumber,
            QRCode = pi.QRCode,
            Status = pi.Status,
            BinId = pi.BinId,
            BinName = pi.Bin?.Name,
            BinCode = pi.Bin?.Code,
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

    [HttpGet("instock")]
    public async Task<IActionResult> GetAllInStock()
    {
        var items = await _repositoryWrapper.ProductItems.GetAllInStockAsync();
        var dtos = items.Select(pi => new ProductItemDto
        {
            Id = pi.Id,
            ProductId = pi.ProductId,
            ProductName = pi.Product?.Name ?? string.Empty,
            ProductBarcode = pi.Product?.Barcode ?? string.Empty,
            ProductSKU = pi.Product?.SKU ?? string.Empty,
            CategoryName = pi.Product?.Category?.Name,
            SerialNumber = pi.SerialNumber,
            QRCode = pi.QRCode,
            Status = pi.Status,
            BinId = pi.BinId,
            BinName = pi.Bin?.Name,
            BinCode = pi.Bin?.Code,
            RecipientName = pi.RecipientName,
            Place = pi.Place,
            ExitDate = pi.ExitDate,
            ProductExitRequestId = pi.ProductExitRequestId,
            Notes = pi.Notes
        }).ToList();

        return Ok(new ListOfObjectsResponseModel<ProductItemDto>
        {
            IsDone = true,
            ReturnMessage = "All available in-stock items retrieved successfully.",
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
            CategoryName = pi.Product?.Category?.Name,
            SerialNumber = pi.SerialNumber,
            QRCode = pi.QRCode,
            Status = pi.Status,
            BinId = pi.BinId,
            BinName = pi.Bin?.Name,
            BinCode = pi.Bin?.Code,
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
            CategoryName = item.Product?.Category?.Name,
            SerialNumber = item.SerialNumber,
            QRCode = item.QRCode,
            Status = item.Status,
            BinId = item.BinId,
            BinName = item.Bin?.Name,
            BinCode = item.Bin?.Code,
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
            CategoryName = pi.Product?.Category?.Name,
            SerialNumber = pi.SerialNumber,
            QRCode = pi.QRCode,
            Status = pi.Status,
            BinId = pi.BinId,
            BinName = pi.Bin?.Name,
            BinCode = pi.Bin?.Code,
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

    [HttpPost("add-serials")]
    public async Task<IActionResult> AddSerials([FromBody] AddProductSerialsRequest request)
    {
        if (request == null || request.ProductId <= 0 || request.SerialNumbers == null || !request.SerialNumbers.Any())
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "بيانات غير مكتملة. يرجى تزويد معرف الصنف وقائمة الأرقام المسلسلة."
            });
        }

        var branchClaim = User.FindFirst("branch_id")?.Value ?? User.FindFirst("BranchId")?.Value;
        int branchId = (_context.CurrentBranchId.HasValue && _context.CurrentBranchId.Value > 0)
            ? _context.CurrentBranchId.Value
            : (int.TryParse(branchClaim, out var bId) && bId > 0 ? bId : 1);
        var userCode = User.FindFirst(ClaimTypes.Name)?.Value ?? "SerialAgent";

        var product = await _context.Products.IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == request.ProductId && p.BranchId == branchId && !p.IsDeleted);

        if (product == null)
        {
            return NotFound(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "الصنف غير موجود أو غير تابع لهذا الفرع."
            });
        }

        var cleanedSerials = request.SerialNumbers
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim())
            .Distinct()
            .ToList();

        if (!cleanedSerials.Any())
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "لم يتم تحديد أي أرقام مسلسلة صالحة للإضافة."
            });
        }

        // Check for duplicates in the same branch
        var existingSerialsInDb = await _context.ProductItems.IgnoreQueryFilters()
            .Where(pi => pi.BranchId == branchId && !pi.IsDeleted && cleanedSerials.Contains(pi.SerialNumber))
            .Select(pi => pi.SerialNumber)
            .ToListAsync();

        var duplicates = new HashSet<string>(existingSerialsInDb, StringComparer.OrdinalIgnoreCase);
        var serialsToAdd = cleanedSerials.Where(s => !duplicates.Contains(s)).ToList();

        if (!serialsToAdd.Any())
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = $"جميع الأرقام المسلسلة المحددة ({string.Join(", ", duplicates)}) مسجلة مسبقاً في النظام."
            });
        }

        foreach (var sn in serialsToAdd)
        {
            await _context.ProductItems.AddAsync(new ProductItem
            {
                ProductId = request.ProductId,
                BranchId = branchId,
                SerialNumber = sn,
                QRCode = sn,
                Status = ProductItemStatus.InStock,
                InsertDate = DateTime.UtcNow,
                InsertUserCode = userCode
            });
        }

        // Update Inventory quantity
        var inventory = await _context.Inventories.IgnoreQueryFilters()
            .FirstOrDefaultAsync(i => i.ProductId == request.ProductId && i.BranchId == branchId && !i.IsDeleted);

        if (inventory == null)
        {
            inventory = new Inventory
            {
                ProductId = request.ProductId,
                BranchId = branchId,
                Quantity = serialsToAdd.Count,
                MinStock = 5,
                MaxStock = 100,
                InsertDate = DateTime.UtcNow,
                InsertUserCode = userCode
            };
            await _context.Inventories.AddAsync(inventory);
        }
        else
        {
            inventory.Quantity += serialsToAdd.Count;
            inventory.LastUpdate = DateTime.UtcNow;
            inventory.UpdateUserCode = userCode;
        }

        await _context.SaveChangesAsync();
        _repositoryWrapper.ProductItems.InvalidateCache();
        _repositoryWrapper.Inventories.InvalidateCache();
        _repositoryWrapper.Products.InvalidateCache();

        string msg = $"تمت إضافة {serialsToAdd.Count} أجهزة (أرقام مسلسلة) للصنف '{product.Name}' بنجاح.";
        if (duplicates.Any())
        {
            msg += $" (تم تخطي {duplicates.Count} سيريالات لأنها موجودة مسبقاً: {string.Join(", ", duplicates.Take(3))})";
        }

        return Ok(new SingleObjectResponseModel
        {
            IsDone = true,
            ReturnMessage = msg
        });
    }

    [HttpPut("{id:int}/serial")]
    public async Task<IActionResult> UpdateSerialNumber([FromRoute] int id, [FromBody] UpdateProductItemSerialRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.SerialNumber))
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "الرقم المسلسل الجديد مطلوب."
            });
        }

        var branchClaim = User.FindFirst("branch_id")?.Value ?? User.FindFirst("BranchId")?.Value;
        int branchId = (_context.CurrentBranchId.HasValue && _context.CurrentBranchId.Value > 0)
            ? _context.CurrentBranchId.Value
            : (int.TryParse(branchClaim, out var bId) && bId > 0 ? bId : 1);
        var userCode = User.FindFirst(ClaimTypes.Name)?.Value ?? "UpdateAgent";

        var item = await _context.ProductItems.IgnoreQueryFilters()
            .FirstOrDefaultAsync(pi => pi.Id == id && pi.BranchId == branchId && !pi.IsDeleted);

        if (item == null)
        {
            return NotFound(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "الجهاز / السيريال غير موجود."
            });
        }

        string newSn = request.SerialNumber.Trim();
        string oldSn = item.SerialNumber;

        if (string.Equals(oldSn, newSn, StringComparison.OrdinalIgnoreCase))
        {
            return Ok(new SingleObjectResponseModel
            {
                IsDone = true,
                ReturnMessage = "الرقم المسلسل لم يتغير."
            });
        }

        // Check if new serial already exists on another item in the branch
        var exists = await _context.ProductItems.IgnoreQueryFilters()
            .AnyAsync(pi => pi.Id != id && pi.BranchId == branchId && pi.SerialNumber.ToLower() == newSn.ToLower() && !pi.IsDeleted);

        if (exists)
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = $"الرقم المسلسل '{newSn}' مسجل بالفعل لجهاز آخر في النظام."
            });
        }

        item.SerialNumber = newSn;
        item.QRCode = newSn;
        item.LastUpdate = DateTime.UtcNow;
        item.UpdateUserCode = userCode;

        // Also update any Compass record referencing old serial number
        var compassRecords = await _context.Compasses.IgnoreQueryFilters()
            .Where(c => c.BranchId == branchId && c.SerialNumber == oldSn && !c.IsDeleted)
            .ToListAsync();

        foreach (var c in compassRecords)
        {
            c.SerialNumber = newSn;
            c.LastUpdate = DateTime.UtcNow;
            c.UpdateUserCode = userCode;
        }

        await _context.SaveChangesAsync();
        _repositoryWrapper.ProductItems.InvalidateCache();
        _repositoryWrapper.Compasses.InvalidateCache();

        return Ok(new SingleObjectResponseModel
        {
            IsDone = true,
            ReturnMessage = $"تم تعديل الرقم المسلسل من '{oldSn}' إلى '{newSn}' بنجاح."
        });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProductItem([FromRoute] int id)
    {
        var branchClaim = User.FindFirst("branch_id")?.Value ?? User.FindFirst("BranchId")?.Value;
        int branchId = (_context.CurrentBranchId.HasValue && _context.CurrentBranchId.Value > 0)
            ? _context.CurrentBranchId.Value
            : (int.TryParse(branchClaim, out var bId) && bId > 0 ? bId : 1);
        var userCode = User.FindFirst(ClaimTypes.Name)?.Value ?? "DeleteAgent";

        var item = await _context.ProductItems.IgnoreQueryFilters()
            .FirstOrDefaultAsync(pi => pi.Id == id && pi.BranchId == branchId && !pi.IsDeleted);

        if (item == null)
        {
            return NotFound(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "الجهاز / السيريال غير موجود أو تم حذفه مسبقاً."
            });
        }

        item.IsDeleted = true;
        item.LastUpdate = DateTime.UtcNow;
        item.UpdateUserCode = userCode;

        // If the item was in-stock, decrement the inventory quantity accordingly
        if (item.Status == ProductItemStatus.InStock)
        {
            var inventory = await _context.Inventories.IgnoreQueryFilters()
                .FirstOrDefaultAsync(i => i.ProductId == item.ProductId && i.BranchId == branchId && !i.IsDeleted);

            if (inventory != null && inventory.Quantity > 0)
            {
                inventory.Quantity = Math.Max(0, inventory.Quantity - 1);
                inventory.LastUpdate = DateTime.UtcNow;
                inventory.UpdateUserCode = userCode;
            }
        }

        await _context.SaveChangesAsync();
        _repositoryWrapper.ProductItems.InvalidateCache();
        _repositoryWrapper.Inventories.InvalidateCache();
        _repositoryWrapper.Products.InvalidateCache();

        return Ok(new SingleObjectResponseModel
        {
            IsDone = true,
            ReturnMessage = $"تم حذف الجهاز ذو الرقم المسلسل '{item.SerialNumber}' بنجاح وتحديث رصيد المخزن."
        });
    }

    [HttpPost("sync-all-missing")]
    public async Task<IActionResult> SyncAllMissingSerials()
    {
        int branchId = (_context.CurrentBranchId.HasValue && _context.CurrentBranchId.Value > 0)
            ? _context.CurrentBranchId.Value
            : (int.TryParse(User.FindFirst("branch_id")?.Value ?? User.FindFirst("BranchId")?.Value, out var bId) && bId > 0 ? bId : 1);
        var userCode = User.FindFirst(ClaimTypes.Name)?.Value ?? "SyncAgent";

        var inventories = await _context.Inventories
            .Include(i => i.Product)
            .Where(i => i.BranchId == branchId && !i.IsDeleted && i.Quantity > 0)
            .ToListAsync();

        int generatedCount = 0;
        foreach (var inv in inventories)
        {
            int currentCount = await _context.ProductItems
                .CountAsync(pi => pi.ProductId == inv.ProductId && pi.BranchId == branchId && !pi.IsDeleted);

            if (currentCount < inv.Quantity)
            {
                int missing = inv.Quantity - currentCount;
                for (int m = 0; m < missing; m++)
                {
                    string sn = $"SN-{inv.Product?.SKU ?? "ITEM"}-{currentCount + m + 1}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
                    await _context.ProductItems.AddAsync(new ProductItem
                    {
                        ProductId = inv.ProductId,
                        BranchId = branchId,
                        SerialNumber = sn,
                        QRCode = sn,
                        Status = ProductItemStatus.InStock,
                        InsertDate = DateTime.UtcNow,
                        InsertUserCode = userCode
                    });
                    generatedCount++;
                }
            }
        }

        if (generatedCount > 0)
        {
            await _context.SaveChangesAsync();
            _repositoryWrapper.ProductItems.InvalidateCache();
        }

        return Ok(new SingleObjectResponseModel
        {
            IsDone = true,
            ReturnMessage = $"تم التحقق وتوليد {generatedCount} رقم تسلسلي للمنتجات غير المسلسلة في المخزون بنجاح."
        });
    }

    private async Task AutoBackfillProductSerialsAsync(int productId)
    {
        try
        {
            int branchId = (_context.CurrentBranchId.HasValue && _context.CurrentBranchId.Value > 0)
                ? _context.CurrentBranchId.Value
                : (int.TryParse(User.FindFirst("branch_id")?.Value ?? User.FindFirst("BranchId")?.Value, out var bId) && bId > 0 ? bId : 1);
            var userCode = User.FindFirst(ClaimTypes.Name)?.Value ?? "AutoBackfill";

            var inventory = await _context.Inventories
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.ProductId == productId && i.BranchId == branchId && !i.IsDeleted);

            if (inventory == null || inventory.Quantity <= 0) return;

            int currentCount = await _context.ProductItems
                .CountAsync(pi => pi.ProductId == productId && pi.BranchId == branchId && !pi.IsDeleted);

            if (currentCount < inventory.Quantity)
            {
                int missing = inventory.Quantity - currentCount;
                for (int m = 0; m < missing; m++)
                {
                    string sn = $"SN-{inventory.Product?.SKU ?? "ITEM"}-{currentCount + m + 1}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
                    await _context.ProductItems.AddAsync(new ProductItem
                    {
                        ProductId = productId,
                        BranchId = branchId,
                        SerialNumber = sn,
                        QRCode = sn,
                        Status = ProductItemStatus.InStock,
                        InsertDate = DateTime.UtcNow,
                        InsertUserCode = userCode
                    });
                }
                await _context.SaveChangesAsync();
                _repositoryWrapper.ProductItems.InvalidateCache();
            }
        }
        catch { }
    }
}
