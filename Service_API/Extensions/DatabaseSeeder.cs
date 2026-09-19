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
        // Ensure branches identity sequence is aligned with existing data
        try
        {
            await context.Database.ExecuteSqlRawAsync(
                "SELECT setval(pg_get_serial_sequence('branches', 'id'), COALESCE((SELECT MAX(id) FROM branches), 1));"
            );
        }
        catch { /* ignore for non-postgres or if sequence doesn't exist yet */ }

        // -1. Seed Branches
        var branchesToSeed = new List<Branch>
        {
            new Branch
            {
                Name = "الفرع الرئيسي - الرياض",
                Code = "MAIN",
                IndustryTemplate = "MilitaryCustody",
                DefaultLanguage = "ar",
                Currency = "SAR",
                TimeZone = "Asia/Riyadh",
                CreatedDate = DateTime.UtcNow,
                MaxUsers = 25,
                MaxProducts = 10000,
                MaxStorageMB = 10240,
                IsActive = true,
                ContactName = "سلطان القحطاني",
                ContactEmail = "main@ohda.com",
                ContactPhone = "+966500000001"
            },
            new Branch
            {
                Name = "فرع منطقة مكة المكرمة - جدة",
                Code = "JEDDAH",
                IndustryTemplate = "Logistics",
                DefaultLanguage = "ar",
                Currency = "SAR",
                TimeZone = "Asia/Riyadh",
                CreatedDate = DateTime.UtcNow,
                MaxUsers = 5,
                MaxProducts = 3000,
                MaxStorageMB = 4096,
                IsActive = true,
                ContactName = "سالم الأحمدي",
                ContactEmail = "jeddah@ohda.com",
                ContactPhone = "+966501234567"
            },
            new Branch
            {
                Name = "فرع المنطقة الشرقية - الدمام",
                Code = "DAMMAM",
                IndustryTemplate = "Warehouse",
                DefaultLanguage = "ar",
                Currency = "SAR",
                TimeZone = "Asia/Riyadh",
                CreatedDate = DateTime.UtcNow,
                MaxUsers = 8,
                MaxProducts = 5000,
                MaxStorageMB = 4096,
                IsActive = true,
                ContactName = "عبدالله الدوسري",
                ContactEmail = "dammam@ohda.com",
                ContactPhone = "+966507654321"
            },
            new Branch
            {
                Name = "فرع منطقة المدينة المنورة",
                Code = "MADINAH",
                IndustryTemplate = "Retail",
                DefaultLanguage = "ar",
                Currency = "SAR",
                TimeZone = "Asia/Riyadh",
                CreatedDate = DateTime.UtcNow,
                SuspendedDate = DateTime.UtcNow,
                MaxUsers = 10,
                MaxProducts = 2000,
                MaxStorageMB = 2048,
                IsActive = false,
                ContactName = "فهد الحربي",
                ContactEmail = "madinah@ohda.com",
                ContactPhone = "+966509876543"
            }
        };

        var defaultBranch = await context.Branches.IgnoreQueryFilters().FirstOrDefaultAsync(b => b.Id == 1 || b.Code == "MAIN");
        if (defaultBranch == null)
        {
            defaultBranch = branchesToSeed[0];
            await context.Branches.AddAsync(defaultBranch);
            await context.SaveChangesAsync();
        }
        else
        {
            defaultBranch.Name = "الفرع الرئيسي - الرياض";
            defaultBranch.Code = "MAIN";
            defaultBranch.MaxUsers = 25;
            defaultBranch.MaxProducts = 10000;
        }

        foreach (var b in branchesToSeed.Skip(1))
        {
            if (!await context.Branches.IgnoreQueryFilters().AnyAsync(x => x.Code.ToLower() == b.Code.ToLower()))
            {
                await context.Branches.AddAsync(b);
            }
        }
        await context.SaveChangesAsync();

        var jeddahBranch = await context.Branches.IgnoreQueryFilters().FirstOrDefaultAsync(b => b.Code == "JEDDAH");
        var dammamBranch = await context.Branches.IgnoreQueryFilters().FirstOrDefaultAsync(b => b.Code == "DAMMAM");
        var madinahBranch = await context.Branches.IgnoreQueryFilters().FirstOrDefaultAsync(b => b.Code == "MADINAH");

        // -0. Seed SuperAdmin User
        var hasher = new PasswordHasher<User>();
        var existingSuperAdmin = await context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Username == "superadmin");
        if (existingSuperAdmin == null)
        {
            var superAdmin = new User
            {
                MilitaryNumber = 1,
                Username = "superadmin",
                Email = "superadmin@ohda.com",
                Role = UserRole.SuperAdmin,
                PersonName = "مدير المنصة العام (SuperAdmin)",
                BranchId = null,
                MustChangePassword = false
            };
            superAdmin.PasswordHash = hasher.HashPassword(superAdmin, "SuperAdmin@2026!");
            await context.Users.AddAsync(superAdmin);
            await context.SaveChangesAsync();
        }
        else
        {
            existingSuperAdmin.Role = UserRole.SuperAdmin;
            existingSuperAdmin.BranchId = null;
            existingSuperAdmin.PasswordHash = hasher.HashPassword(existingSuperAdmin, "SuperAdmin@2026!");
            await context.SaveChangesAsync();
        }

        // 0. Seed UserGroups for branches if missing
        var groupsToEnsure = new List<UserGroup>
        {
            new UserGroup { BranchId = defaultBranch.Id, Name = "Admins", Description = "Full system administration group" },
            new UserGroup { BranchId = defaultBranch.Id, Name = "Managers", Description = "Inventory managers group" },
            new UserGroup { BranchId = defaultBranch.Id, Name = "Supervisors", Description = "Warehouse supervisors group" },
            new UserGroup { BranchId = defaultBranch.Id, Name = "Employees", Description = "Regular employee staff group" }
        };

        if (jeddahBranch != null)
        {
            groupsToEnsure.Add(new UserGroup { BranchId = jeddahBranch.Id, Name = "إدارة فرع جدة", Description = "مسؤولي فرع جدة" });
            groupsToEnsure.Add(new UserGroup { BranchId = jeddahBranch.Id, Name = "موظفي فرع جدة", Description = "موظفي فرع جدة" });
        }

        if (dammamBranch != null)
        {
            groupsToEnsure.Add(new UserGroup { BranchId = dammamBranch.Id, Name = "إدارة فرع الدمام", Description = "مسؤولي فرع الدمام" });
            groupsToEnsure.Add(new UserGroup { BranchId = dammamBranch.Id, Name = "موظفي فرع الدمام", Description = "موظفي فرع الدمام" });
        }

        if (madinahBranch != null)
        {
            groupsToEnsure.Add(new UserGroup { BranchId = madinahBranch.Id, Name = "إدارة فرع المدينة", Description = "مسؤولي فرع المدينة" });
        }

        foreach (var grp in groupsToEnsure)
        {
            var exists = await context.UserGroups.IgnoreQueryFilters()
                .AnyAsync(g => g.BranchId == grp.BranchId && g.Name == grp.Name);
            if (!exists)
            {
                await context.UserGroups.AddAsync(grp);
            }
        }
        await context.SaveChangesAsync();

        // 1. Seed Users (Dedicated Tenant Admin for each branch + sample branch staff)
        var adminGroup = await context.UserGroups.IgnoreQueryFilters().FirstOrDefaultAsync(g => g.Name == "Admins");
        var managerGroup = await context.UserGroups.IgnoreQueryFilters().FirstOrDefaultAsync(g => g.Name == "Managers");
        var supervisorGroup = await context.UserGroups.IgnoreQueryFilters().FirstOrDefaultAsync(g => g.Name == "Supervisors");
        var employeeGroup = await context.UserGroups.IgnoreQueryFilters().FirstOrDefaultAsync(g => g.Name == "Employees");

        var jeddahAdminGroup = jeddahBranch != null 
            ? await context.UserGroups.IgnoreQueryFilters().FirstOrDefaultAsync(g => g.BranchId == jeddahBranch.Id && g.Name.Contains("إدارة"))
            : null;
        var dammamAdminGroup = dammamBranch != null 
            ? await context.UserGroups.IgnoreQueryFilters().FirstOrDefaultAsync(g => g.BranchId == dammamBranch.Id && g.Name.Contains("إدارة"))
            : null;
        var madinahAdminGroup = madinahBranch != null 
            ? await context.UserGroups.IgnoreQueryFilters().FirstOrDefaultAsync(g => g.BranchId == madinahBranch.Id && g.Name.Contains("إدارة"))
            : null;

        var usersToSeed = new List<User>
        {
            // Branch 1 (Main Riyadh) Admin & Staff
            new User
            {
                MilitaryNumber = 10001,
                Username = "admin",
                Email = "admin@ohda.com",
                Role = UserRole.Admin,
                PersonName = "سليمان الرشيد (مدير فرع الرياض)",
                UserGroupId = adminGroup?.Id,
                BranchId = defaultBranch.Id
            },
            new User
            {
                MilitaryNumber = 10002,
                Username = "manager1",
                Email = "manager1@ohda.com",
                Role = UserRole.Manager,
                PersonName = "أحمد منصور (مدير المستودع)",
                UserGroupId = managerGroup?.Id,
                BranchId = defaultBranch.Id
            },
            new User
            {
                MilitaryNumber = 10003,
                Username = "supervisor1",
                Email = "supervisor1@ohda.com",
                Role = UserRole.Supervisor,
                PersonName = "خالد التميمي (مشرف العمليات)",
                UserGroupId = supervisorGroup?.Id,
                BranchId = defaultBranch.Id
            },
            new User
            {
                MilitaryNumber = 10004,
                Username = "employee1",
                Email = "employee1@ohda.com",
                Role = UserRole.Employee,
                PersonName = "عمر الحربي (أمين عهدة)",
                UserGroupId = employeeGroup?.Id,
                BranchId = defaultBranch.Id
            },
            new User
            {
                MilitaryNumber = 10005,
                Username = "employee2",
                Email = "employee2@ohda.com",
                Role = UserRole.Employee,
                PersonName = "سارة القحطاني (مستلم عهدة)",
                UserGroupId = employeeGroup?.Id,
                BranchId = defaultBranch.Id
            }
        };

        // Add Branch 2 (Jeddah) Admin & Staff
        if (jeddahBranch != null)
        {
            usersToSeed.Add(new User
            {
                MilitaryNumber = 20001,
                Username = "admin_jeddah",
                Email = "admin.jeddah@ohda.com",
                Role = UserRole.Admin,
                PersonName = "سالم الأحمدي (مدير فرع جدة)",
                UserGroupId = jeddahAdminGroup?.Id,
                BranchId = jeddahBranch.Id
            });
            usersToSeed.Add(new User
            {
                MilitaryNumber = 20002,
                Username = "employee_jeddah1",
                Email = "emp1.jeddah@ohda.com",
                Role = UserRole.Employee,
                PersonName = "ماجد الغامدي (أمين عهدة جدة)",
                UserGroupId = null,
                BranchId = jeddahBranch.Id
            });
        }

        // Add Branch 3 (Dammam) Admin & Staff
        if (dammamBranch != null)
        {
            usersToSeed.Add(new User
            {
                MilitaryNumber = 30001,
                Username = "admin_dammam",
                Email = "admin.dammam@ohda.com",
                Role = UserRole.Admin,
                PersonName = "عبدالله الدوسري (مدير فرع الدمام)",
                UserGroupId = dammamAdminGroup?.Id,
                BranchId = dammamBranch.Id
            });
            usersToSeed.Add(new User
            {
                MilitaryNumber = 30002,
                Username = "employee_dammam1",
                Email = "emp1.dammam@ohda.com",
                Role = UserRole.Employee,
                PersonName = "فيصل الخالدي (أمين عهدة الدمام)",
                UserGroupId = null,
                BranchId = dammamBranch.Id
            });
        }

        // Add Branch 4 (Madinah) Admin
        if (madinahBranch != null)
        {
            usersToSeed.Add(new User
            {
                MilitaryNumber = 40001,
                Username = "admin_madinah",
                Email = "admin.madinah@ohda.com",
                Role = UserRole.Admin,
                PersonName = "فهد الحربي (مدير فرع المدينة)",
                UserGroupId = madinahAdminGroup?.Id,
                BranchId = madinahBranch.Id
            });
        }

        foreach (var u in usersToSeed)
        {
            var existingUser = await context.Users.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.MilitaryNumber == u.MilitaryNumber || x.Username == u.Username);
            if (existingUser == null)
            {
                string pass = u.Role switch
                {
                    UserRole.Admin => "Admin@123",
                    UserRole.Manager => "Manager@123",
                    UserRole.Supervisor => "Supervisor@123",
                    _ => "Employee@123"
                };
                u.PasswordHash = hasher.HashPassword(u, pass);
                await context.Users.AddAsync(u);
            }
            else
            {
                if (existingUser.BranchId == null && u.BranchId != null)
                {
                    existingUser.BranchId = u.BranchId;
                }
                if (u.Role == UserRole.Admin)
                {
                    existingUser.Role = UserRole.Admin;
                }
                if (existingUser.UserGroupId == null && u.UserGroupId != null)
                {
                    existingUser.UserGroupId = u.UserGroupId;
                }
            }
        }

        // Backfill any remaining users without branch to default branch (Riyadh)
        var nullBranchUsers = await context.Users.IgnoreQueryFilters()
            .Where(u => u.BranchId == null && u.Role != UserRole.SuperAdmin)
            .ToListAsync();
        foreach (var u in nullBranchUsers)
        {
            u.BranchId = defaultBranch.Id;
        }

        await context.SaveChangesAsync();

        // 2. Seed Departments
        if (!await context.Departments.AnyAsync())
        {
            var depts = new List<Department>
            {
                new Department { Name = "قسم تكنولوجيا المعلومات والشبكات", Description = "مسؤول عن أجهزة الكمبيوتر الخوادم والشبكات" },
                new Department { Name = "إدارة المستودعات واللوجستيات", Description = "مسؤول عن تخزين وصرف العهد والأصناف" },
                new Department { Name = "قسم الصيانة والدعم الفني", Description = "مسؤول عن إصلاح وفحص المعدات" },
                new Department { Name = "الشؤون الإدارية والمالية", Description = "المشتريات والمكتبية والعقود" },
                new Department { Name = "العمليات والميدان", Description = "التجهيزات الميدانية والأجهزة اللاسلكية" },
                new Department { Name = "الخدمات الطبية والإسعاف", Description = "المعدات الطبية والإسعافية" }
            };
            await context.Departments.AddRangeAsync(depts);
            await context.SaveChangesAsync();
        }

        // 3. Seed ProductStates
        if (!await context.ProductStates.AnyAsync())
        {
            var states = new List<ProductState>
            {
                new ProductState { Name = "جديد / ممتاز", Code = "NEW" },
                new ProductState { Name = "مستخدم - بحالة جيدة", Code = "GOOD" },
                new ProductState { Name = "تحت الصيانة", Code = "MAINT" },
                new ProductState { Name = "تالف / غير صالح", Code = "DAMAGED" },
                new ProductState { Name = "مكهن / رجيع", Code = "RETIRED" }
            };
            await context.ProductStates.AddRangeAsync(states);
            await context.SaveChangesAsync();
        }

        // 4. Seed Categories
        if (!await context.Categories.AnyAsync())
        {
            var categories = new List<Category>
            {
                new Category { Name = "أجهزة حاسب آلي ومحمول", Description = "لابتوبات وأجهزة مكتبية وخوادم" },
                new Category { Name = "معدات شبكات واتصالات", Description = "محولات شبكة راوترات ومقويات إشارة" },
                new Category { Name = "أجهزة لاسلكي وتتبع", Description = "أجهزة اتصال يدوي وتتبع جغرافي" },
                new Category { Name = "أجهزة مسح وطباعة باركود", Description = "قارئ الباركود وطابعات الملصقات" },
                new Category { Name = "معدات وتجهيزات ميدانية", Description = "مناظير وأجهزة رؤية وتجهيزات خاصة" },
                new Category { Name = "أثاث وتجهيزات مكتبية", Description = "مكاتب كراسي وخزائن حديدية" }
            };
            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }

        // 5. Seed Suppliers
        if (!await context.Suppliers.AnyAsync())
        {
            var suppliers = new List<Supplier>
            {
                new Supplier { CompanyName = "شركة تقنية المستقبل للحلول الرقمية", ContactName = "م. عبد الله الفهد", Phone = "0501234567", Email = "info@futuretech.sa" },
                new Supplier { CompanyName = "مؤسسة الاتصالات المتقدمة للتجهيزات", ContactName = "أ. سامي طارق", Phone = "0559876543", Email = "sales@advcomm.sa" },
                new Supplier { CompanyName = "شركة التوريدات الوطنية الموحدة", ContactName = "م. فهد العمري", Phone = "0541122334", Email = "contact@natsupplies.sa" },
                new Supplier { CompanyName = "مجموعة الفارابي للمعدات المكتبية", ContactName = "أ. طارق زياد", Phone = "0564455667", Email = "office@alfarabi.sa" }
            };
            await context.Suppliers.AddRangeAsync(suppliers);
            await context.SaveChangesAsync();
        }

        // 6. Seed Products
        if (!await context.Products.AnyAsync())
        {
            var catComp = await context.Categories.FirstOrDefaultAsync(c => c.Name.Contains("حاسب"));
            var catNet = await context.Categories.FirstOrDefaultAsync(c => c.Name.Contains("شبكات"));
            var catRadio = await context.Categories.FirstOrDefaultAsync(c => c.Name.Contains("لاسلكي"));
            var catScan = await context.Categories.FirstOrDefaultAsync(c => c.Name.Contains("مسح"));
            var catTac = await context.Categories.FirstOrDefaultAsync(c => c.Name.Contains("ميدانية"));
            var catFur = await context.Categories.FirstOrDefaultAsync(c => c.Name.Contains("أثاث"));

            var supTech = await context.Suppliers.FirstOrDefaultAsync(s => s.CompanyName.Contains("تقنية المستقبل"));
            var supComm = await context.Suppliers.FirstOrDefaultAsync(s => s.CompanyName.Contains("الاتصالات المتقدمة"));
            var supNat = await context.Suppliers.FirstOrDefaultAsync(s => s.CompanyName.Contains("التوريدات الوطنية"));
            var supFarabi = await context.Suppliers.FirstOrDefaultAsync(s => s.CompanyName.Contains("الفارابي"));

            var products = new List<Product>
            {
                new Product
                {
                    Name = "حاسب محمول Dell Latitude 5540",
                    SKU = "LAP-DELL-5540",
                    Barcode = "628100100001",
                    CategoryId = catComp?.Id ?? 1,
                    SupplierId = supTech?.Id,
                    UnitPrice = 4200,
                    PurchasePrice = 3800,
                    AssetValue = 4200,
                    InventoryType = InventoryType.Purchased
                },
                new Product
                {
                    Name = "حاسب محمول Lenovo ThinkPad T14",
                    SKU = "LAP-LNV-T14",
                    Barcode = "628100100002",
                    CategoryId = catComp?.Id ?? 1,
                    SupplierId = supTech?.Id,
                    UnitPrice = 4800,
                    PurchasePrice = 4300,
                    AssetValue = 4800,
                    InventoryType = InventoryType.Purchased
                },
                new Product
                {
                    Name = "محول شبكة Cisco Catalyst 24P",
                    SKU = "NET-CSCO-2960",
                    Barcode = "628100200001",
                    CategoryId = catNet?.Id ?? 2,
                    SupplierId = supComm?.Id,
                    UnitPrice = 8500,
                    PurchasePrice = 7900,
                    AssetValue = 8500,
                    InventoryType = InventoryType.Purchased
                },
                new Product
                {
                    Name = "جهاز اتصال لاسلكي يدوي Motorola DP4801",
                    SKU = "RAD-MOTO-4801",
                    Barcode = "628100300001",
                    CategoryId = catRadio?.Id ?? 3,
                    SupplierId = supComm?.Id,
                    UnitPrice = 3200,
                    PurchasePrice = 2900,
                    AssetValue = 3200,
                    InventoryType = InventoryType.Owned
                },
                new Product
                {
                    Name = "قارئ باركود ليزري Zebra DS2208",
                    SKU = "SCN-ZB-2208",
                    Barcode = "628100400001",
                    CategoryId = catScan?.Id ?? 4,
                    SupplierId = supNat?.Id,
                    UnitPrice = 1100,
                    PurchasePrice = 950,
                    AssetValue = 1100,
                    InventoryType = InventoryType.Purchased
                },
                new Product
                {
                    Name = "منظار رؤية ليلية تكتيكي Gen3",
                    SKU = "TAC-NVD-G3",
                    Barcode = "628100500001",
                    CategoryId = catTac?.Id ?? 5,
                    SupplierId = supNat?.Id,
                    UnitPrice = 18500,
                    PurchasePrice = 17000,
                    AssetValue = 18500,
                    InventoryType = InventoryType.Owned
                },
                new Product
                {
                    Name = "مكتب إداري فاخر مع كرسيه",
                    SKU = "FUR-DSK-DLX",
                    Barcode = "628100600001",
                    CategoryId = catFur?.Id ?? 6,
                    SupplierId = supFarabi?.Id,
                    UnitPrice = 1500,
                    PurchasePrice = 1200,
                    AssetValue = 1500,
                    InventoryType = InventoryType.Purchased
                },
                new Product
                {
                    Name = "طابعة ليزر متعددة الوظائف HP LaserJet M507",
                    SKU = "PRN-HP-M507",
                    Barcode = "628100400002",
                    CategoryId = catScan?.Id ?? 4,
                    SupplierId = supFarabi?.Id,
                    UnitPrice = 2600,
                    PurchasePrice = 2300,
                    AssetValue = 2600,
                    InventoryType = InventoryType.Purchased
                }
            };
            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }

        // 7. Seed Inventories
        if (!await context.Inventories.AnyAsync())
        {
            var products = await context.Products.ToListAsync();
            var inventoryList = new List<Inventory>();

            foreach (var prod in products)
            {
                int qty = prod.SKU switch
                {
                    "LAP-DELL-5540" => 15,
                    "LAP-LNV-T14" => 10,
                    "NET-CSCO-2960" => 8,
                    "RAD-MOTO-4801" => 25,
                    "SCN-ZB-2208" => 12,
                    "TAC-NVD-G3" => 6,
                    "FUR-DSK-DLX" => 20,
                    "PRN-HP-M507" => 5,
                    _ => 10
                };

                inventoryList.Add(new Inventory
                {
                    ProductId = prod.Id,
                    Quantity = qty,
                    MinStock = Math.Max(1, qty / 4),
                    MaxStock = qty * 2
                });
            }
            await context.Inventories.AddRangeAsync(inventoryList);
            await context.SaveChangesAsync();
        }

        // 8. Seed ProductItems (Serials & QR Codes)
        if (!await context.ProductItems.AnyAsync())
        {
            var inventories = await context.Inventories.Include(i => i.Product).ToListAsync();
            var departments = await context.Departments.ToListAsync();

            var itemsToSeed = new List<ProductItem>();

            foreach (var inv in inventories)
            {
                string sku = inv.Product?.SKU ?? $"PROD-{inv.ProductId}";
                int totalToCreate = inv.Quantity + 3; // Seed available + exited/maintenance units

                for (int i = 1; i <= totalToCreate; i++)
                {
                    string guidHex = Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
                    string serialNumber = $"SN-{sku}-{i:D3}-{guidHex}";
                    string qrCode = $"QR-{serialNumber}";

                    ProductItemStatus status;
                    string? recipientName = null;
                    string? place = null;
                    DateTime? exitDate = null;
                    string? notes = null;

                    if (i <= inv.Quantity)
                    {
                        status = ProductItemStatus.InStock;
                    }
                    else if (i == inv.Quantity + 1)
                    {
                        status = ProductItemStatus.Exited;
                        recipientName = "م. خالد سعيد";
                        place = departments.FirstOrDefault()?.Name ?? "قسم تكنولوجيا المعلومات والشبكات";
                        exitDate = DateTime.UtcNow.AddDays(-10);
                        notes = "صرف عهدة شخصية بموجب الإذن #EX-1001";
                    }
                    else if (i == inv.Quantity + 2)
                    {
                        status = ProductItemStatus.InMaintenance;
                        notes = "إرسال للصيانة الدورية - تبديل قطعة داخلية";
                    }
                    else
                    {
                        status = ProductItemStatus.Damaged;
                        notes = "تلف بسبب ارتفاع التيار الكهربائي أثناء التشغيل";
                    }

                    itemsToSeed.Add(new ProductItem
                    {
                        ProductId = inv.ProductId,
                        SerialNumber = serialNumber,
                        QRCode = qrCode,
                        Status = status,
                        RecipientName = recipientName,
                        Place = place,
                        ExitDate = exitDate,
                        Notes = notes
                    });
                }
            }

            await context.ProductItems.AddRangeAsync(itemsToSeed);
            await context.SaveChangesAsync();
        }

        // 9. Seed Pages if missing
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

        // 10. Seed GroupPagePermissions & Assign Pages to SuperAdmin
        var allSeedGroups = await context.UserGroups.IgnoreQueryFilters().ToListAsync();
        var allSeedPages = await context.Pages.IgnoreQueryFilters().ToListAsync();
        var adminUserForPerm = await context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Role == UserRole.SuperAdmin || u.Role == UserRole.Admin);

        if (allSeedPages.Any() && allSeedGroups.Any())
        {
            foreach (var group in allSeedGroups)
            {
                var allowedPaths = (group.Name.Contains("Admin") || group.Name.Contains("إدارة"))
                    ? allSeedPages.Select(p => p.Path).ToList()
                    : group.Name.Contains("Manager")
                        ? new List<string> { "/dashboard", "/products", "/inventory", "/warehouse-bins", "/exit-requests", "/entry-requests", "/categories", "/suppliers", "/compass", "/departments", "/product-states" }
                        : group.Name.Contains("Supervisor")
                            ? new List<string> { "/dashboard", "/products", "/inventory", "/warehouse-bins", "/exit-requests", "/entry-requests", "/compass" }
                            : group.Name.Contains("Employee") || group.Name.Contains("موظف")
                                ? new List<string> { "/dashboard", "/products", "/exit-requests", "/entry-requests", "/scan", "/compass" }
                                : new List<string>();

                foreach (var page in allSeedPages.Where(p => allowedPaths.Contains(p.Path, StringComparer.OrdinalIgnoreCase)))
                {
                    var exists = await context.GroupPagePermissions.IgnoreQueryFilters()
                        .AnyAsync(gp => gp.UserGroupId == group.Id && gp.PageId == page.Id);
                    if (!exists)
                    {
                        await context.GroupPagePermissions.AddAsync(new GroupPagePermission
                        {
                            UserGroupId = group.Id,
                            PageId = page.Id,
                            BranchId = group.BranchId,
                            GrantedByUserId = adminUserForPerm?.MilitaryNumber ?? 10001
                        });
                    }
                }
            }
            await context.SaveChangesAsync();
        }

        // 10.b Explicitly assign all active pages to SuperAdmin user
        var superAdminForPerm = await context.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Username == "superadmin" || u.Role == UserRole.SuperAdmin);
        if (superAdminForPerm != null && allSeedPages.Any())
        {
            foreach (var page in allSeedPages)
            {
                var hasUserPerm = await context.UserPagePermissions.IgnoreQueryFilters()
                    .AnyAsync(up => up.UserId == superAdminForPerm.MilitaryNumber && up.PageId == page.Id);
                if (!hasUserPerm)
                {
                    await context.UserPagePermissions.AddAsync(new UserPagePermission
                    {
                        UserId = superAdminForPerm.MilitaryNumber,
                        PageId = page.Id,
                        BranchId = defaultBranch.Id,
                        GrantedByUserId = superAdminForPerm.MilitaryNumber
                    });
                }
            }
            await context.SaveChangesAsync();
        }

        // 11. Seed ApprovalConfigs
        var adminGrp = await context.UserGroups.FirstOrDefaultAsync(g => g.Name == "Admins");
        var managerGrp = await context.UserGroups.FirstOrDefaultAsync(g => g.Name == "Managers");
        var supervisorGrp = await context.UserGroups.FirstOrDefaultAsync(g => g.Name == "Supervisors");
        var employeeGrp = await context.UserGroups.FirstOrDefaultAsync(g => g.Name == "Employees");

        var existingConfigs = await context.ApprovalConfigs.ToListAsync();
        if (!existingConfigs.Any())
        {
            var configs = new List<ApprovalConfig>();

            if (employeeGrp != null)
                configs.Add(new ApprovalConfig { RequestType = RequestType.Exit, UserGroupId = employeeGrp.Id, WorkflowRole = WorkflowRole.Requester });
            if (managerGrp != null)
                configs.Add(new ApprovalConfig { RequestType = RequestType.Exit, UserGroupId = managerGrp.Id, WorkflowRole = WorkflowRole.Reviewer });
            if (supervisorGrp != null)
                configs.Add(new ApprovalConfig { RequestType = RequestType.Exit, UserGroupId = supervisorGrp.Id, WorkflowRole = WorkflowRole.Approver });
            if (adminGrp != null)
                configs.Add(new ApprovalConfig { RequestType = RequestType.Exit, UserGroupId = adminGrp.Id, WorkflowRole = WorkflowRole.Approver });

            if (employeeGrp != null)
                configs.Add(new ApprovalConfig { RequestType = RequestType.Entry, UserGroupId = employeeGrp.Id, WorkflowRole = WorkflowRole.Requester });
            if (managerGrp != null)
                configs.Add(new ApprovalConfig { RequestType = RequestType.Entry, UserGroupId = managerGrp.Id, WorkflowRole = WorkflowRole.Reviewer });
            if (supervisorGrp != null)
                configs.Add(new ApprovalConfig { RequestType = RequestType.Entry, UserGroupId = supervisorGrp.Id, WorkflowRole = WorkflowRole.Approver });
            if (adminGrp != null)
                configs.Add(new ApprovalConfig { RequestType = RequestType.Entry, UserGroupId = adminGrp.Id, WorkflowRole = WorkflowRole.Approver });

            await context.ApprovalConfigs.AddRangeAsync(configs);
            await context.SaveChangesAsync();
        }

        // 12. Seed ProductExitRequests & Items
        if (!await context.ProductExitRequests.AnyAsync())
        {
            var emp1 = await context.Users.FirstOrDefaultAsync(u => u.Username == "employee1");
            var emp2 = await context.Users.FirstOrDefaultAsync(u => u.Username == "employee2");
            var sup1 = await context.Users.FirstOrDefaultAsync(u => u.Username == "supervisor1");
            var mngr1 = await context.Users.FirstOrDefaultAsync(u => u.Username == "manager1");

            var deptIT = await context.Departments.FirstOrDefaultAsync(d => d.Name.Contains("تكنولوجيا"));
            var deptOps = await context.Departments.FirstOrDefaultAsync(d => d.Name.Contains("العمليات"));
            var deptMaint = await context.Departments.FirstOrDefaultAsync(d => d.Name.Contains("الصيانة"));
            var deptMed = await context.Departments.FirstOrDefaultAsync(d => d.Name.Contains("الطبية"));

            var prods = await context.Products.ToListAsync();

            var exitReqs = new List<ProductExitRequest>
            {
                new ProductExitRequest
                {
                    RecipientName = "م. خالد سعيد",
                    DepartmentId = deptIT?.Id,
                    Purpose = "تجهيز معمل الحاسب الآلي والشبكات الجديد",
                    RequestedByUserId = emp1?.MilitaryNumber ?? 10001,
                    SupervisorId = sup1?.MilitaryNumber,
                    ManagerId = mngr1?.MilitaryNumber,
                    Status = RequestStatus.Approved,
                    Items = new List<ProductExitRequestItem>
                    {
                        new ProductExitRequestItem { ProductId = prods.ElementAtOrDefault(0)?.Id ?? 1, Quantity = 2, Status = RequestStatus.Approved, Notes = "تسليم أجهزة لابتوب Dell" },
                        new ProductExitRequestItem { ProductId = prods.ElementAtOrDefault(2)?.Id ?? 3, Quantity = 1, Status = RequestStatus.Approved, Notes = "محول شبكة Cisco" }
                    }
                },
                new ProductExitRequest
                {
                    RecipientName = "عمر الحربي",
                    DepartmentId = deptOps?.Id,
                    Purpose = "دورية وتغطية ميدانية طارئة",
                    RequestedByUserId = emp2?.MilitaryNumber ?? 10001,
                    SupervisorId = sup1?.MilitaryNumber,
                    Status = RequestStatus.SupervisorApproved,
                    Items = new List<ProductExitRequestItem>
                    {
                        new ProductExitRequestItem { ProductId = prods.ElementAtOrDefault(3)?.Id ?? 4, Quantity = 3, Status = RequestStatus.SupervisorApproved, Notes = "أجهزة لاسلكي Motorola" },
                        new ProductExitRequestItem { ProductId = prods.ElementAtOrDefault(5)?.Id ?? 6, Quantity = 1, Status = RequestStatus.SupervisorApproved, Notes = "منظار رؤية ليلية" }
                    }
                },
                new ProductExitRequest
                {
                    RecipientName = "أحمد منصور",
                    DepartmentId = deptMaint?.Id,
                    Purpose = "استبدال أجهزة المسح في مستودع الجملة",
                    RequestedByUserId = emp1?.MilitaryNumber ?? 10001,
                    Status = RequestStatus.Pending,
                    Items = new List<ProductExitRequestItem>
                    {
                        new ProductExitRequestItem { ProductId = prods.ElementAtOrDefault(4)?.Id ?? 5, Quantity = 2, Status = RequestStatus.Pending, Notes = "قارئ باركود Zebra" }
                    }
                },
                new ProductExitRequest
                {
                    RecipientName = "سارة القحطاني",
                    DepartmentId = deptMed?.Id,
                    Purpose = "تجهيز عيادة الإسعافات الأولية",
                    RequestedByUserId = emp2?.MilitaryNumber ?? 10001,
                    SupervisorId = sup1?.MilitaryNumber,
                    Status = RequestStatus.Rejected,
                    RejectionReason = "الصنف المطلوب غير متاح حالياً بالكمية المحددة في القسم الطبّي",
                    Items = new List<ProductExitRequestItem>
                    {
                        new ProductExitRequestItem { ProductId = prods.ElementAtOrDefault(1)?.Id ?? 2, Quantity = 5, Status = RequestStatus.Rejected }
                    }
                }
            };

            await context.ProductExitRequests.AddRangeAsync(exitReqs);
            await context.SaveChangesAsync();
        }

        // 13. Seed ProductEntryRequests & Items
        if (!await context.ProductEntryRequests.AnyAsync())
        {
            var emp1 = await context.Users.FirstOrDefaultAsync(u => u.Username == "employee1");
            var emp2 = await context.Users.FirstOrDefaultAsync(u => u.Username == "employee2");
            var sup1 = await context.Users.FirstOrDefaultAsync(u => u.Username == "supervisor1");
            var mngr1 = await context.Users.FirstOrDefaultAsync(u => u.Username == "manager1");

            var deptIT = await context.Departments.FirstOrDefaultAsync(d => d.Name.Contains("تكنولوجيا"));
            var supplierTech = await context.Suppliers.FirstOrDefaultAsync(s => s.CompanyName.Contains("تقنية المستقبل"));
            var supplierNat = await context.Suppliers.FirstOrDefaultAsync(s => s.CompanyName.Contains("التوريدات الوطنية"));
            var newState = await context.ProductStates.FirstOrDefaultAsync(s => s.Code == "NEW");

            var prods = await context.Products.ToListAsync();

            var entryReqs = new List<ProductEntryRequest>
            {
                new ProductEntryRequest
                {
                    FromSource = "توريد إمبادي بموجب العقد #2026-CT",
                    DepartmentId = deptIT?.Id,
                    ProductStateId = newState?.Id,
                    InvoiceNumber = "INV-2026-0091",
                    Notes = "استلام شحنة حواسيب ومحولات جديدة وتم فحصها",
                    ReceivedByUserId = emp1?.MilitaryNumber ?? 10001,
                    SupervisorId = sup1?.MilitaryNumber,
                    ManagerId = mngr1?.MilitaryNumber,
                    Status = RequestStatus.Approved,
                    Items = new List<ProductEntryRequestItem>
                    {
                        new ProductEntryRequestItem { ProductId = prods.ElementAtOrDefault(0)?.Id ?? 1, Quantity = 5, ProductStateId = newState?.Id, Status = RequestStatus.Approved, Notes = "لابتوبات جديدة" },
                        new ProductEntryRequestItem { ProductId = prods.ElementAtOrDefault(2)?.Id ?? 3, Quantity = 2, ProductStateId = newState?.Id, Status = RequestStatus.Approved, Notes = "محولات شبكة جديدة" }
                    }
                },
                new ProductEntryRequest
                {
                    FromSource = "شركة التوريدات الوطنية الموحدة",
                    ProductStateId = newState?.Id,
                    InvoiceNumber = "INV-99231",
                    Notes = "شحنة طابعات ومعدات مكتبية جديدة تحت المعاينة الفنية",
                    ReceivedByUserId = emp2?.MilitaryNumber ?? 10001,
                    Status = RequestStatus.Pending,
                    Items = new List<ProductEntryRequestItem>
                    {
                        new ProductEntryRequestItem { ProductId = prods.ElementAtOrDefault(7)?.Id ?? 8, Quantity = 3, ProductStateId = newState?.Id, Status = RequestStatus.Pending, Notes = "طابعات HP" }
                    }
                },
                new ProductEntryRequest
                {
                    FromSource = "مرتجع من قسم العمليات",
                    Notes = "أجهزة تالفة جزئياً تم رفض استلامها بالمستودع الرئيسي",
                    ReceivedByUserId = emp1?.MilitaryNumber ?? 10001,
                    SupervisorId = sup1?.MilitaryNumber,
                    Status = RequestStatus.Rejected,
                    RejectionReason = "تغليف غير مطابق ومعدات متضررة أثناء النقل",
                    Items = new List<ProductEntryRequestItem>
                    {
                        new ProductEntryRequestItem { ProductId = prods.ElementAtOrDefault(3)?.Id ?? 4, Quantity = 2, Status = RequestStatus.Rejected }
                    }
                }
            };

            await context.ProductEntryRequests.AddRangeAsync(entryReqs);
            await context.SaveChangesAsync();
        }

        // 14. Seed Compass Movement Logs
        if (!await context.Compasses.AnyAsync())
        {
            var depts = await context.Departments.ToListAsync();
            var states = await context.ProductStates.ToListAsync();
            var prods = await context.Products.ToListAsync();

            var compassLogs = new List<Compass>
            {
                new Compass
                {
                    SerialNumber = "SN-LAP-DELL-5540-001-A1B2C3",
                    ProductName = prods.ElementAtOrDefault(0)?.Name ?? "حاسب محمول Dell Latitude 5540",
                    RecipientName = "م. خالد سعيد",
                    Place = depts.ElementAtOrDefault(0)?.Name ?? "قسم تكنولوجيا المعلومات والشبكات",
                    ExitDate = DateTime.UtcNow.AddDays(-15),
                    Type = CompassType.Exit,
                    DepartmentId = depts.ElementAtOrDefault(0)?.Id,
                    ProductStateId = states.FirstOrDefault(s => s.Code == "NEW")?.Id,
                    Notes = "تسليم عهدة كمبيوتر لابتوب للعمل الفني"
                },
                new Compass
                {
                    SerialNumber = "SN-RAD-MOTO-4801-001-X9Y8Z7",
                    ProductName = prods.ElementAtOrDefault(3)?.Name ?? "جهاز اتصال لاسلكي يدوي Motorola",
                    RecipientName = "عمر الحربي",
                    Place = depts.ElementAtOrDefault(4)?.Name ?? "العمليات والميدان",
                    ExitDate = DateTime.UtcNow.AddDays(-7),
                    Type = CompassType.Exit,
                    DepartmentId = depts.ElementAtOrDefault(4)?.Id,
                    ProductStateId = states.FirstOrDefault(s => s.Code == "GOOD")?.Id,
                    Notes = "صرف جهاز لاسلكي للمهمات الخارجية"
                },
                new Compass
                {
                    SerialNumber = "SN-TAC-NVD-G3-001-M4N5O6",
                    ProductName = prods.ElementAtOrDefault(5)?.Name ?? "منظار رؤية ليلية تكتيكي Gen3",
                    RecipientName = "سارة القحطاني",
                    Place = depts.ElementAtOrDefault(4)?.Name ?? "العمليات والميدان",
                    ExitDate = DateTime.UtcNow.AddDays(-3),
                    Type = CompassType.Exit,
                    DepartmentId = depts.ElementAtOrDefault(4)?.Id,
                    ProductStateId = states.FirstOrDefault(s => s.Code == "GOOD")?.Id,
                    Notes = "عهدة مؤقتة لمشروع التغطية الميدانية"
                },
                new Compass
                {
                    SerialNumber = "SN-PRN-HP-M507-001-P1Q2R3",
                    ProductName = prods.ElementAtOrDefault(7)?.Name ?? "طابعة ليزر HP LaserJet",
                    RecipientName = "أحمد منصور",
                    Place = depts.ElementAtOrDefault(3)?.Name ?? "الشؤون الإدارية والمالية",
                    ExitDate = DateTime.UtcNow.AddDays(-1),
                    Type = CompassType.Entry,
                    DepartmentId = depts.ElementAtOrDefault(3)?.Id,
                    ProductStateId = states.FirstOrDefault(s => s.Code == "NEW")?.Id,
                    Notes = "إدخال طابعة جديدة لسجلات المستودع"
                }
            };

            await context.Compasses.AddRangeAsync(compassLogs);
            await context.SaveChangesAsync();
        }

        // 15. Seed Notifications
        if (!await context.Notifications.AnyAsync())
        {
            var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Role == UserRole.Admin);
            var emp1 = await context.Users.FirstOrDefaultAsync(u => u.Username == "employee1");
            var sup1 = await context.Users.FirstOrDefaultAsync(u => u.Username == "supervisor1");

            var notifications = new List<Notification>
            {
                new Notification
                {
                    UserId = adminUser?.MilitaryNumber ?? 10001,
                    Title = "طلب صرف عهدة جديد",
                    Message = "تم إضافة طلب صرف عهدة جديد برقم #ER-1002 ينتظر مراجعتك",
                    Type = NotificationType.ExitRequest,
                    IsRead = false,
                    InsertDate = DateTime.UtcNow.AddHours(-2)
                },
                new Notification
                {
                    UserId = sup1?.MilitaryNumber ?? 10003,
                    Title = "تنبيه حد المخزون الحرج",
                    Message = "تنبيه: صنف (طابعة ليزر HP LaserJet) وصل إلى الحد الأدنى للمخزون (5 أجهزة)",
                    Type = NotificationType.StockAlert,
                    IsRead = false,
                    InsertDate = DateTime.UtcNow.AddHours(-5)
                },
                new Notification
                {
                    UserId = emp1?.MilitaryNumber ?? 10004,
                    Title = "اعتماد طلب توريد",
                    Message = "تمت الموافقة النهائية على طلب التوريد #EN-2001 وإضافة الأصناف للمخزون",
                    Type = NotificationType.EntryRequest,
                    IsRead = true,
                    InsertDate = DateTime.UtcNow.AddDays(-1)
                },
                new Notification
                {
                    UserId = adminUser?.MilitaryNumber ?? 10001,
                    Title = "إشعار نظام",
                    Message = "مرحباً بك في نظام إدارة العهدة والمخزون. جميع الخدمات تعمل بكفاءة عالية.",
                    Type = NotificationType.Info,
                    IsRead = true,
                    InsertDate = DateTime.UtcNow.AddDays(-2)
                }
            };

            await context.Notifications.AddRangeAsync(notifications);
            await context.SaveChangesAsync();
        }

        // 16. Ensure WarehouseBins table & seed sample bins
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
        catch { }

        if (!await context.WarehouseBins.AnyAsync())
        {
            var depts = await context.Departments.ToListAsync();
            var mainDept = depts.FirstOrDefault(d => d.Name.Contains("المستودع") || d.Name.Contains("تقنية")) ?? depts.FirstOrDefault();
            int deptId = mainDept?.Id ?? 1;

            var sampleBins = new List<WarehouseBin>
            {
                new WarehouseBin
                {
                    Code = "A-01-R01",
                    Name = "رف الحواسب المحمولة (Laptops Rack)",
                    Aisle = "A",
                    Shelf = "01",
                    Capacity = 50,
                    DepartmentId = deptId,
                    Description = "مخصص لتخزين حواسيب Dell و ThinkPad المحمولة"
                },
                new WarehouseBin
                {
                    Code = "A-02-R01",
                    Name = "رف معدات الشبكات والراوترات (Network Rack)",
                    Aisle = "A",
                    Shelf = "02",
                    Capacity = 30,
                    DepartmentId = deptId,
                    Description = "مخصص لمحولات سيسكو وموزعات الشبكة"
                },
                new WarehouseBin
                {
                    Code = "B-01-R02",
                    Name = "رف أجهزة الاتصال واللاسلكي (Radio Comms)",
                    Aisle = "B",
                    Shelf = "01",
                    Capacity = 40,
                    DepartmentId = deptId,
                    Description = "أجهزة موتورولا واللاسلكي التكتيكي"
                },
                new WarehouseBin
                {
                    Code = "B-02-R03",
                    Name = "رف الطابعات والماسحات الضوئية (Printers & Scanners)",
                    Aisle = "B",
                    Shelf = "02",
                    Capacity = 20,
                    DepartmentId = deptId,
                    Description = "طابعات HP وماسحات الباركود وملحقاتها"
                },
                new WarehouseBin
                {
                    Code = "C-01-R01",
                    Name = "رف الأجهزة تحت الصيانة والرجيع (Maintenance Bay)",
                    Aisle = "C",
                    Shelf = "01",
                    Capacity = 25,
                    DepartmentId = deptId,
                    Description = "الأجهزة التالفة أو المحولة للصيانة الفنية"
                }
            };

            await context.WarehouseBins.AddRangeAsync(sampleBins);
            await context.SaveChangesAsync();

            // Link existing ProductItems to some bins
            var createdBins = await context.WarehouseBins.ToListAsync();
            var items = await context.ProductItems.Where(i => i.BinId == null).Take(30).ToListAsync();
            if (createdBins.Any() && items.Any())
            {
                for (int i = 0; i < items.Count; i++)
                {
                    items[i].BinId = createdBins[i % createdBins.Count].Id;
                }
                await context.SaveChangesAsync();
            }
        }
    }
}

