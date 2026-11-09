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
