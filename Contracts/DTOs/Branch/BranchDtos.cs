using Contracts.BaseDtos;

namespace Contracts.DTOs.Branch;

public class BranchDto : BaseDto
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? LogoUrl { get; set; }
    public string DefaultLanguage { get; set; } = "ar";
    public string Currency { get; set; } = "SAR";
    public string TimeZone { get; set; } = "Asia/Riyadh";
    public string IndustryTemplate { get; set; } = "General";
    public DateTime CreatedDate { get; set; }
    public DateTime? SuspendedDate { get; set; }
    public int MaxUsers { get; set; }
    public int MaxProducts { get; set; }
    public int MaxStorageMB { get; set; }
    public int CurrentUserCount { get; set; }
    public int CurrentProductCount { get; set; }
}

public class BranchCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string IndustryTemplate { get; set; } = "General";
    public string DefaultLanguage { get; set; } = "ar";
    public string Currency { get; set; } = "SAR";
    public string TimeZone { get; set; } = "Asia/Riyadh";
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public int MaxUsers { get; set; } = 10;
    public int MaxProducts { get; set; } = 1000;
    public int MaxStorageMB { get; set; } = 1024;

    // Initial Branch Admin account creation
    public string AdminUsername { get; set; } = string.Empty;
    public string AdminEmail { get; set; } = string.Empty;
    public string? AdminPersonName { get; set; }
    public int? AdminMilitaryNumber { get; set; }
}

public class BranchUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? LogoUrl { get; set; }
    public string DefaultLanguage { get; set; } = "ar";
    public string Currency { get; set; } = "SAR";
    public string TimeZone { get; set; } = "Asia/Riyadh";
    public string IndustryTemplate { get; set; } = "General";
    public int MaxUsers { get; set; }
    public int MaxProducts { get; set; }
    public int MaxStorageMB { get; set; }
}

public class BranchCreatedResultDto
{
    public BranchDto Branch { get; set; } = null!;
    public string AdminUsername { get; set; } = string.Empty;
    public string TemporaryPassword { get; set; } = string.Empty;
    public int AdminMilitaryNumber { get; set; }
}

public class BranchStatsDto
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string BranchCode { get; set; } = string.Empty;
    public int TotalUsers { get; set; }
    public int TotalProducts { get; set; }
    public int TotalItemsInStock { get; set; }
    public int TotalItemsExited { get; set; }
    public int TotalExitRequests { get; set; }
    public int TotalEntryRequests { get; set; }
    public int MaxUsers { get; set; }
    public int MaxProducts { get; set; }
    public double UserQuotaPercentage => MaxUsers > 0 ? (double)TotalUsers / MaxUsers * 100 : 0;
    public double ProductQuotaPercentage => MaxProducts > 0 ? (double)TotalProducts / MaxProducts * 100 : 0;
}
