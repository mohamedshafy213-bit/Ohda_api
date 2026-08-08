using Contracts.DTOs.Page;
using Contracts.DTOs.UserPagePermission;
using Contracts.interfaces.Repository;

namespace Contracts.Interfaces.Repository;

public interface IUserPagePermissionRepository : IRepositoryBase<Entities.Models.Tables.UserPagePermission, UserPagePermissionDto, GrantPermissionDto, UserPagePermissionUpdateDto>
{
    Task<List<PageDto>> GetAllowedPagesForUserAsync(int userId);
    Task<bool> GrantPermissionAsync(int userId, int pageId, int grantedByUserId);
    Task<bool> RevokePermissionAsync(int userId, int pageId);
    Task GrantAllPagesToUserAsync(int userId, int grantedByUserId);
}
