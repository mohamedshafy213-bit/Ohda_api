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

        builder.HasIndex(p => p.CategoryId);
        builder.HasIndex(p => p.SupplierId);
    }
}

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.MilitaryNumber);
        builder.HasIndex(u => u.Username).IsUnique();
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.Username).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(150);
        builder.Property(u => u.PasswordHash).IsRequired();

        builder.HasOne(u => u.UserGroup)
            .WithMany(g => g.Users)
            .HasForeignKey(u => u.UserGroupId)
            .OnDelete(DeleteBehavior.SetNull);
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
        builder.Property(r => r.Purpose).HasMaxLength(500);

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

        builder.HasIndex(r => r.DepartmentId);
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.InsertDate);
    }
}

public class ProductExitRequestItemConfiguration : IEntityTypeConfiguration<ProductExitRequestItem>
{
    public void Configure(EntityTypeBuilder<ProductExitRequestItem> builder)
    {
        builder.HasKey(i => i.Id);
        
        builder.HasOne(i => i.ProductExitRequest)
            .WithMany(r => r.Items)
            .HasForeignKey(i => i.ProductExitRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Product)
            .WithMany()
            .HasForeignKey(i => i.ProductId)
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

        builder.HasIndex(r => r.DepartmentId);
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.InsertDate);
    }
}

public class ProductEntryRequestItemConfiguration : IEntityTypeConfiguration<ProductEntryRequestItem>
{
    public void Configure(EntityTypeBuilder<ProductEntryRequestItem> builder)
    {
        builder.HasKey(i => i.Id);

        builder.HasOne(i => i.ProductEntryRequest)
            .WithMany(r => r.Items)
            .HasForeignKey(i => i.ProductEntryRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Product)
            .WithMany()
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.ProductState)
            .WithMany()
            .HasForeignKey(i => i.ProductStateId)
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

public class UserGroupConfiguration : IEntityTypeConfiguration<UserGroup>
{
    public void Configure(EntityTypeBuilder<UserGroup> builder)
    {
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Name).IsRequired().HasMaxLength(100);
        builder.Property(g => g.Description).HasMaxLength(250);
    }
}

public class GroupPagePermissionConfiguration : IEntityTypeConfiguration<GroupPagePermission>
{
    public void Configure(EntityTypeBuilder<GroupPagePermission> builder)
    {
        builder.HasKey(p => p.Id);

        builder.HasOne(p => p.UserGroup)
            .WithMany(g => g.GroupPagePermissions)
            .HasForeignKey(p => p.UserGroupId)
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

public class ProductItemConfiguration : IEntityTypeConfiguration<ProductItem>
{
    public void Configure(EntityTypeBuilder<ProductItem> builder)
    {
        builder.HasKey(pi => pi.Id);
        builder.HasIndex(pi => pi.SerialNumber).IsUnique();

        builder.Property(pi => pi.SerialNumber).IsRequired().HasMaxLength(100);
        builder.Property(pi => pi.QRCode).IsRequired().HasMaxLength(500);

        builder.HasOne(pi => pi.Product)
            .WithMany()
            .HasForeignKey(pi => pi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pi => pi.ProductExitRequest)
            .WithMany()
            .HasForeignKey(pi => pi.ProductExitRequestId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class CompassConfiguration : IEntityTypeConfiguration<Compass>
{
    public void Configure(EntityTypeBuilder<Compass> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.SerialNumber).IsRequired().HasMaxLength(100);
        builder.Property(c => c.ProductName).IsRequired().HasMaxLength(200);
        builder.Property(c => c.RecipientName).IsRequired().HasMaxLength(150);
        builder.Property(c => c.Place).IsRequired().HasMaxLength(150);

        builder.HasOne(c => c.ProductExitRequest)
            .WithMany()
            .HasForeignKey(c => c.ProductExitRequestId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(c => c.DepartmentId);
        builder.HasIndex(c => c.InsertDate);
    }
}

