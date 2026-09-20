using Entities.Models.Databases;
using Entities.Models.Enums;
using Entities.Models.Tables;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Service_API.Extensions;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(RepositoryContext context)
    {
        // 1. Ensure warehouse_bins table exists safely if running on Postgres
        try
        {
            await context.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS warehouse_bins (
                    id SERIAL PRIMARY KEY,
                    code VARCHAR(100) NOT NULL,
                    name VARCHAR(255) NOT NULL,
                    aisle VARCHAR(50),
                    shelf VARCHAR(50),
                    capacity INT,
                    description TEXT,
                    is_active BOOLEAN DEFAULT TRUE,
                    department_id INT NOT NULL,
                    insert_user_code VARCHAR(100),
                    insert_date TIMESTAMP WITH TIME ZONE,
                    update_user_code VARCHAR(100),
                    last_update TIMESTAMP WITH TIME ZONE,
                    is_deleted BOOLEAN DEFAULT FALSE,
                    delete_user_code VARCHAR(100),
                    delete_date TIMESTAMP WITH TIME ZONE
                );

                ALTER TABLE product_items ADD COLUMN IF NOT EXISTS bin_id INT;
                ALTER TABLE inventories ADD COLUMN IF NOT EXISTS bin_id INT;
                ALTER TABLE product_entry_request_items ADD COLUMN IF NOT EXISTS bin_id INT;
            ");
        }
        catch { /* ignore if not supported by provider */ }

        // 2. Ensure branches identity sequence is aligned (PostgreSQL)
        try
        {
            await context.Database.ExecuteSqlRawAsync(
                "SELECT setval(pg_get_serial_sequence('branches', 'id'), COALESCE((SELECT MAX(id) FROM branches), 1));"
            );
        }
        catch { /* ignore for non-postgres */ }

        // 3. Ensure Default Main Branch (required for tenant foreign key on UserPagePermissions)
        var defaultBranch = await context.Branches.IgnoreQueryFilters().FirstOrDefaultAsync(b => b.Id == 1 || b.Code == "MAIN");
        if (defaultBranch == null)
        {
            defaultBranch = new Branch
            {
                Name = "الفرع الرئيسي - الرياض",
                Code = "MAIN",
                IndustryTemplate = "MilitaryCustody",
                DefaultLanguage = "ar",
                Currency = "SAR",
                TimeZone = "Asia/Riyadh",
                CreatedDate = DateTime.UtcNow,
                MaxUsers = 100,
                MaxProducts = 100000,
                MaxStorageMB = 51200,
                IsActive = true,
                ContactName = "سلطان القحطاني",
                ContactEmail = "main@ohda.com",
                ContactPhone = "+966500000001"
            };
            await context.Branches.AddAsync(defaultBranch);
            await context.SaveChangesAsync();
        }

        // 4. Ensure SuperAdmin User
        var hasher = new PasswordHasher<User>();
        var superAdmin = await context.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Username == "superadmin" || u.Role == UserRole.SuperAdmin || u.MilitaryNumber == 1);

        if (superAdmin == null)
        {
            superAdmin = new User
            {
                MilitaryNumber = 1,
                Username = "superadmin",
                Email = "superadmin@ohda.com",
                Role = UserRole.SuperAdmin,
                PersonName = "مدير المنصة العام (SuperAdmin)",
                BranchId = null,
                UserGroupId = null,
                MustChangePassword = false
            };
            superAdmin.PasswordHash = hasher.HashPassword(superAdmin, "SuperAdmin@2026!");
            await context.Users.AddAsync(superAdmin);
            await context.SaveChangesAsync();
        }
        else
        {
            superAdmin.Role = UserRole.SuperAdmin;
            superAdmin.BranchId = null;
            superAdmin.UserGroupId = null;
            superAdmin.PasswordHash = hasher.HashPassword(superAdmin, "SuperAdmin@2026!");
            await context.SaveChangesAsync();
        }

        // 5. Ensure System Pages
        var defaultPages = new List<Page>
        {
            new Page { Title = "Dashboard", Path = "/dashboard", Icon = "LayoutDashboard", SortOrder = 1 },
            new Page { Title = "Products", Path = "/products", Icon = "Package", SortOrder = 2 },
            new Page { Title = "Inventory", Path = "/inventory", Icon = "Boxes", SortOrder = 3 },
            new Page { Title = "Exit Requests", Path = "/exit-requests", Icon = "ArrowUpRight", SortOrder = 4 },
            new Page { Title = "Entry Requests", Path = "/entry-requests", Icon = "ArrowDownLeft", SortOrder = 5 },
            new Page { Title = "Barcode Scan", Path = "/scan", Icon = "QrCode", SortOrder = 6 },
            new Page { Title = "Categories", Path = "/categories", Icon = "Tags", SortOrder = 7 },
            new Page { Title = "Suppliers", Path = "/suppliers", Icon = "Truck", SortOrder = 8 },
            new Page { Title = "Users & Permissions", Path = "/users", Icon = "Users", SortOrder = 9 },
            new Page { Title = "Compass Log", Path = "/compass", Icon = "explore", SortOrder = 10 },
            new Page { Title = "Departments", Path = "/departments", Icon = "Building", SortOrder = 11 },
            new Page { Title = "Product States", Path = "/product-states", Icon = "Activity", SortOrder = 12 },
            new Page { Title = "Acceptance Settings", Path = "/approval-config", Icon = "Settings", SortOrder = 13 },
            new Page { Title = "إدارة الفروع (Branches)", Path = "/branches", Icon = "Building2", SortOrder = 14 },
            new Page { Title = "لوحة مؤشرات الفروع (Branches Dashboard)", Path = "/branches-dashboard", Icon = "Activity", SortOrder = 15 },
            new Page { Title = "أماكن وأرفف التخزين", Path = "/warehouse-bins", Icon = "Layers", SortOrder = 16 }
        };

        foreach (var p in defaultPages)
        {
            if (!await context.Pages.AnyAsync(x => x.Path.ToLower() == p.Path.ToLower()))
            {
                await context.Pages.AddAsync(p);
            }
        }
        await context.SaveChangesAsync();

        // 6. Assign ALL Active Pages to SuperAdmin User
        var allPages = await context.Pages.IgnoreQueryFilters().Where(p => !p.IsDeleted).ToListAsync();
        if (superAdmin != null && allPages.Any())
        {
            foreach (var page in allPages)
            {
                var hasUserPerm = await context.UserPagePermissions.IgnoreQueryFilters()
                    .AnyAsync(up => up.UserId == superAdmin.MilitaryNumber && up.PageId == page.Id);
                if (!hasUserPerm)
                {
                    await context.UserPagePermissions.AddAsync(new UserPagePermission
                    {
                        UserId = superAdmin.MilitaryNumber,
                        PageId = page.Id,
                        BranchId = defaultBranch?.Id ?? 1,
                        GrantedByUserId = superAdmin.MilitaryNumber
                    });
                }
            }
            await context.SaveChangesAsync();
        }
    }
}
