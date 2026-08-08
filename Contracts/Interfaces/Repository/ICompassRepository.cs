using Contracts.DTOs.Compass;
using Contracts.interfaces.Repository;
using Entities.Models.Tables;

namespace Contracts.Interfaces.Repository;

public interface ICompassRepository : IRepositoryBase<Compass, CompassDto, CompassCreateDto, CompassUpdateDto>
{
    Task<IEnumerable<Compass>> SearchCompassRecordsAsync(string? query);
    Task<Compass?> GetBySerialNumberAsync(string serialNumber);
}
