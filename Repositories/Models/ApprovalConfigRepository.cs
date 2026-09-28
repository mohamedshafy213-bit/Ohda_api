using Contracts.DTOs.ApprovalConfig;
using Contracts.Interfaces.Repository;
using Entities.Models.Databases;
using Entities.Models.Tables;
using Entities.Models.Enums;
using LoggerService;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Repositories.Repositories;

namespace Repositories.Models;

public class ApprovalConfigRepository
    : RepositoryBase<ApprovalConfig, ApprovalConfigDto, ApprovalConfigCreateDto, ApprovalConfigUpdateDto>, IApprovalConfigRepository
{
    public ApprovalConfigRepository(
        ILoggerManager logger,
        RepositoryContext repositoryContext,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
        : base(logger, repositoryContext, httpContextAccessor, mapper)
    {
    }

    public async Task<IEnumerable<ApprovalConfig>> GetConfigsByTypeAsync(RequestType requestType)
    {
        var cache = MemoryCache;
        var branchId = RepositoryContext.CurrentBranchId ?? 0;
        string cacheKey = $"ApprovalConfigs_Type_{branchId}_{requestType}";

        if (cache != null && cache.TryGetValue(cacheKey, out List<ApprovalConfig>? cachedConfigs) && cachedConfigs != null)
        {
            return cachedConfigs;
        }

        var result = await RepositoryContext.ApprovalConfigs
            .AsNoTracking()
            .Include(a => a.UserGroup)
            .Where(a => a.RequestType == requestType && a.IsActive && !a.IsDeleted)
            .ToListAsync();

        if (cache != null)
        {
            cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));
        }

        return result;
    }

    public async Task<bool> IsActionAllowedAsync(RequestType requestType, int userGroupId, WorkflowRole role)
    {
        var cache = MemoryCache;
        var branchId = RepositoryContext.CurrentBranchId ?? 0;
        string cacheKey = $"ApprovalConfig_Allowed_{branchId}_{requestType}_{userGroupId}_{role}";

        if (cache != null && cache.TryGetValue(cacheKey, out bool isAllowed))
        {
            return isAllowed;
        }

        bool result = await RepositoryContext.ApprovalConfigs
            .AsNoTracking()
            .AnyAsync(a => a.RequestType == requestType &&
                           a.UserGroupId == userGroupId &&
                           a.WorkflowRole == role &&
                           a.IsActive &&
                           !a.IsDeleted);

        if (cache != null)
        {
            cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));
        }

        return result;
    }
}
