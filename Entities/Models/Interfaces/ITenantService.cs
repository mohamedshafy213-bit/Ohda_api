namespace Entities.Models.Interfaces;

/// <summary>
/// Service to get the current tenant (university) ID from the HTTP context
/// </summary>
public interface ITenantService
{
    /// <summary>
    /// Gets the current university ID from the authenticated user's claims
    /// </summary>
    int? GetCurrentUniversityId();

    /// <summary>
    /// Checks if tenant filtering should be applied
    /// </summary>
    bool ShouldApplyTenantFilter();
}







