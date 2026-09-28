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
}
