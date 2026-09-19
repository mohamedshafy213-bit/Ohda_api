using Entities.Models.BaseTables;
using Entities.Models.Extensions;
using Entities.Models.Interfaces;
using Entities.Models.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace Entities.Models.Databases;

public class RepositoryContext : DbContext
{
    public virtual DbSet<Branch> Branches { get; set; } = null!;
    public virtual DbSet<PlatformAuditLog> PlatformAuditLogs { get; set; } = null!;
    public virtual DbSet<User> Users { get; set; } = null!;
    public virtual DbSet<Category> Categories { get; set; } = null!;
    public virtual DbSet<Supplier> Suppliers { get; set; } = null!;
    public virtual DbSet<Product> Products { get; set; } = null!;
    public virtual DbSet<Inventory> Inventories { get; set; } = null!;
    public virtual DbSet<Order> Orders { get; set; } = null!;
    public virtual DbSet<OrderDetail> OrderDetails { get; set; } = null!;
    public virtual DbSet<ScanTransaction> ScanTransactions { get; set; } = null!;
    public virtual DbSet<ProductExitRequest> ProductExitRequests { get; set; } = null!;
    public virtual DbSet<ProductEntryRequest> ProductEntryRequests { get; set; } = null!;
    public virtual DbSet<ProductExitRequestItem> ProductExitRequestItems { get; set; } = null!;
    public virtual DbSet<ProductEntryRequestItem> ProductEntryRequestItems { get; set; } = null!;
    public virtual DbSet<Page> Pages { get; set; } = null!;
    public virtual DbSet<UserPagePermission> UserPagePermissions { get; set; } = null!;
    public virtual DbSet<Notification> Notifications { get; set; } = null!;
    public virtual DbSet<UserGroup> UserGroups { get; set; } = null!;
    public virtual DbSet<GroupPagePermission> GroupPagePermissions { get; set; } = null!;
    public virtual DbSet<ProductItem> ProductItems { get; set; } = null!;
    public virtual DbSet<Compass> Compasses { get; set; } = null!;
    public virtual DbSet<ApprovalConfig> ApprovalConfigs { get; set; } = null!;
    public virtual DbSet<Department> Departments { get; set; } = null!;
    public virtual DbSet<ProductState> ProductStates { get; set; } = null!;
    public virtual DbSet<WarehouseBin> WarehouseBins { get; set; } = null!;

    protected readonly IConfiguration _configuration;
    protected readonly IHttpContextAccessor _httpContextAccessor;
    protected readonly ITenantService? _tenantService;
    protected readonly ICurrentBranch? _currentBranch;

    public int? CurrentBranchId
    {
        get
        {
            if (_httpContextAccessor == null) return null;
            var id = GlobalQueryFilterExtensions.ResolveBranchId(_httpContextAccessor);
            return id != 0 ? id : null;
        }
    }

    public int BranchFilterId => CurrentBranchId ?? 0;

    public bool HasSupportAccess => _httpContextAccessor != null && GlobalQueryFilterExtensions.CheckSupportAccess(_httpContextAccessor);

    public RepositoryContext(
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor,
        ITenantService? tenantService = null,
        ICurrentBranch? currentBranch = null)
    {
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _tenantService = tenantService;
        _currentBranch = currentBranch;
    }

    public RepositoryContext(
        DbContextOptions options,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor,
        ITenantService? tenantService = null,
        ICurrentBranch? currentBranch = null)
        : base(options)
    {
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _tenantService = tenantService;
        _currentBranch = currentBranch;
    }

    protected void ApplyConfiguration(ModelBuilder modelBuilder, string[] namespaces)
    {
        var typesToRegister = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.GetInterfaces().Any(gi => gi.IsGenericType && gi.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>)))
            .Where(t => namespaces.Contains(t.Namespace))
            .OrderBy(t => Array.IndexOf(namespaces, t.Namespace))
            .ToList();

        foreach (var type in typesToRegister)
        {
            dynamic configurationInstance = Activator.CreateInstance(type)!;
            modelBuilder.ApplyConfiguration(configurationInstance);
        }
    }

    private void EnforceTenantSecurityAndAudit()
    {
        var entries = ChangeTracker.Entries().ToList();
        var currentBranchId = CurrentBranchId;
        var userCode = _httpContextAccessor?.HttpContext?.User?.Identity?.Name;

        foreach (var entry in entries)
        {
            if (entry.Entity is ITenantEntity tenantEntity)
            {
                if (entry.State == EntityState.Added)
                {
                    if (tenantEntity.BranchId == 0)
                    {
                        tenantEntity.BranchId = currentBranchId.HasValue && currentBranchId.Value > 0 ? currentBranchId.Value : 1;
                    }
                    else if (currentBranchId.HasValue && currentBranchId.Value > 0 && !GlobalQueryFilterExtensions.CheckSupportAccess(_httpContextAccessor!))
                    {
                        if (tenantEntity.BranchId != currentBranchId.Value)
                        {
                            throw new InvalidOperationException($"Cross-tenant creation forbidden. Target BranchId {tenantEntity.BranchId} does not match active branch context {currentBranchId.Value}.");
                        }
                    }
                }
                else if (entry.State == EntityState.Modified)
                {
                    var branchIdProp = entry.Property(nameof(ITenantEntity.BranchId));
                    if (branchIdProp.IsModified && !Equals(branchIdProp.OriginalValue, branchIdProp.CurrentValue))
                    {
                        throw new InvalidOperationException("Altering BranchId of an existing record is prohibited.");
                    }
                }
            }

            if (entry.Entity is ISoftDelete softDelete)
            {
                if (entry.State == EntityState.Added)
                {
                    if (entry.Entity is BaseTable baseTable)
                    {
                        baseTable.InsertDate ??= DateTime.UtcNow;
                        baseTable.InsertUserCode ??= userCode;
                    }
                }
                else if (entry.State == EntityState.Modified)
                {
                    if (entry.Entity is BaseTable baseTable)
                    {
                        baseTable.LastUpdate = DateTime.UtcNow;
                        baseTable.UpdateUserCode = userCode;
                    }
                }
                else if (entry.State == EntityState.Deleted)
                {
                    entry.State = EntityState.Modified;
                    softDelete.IsDeleted = true;
                    if (entry.Entity is BaseTable baseTable)
                    {
                        baseTable.DeleteDate = DateTime.UtcNow;
                        baseTable.DeleteUserCode = userCode;
                    }
                }
            }
        }
    }

    public override int SaveChanges()
    {
        EnforceTenantSecurityAndAudit();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        EnforceTenantSecurityAndAudit();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        EnforceTenantSecurityAndAudit();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        EnforceTenantSecurityAndAudit();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
}
