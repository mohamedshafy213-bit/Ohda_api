using Contracts.BaseDtos;
using Contracts.DTOs.Page;
using Entities.Models.Enums;

namespace Contracts.DTOs.User;

public class UserDto
{
    public int MilitaryNumber { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string? PersonName { get; set; }
    public int? UserGroupId { get; set; }
    public string? UserGroupName { get; set; }
}

public class UserCreateDto
{
    public int MilitaryNumber { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Employee;
    public string? PersonName { get; set; }
    public int? UserGroupId { get; set; }
}

public class UserUpdateDto
{
    public int MilitaryNumber { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Password { get; set; }
    public UserRole Role { get; set; }
    public string? PersonName { get; set; }
    public int? UserGroupId { get; set; }
}

public class LoginDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = null!;
    public List<PageDto> AllowedPages { get; set; } = new();
}
