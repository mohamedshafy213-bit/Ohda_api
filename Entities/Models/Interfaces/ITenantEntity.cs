namespace Entities.Models.Interfaces;

/// <summary>
/// Interface to mark entities that belong to a specific branch/tenant
/// </summary>
public interface ITenantEntity
{
    int BranchId { get; set; }
}
