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
        // 0. Seed UserGroups if none exist
        if (!await context.UserGroups.AnyAsync())
        {
            var groups = new List<UserGroup>
            {
                new UserGroup { Name = "Admins", Description = "Full system administration group" },
                new UserGroup { Name = "Managers", Description = "Inventory managers group" },
                new UserGroup { Name = "Supervisors", Description = "Warehouse supervisors group" },
                new UserGroup { Name = "Employees", Description = "Regular employee staff group" }
            };

            await context.UserGroups.AddRangeAsync(groups);
            await context.SaveChangesAsync();
        }

        // 1. Seed Users if none exist
        if (!await context.Users.AnyAsync())
        {
            var hasher = new PasswordHasher<User>();
            var adminGroup = await context.UserGroups.FirstOrDefaultAsync(g => g.Name == "Admins");

            var admin = new User
            {
                Username = "admin",
                Email = "admin@ohda.com",
                Role = UserRole.Admin,
                PersonName = "System Admin",
                UserGroupId = adminGroup?.Id
            };
            admin.PasswordHash = hasher.HashPassword(admin, "Admin@123");

            await context.Users.AddAsync(admin);
            await context.SaveChangesAsync();
        }

        // 6.5. Seed Pages if none exist
        if (!await context.Pages.AnyAsync())
        {
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
                new Page { Title = "Acceptance Settings", Path = "/approval-config", Icon = "Settings", SortOrder = 13 }
            };

            await context.Pages.AddRangeAsync(defaultPages);
            await context.SaveChangesAsync();
        }
        else
        {
             var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Role == UserRole.Admin);
             if (adminUser != null)
             {
                 var pagesToSeed = new List<(string Path, string Title, string Icon, int SortOrder, string[] AllowedGroups)>
                 {
                     ("/compass", "Compass Log", "explore", 10, new[] { "Admins", "Managers", "Supervisors", "Employees" }),
                     ("/departments", "Departments", "Building", 11, new[] { "Admins", "Managers" }),
                     ("/product-states", "Product States", "Activity", 12, new[] { "Admins", "Managers" }),
                     ("/approval-config", "Acceptance Settings", "Settings", 13, new[] { "Admins" })
                 };

                 foreach (var item in pagesToSeed)
                 {
                     var page = await context.Pages.FirstOrDefaultAsync(p => p.Path == item.Path);
                     if (page == null)
                     {
                         page = new Page { Title = item.Title, Path = item.Path, Icon = item.Icon, SortOrder = item.SortOrder };
                         await context.Pages.AddAsync(page);
                         await context.SaveChangesAsync();
                     }

                     var groups = await context.UserGroups.Where(g => item.AllowedGroups.Contains(g.Name)).ToListAsync();
                     foreach (var group in groups)
                     {
                         var hasPerm = await context.GroupPagePermissions.AnyAsync(gpp => gpp.UserGroupId == group.Id && gpp.PageId == page.Id);
                         if (!hasPerm)
                         {
                             await context.GroupPagePermissions.AddAsync(new GroupPagePermission
                             {
                                 UserGroupId = group.Id,
                                 PageId = page.Id,
                                 GrantedByUserId = adminUser.Id
                             });
                         }
                     }
                     await context.SaveChangesAsync();
                 }
             }
        }


        // 7. Seed GroupPagePermissions if none exist
        if (!await context.GroupPagePermissions.AnyAsync())
        {
            var groups = await context.UserGroups.ToListAsync();
            var pages = await context.Pages.ToListAsync();
            var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Role == UserRole.Admin);

            if (adminUser != null && pages.Any() && groups.Any())
            {
                var permissions = new List<GroupPagePermission>();

                foreach (var group in groups)
                {
                    var allowedPaths = group.Name switch
                    {
                        "Admins" => pages.Select(p => p.Path).ToList(),
                        "Managers" => new List<string> { "/dashboard", "/products", "/inventory", "/exit-requests", "/entry-requests", "/categories", "/suppliers", "/compass", "/departments", "/product-states" },
                        "Supervisors" => new List<string> { "/dashboard", "/products", "/inventory", "/exit-requests", "/entry-requests", "/compass" },
                        "Employees" => new List<string> { "/dashboard", "/products", "/exit-requests", "/entry-requests", "/scan", "/compass" },
                        _ => new List<string>()
                    };

                    foreach (var page in pages.Where(p => allowedPaths.Contains(p.Path, StringComparer.OrdinalIgnoreCase)))
                    {
                        permissions.Add(new GroupPagePermission
                        {
                            UserGroupId = group.Id,
                            PageId = page.Id,
                            GrantedByUserId = adminUser.Id
                        });
                    }
                }


                await context.GroupPagePermissions.AddRangeAsync(permissions);
                await context.SaveChangesAsync();
            }
        }

        // 8. Seed ApprovalConfigs if none exist or update them
        var adminGrp = await context.UserGroups.FirstOrDefaultAsync(g => g.Name == "Admins");
        var managerGrp = await context.UserGroups.FirstOrDefaultAsync(g => g.Name == "Managers");
        var supervisorGrp = await context.UserGroups.FirstOrDefaultAsync(g => g.Name == "Supervisors");
        var employeeGrp = await context.UserGroups.FirstOrDefaultAsync(g => g.Name == "Employees");

        var configs = new List<ApprovalConfig>();

        // Exit Configs
        if (employeeGrp != null)
            configs.Add(new ApprovalConfig { RequestType = RequestType.Exit, UserGroupId = employeeGrp.Id, WorkflowRole = WorkflowRole.Requester });
        if (managerGrp != null)
            configs.Add(new ApprovalConfig { RequestType = RequestType.Exit, UserGroupId = managerGrp.Id, WorkflowRole = WorkflowRole.Reviewer });
        if (supervisorGrp != null)
            configs.Add(new ApprovalConfig { RequestType = RequestType.Exit, UserGroupId = supervisorGrp.Id, WorkflowRole = WorkflowRole.Approver });
        if (adminGrp != null)
            configs.Add(new ApprovalConfig { RequestType = RequestType.Exit, UserGroupId = adminGrp.Id, WorkflowRole = WorkflowRole.Approver });

        // Entry Configs
        if (employeeGrp != null)
            configs.Add(new ApprovalConfig { RequestType = RequestType.Entry, UserGroupId = employeeGrp.Id, WorkflowRole = WorkflowRole.Requester });
        if (managerGrp != null)
            configs.Add(new ApprovalConfig { RequestType = RequestType.Entry, UserGroupId = managerGrp.Id, WorkflowRole = WorkflowRole.Reviewer });
        if (supervisorGrp != null)
            configs.Add(new ApprovalConfig { RequestType = RequestType.Entry, UserGroupId = supervisorGrp.Id, WorkflowRole = WorkflowRole.Approver });
        if (adminGrp != null)
            configs.Add(new ApprovalConfig { RequestType = RequestType.Entry, UserGroupId = adminGrp.Id, WorkflowRole = WorkflowRole.Approver });

        var existingConfigs = await context.ApprovalConfigs.ToListAsync();
        if (existingConfigs.Any())
        {
            context.ApprovalConfigs.RemoveRange(existingConfigs);
            await context.SaveChangesAsync();
        }

        await context.ApprovalConfigs.AddRangeAsync(configs);
        await context.SaveChangesAsync();

        // 9. Sync ProductItems with Inventories (Self-healing database alignment)
        var inventories = await context.Inventories.Include(i => i.Product).ToListAsync();
        foreach (var inv in inventories)
        {
            var inStockCount = await context.ProductItems.CountAsync(pi => pi.ProductId == inv.ProductId && pi.Status == ProductItemStatus.InStock && !pi.IsDeleted);
            int missingCount = inv.Quantity - inStockCount;
            if (missingCount > 0)
            {
                string sku = inv.Product?.SKU ?? $"P{inv.ProductId}";
                for (int u = 1; u <= missingCount; u++)
                {
                    string guidSuffix = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
                    string serialNumber = $"SN-{sku}-{inStockCount + u}-{guidSuffix}";
                    string qrCode = $"QR-{serialNumber}";

                    await context.ProductItems.AddAsync(new ProductItem
                    {
                        ProductId = inv.ProductId,
                        SerialNumber = serialNumber,
                        QRCode = qrCode,
                        Status = ProductItemStatus.InStock
                    });
                }
            }
        }
        await context.SaveChangesAsync();
    }
}
