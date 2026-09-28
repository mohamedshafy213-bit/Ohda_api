using Contracts.DTOs.Page;
using Contracts.Interfaces.Repository;
using Entities.Models.Databases;
using Entities.Models.Tables;
using LoggerService;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Repositories.Repositories;

using Microsoft.EntityFrameworkCore;

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

    public async Task<List<PageDto>> GetPagesFilteredAsync(bool isSuperAdmin)
    {
        var query = RepositoryContext.Pages
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(p => !p.IsDeleted);

        if (!isSuperAdmin)
        {
            query = query.Where(p => !p.Path.ToLower().Contains("branches"));
        }

        return await query
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
