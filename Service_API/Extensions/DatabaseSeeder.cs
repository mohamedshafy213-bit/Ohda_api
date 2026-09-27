using Entities.Models.Databases;
using Entities.Models.Enums;
using Entities.Models.Tables;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service_API.Extensions;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(RepositoryContext context)
    {
        try
        {
            // 1. PostgreSQL specific sequence alignment (skipped for SQL Server)
            if (context.Database.IsNpgsql())
            {
                try
                {
                    await context.Database.ExecuteSqlRawAsync(
                        "SELECT setval(pg_get_serial_sequence('branches', 'id'), COALESCE((SELECT MAX(id) FROM branches), 1));"
                    );
                }
                catch { /* ignore */ }
            }

            // 2. Ensure Default Main Branch
            var defaultBranch = await context.Branches.IgnoreQueryFilters()
                .FirstOrDefaultAsync(b => b.Id == 1 || b.Code == "MAIN");

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

            // 3. Ensure SuperAdmin User
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
                    MustChangePassword = true
                };
                superAdmin.PasswordHash = hasher.HashPassword(superAdmin, "P@ssw0rd");
                await context.Users.AddAsync(superAdmin);
                await context.SaveChangesAsync();
            }
            else
            {
                superAdmin.Role = UserRole.SuperAdmin;
                superAdmin.BranchId = null;
                superAdmin.UserGroupId = null;
                await context.SaveChangesAsync();
            }

            // 4. Ensure System Pages (Single Batch Query)
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
                new Page { Title = "أماكن وأرفف التخزين", Path = "/warehouse-bins", Icon = "Layers", SortOrder = 16 },
                new Page { Title = "طباعة الباركود والملصقات", Path = "/barcode-print", Icon = "Printer", SortOrder = 17 }
            };

            var existingPages = await context.Pages.IgnoreQueryFilters()
                .AsNoTracking()
                .ToListAsync();

            var existingPaths = existingPages
                .Where(p => !string.IsNullOrEmpty(p.Path))
                .Select(p => p.Path.ToLower().Trim())
                .ToHashSet();

            var pagesToAdd = defaultPages
                .Where(p => !existingPaths.Contains(p.Path.ToLower().Trim()))
                .ToList();

            if (pagesToAdd.Any())
            {
                await context.Pages.AddRangeAsync(pagesToAdd);
                await context.SaveChangesAsync();
            }

            // 5. Ensure SuperAdmin has user page permissions for all active pages
            var allActivePages = await context.Pages.IgnoreQueryFilters().Where(p => !p.IsDeleted).ToListAsync();
            if (superAdmin != null && allActivePages.Any())
            {
                var existingUserPermPageIds = await context.UserPagePermissions.IgnoreQueryFilters()
                    .Where(up => up.UserId == superAdmin.MilitaryNumber && !up.IsDeleted)
                    .Select(up => up.PageId)
                    .ToListAsync();

                var userPermsToAdd = allActivePages
                    .Where(p => !existingUserPermPageIds.Contains(p.Id))
                    .Select(p => new UserPagePermission
                    {
                        UserId = superAdmin.MilitaryNumber,
                        PageId = p.Id,
                        BranchId = defaultBranch?.Id ?? 1,
                        GrantedByUserId = superAdmin.MilitaryNumber
                    })
                    .ToList();

                if (userPermsToAdd.Any())
                {
                    await context.UserPagePermissions.AddRangeAsync(userPermsToAdd);
                    await context.SaveChangesAsync();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DatabaseSeeder Warning] Seeding encountered non-fatal error: {ex.Message}");
        }
    }
}
