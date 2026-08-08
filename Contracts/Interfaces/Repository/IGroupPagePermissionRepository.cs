using Contracts.DTOs.GroupPagePermission;
using Contracts.DTOs.Page;
using Contracts.interfaces.Repository;
using Entities.Models.Tables;

namespace Contracts.Interfaces.Repository;

public interface IGroupPagePermissionRepository : IRepositoryBase<GroupPagePermission, GroupPagePermissionDto, GrantGroupPermissionDto, GroupPagePermissionUpdateDto>
{
    Task<List<PageDto>> GetAllowedPagesForGroupAsync(int groupId);
    Task<bool> GrantPermissionAsync(int groupId, int pageId, int grantedByUserId);
    Task<bool> RevokePermissionAsync(int groupId, int pageId);
    Task GrantAllPagesToGroupAsync(int groupId, int grantedByUserId);
}
