using Contracts.DTOs.UserGroup;
using Contracts.interfaces.Repository;
using Entities.Models.Tables;

namespace Contracts.Interfaces.Repository;

public interface IUserGroupRepository : IRepositoryBase<UserGroup, UserGroupDto, UserGroupCreateDto, UserGroupUpdateDto>
{
}
