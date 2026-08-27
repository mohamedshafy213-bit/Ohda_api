using Contracts.DTOs.ApprovalConfig;
using Contracts.interfaces.Repository;
using Entities.Models.Tables;
using Entities.Models.Enums;

namespace Contracts.Interfaces.Repository;

public interface IApprovalConfigRepository : IRepositoryBase<ApprovalConfig, ApprovalConfigDto, ApprovalConfigCreateDto, ApprovalConfigUpdateDto>
{
    Task<IEnumerable<ApprovalConfig>> GetConfigsByTypeAsync(RequestType requestType);
    Task<bool> IsActionAllowedAsync(RequestType requestType, int userGroupId, WorkflowRole role);
}
