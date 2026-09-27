using Contracts.DTOs.Page;
using Contracts.DTOs.UserPagePermission;
using Contracts.Interfaces.Repository;
using Entities.Models.Databases;
using Entities.Models.Tables;
using LoggerService;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
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

    private void InvalidateUserPermissionCache(int userId)
    {
        var cache = MemoryCache;
        if (cache != null)
        {
            cache.Remove($"UserAllowedPages_{userId}");
        }
    }

    public async Task<List<PageDto>> GetAllowedPagesForUserAsync(int userId)
    {
        var cache = MemoryCache;
        string cacheKey = $"UserAllowedPages_{userId}";
        if (cache != null && cache.TryGetValue(cacheKey, out List<PageDto>? cachedPages) && cachedPages != null)
        {
            return cachedPages;
        }

        var user = await RepositoryContext.Users
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.MilitaryNumber == userId);

        if (user == null)
            return new List<PageDto>();

        List<PageDto> result;

        if (user.Role == Entities.Models.Enums.UserRole.SuperAdmin)
        {
            // SuperAdmin gets access to all active pages including branch platform management
            result = await RepositoryContext.Pages
                .IgnoreQueryFilters()
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
        else
        {
            // 1. Direct user-specific page permissions
            var directUserPages = await RepositoryContext.UserPagePermissions
                .IgnoreQueryFilters()
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

            if (directUserPages.Any())
            {
                result = directUserPages;
            }
            else if (user.UserGroupId.HasValue)
            {
                // 2. User group permissions if assigned
                var groupPages = await RepositoryContext.GroupPagePermissions
                    .IgnoreQueryFilters()
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

                if (groupPages.Any())
                {
                    result = groupPages;
                }
                else if (user.Role == Entities.Models.Enums.UserRole.Admin)
                {
                    // 3. Fallback for branch Admin without specific group
                    result = await RepositoryContext.Pages
                        .IgnoreQueryFilters()
                        .AsNoTracking()
                        .Where(p => !p.IsDeleted && p.Path != "/branches" && p.Path != "/branches-dashboard" && p.Path != "/ohda/branches" && p.Path != "/ohda/branches-dashboard")
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
                else
                {
                    // 4. Default minimal fallback for other users
                    result = await RepositoryContext.Pages
                        .IgnoreQueryFilters()
                        .AsNoTracking()
                        .Where(p => !p.IsDeleted && (p.Path == "/dashboard" || p.Path == "/ohda/dashboard"))
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
            }
            else if (user.Role == Entities.Models.Enums.UserRole.Admin)
            {
                result = await RepositoryContext.Pages
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .Where(p => !p.IsDeleted && p.Path != "/branches" && p.Path != "/branches-dashboard" && p.Path != "/ohda/branches" && p.Path != "/ohda/branches-dashboard")
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
            else
            {
                result = await RepositoryContext.Pages
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .Where(p => !p.IsDeleted && (p.Path == "/dashboard" || p.Path == "/ohda/dashboard"))
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
        }

        if (cache != null)
        {
            cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));
        }

        return result;
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
        InvalidateUserPermissionCache(userId);
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
        InvalidateUserPermissionCache(userId);
        return true;
    }

    public async Task GrantAllPagesToUserAsync(int userId, int grantedByUserId)
    {
        var allPages = await RepositoryContext.Pages.Where(p => !p.IsDeleted).ToListAsync();
        foreach (var page in allPages)
        {
            await GrantPermissionAsync(userId, page.Id, grantedByUserId);
        }
        InvalidateUserPermissionCache(userId);
    }
}
