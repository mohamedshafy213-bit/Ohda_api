using Entities.Models.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace Entities.Models.Databases.PostgresDb
{
    public partial class PostgresContext : RepositoryContext
    {
        public PostgresContext(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ITenantService? tenantService = null) 
            : base(configuration, httpContextAccessor, tenantService)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = _configuration.GetConnectionString("PostgresConnection");
                optionsBuilder.UseNpgsql(connectionString, b => b.MigrationsAssembly("Entities"))
                              .UseSnakeCaseNamingConvention();
                optionsBuilder.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var namespaces = new[] { "Entities.Models.Configurations", "Entities.Models.Databases.PostgresDb.Configurations" };
            ApplyConfiguration(modelBuilder, namespaces);

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);        
    }
}
