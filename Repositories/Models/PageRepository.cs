using Contracts.DTOs.Page;
using Contracts.Interfaces.Repository;
using Entities.Models.Databases;
using Entities.Models.Enums;
using Entities.Models.Tables;
using LoggerService;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.Repositories;

namespace Repositories.Models;

public class PageRepository
    : RepositoryBase<Page, PageDto, PageCreateDto, PageUpdateDto>, IPageRepository
{
    public PageRepository(
        ILoggerManager logger,
        RepositoryContext repositoryContext,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
        : base(logger, repositoryContext, httpContextAccessor, mapper)
    {
    }

    public async Task<List<PageDto>> GetAllowedPagesForRoleAsync(UserRole role)
    {
        string roleName = role.ToString();

        var pages = await RepositoryContext.Pages
            .AsNoTracking()
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.SortOrder)
            .ToListAsync();

        var allowedPages = pages
            .Where(p => p.AllowedRoles.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                         .Contains(roleName, StringComparer.OrdinalIgnoreCase))
            .Select(p => new PageDto
            {
                Id = p.Id,
                Title = p.Title,
                Path = p.Path,
                Icon = p.Icon,
                SortOrder = p.SortOrder,
                AllowedRoles = p.AllowedRoles
            })
            .ToList();

        return allowedPages;
    }
}
