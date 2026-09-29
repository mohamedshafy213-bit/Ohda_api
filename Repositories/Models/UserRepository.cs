using Contracts.DTOs.User;
using Contracts.Interfaces.Repository;
using Entities.Models.Databases;
using Entities.Models.Tables;
using LoggerService;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.Repositories;

namespace Repositories.Models;

public class UserRepository
    : RepositoryBase<User, UserDto, UserCreateDto, UserUpdateDto>, IUserRepository
{
    public UserRepository(
        ILoggerManager logger,
        RepositoryContext repositoryContext,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
        : base(logger, repositoryContext, httpContextAccessor, mapper)
    {
    }

    public async Task<User?> GetByUsernameAsync(string username, bool trackChanges = false)
    {
        var cleanUsername = username.Trim();
        var query = RepositoryContext.Users.IgnoreQueryFilters();
        if (!trackChanges)
            query = query.AsNoTracking();

        return await query
            .Include(u => u.UserGroup)
            .Include(u => u.Branch)
            .FirstOrDefaultAsync(u => u.Username == cleanUsername && !u.IsDeleted);
    }

    public async Task<User?> GetByIdWithGroupAsync(int id, bool trackChanges = false)
    {
        var query = RepositoryContext.Users.IgnoreQueryFilters();
        if (!trackChanges)
            query = query.AsNoTracking();

        return await query
            .Include(u => u.UserGroup)
            .Include(u => u.Branch)
            .FirstOrDefaultAsync(u => u.MilitaryNumber == id && !u.IsDeleted);
    }

    public void UpdateDirect(User user)
    {
        RepositoryContext.Users.Update(user);
    }

    public async Task<List<UserDto>> GetUsersFilteredAsync(int? branchId, bool isSuperAdmin, int? userBranchId)
    {
        var query = RepositoryContext.Users
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(u => !u.IsDeleted);

        if (!isSuperAdmin)
        {
            // Branch Admin only sees users belonging to their branch
            query = query.Where(u => u.BranchId == userBranchId);
        }
        else if (branchId.HasValue && branchId.Value > 0)
        {
            // SuperAdmin filtering by a specific branch
            query = query.Where(u => u.BranchId == branchId.Value);
        }

        return await query
            .OrderBy(u => u.Role)
            .ThenBy(u => u.MilitaryNumber)
            .Select(u => new UserDto
            {
                MilitaryNumber = u.MilitaryNumber,
                Username = u.Username,
                Email = u.Email,
                Role = u.Role,
                PersonName = u.PersonName,
                UserGroupId = u.UserGroupId,
                UserGroupName = u.UserGroup != null ? u.UserGroup.Name : null,
                BranchId = u.BranchId,
                BranchName = u.Branch != null ? u.Branch.Name : null,
                MustChangePassword = u.MustChangePassword
            })
            .ToListAsync();
    }

    public async Task<UserLoginResult?> GetUserForLoginAsync(string username)
    {
        var cleanUsername = username.Trim();
        return await RepositoryContext.Users
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(u => u.Username == cleanUsername && !u.IsDeleted)
            .Select(u => new UserLoginResult
            {
                MilitaryNumber = u.MilitaryNumber,
                Username = u.Username,
                Email = u.Email,
                PasswordHash = u.PasswordHash,
                Role = u.Role,
                PersonName = u.PersonName,
                UserGroupId = u.UserGroupId,
                UserGroupName = u.UserGroup != null ? u.UserGroup.Name : null,
                BranchId = u.BranchId,
                BranchName = u.Branch != null ? u.Branch.Name : null,
                BranchIsActive = u.Branch == null || u.Branch.IsActive,
                BranchIsDeleted = u.Branch != null && u.Branch.IsDeleted,
                MustChangePassword = u.MustChangePassword
            })
            .FirstOrDefaultAsync();
    }

    public async Task<bool> ExistsAsync(int militaryNumber)
    {
        return await RepositoryContext.Users
            .IgnoreQueryFilters()
            .AsNoTracking()
            .AnyAsync(u => u.MilitaryNumber == militaryNumber && !u.IsDeleted);
    }

    public async Task<int> GetNextMilitaryNumberAsync(int branchId)
    {
        int baseNumber = (branchId * 10000) + 1;
        int maxInRange = (branchId + 1) * 10000;
        var maxMilitary = await RepositoryContext.Users
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(u => u.MilitaryNumber >= baseNumber && u.MilitaryNumber < maxInRange)
            .MaxAsync(u => (int?)u.MilitaryNumber);

        return (maxMilitary ?? (baseNumber - 1)) + 1;
    }

    public async Task<UserDto?> GetUserDtoByIdAsync(int militaryNumber)
    {
        return await RepositoryContext.Users
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(u => u.MilitaryNumber == militaryNumber && !u.IsDeleted)
            .Select(u => new UserDto
            {
                MilitaryNumber = u.MilitaryNumber,
                Username = u.Username,
                Email = u.Email,
                Role = u.Role,
                PersonName = u.PersonName,
                UserGroupId = u.UserGroupId,
                UserGroupName = u.UserGroup != null ? u.UserGroup.Name : null,
                BranchId = u.BranchId,
                BranchName = u.Branch != null ? u.Branch.Name : null,
                MustChangePassword = u.MustChangePassword
            })
            .FirstOrDefaultAsync();
    }
}
