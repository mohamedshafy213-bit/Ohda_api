namespace Entities.Models.Interfaces;

/// <summary>
/// Interface to mark entities that belong to a specific tenant (university)
/// </summary>
public interface ITenantEntity
{
    int UniversityId { get; set; }
}







