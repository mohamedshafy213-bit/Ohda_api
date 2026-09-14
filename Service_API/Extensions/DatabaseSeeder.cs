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

        // 1. Seed Users (Multi-role coverage for testing)
        var hasher = new PasswordHasher<User>();
        var adminGroup = await context.UserGroups.FirstOrDefaultAsync(g => g.Name == "Admins");
        var managerGroup = await context.UserGroups.FirstOrDefaultAsync(g => g.Name == "Managers");
        var supervisorGroup = await context.UserGroups.FirstOrDefaultAsync(g => g.Name == "Supervisors");
        var employeeGroup = await context.UserGroups.FirstOrDefaultAsync(g => g.Name == "Employees");

        var usersToSeed = new List<User>
        {
            new User
            {
                MilitaryNumber = 10001,
                Username = "admin",
                Email = "admin@ohda.com",
                Role = UserRole.Admin,
                PersonName = "System Admin",
                UserGroupId = adminGroup?.Id
            },
            new User
            {
                MilitaryNumber = 10002,
                Username = "manager1",
                Email = "manager1@ohda.com",
                Role = UserRole.Manager,
                PersonName = "أحمد منصور (مدير المستودع)",
                UserGroupId = managerGroup?.Id
            },
            new User
            {
                MilitaryNumber = 10003,
                Username = "supervisor1",
                Email = "supervisor1@ohda.com",
                Role = UserRole.Supervisor,
                PersonName = "خالد التميمي (مشرف العمليات)",
                UserGroupId = supervisorGroup?.Id
            },
            new User
            {
                MilitaryNumber = 10004,
                Username = "employee1",
                Email = "employee1@ohda.com",
                Role = UserRole.Employee,
                PersonName = "عمر الحربي (أمين عهدة)",
                UserGroupId = employeeGroup?.Id
            },
            new User
            {
                MilitaryNumber = 10005,
                Username = "employee2",
                Email = "employee2@ohda.com",
                Role = UserRole.Employee,
                PersonName = "سارة القحطاني (مستلم عهدة)",
                UserGroupId = employeeGroup?.Id
            }
        };

        foreach (var u in usersToSeed)
        {
            if (!await context.Users.AnyAsync(x => x.MilitaryNumber == u.MilitaryNumber || x.Username == u.Username))
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

        // 9. Seed Pages if none exist or update permissions
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

        // 10. Seed GroupPagePermissions if none exist
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
                            GrantedByUserId = adminUser.MilitaryNumber
                        });
                    }
                }

                await context.GroupPagePermissions.AddRangeAsync(permissions);
                await context.SaveChangesAsync();
            }
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
    }
}

