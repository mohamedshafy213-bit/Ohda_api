using Entities.Models.BaseTables;
using Entities.Models.Enums;

namespace Entities.Models.Tables;

public class User : BaseTable
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Employee;
    public string? PersonName { get; set; }
}
