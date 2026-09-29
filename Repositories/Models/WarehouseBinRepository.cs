using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.DTOs.WarehouseBin;
using Contracts.Interfaces.Repository;
using Entities.Models.Databases;
using Entities.Models.Tables;
using LoggerService;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.Repositories;

namespace Repositories.Models;

public class WarehouseBinRepository
    : RepositoryBase<WarehouseBin, WarehouseBinDto, WarehouseBinCreateDto, WarehouseBinUpdateDto>, IWarehouseBinRepository
{
    public WarehouseBinRepository(
        ILoggerManager logger,
        RepositoryContext repositoryContext,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
        : base(logger, repositoryContext, httpContextAccessor, mapper)
    {
    }

    public async Task<WarehouseBin?> GetByIdAsync(int id)
    {
        return await RepositoryContext.WarehouseBins
            .Include(b => b.Department)
            .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);
    }

    public async Task<IEnumerable<WarehouseBin>> GetAllWithDetailsAsync(int? departmentId = null)
    {
        var query = RepositoryContext.WarehouseBins
            .Include(b => b.Department)
            .Where(b => !b.IsDeleted);

        if (departmentId.HasValue && departmentId.Value > 0)
        {
            query = query.Where(b => b.DepartmentId == departmentId.Value);
        }

        return await query.OrderBy(b => b.Code).ToListAsync();
    }

    public async Task<IEnumerable<ProductItem>> GetItemsByBinIdAsync(int binId)
    {
        return await RepositoryContext.ProductItems
            .Include(pi => pi.Product)
            .Where(pi => pi.BinId == binId && !pi.IsDeleted)
            .OrderBy(pi => pi.SerialNumber)
            .ToListAsync();
    }

    public async Task<List<WarehouseBinDto>> GetBinsWithCountsAsync(int? departmentId = null)
    {
        var query = RepositoryContext.WarehouseBins
            .AsNoTracking()
            .Where(b => !b.IsDeleted);

        if (departmentId.HasValue && departmentId.Value > 0)
        {
            query = query.Where(b => b.DepartmentId == departmentId.Value);
        }

        return await query
            .OrderBy(b => b.Code)
            .Select(b => new WarehouseBinDto
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
                DepartmentName = b.Department != null ? b.Department.Name : "غير محدد",
                ItemsCount = RepositoryContext.ProductItems.Count(pi => pi.BinId == b.Id && !pi.IsDeleted),
                InsertDate = b.InsertDate,
                LastUpdate = b.LastUpdate
            })
            .ToListAsync();
    }

    public async Task<List<WarehouseBinItemDto>> GetBinItemsDtoAsync(int binId)
    {
        return await RepositoryContext.ProductItems
            .AsNoTracking()
            .Where(pi => pi.BinId == binId && !pi.IsDeleted)
            .OrderBy(pi => pi.SerialNumber)
            .Select(pi => new WarehouseBinItemDto
            {
                Id = pi.Id,
                ProductId = pi.ProductId,
                ProductName = pi.Product != null ? pi.Product.Name : "Unknown",
                ProductSKU = pi.Product != null ? pi.Product.SKU : "N/A",
                SerialNumber = pi.SerialNumber,
                QRCode = pi.QRCode,
                Status = (int)pi.Status,
                StatusLabel = pi.Status == Entities.Models.Enums.ProductItemStatus.InStock ? "متوفر بالمستودع" :
                              pi.Status == Entities.Models.Enums.ProductItemStatus.Exited ? "منصرف كعهدة" :
                              pi.Status == Entities.Models.Enums.ProductItemStatus.Damaged ? "تالف" :
                              pi.Status == Entities.Models.Enums.ProductItemStatus.InMaintenance ? "تحت الصيانة" : "أخرى",
                Place = pi.Place,
                InsertDate = pi.InsertDate
            })
            .ToListAsync();
    }

    public async Task<(bool Success, string Message)> AssignProductToBinAsync(int binId, int productId, int quantity)
    {
        var bin = await RepositoryContext.WarehouseBins.FirstOrDefaultAsync(b => b.Id == binId && !b.IsDeleted);
        if (bin == null)
            return (false, "الرف غير موجود أو تم حذفه.");

        var product = await RepositoryContext.Products.FirstOrDefaultAsync(p => p.Id == productId && !p.IsDeleted);
        if (product == null)
            return (false, "الصنف المحدد غير موجود.");

        if (quantity <= 0) quantity = 1;

        int currentCount = await RepositoryContext.ProductItems.CountAsync(pi => pi.BinId == binId && !pi.IsDeleted);
        if (bin.Capacity.HasValue && bin.Capacity.Value > 0)
        {
            int remaining = bin.Capacity.Value - currentCount;
            if (quantity > remaining)
            {
                return (false, $"السعة المتبقية في الرف ({remaining}) لا تكفي لتسكين الكمية المطلوبة ({quantity}).");
            }
        }

        // Find available unassigned product items in stock
        var availableItems = await RepositoryContext.ProductItems
            .Where(pi => pi.ProductId == productId && pi.Status == Entities.Models.Enums.ProductItemStatus.InStock && !pi.IsDeleted && pi.BinId != binId)
            .Take(quantity)
            .ToListAsync();

        int assignedCount = 0;
        foreach (var item in availableItems)
        {
            item.BinId = binId;
            item.Place = bin.Name;
            item.LastUpdate = DateTime.UtcNow;
            assignedCount++;
        }

        // If more items requested than existing unassigned items, create new serialized items for the remaining quantity
        int needed = quantity - assignedCount;
        if (needed > 0)
        {
            string barcode = !string.IsNullOrWhiteSpace(product.Barcode) ? product.Barcode : (!string.IsNullOrWhiteSpace(product.SKU) ? product.SKU : "PRD");
            for (int i = 0; i < needed; i++)
            {
                string guidSuffix = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
                string serialNumber = $"SN-{barcode}-{guidSuffix}";
                string qrCode = $"QR-{serialNumber}";
                var newItem = new ProductItem
                {
                    ProductId = productId,
                    SerialNumber = serialNumber,
                    QRCode = qrCode,
                    Status = Entities.Models.Enums.ProductItemStatus.InStock,
                    BinId = binId,
                    Place = bin.Name,
                    BranchId = bin.BranchId,
                    InsertDate = DateTime.UtcNow,
                    IsDeleted = false
                };
                await RepositoryContext.ProductItems.AddAsync(newItem);
                assignedCount++;
            }
        }

        // Also associate inventory record if not linked
        var inventory = await RepositoryContext.Inventories.FirstOrDefaultAsync(inv => inv.ProductId == productId && !inv.IsDeleted);
        if (inventory != null && inventory.BinId == null)
        {
            inventory.BinId = binId;
        }

        await RepositoryContext.SaveChangesAsync();
        return (true, $"تم تسكين {assignedCount} قطعة من '{product.Name}' بنجاح في الرف {bin.Code} ({bin.Name})");
    }

    public async Task<(bool Success, string Message)> UnassignItemFromBinAsync(int binId, int itemId)
    {
        var item = await RepositoryContext.ProductItems.FirstOrDefaultAsync(pi => pi.Id == itemId && !pi.IsDeleted);
        if (item == null)
            return (false, "الجهاز أو الصنف غير موجود.");

        item.BinId = null;
        item.LastUpdate = DateTime.UtcNow;
        await RepositoryContext.SaveChangesAsync();

        return (true, "تم إلغاء تسكين الجهاز من الرف بنجاح.");
    }
}

