using Entities.Models.BaseTables;

namespace Entities.Models.Tables;

public class Branch : BaseTable
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
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? SuspendedDate { get; set; }
    public int MaxUsers { get; set; } = 10;
    public int MaxProducts { get; set; } = 1000;
    public int MaxStorageMB { get; set; } = 1024;
}
