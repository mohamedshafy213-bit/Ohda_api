using Contracts.DTOs.UserGroup;
using Contracts.Interfaces.Repository;
using Entities.Models.Databases;
using Entities.Models.Tables;
using LoggerService;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
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
}
