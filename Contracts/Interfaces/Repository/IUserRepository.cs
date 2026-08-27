using Contracts.DTOs.User;
using Contracts.interfaces.Repository;
using Entities.Models.Tables;

namespace Contracts.Interfaces.Repository;

public interface IUserRepository : IRepositoryBase<User, UserDto, UserCreateDto, UserUpdateDto>
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByIdWithGroupAsync(int id);
}
