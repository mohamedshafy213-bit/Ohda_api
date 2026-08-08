namespace Entities.Models.Interfaces;

/// <summary>
/// Interface to mark entities that support soft delete
/// </summary>
public interface ISoftDelete
{
    bool IsDeleted { get; set; }
}







