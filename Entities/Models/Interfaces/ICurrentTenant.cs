namespace Entities.Models.Interfaces;

/// <summary>
/// Scoped tenant service holding the active BranchId, UserId, and SuperAdmin context.
/// </summary>
public interface ICurrentTenant
{
    int? BranchId { get; set; }
    int? UserId { get; set; }
    bool IsSuperAdmin { get; set; }
    bool IgnoreTenantFilter { get; set; }
    void SetTenant(int? branchId, int? userId = null, bool isSuperAdmin = false);
    int EnsureBranchId();
}
