using Contracts.DTOs.Compass;
using Contracts.Interfaces.Repository;
using Entities.Models.Databases;
using Entities.Models.Tables;
using Entities.Models.Enums;
using LoggerService;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.Repositories;

namespace Repositories.Models;

public class CompassRepository
    : RepositoryBase<Compass, CompassDto, CompassCreateDto, CompassUpdateDto>, ICompassRepository
{
    public CompassRepository(
        ILoggerManager logger,
        RepositoryContext repositoryContext,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
        : base(logger, repositoryContext, httpContextAccessor, mapper)
    {
    }

    public async Task<IEnumerable<Compass>> SearchCompassRecordsAsync(
        string? query,
        int? departmentId,
        CompassType? type,
        int? stateId,
        DateTime? startDate,
        DateTime? endDate,
        int? pageNumber = null,
        int? pageSize = null)
    {
        var dbQuery = RepositoryContext.Compasses
            .Include(c => c.ProductExitRequest)
            .Include(c => c.ProductEntryRequest)
            .Include(c => c.Department)
            .Include(c => c.ProductState)
            .Where(c => !c.IsDeleted);

        if (!string.IsNullOrWhiteSpace(query))
        {
            query = query.Trim().ToLower();
            dbQuery = dbQuery.Where(c =>
                c.SerialNumber.ToLower().Contains(query) ||
                c.ProductName.ToLower().Contains(query) ||
                c.RecipientName.ToLower().Contains(query) ||
                c.Place.ToLower().Contains(query)
            );
        }

        if (departmentId.HasValue)
        {
            dbQuery = dbQuery.Where(c => c.DepartmentId == departmentId.Value);
        }

        if (type.HasValue)
        {
            dbQuery = dbQuery.Where(c => c.Type == type.Value);
        }

        if (stateId.HasValue)
        {
            dbQuery = dbQuery.Where(c => c.ProductStateId == stateId.Value);
        }

        if (startDate.HasValue)
        {
            dbQuery = dbQuery.Where(c => c.ExitDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            dbQuery = dbQuery.Where(c => c.ExitDate <= endDate.Value);
        }

        if (pageNumber.HasValue && pageSize.HasValue)
        {
            int skip = (pageNumber.Value - 1) * pageSize.Value;
            dbQuery = dbQuery.Skip(skip).Take(pageSize.Value);
        }
        else
        {
            dbQuery = dbQuery.Take(100);
        }

        return await dbQuery.ToListAsync();
    }

    public async Task<Compass?> GetBySerialNumberAsync(string serialNumber)
    {
        return await RepositoryContext.Compasses
            .Include(c => c.ProductExitRequest)
            .FirstOrDefaultAsync(c => c.SerialNumber == serialNumber && !c.IsDeleted);
    }
}
