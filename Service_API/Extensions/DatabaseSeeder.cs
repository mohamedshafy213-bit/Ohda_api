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
        // 1. Seed Users if none exist
        if (!await context.Users.AnyAsync())
        {
            var hasher = new PasswordHasher<User>();

            var admin = new User
            {
                Username = "admin",
                Email = "admin@ohda.com",
                Role = UserRole.Admin,
                PersonName = "System Admin"
            };
            admin.PasswordHash = hasher.HashPassword(admin, "Admin@123");

            var employee = new User
            {
                Username = "employee",
                Email = "employee@ohda.com",
                Role = UserRole.Employee,
                PersonName = "John Employee"
            };
            employee.PasswordHash = hasher.HashPassword(employee, "Employee@123");

            var supervisor = new User
            {
                Username = "supervisor",
                Email = "supervisor@ohda.com",
                Role = UserRole.Supervisor,
                PersonName = "Sarah Supervisor"
            };
            supervisor.PasswordHash = hasher.HashPassword(supervisor, "Supervisor@123");

            var manager = new User
            {
                Username = "manager",
                Email = "manager@ohda.com",
                Role = UserRole.Manager,
                PersonName = "Mike Manager"
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

        // 7. Seed UserPagePermissions for default users if none exist
        if (!await context.UserPagePermissions.AnyAsync())
        {
            var users = await context.Users.ToListAsync();
            var pages = await context.Pages.ToListAsync();
            var adminUser = users.FirstOrDefault(u => u.Role == UserRole.Admin);

            if (adminUser != null && pages.Any())
            {
                var permissions = new List<UserPagePermission>();

                foreach (var user in users)
                {
                    foreach (var page in pages)
                    {
                        // Check if role is contained in page's default AllowedRoles string
                        bool isAllowedByRole = page.AllowedRoles
                            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                            .Contains(user.Role.ToString(), StringComparer.OrdinalIgnoreCase);

                        if (isAllowedByRole || user.Role == UserRole.Admin)
                        {
                            permissions.Add(new UserPagePermission
                            {
                                UserId = user.Id,
                                PageId = page.Id,
                                GrantedByUserId = adminUser.Id
                            });
                        }
                    }
                }

                await context.UserPagePermissions.AddRangeAsync(permissions);
                await context.SaveChangesAsync();
            }
        }
    }
}
