using Entities.Models.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Entities.Models.Databases.PostgresDb.Configurations
{
    public class PostgresSerlogConfiguration : IEntityTypeConfiguration<Serlog>
    {
        public void Configure(EntityTypeBuilder<Serlog> entity)
        {
            entity.Property(e => e.Serts)
                  .HasDefaultValueSql("NOW()");
        }
    }
}
