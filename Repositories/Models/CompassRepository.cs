using Contracts.DTOs.Compass;
using Contracts.Interfaces.Repository;
using Entities.Models.Databases;
using Entities.Models.Tables;
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

    public async Task<IEnumerable<Compass>> SearchCompassRecordsAsync(string? query)
    {
        var dbQuery = RepositoryContext.Compasses
            .Include(c => c.ProductExitRequest)
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

        return await dbQuery.ToListAsync();
    }

    public async Task<Compass?> GetBySerialNumberAsync(string serialNumber)
    {
        return await RepositoryContext.Compasses
            .Include(c => c.ProductExitRequest)
            .FirstOrDefaultAsync(c => c.SerialNumber == serialNumber && !c.IsDeleted);
    }
}
