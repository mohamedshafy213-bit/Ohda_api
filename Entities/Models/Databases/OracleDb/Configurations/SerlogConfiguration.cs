using Entities.Models.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Entities.Models.Configurations;

public class SerlogConfiguration : IEntityTypeConfiguration<Serlog>
{
    public void Configure(EntityTypeBuilder<Serlog> entity)
    {
        entity.HasKey(e => e.Id);
        entity.ToTable("SERLOGS");

        entity.Property(e => e.Id).HasColumnName("ID");
        entity.Property(e => e.Serexception).HasColumnName("SEREXCEPTION");
        entity.Property(e => e.Serlevel).HasMaxLength(15).HasColumnName("SERLEVEL");
        entity.Property(e => e.Sermessage).HasColumnName("SERMESSAGE");
        entity.Property(e => e.Serproperties).HasColumnName("SERPROPERTIES");
        entity.Property(e => e.Sertemplate).HasColumnName("SERTEMPLATE");
        entity.Property(e => e.Sertimestamp).HasMaxLength(100).HasColumnName("SERTIMESTAMP");
        entity.Property(e => e.Serts).HasDefaultValueSql("GETDATE()").HasColumnName("SERTS");
    }
}
