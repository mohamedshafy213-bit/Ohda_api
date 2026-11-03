using Contracts.DTOs.ApprovalConfig;
using Contracts.enums;
using Contracts.Interfaces.Repository;
using Contracts.Responses;
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

    public override async Task<SingleObjectResponseModel> FindAll(int? pageNumber = null, int? pageSize = null)
    {
        try
        {
            var configs = await RepositoryContext.ApprovalConfigs
                .AsNoTracking()
                .Include(a => a.UserGroup)
                    .ThenInclude(g => g.Users)
                .Where(a => !a.IsDeleted)
                .OrderBy(a => a.RequestType)
                .ThenBy(a => a.StepOrder)
                .ToListAsync();

            var dtos = configs.Select(a => new ApprovalConfigDto
            {
                Id = a.Id,
                RequestType = a.RequestType,
                UserGroupId = a.UserGroupId,
                UserGroupName = a.UserGroup?.Name,
                WorkflowRole = a.WorkflowRole,
                StepOrder = a.StepOrder,
                StepName = a.StepName,
                ColorHex = a.ColorHex,
                IsActive = a.IsActive,
                EligibleUsers = a.UserGroup?.Users?
                    .Where(u => !u.IsDeleted)
                    .Select(u => new ApprovalUserSummaryDto
                    {
                        MilitaryNumber = u.MilitaryNumber,
                        Username = u.Username,
                        PersonName = u.PersonName
                    }).ToList() ?? new List<ApprovalUserSummaryDto>()
            }).ToList();

            return new ListOfObjectsResponseModel<ApprovalConfigDto>
            {
                ErrorCode = ErrorCatalog.noError,
                IsDone = true,
                ReturnMessage = "Configs loaded successfully",
                Objects = dtos,
                TotalCount = dtos.Count,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
        catch (Exception ex)
        {
            return new SingleObjectResponseModel
            {
                ErrorCode = ErrorCatalog.DataBaseFauiler,
                IsDone = false,
                ReturnMessage = ex.Message
            };
        }
    }

    public async Task<IEnumerable<ApprovalConfig>> GetConfigsByTypeAsync(RequestType requestType)
    {
        return await RepositoryContext.ApprovalConfigs
            .AsNoTracking()
            .Include(a => a.UserGroup)
                .ThenInclude(g => g.Users)
            .Where(a => a.RequestType == requestType && a.IsActive && !a.IsDeleted)
            .OrderBy(a => a.StepOrder)
            .ToListAsync();
    }

    public async Task<bool> IsActionAllowedAsync(RequestType requestType, int userGroupId, WorkflowRole role)
    {
        return await RepositoryContext.ApprovalConfigs
            .AsNoTracking()
            .AnyAsync(a => a.RequestType == requestType &&
                           a.UserGroupId == userGroupId &&
                           a.WorkflowRole == role &&
                           a.IsActive &&
                           !a.IsDeleted);
    }
}
