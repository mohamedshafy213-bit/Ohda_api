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
        return await RepositoryContext.Users
            .Include(u => u.UserGroup)
            .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower() && !u.IsDeleted);
    }

    public async Task<User?> GetByIdWithGroupAsync(int id)
    {
        return await RepositoryContext.Users
            .Include(u => u.UserGroup)
            .FirstOrDefaultAsync(u => u.MilitaryNumber == id && !u.IsDeleted);
    }
}
