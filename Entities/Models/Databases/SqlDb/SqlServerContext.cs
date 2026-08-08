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
            optionsBuilder.UseSqlServer(connectionString, b => b.MigrationsAssembly("Entities"));
            optionsBuilder.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var namespaces = new[] { "Entities.Models.Configurations", "Entities.Models.Databases.SqlDb.Configurations" };
        ApplyConfiguration(modelBuilder, namespaces);

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
