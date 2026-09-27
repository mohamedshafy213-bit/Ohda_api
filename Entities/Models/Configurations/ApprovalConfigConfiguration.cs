using Entities.Models.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Entities.Models.Configurations;

public class ApprovalConfigConfiguration : IEntityTypeConfiguration<ApprovalConfig>
{
    public void Configure(EntityTypeBuilder<ApprovalConfig> builder)
    {
        builder.HasKey(a => a.Id);

        builder.HasOne(a => a.Branch)
            .WithMany()
            .HasForeignKey(a => a.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.UserGroup)
            .WithMany()
            .HasForeignKey(a => a.UserGroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
