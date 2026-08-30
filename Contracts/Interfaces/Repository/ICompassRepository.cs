using Contracts.DTOs.Compass;
using Contracts.interfaces.Repository;
using Entities.Models.Tables;
using Entities.Models.Enums;

namespace Contracts.Interfaces.Repository;

public interface ICompassRepository : IRepositoryBase<Compass, CompassDto, CompassCreateDto, CompassUpdateDto>
{
    Task<IEnumerable<Compass>> SearchCompassRecordsAsync(
        string? query,
        int? departmentId,
        CompassType? type,
        int? stateId,
        DateTime? startDate,
        DateTime? endDate,
        int? pageNumber = null,
        int? pageSize = null);
    Task<Compass?> GetBySerialNumberAsync(string serialNumber);
}
