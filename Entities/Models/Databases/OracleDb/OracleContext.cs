using Entities.Models.Extensions;
using Entities.Models.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Entities.Models.Databases.OracleDb
{
    public partial class OracleContext : RepositoryContext
    {
        public OracleContext(
            IConfiguration configuration,
            IHttpContextAccessor? httpContextAccessor = null,
            ICurrentTenant? currentTenant = null,
            ITenantService? tenantService = null,
            ICurrentBranch? currentBranch = null) 
            : base(configuration, httpContextAccessor, currentTenant, tenantService, currentBranch)
        {
        }

        public OracleContext(
            DbContextOptions<OracleContext> options,
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

            modelBuilder.ApplyGlobalFilters(this);
        }
    }
}
