using Contracts.DTOs.Branch;
using Contracts.interfaces.Repository;
using Contracts.Responses;
using Entities.Models.Tables;

namespace Contracts.Interfaces.Repository;

public interface IBranchRepository : IRepositoryBase<Branch, BranchDto, BranchCreateDto, BranchUpdateDto>
{
    Task<Branch?> GetByIdAsync(int id);
    Task<Branch?> GetByCodeAsync(string code);
    Task<SingleObjectResponseModel> GetStatsAsync(int branchId);
    Task<ListOfObjectsResponseModel<BranchStatsDto>> GetAllBranchStatsAsync();
    Task<SingleObjectResponseModel<BranchCreatedResultDto>> CreateWithInitialAdminAsync(BranchCreateDto dto);
    Task<int> GetActiveUserCountAsync(int branchId);
    Task<int> GetActiveProductCountAsync(int branchId);
    Task<Dictionary<int, int>> GetActiveUserCountsByBranchIdsAsync(IEnumerable<int> branchIds);
    Task<Dictionary<int, int>> GetActiveProductCountsByBranchIdsAsync(IEnumerable<int> branchIds);
}
