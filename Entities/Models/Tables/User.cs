using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Entities.Models.Enums;
using Entities.Models.Interfaces;

namespace Entities.Models.Tables;

public class User : ISoftDelete
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int MilitaryNumber { get; set; }

    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Employee;
    public string? PersonName { get; set; }

    public int? UserGroupId { get; set; }
    public UserGroup? UserGroup { get; set; }

    // Replicate BaseTable properties directly
    public string? InsertUserCode { get; set; }
    public DateTime? InsertDate { get; set; } = DateTime.UtcNow;
    public string? UpdateUserCode { get; set; }
    public DateTime? LastUpdate { get; set; }
    public bool IsDeleted { get; set; } = false;
    public string? DeleteUserCode { get; set; }
    public DateTime? DeleteDate { get; set; }
}
