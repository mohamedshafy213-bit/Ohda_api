using System.ComponentModel.DataAnnotations;
using Entities.Models.Interfaces;

namespace Entities.Models.BaseTables;

public class BaseTable : ISoftDelete
{
    [Key]
    public int Id { get; set; }

    public string? InsertUserCode { get; set; }
    public DateTime? InsertDate { get; set; } = DateTime.UtcNow;
    public string? UpdateUserCode { get; set; }
    public DateTime? LastUpdate { get; set; }
    public bool IsDeleted { get; set; } = false;
    public string? DeleteUserCode { get; set; }
    public DateTime? DeleteDate { get; set; }
}
