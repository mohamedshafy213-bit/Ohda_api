using Entities.Models.Extensions;
using Entities.Models.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Entities.Models.Databases.OracleDb
{
    public partial class OracleContext : RepositoryContext
    {
        public OracleContext(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ITenantService? tenantService = null) 
            : base(configuration, httpContextAccessor, tenantService)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = _configuration.GetConnectionString("OracleConnection");
                optionsBuilder.UseOracle(connectionString, b =>
                        b.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion19));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var namespaces = new[] { "Entities.Models.Configurations", "Entities.Models.Databases.OracleDb.Configurations" };
            ApplyConfiguration(modelBuilder, namespaces);

            // Apply global query filters for soft delete and multi-tenancy
            //modelBuilder.ApplyGlobalFilters(_tenantService);

            //modelBuilder.Entity<MEMOS_FROM>()
            //   .Property(e => e.MEMO_ID)
            //   .HasDefaultValueSql("seq_example.NEXTVAL");
        }
    }
}
