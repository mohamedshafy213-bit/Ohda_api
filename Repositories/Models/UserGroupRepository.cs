using Contracts.DTOs.UserGroup;
using Contracts.Interfaces.Repository;
using Entities.Models.Databases;
using Entities.Models.Tables;
using LoggerService;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.Repositories;

namespace Repositories.Models;

public class UserGroupRepository
    : RepositoryBase<UserGroup, UserGroupDto, UserGroupCreateDto, UserGroupUpdateDto>, IUserGroupRepository
{
    public UserGroupRepository(
        ILoggerManager logger,
        RepositoryContext repositoryContext,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
        : base(logger, repositoryContext, httpContextAccessor, mapper)
    {
    }

    public async Task<List<UserGroupDto>> GetGroupsFilteredAsync(int? branchId, bool isSuperAdmin, int? userBranchId)
    {
        var query = RepositoryContext.UserGroups
            .IgnoreQueryFilters()
            .Include(g => g.Branch)
            .AsNoTracking()
            .Where(g => !g.IsDeleted && (g.Branch == null || !g.Branch.IsDeleted));

        if (!isSuperAdmin)
        {
            // Branch user only sees their own branch groups
            query = query.Where(g => g.BranchId == userBranchId);
        }
        else if (branchId.HasValue && branchId.Value > 0)
        {
            // SuperAdmin filtering by a specific branch
            query = query.Where(g => g.BranchId == branchId.Value);
        }

        return await query
            .OrderBy(g => g.Id)
            .Select(g => new UserGroupDto
            {
                Id = g.Id,
                Name = g.Name,
                Description = g.Description,
                BranchId = g.BranchId,
                BranchName = g.Branch != null ? g.Branch.Name : null
            })
            .ToListAsync();
    }
}
