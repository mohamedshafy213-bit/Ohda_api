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
        var group = await RepositoryContext.UserGroups
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(g => g.Id == groupId && !g.IsDeleted);

        bool isBranchGroup = group != null && group.BranchId > 0;

        var query = RepositoryContext.GroupPagePermissions
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(p => p.UserGroupId == groupId && !p.IsDeleted && p.Page != null && !p.Page.IsDeleted);

        if (isBranchGroup)
        {
            query = query.Where(p => !p.Page!.Path.ToLower().Contains("branches"));
        }

        return await query
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
        var group = await RepositoryContext.UserGroups
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(g => g.Id == groupId && !g.IsDeleted);
        if (group == null)
            return false;

        var page = await RepositoryContext.Pages
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == pageId && !p.IsDeleted);
        if (page == null)
            return false;

        // Block SuperAdmin platform management pages from being granted to branch groups
        if (group.BranchId > 0 && page.Path.ToLower().Contains("branches"))
        {
            return false;
        }

        var existing = await RepositoryContext.GroupPagePermissions
            .FirstOrDefaultAsync(p => p.UserGroupId == groupId && p.PageId == pageId && !p.IsDeleted);

        if (existing != null)
            return true;

        var permission = new GroupPagePermission
        {
            BranchId = group.BranchId,
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
        var group = await RepositoryContext.UserGroups
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(g => g.Id == groupId && !g.IsDeleted);
        if (group == null) return;

        var query = RepositoryContext.Pages
            .IgnoreQueryFilters()
            .Where(p => !p.IsDeleted);

        if (group.BranchId > 0)
        {
            query = query.Where(p => !p.Path.ToLower().Contains("branches"));
        }

        var allPages = await query.ToListAsync();
        foreach (var page in allPages)
        {
            await GrantPermissionAsync(groupId, page.Id, grantedByUserId);
        }
    }
}
