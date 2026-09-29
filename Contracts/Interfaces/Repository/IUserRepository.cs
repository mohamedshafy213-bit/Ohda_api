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

    /// <summary>
    /// Lightweight login query: loads User + only the needed columns from Branch/UserGroup via projection.
    /// Returns a User entity with BranchName/UserGroupName populated in a single query, no Include.
    /// </summary>
    Task<UserLoginResult?> GetUserForLoginAsync(string username);

    /// <summary>
    /// Check if a user exists by MilitaryNumber without loading navigation properties.
    /// </summary>
    Task<bool> ExistsAsync(int militaryNumber);

    /// <summary>
    /// Lightweight projection query: loads UserDto with BranchName and UserGroupName directly.
    /// </summary>
    Task<UserDto?> GetUserDtoByIdAsync(int militaryNumber);

    /// <summary>
    /// Get the next available MilitaryNumber using MAX() instead of a loop.
    /// </summary>
    Task<int> GetNextMilitaryNumberAsync(int branchId);
}
