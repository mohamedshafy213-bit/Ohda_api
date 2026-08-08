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
            var managerGroup = await context.UserGroups.FirstOrDefaultAsync(g => g.Name == "Managers");
            var supervisorGroup = await context.UserGroups.FirstOrDefaultAsync(g => g.Name == "Supervisors");
            var employeeGroup = await context.UserGroups.FirstOrDefaultAsync(g => g.Name == "Employees");

            var admin = new User
            {
                Username = "admin",
                Email = "admin@ohda.com",
                Role = UserRole.Admin,
                PersonName = "System Admin",
                UserGroupId = adminGroup?.Id
            };
            admin.PasswordHash = hasher.HashPassword(admin, "Admin@123");

            var employee = new User
            {
                Username = "employee",
                Email = "employee@ohda.com",
                Role = UserRole.Employee,
                PersonName = "John Employee",
                UserGroupId = employeeGroup?.Id
            };
            employee.PasswordHash = hasher.HashPassword(employee, "Employee@123");

            var supervisor = new User
            {
                Username = "supervisor",
                Email = "supervisor@ohda.com",
                Role = UserRole.Supervisor,
                PersonName = "Sarah Supervisor",
                UserGroupId = supervisorGroup?.Id
            };
            supervisor.PasswordHash = hasher.HashPassword(supervisor, "Supervisor@123");

            var manager = new User
            {
                Username = "manager",
                Email = "manager@ohda.com",
                Role = UserRole.Manager,
                PersonName = "Mike Manager",
                UserGroupId = managerGroup?.Id
            };
            manager.PasswordHash = hasher.HashPassword(manager, "Manager@123");

            await context.Users.AddRangeAsync(admin, employee, supervisor, manager);
            await context.SaveChangesAsync();
        }

        // 2. Seed Categories if none exist
        if (!await context.Categories.AnyAsync())
        {
            var categories = new List<Category>
            {
                new Category { Name = "Electronics", Description = "Electronic hardware, laptops, and accessories" },
                new Category { Name = "Office Supplies", Description = "Office furniture, paper, and stationary" },
                new Category { Name = "Warehouse Equipment", Description = "Barcode scanners, pallets, and packing material" }
            };

            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }

        // 3. Seed Suppliers if none exist
        if (!await context.Suppliers.AnyAsync())
        {
            var suppliers = new List<Supplier>
            {
                new Supplier { CompanyName = "TechCorp Global", ContactName = "Alex Rivera", Phone = "+962791112233", Email = "sales@techcorp.com" },
                new Supplier { CompanyName = "LogiQuip Supplies", ContactName = "Maria Garcia", Phone = "+962794445566", Email = "orders@logiquip.com" }
            };

            await context.Suppliers.AddRangeAsync(suppliers);
            await context.SaveChangesAsync();
        }

        // 4. Seed Products if none exist
        if (!await context.Products.AnyAsync())
        {
            var electronicsCat = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Electronics");
            var officeCat = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Office Supplies");
            var warehouseCat = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Warehouse Equipment");

            var techCorp = await context.Suppliers.FirstOrDefaultAsync(s => s.CompanyName == "TechCorp Global");
            var logiQuip = await context.Suppliers.FirstOrDefaultAsync(s => s.CompanyName == "LogiQuip Supplies");

            if (electronicsCat != null && warehouseCat != null && techCorp != null && logiQuip != null)
            {
                var products = new List<Product>
                {
                    new Product
                    {
                        Name = "Dell XPS 15 Laptop",
                        SKU = "DELL-XPS-15",
                        Barcode = "6281000000001",
                        CategoryId = electronicsCat.Id,
                        SupplierId = techCorp.Id,
                        UnitPrice = 1200.00m,
                        InventoryType = InventoryType.Purchased,
                        PurchasePrice = 950.00m
                    },
                    new Product
                    {
                        Name = "Wireless Optical Mouse",
                        SKU = "LOGI-MOU-01",
                        Barcode = "6281000000002",
                        CategoryId = electronicsCat.Id,
                        SupplierId = techCorp.Id,
                        UnitPrice = 25.00m,
                        InventoryType = InventoryType.Purchased,
                        PurchasePrice = 15.00m
                    },
                    new Product
                    {
                        Name = "Handheld Barcode Scanner 2D",
                        SKU = "SCAN-2D-001",
                        Barcode = "6281000000003",
                        CategoryId = warehouseCat.Id,
                        SupplierId = logiQuip.Id,
                        UnitPrice = 150.00m,
                        InventoryType = InventoryType.Owned,
                        AssetValue = 150.00m
                    },
                    new Product
                    {
                        Name = "Ergonomic Mesh Office Chair",
                        SKU = "CHAIR-ERG-01",
                        Barcode = "6281000000004",
                        CategoryId = officeCat?.Id ?? electronicsCat.Id,
                        SupplierId = logiQuip.Id,
                        UnitPrice = 220.00m,
                        InventoryType = InventoryType.Purchased,
                        PurchasePrice = 170.00m
                    }
                };

                await context.Products.AddRangeAsync(products);
                await context.SaveChangesAsync();
            }
        }

        // 5. Seed Inventory if none exist
        if (!await context.Inventories.AnyAsync())
        {
            var products = await context.Products.ToListAsync();
            var inventories = new List<Inventory>();

            foreach (var product in products)
            {
                inventories.Add(new Inventory
                {
                    ProductId = product.Id,
                    Quantity = product.SKU == "DELL-XPS-15" ? 25 : (product.SKU == "SCAN-2D-001" ? 10 : 50),
                    MinStock = 5,
                    MaxStock = 100
                });
            }

            await context.Inventories.AddRangeAsync(inventories);
            await context.SaveChangesAsync();
        }

        // 6. Seed Sample ProductExitRequests if none exist
        if (!await context.ProductExitRequests.AnyAsync())
        {
            var employeeUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "employee");
            var supervisorUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "supervisor");
            var managerUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "manager");
            var product = await context.Products.FirstOrDefaultAsync();

            if (employeeUser != null && product != null)
            {
                var exitRequests = new List<ProductExitRequest>
                {
                    new ProductExitRequest
                    {
                        ProductId = product.Id,
                        RequestedQuantity = 2,
                        RecipientName = "Finance Department",
                        RecipientDepartment = "Finance",
                        Purpose = "New employee onboard setup",
                        Status = RequestStatus.Pending,
                        RequestedByUserId = employeeUser.Id
                    },
                    new ProductExitRequest
                    {
                        ProductId = product.Id,
                        RequestedQuantity = 1,
                        RecipientName = "IT Support",
                        RecipientDepartment = "IT",
                        Purpose = "Replacement hardware",
                        Status = RequestStatus.SupervisorApproved,
                        RequestedByUserId = employeeUser.Id,
                        SupervisorId = supervisorUser?.Id
                    }
                };

                await context.ProductExitRequests.AddRangeAsync(exitRequests);
                await context.SaveChangesAsync();
            }
        }

        // 6.5. Seed Pages if none exist
        if (!await context.Pages.AnyAsync())
        {
            var defaultPages = new List<Page>
            {
                new Page { Id = 1, Title = "Dashboard", Path = "/dashboard", Icon = "LayoutDashboard", SortOrder = 1 },
                new Page { Id = 2, Title = "Products", Path = "/products", Icon = "Package", SortOrder = 2 },
                new Page { Id = 3, Title = "Inventory", Path = "/inventory", Icon = "Boxes", SortOrder = 3 },
                new Page { Id = 4, Title = "Exit Requests", Path = "/exit-requests", Icon = "ArrowUpRight", SortOrder = 4 },
                new Page { Id = 5, Title = "Entry Requests", Path = "/entry-requests", Icon = "ArrowDownLeft", SortOrder = 5 },
                new Page { Id = 6, Title = "Barcode Scan", Path = "/scan", Icon = "QrCode", SortOrder = 6 },
                new Page { Id = 7, Title = "Categories", Path = "/categories", Icon = "Tags", SortOrder = 7 },
                new Page { Id = 8, Title = "Suppliers", Path = "/suppliers", Icon = "Truck", SortOrder = 8 },
                new Page { Id = 9, Title = "Users & Permissions", Path = "/users", Icon = "Users", SortOrder = 9 },
                new Page { Id = 11, Title = "Compass Log", Path = "/compass", Icon = "explore", SortOrder = 10 }
            };

            await context.Pages.AddRangeAsync(defaultPages);
            await context.SaveChangesAsync();
        }
        else
        {
             var hasCompassPage = await context.Pages.AnyAsync(p => p.Id == 11 || p.Path == "/compass");
             if (!hasCompassPage)
             {
                 var compassPage = new Page { Id = 11, Title = "Compass Log", Path = "/compass", Icon = "explore", SortOrder = 10 };
                 await context.Pages.AddAsync(compassPage);
                 await context.SaveChangesAsync();

                 var groups = await context.UserGroups.ToListAsync();
                 var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Role == UserRole.Admin);
                 if (adminUser != null)
                 {
                     foreach (var group in groups)
                     {
                         var hasPerm = await context.GroupPagePermissions.AnyAsync(gpp => gpp.UserGroupId == group.Id && gpp.PageId == 11);
                         if (!hasPerm)
                         {
                             await context.GroupPagePermissions.AddAsync(new GroupPagePermission
                             {
                                 UserGroupId = group.Id,
                                 PageId = 11,
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
                        "Managers" => new List<string> { "/dashboard", "/products", "/inventory", "/exit-requests", "/entry-requests", "/categories", "/suppliers", "/compass" },
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
    }
}
