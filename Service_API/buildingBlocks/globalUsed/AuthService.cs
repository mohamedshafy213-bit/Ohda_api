using Contracts.DTOs.User;
using Entities.Models.Databases;
using Entities.Models.Tables;
using Microsoft.EntityFrameworkCore;
using Service_API.Helpers;

namespace Service_API.buildingBlocks.servicies;

public interface IAuthService
{
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User?> GetUserByIdAsync(int userId);
    Task<bool> ValidatePasswordAsync(string password, string passwordHash, User user);
    Task<bool> UsernameExistsAsync(string username);
}

public class AuthService : IAuthService
{
    private readonly RepositoryContext _repositoryContext;

    public AuthService(RepositoryContext repositoryContext)
    {
        _repositoryContext = repositoryContext;
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _repositoryContext.Users
            .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower() && !u.IsDeleted);
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        return await _repositoryContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
    }

    public async Task<bool> ValidatePasswordAsync(string password, string passwordHash, User user)
    {
        return await Task.FromResult(PasswordHasherHelper.VerifyPassword(user, passwordHash, password));
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await _repositoryContext.Users
            .AnyAsync(u => u.Username.ToLower() == username.ToLower() && !u.IsDeleted);
    }
}