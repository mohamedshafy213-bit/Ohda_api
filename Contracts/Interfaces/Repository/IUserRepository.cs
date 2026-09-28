using Contracts.DTOs.User;
using Contracts.interfaces.Repository;
using Entities.Models.Tables;

namespace Contracts.Interfaces.Repository;

public interface IUserRepository : IRepositoryBase<User, UserDto, UserCreateDto, UserUpdateDto>
{
    Task<User?> GetByUsernameAsync(string username, bool trackChanges = false);
    Task<User?> GetByIdWithGroupAsync(int id, bool trackChanges = false);
    Task<List<UserDto>> GetUsersFilteredAsync(int? branchId, bool isSuperAdmin, int? userBranchId);
    void UpdateDirect(User user);
}
