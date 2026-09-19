using Entities.Models.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Entities.Models.Configurations;

public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Name).IsRequired().HasMaxLength(150);
        builder.Property(b => b.Code).IsRequired().HasMaxLength(50);
        builder.Property(b => b.DefaultLanguage).IsRequired().HasMaxLength(10).HasDefaultValue("ar");
        builder.Property(b => b.Currency).IsRequired().HasMaxLength(10).HasDefaultValue("SAR");
        builder.Property(b => b.TimeZone).IsRequired().HasMaxLength(50).HasDefaultValue("Asia/Riyadh");
        builder.Property(b => b.IndustryTemplate).IsRequired().HasMaxLength(50).HasDefaultValue("General");

        builder.HasIndex(b => b.Code).IsUnique();
        builder.HasIndex(b => b.IsActive);
        builder.HasIndex(b => b.IsDeleted);
    }
}

public class PlatformAuditLogConfiguration : IEntityTypeConfiguration<PlatformAuditLog>
{
    public void Configure(EntityTypeBuilder<PlatformAuditLog> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Action).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Reason).HasMaxLength(500);
        builder.Property(a => a.IpAddress).HasMaxLength(50);

        builder.HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(a => a.Branch)
            .WithMany()
            .HasForeignKey(a => a.BranchId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(a => a.BranchId);
        builder.HasIndex(a => a.Timestamp);
    }
}
