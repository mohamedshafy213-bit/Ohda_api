using Contracts.DTOs.ApprovalConfig;
using Contracts.Interfaces.Repository;
using Entities.Models.Databases;
using Entities.Models.Tables;
using Entities.Models.Enums;
using LoggerService;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
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
        return await RepositoryContext.ApprovalConfigs
            .Include(a => a.UserGroup)
            .Where(a => a.RequestType == requestType && a.IsActive && !a.IsDeleted)
            .ToListAsync();
    }

    public async Task<bool> IsActionAllowedAsync(RequestType requestType, int userGroupId, WorkflowRole role)
    {
        return await RepositoryContext.ApprovalConfigs
            .AnyAsync(a => a.RequestType == requestType &&
                           a.UserGroupId == userGroupId &&
                           a.WorkflowRole == role &&
                           a.IsActive &&
                           !a.IsDeleted);
    }
}
