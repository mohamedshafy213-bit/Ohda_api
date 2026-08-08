using Entities.Models.Interfaces;
using Entities.Models.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace Entities.Models.Databases;

public class RepositoryContext : DbContext
{
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
    public virtual DbSet<Page> Pages { get; set; } = null!;
    public virtual DbSet<UserPagePermission> UserPagePermissions { get; set; } = null!;
    public virtual DbSet<Notification> Notifications { get; set; } = null!;

    protected readonly IConfiguration _configuration;
    protected readonly IHttpContextAccessor _httpContextAccessor;
    protected readonly ITenantService? _tenantService;

    public RepositoryContext(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ITenantService? tenantService = null)
    {
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _tenantService = tenantService;
    }

    protected void ApplyConfiguration(ModelBuilder modelBuilder, string[] namespaces)
    {
        var typesToRegister = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.GetInterfaces().Any(gi => gi.IsGenericType && gi.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>)))
            .Where(t => namespaces.Contains(t.Namespace))
            .ToList();

        foreach (var type in typesToRegister)
        {
            dynamic configurationInstance = Activator.CreateInstance(type)!;
            modelBuilder.ApplyConfiguration(configurationInstance);
        }
    }
}
