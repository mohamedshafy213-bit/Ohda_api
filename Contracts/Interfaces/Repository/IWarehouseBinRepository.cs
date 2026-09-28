using System.Collections.Generic;
using System.Threading.Tasks;
using Contracts.DTOs.WarehouseBin;
using Contracts.interfaces.Repository;
using Entities.Models.Tables;

namespace Contracts.Interfaces.Repository;

public interface IWarehouseBinRepository : IRepositoryBase<WarehouseBin, WarehouseBinDto, WarehouseBinCreateDto, WarehouseBinUpdateDto>
{
    Task<WarehouseBin?> GetByIdAsync(int id);
    Task<IEnumerable<WarehouseBin>> GetAllWithDetailsAsync(int? departmentId = null);
    Task<IEnumerable<ProductItem>> GetItemsByBinIdAsync(int binId);
    Task<List<WarehouseBinDto>> GetBinsWithCountsAsync(int? departmentId = null);
    Task<List<WarehouseBinItemDto>> GetBinItemsDtoAsync(int binId);
}
