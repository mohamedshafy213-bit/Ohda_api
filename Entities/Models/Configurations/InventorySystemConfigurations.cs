using Entities.Models.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Entities.Models.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => p.Barcode).IsUnique();
        builder.HasIndex(p => p.SKU).IsUnique();

        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.SKU).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Barcode).IsRequired().HasMaxLength(100);

        builder.Property(p => p.UnitPrice).HasPrecision(18, 2);
        builder.Property(p => p.PurchasePrice).HasPrecision(18, 2);
        builder.Property(p => p.AssetValue).HasPrecision(18, 2);

        builder.HasOne(p => p.Category)
            .WithMany()
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Supplier)
            .WithMany()
            .HasForeignKey(p => p.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        builder.HasIndex(u => u.Username).IsUnique();
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.Username).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(150);
        builder.Property(u => u.PasswordHash).IsRequired();
    }
}

public class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
{
    public void Configure(EntityTypeBuilder<Inventory> builder)
    {
        builder.HasKey(i => i.Id);

        builder.HasOne(i => i.Product)
            .WithMany()
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.TotalAmount).HasPrecision(18, 2);

        builder.HasOne(o => o.User)
            .WithMany()
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.OrderDetails)
            .WithOne(d => d.Order)
            .HasForeignKey(d => d.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class OrderDetailConfiguration : IEntityTypeConfiguration<OrderDetail>
{
    public void Configure(EntityTypeBuilder<OrderDetail> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.UnitPrice).HasPrecision(18, 2);

        builder.HasOne(d => d.Product)
            .WithMany()
            .HasForeignKey(d => d.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ProductExitRequestConfiguration : IEntityTypeConfiguration<ProductExitRequest>
{
    public void Configure(EntityTypeBuilder<ProductExitRequest> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.RecipientName).IsRequired().HasMaxLength(150);
        builder.Property(r => r.RecipientDepartment).HasMaxLength(150);
        builder.Property(r => r.Purpose).HasMaxLength(500);

        builder.HasOne(r => r.Product)
            .WithMany()
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.RequestedByUser)
            .WithMany()
            .HasForeignKey(r => r.RequestedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Supervisor)
            .WithMany()
            .HasForeignKey(r => r.SupervisorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Manager)
            .WithMany()
            .HasForeignKey(r => r.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ProductEntryRequestConfiguration : IEntityTypeConfiguration<ProductEntryRequest>
{
    public void Configure(EntityTypeBuilder<ProductEntryRequest> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.FromSource).IsRequired().HasMaxLength(200);
        builder.Property(r => r.InvoiceNumber).HasMaxLength(100);
        builder.Property(r => r.Notes).HasMaxLength(500);

        builder.HasOne(r => r.Product)
            .WithMany()
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.ReceivedByUser)
            .WithMany()
            .HasForeignKey(r => r.ReceivedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Supervisor)
            .WithMany()
            .HasForeignKey(r => r.SupervisorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Manager)
            .WithMany()
            .HasForeignKey(r => r.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class UserPagePermissionConfiguration : IEntityTypeConfiguration<UserPagePermission>
{
    public void Configure(EntityTypeBuilder<UserPagePermission> builder)
    {
        builder.HasKey(p => p.Id);

        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Page)
            .WithMany()
            .HasForeignKey(p => p.PageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.GrantedByUser)
            .WithMany()
            .HasForeignKey(p => p.GrantedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(n => n.Id);

        builder.Property(n => n.Title).IsRequired().HasMaxLength(200);
        builder.Property(n => n.Message).IsRequired().HasMaxLength(1000);

        builder.HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
