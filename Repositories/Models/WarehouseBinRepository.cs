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
}
