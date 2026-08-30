using Contracts.DTOs.Page;
using Contracts.DTOs.UserPagePermission;
using Contracts.Interfaces.Repository;
using Entities.Models.Databases;
using Entities.Models.Tables;
using LoggerService;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.Repositories;

namespace Repositories.Models;

public class UserPagePermissionRepository
    : RepositoryBase<UserPagePermission, UserPagePermissionDto, GrantPermissionDto, UserPagePermissionUpdateDto>, IUserPagePermissionRepository
{
    public UserPagePermissionRepository(
        ILoggerManager logger,
        RepositoryContext repositoryContext,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
        : base(logger, repositoryContext, httpContextAccessor, mapper)
    {
    }

    public async Task<List<PageDto>> GetAllowedPagesForUserAsync(int userId)
    {
        var user = await RepositoryContext.Users.FindAsync(userId);
        if (user == null)
            return new List<PageDto>();

        if (user.Role == Entities.Models.Enums.UserRole.Admin)
        {
            // Admin automatically gets access to all active pages
            return await RepositoryContext.Pages
                .AsNoTracking()
                .Where(p => !p.IsDeleted)
                .OrderBy(p => p.SortOrder)
                .Select(p => new PageDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Path = p.Path,
                    Icon = p.Icon,
                    SortOrder = p.SortOrder
                })
                .ToListAsync();
        }

        if (user.UserGroupId.HasValue)
        {
            return await RepositoryContext.GroupPagePermissions
                .AsNoTracking()
                .Where(p => p.UserGroupId == user.UserGroupId.Value && !p.IsDeleted && p.Page != null && !p.Page.IsDeleted)
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

        return await RepositoryContext.UserPagePermissions
            .AsNoTracking()
            .Where(p => p.UserId == userId && !p.IsDeleted && p.Page != null && !p.Page.IsDeleted)
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

    public async Task<bool> GrantPermissionAsync(int userId, int pageId, int grantedByUserId)
    {
        var userExists = await RepositoryContext.Users.AnyAsync(u => u.MilitaryNumber == userId && !u.IsDeleted);
        if (!userExists)
            return false;

        var pageExists = await RepositoryContext.Pages.AnyAsync(p => p.Id == pageId && !p.IsDeleted);
        if (!pageExists)
            return false;

        var existing = await RepositoryContext.UserPagePermissions
            .FirstOrDefaultAsync(p => p.UserId == userId && p.PageId == pageId && !p.IsDeleted);

        if (existing != null)
            return true;

        var permission = new UserPagePermission
        {
            UserId = userId,
            PageId = pageId,
            GrantedByUserId = grantedByUserId
        };

        await RepositoryContext.UserPagePermissions.AddAsync(permission);
        await RepositoryContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RevokePermissionAsync(int userId, int pageId)
    {
        var existing = await RepositoryContext.UserPagePermissions
            .FirstOrDefaultAsync(p => p.UserId == userId && p.PageId == pageId && !p.IsDeleted);

        if (existing == null)
            return false;

        existing.IsDeleted = true;
        existing.DeleteDate = DateTime.UtcNow;
        await RepositoryContext.SaveChangesAsync();
        return true;
    }

    public async Task GrantAllPagesToUserAsync(int userId, int grantedByUserId)
    {
        var allPages = await RepositoryContext.Pages.Where(p => !p.IsDeleted).ToListAsync();
        foreach (var page in allPages)
        {
            await GrantPermissionAsync(userId, page.Id, grantedByUserId);
        }
    }
}
