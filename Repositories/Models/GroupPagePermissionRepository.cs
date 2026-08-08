using Contracts.DTOs.GroupPagePermission;
using Contracts.DTOs.Page;
using Contracts.Interfaces.Repository;
using Entities.Models.Databases;
using Entities.Models.Tables;
using LoggerService;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.Repositories;

namespace Repositories.Models;

public class GroupPagePermissionRepository
    : RepositoryBase<GroupPagePermission, GroupPagePermissionDto, GrantGroupPermissionDto, GroupPagePermissionUpdateDto>, IGroupPagePermissionRepository
{
    public GroupPagePermissionRepository(
        ILoggerManager logger,
        RepositoryContext repositoryContext,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
        : base(logger, repositoryContext, httpContextAccessor, mapper)
    {
    }

    public async Task<List<PageDto>> GetAllowedPagesForGroupAsync(int groupId)
    {
        return await RepositoryContext.GroupPagePermissions
            .AsNoTracking()
            .Where(p => p.UserGroupId == groupId && !p.IsDeleted && p.Page != null && !p.Page.IsDeleted)
            .OrderBy(p => p.Page!.SortOrder)
            .Select(p => new PageDto
            {
                Id = p.Page!.Id,
                Title = p.Page.Title,
                Path = p.Page.Path,
                Icon = p.Page.Icon,
                SortOrder = p.Page.SortOrder
            })
            .ToListAsync();
    }

    public async Task<bool> GrantPermissionAsync(int groupId, int pageId, int grantedByUserId)
    {
        var groupExists = await RepositoryContext.UserGroups.AnyAsync(g => g.Id == groupId && !g.IsDeleted);
        if (!groupExists)
            return false;

        var pageExists = await RepositoryContext.Pages.AnyAsync(p => p.Id == pageId && !p.IsDeleted);
        if (!pageExists)
            return false;

        var existing = await RepositoryContext.GroupPagePermissions
            .FirstOrDefaultAsync(p => p.UserGroupId == groupId && p.PageId == pageId && !p.IsDeleted);

        if (existing != null)
            return true;

        var permission = new GroupPagePermission
        {
            UserGroupId = groupId,
            PageId = pageId,
            GrantedByUserId = grantedByUserId
        };

        await RepositoryContext.GroupPagePermissions.AddAsync(permission);
        await RepositoryContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RevokePermissionAsync(int groupId, int pageId)
    {
        var existing = await RepositoryContext.GroupPagePermissions
            .FirstOrDefaultAsync(p => p.UserGroupId == groupId && p.PageId == pageId && !p.IsDeleted);

        if (existing == null)
            return false;

        existing.IsDeleted = true;
        existing.DeleteDate = DateTime.UtcNow;
        await RepositoryContext.SaveChangesAsync();
        return true;
    }

    public async Task GrantAllPagesToGroupAsync(int groupId, int grantedByUserId)
    {
        var allPages = await RepositoryContext.Pages.Where(p => !p.IsDeleted).ToListAsync();
        foreach (var page in allPages)
        {
            await GrantPermissionAsync(groupId, page.Id, grantedByUserId);
        }
    }
}
