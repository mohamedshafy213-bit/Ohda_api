using Entities.Models.Extensions;
using Entities.Models.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace Entities.Models.Databases.SqlDb;

public partial class SqlServerContext : RepositoryContext
{
    public SqlServerContext(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ITenantService? tenantService = null)
        : base(configuration, httpContextAccessor, tenantService)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = _configuration.GetConnectionString("SqlServerConnection");
            optionsBuilder.UseSqlServer(connectionString, b =>
            {
                b.MigrationsAssembly("Entities");
                b.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null);
            });
            optionsBuilder.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
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

        modelBuilder.ApplyGlobalFilters(_httpContextAccessor);

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
