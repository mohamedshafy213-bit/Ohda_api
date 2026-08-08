using Entities.Models.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Entities.Models.Configurations;

public class PageConfiguration : IEntityTypeConfiguration<Page>
{
    public void Configure(EntityTypeBuilder<Page> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Title).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Path).IsRequired().HasMaxLength(150);

        builder.HasData(
            new Page { Id = 1, Title = "Dashboard", Path = "/dashboard", Icon = "dashboard", SortOrder = 1 },
            new Page { Id = 2, Title = "Products", Path = "/products", Icon = "inventory_2", SortOrder = 2 },
            new Page { Id = 3, Title = "Inventory Stock", Path = "/inventory", Icon = "warehouse", SortOrder = 3 },
            new Page { Id = 4, Title = "Scan Barcode", Path = "/scan", Icon = "qr_code_scanner", SortOrder = 4 },
            new Page { Id = 5, Title = "Product Exit Requests", Path = "/exit-requests", Icon = "assignment_return", SortOrder = 5 },
            new Page { Id = 6, Title = "Orders", Path = "/orders", Icon = "shopping_cart", SortOrder = 6 },
            new Page { Id = 7, Title = "Categories", Path = "/categories", Icon = "category", SortOrder = 7 },
            new Page { Id = 8, Title = "Suppliers", Path = "/suppliers", Icon = "local_shipping", SortOrder = 8 },
            new Page { Id = 9, Title = "User Management", Path = "/users", Icon = "group", SortOrder = 9 },
            new Page { Id = 11, Title = "Compass Log", Path = "/compass", Icon = "explore", SortOrder = 10 }
        );
    }
}
