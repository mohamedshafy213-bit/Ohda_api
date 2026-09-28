using Entities.Models.Extensions;
using Entities.Models.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace Entities.Models.Databases.SqlDb;

public partial class SqlServerContext : RepositoryContext
{
    public SqlServerContext(
        IConfiguration configuration,
        IHttpContextAccessor? httpContextAccessor = null,
        ICurrentTenant? currentTenant = null,
        ITenantService? tenantService = null,
        ICurrentBranch? currentBranch = null)
        : base(configuration, httpContextAccessor, currentTenant, tenantService, currentBranch)
    {
    }

    public SqlServerContext(
        DbContextOptions<SqlServerContext> options,
        IConfiguration configuration,
        IHttpContextAccessor? httpContextAccessor = null,
        ICurrentTenant? currentTenant = null,
        ITenantService? tenantService = null,
        ICurrentBranch? currentBranch = null)
        : base(options, configuration, httpContextAccessor, currentTenant, tenantService, currentBranch)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));

        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = _configuration.GetConnectionString("SqlServerConnection");
            optionsBuilder.UseSqlServer(connectionString, b =>
            {
                b.MigrationsAssembly("Entities");
                b.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(3), errorNumbersToAdd: null);
            });
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var namespaces = new[] { "Entities.Models.Configurations", "Entities.Models.Databases.SqlDb.Configurations" };
        ApplyConfiguration(modelBuilder, namespaces);

        // Prevent multiple cascade paths in SQL Server for all tenant entities referencing Branch
        foreach (var foreignKey in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            if (foreignKey.PrincipalEntityType.ClrType == typeof(Entities.Models.Tables.Branch))
            {
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }

        modelBuilder.ApplyGlobalFilters(this);

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
