using Contracts.DTOs.Page;
using Contracts.DTOs.UserPagePermission;
using Contracts.interfaces.Repository;

namespace Contracts.Interfaces.Repository;

public interface IUserPagePermissionRepository : IRepositoryBase<Entities.Models.Tables.UserPagePermission, UserPagePermissionDto, GrantPermissionDto, UserPagePermissionUpdateDto>
{
    Task<List<PageDto>> GetAllowedPagesForUserAsync(long userId, Entities.Models.Enums.UserRole? role = null, int? userGroupId = null);
    Task<bool> GrantPermissionAsync(long userId, int pageId, long grantedByUserId);
    Task<bool> RevokePermissionAsync(long userId, int pageId);
    Task GrantAllPagesToUserAsync(long userId, long grantedByUserId);
}
