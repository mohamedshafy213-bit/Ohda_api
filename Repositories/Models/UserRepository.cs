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

    public async Task<User?> GetByUsernameAsync(string username)
    {
        var cleanUsername = username.Trim();
        return await RepositoryContext.Users
            .IgnoreQueryFilters()
            .Include(u => u.UserGroup)
            .Include(u => u.Branch)
            .FirstOrDefaultAsync(u => u.Username == cleanUsername && !u.IsDeleted);
    }

    public async Task<User?> GetByIdWithGroupAsync(int id)
    {
        return await RepositoryContext.Users
            .IgnoreQueryFilters()
            .Include(u => u.UserGroup)
            .Include(u => u.Branch)
            .FirstOrDefaultAsync(u => u.MilitaryNumber == id && !u.IsDeleted);
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
}
