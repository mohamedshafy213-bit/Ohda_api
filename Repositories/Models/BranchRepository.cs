using Contracts.DTOs.Branch;
using Contracts.enums;
using Contracts.Interfaces.Repository;
using Contracts.Responses;
using Entities.Models.Databases;
using Entities.Models.Enums;
using Entities.Models.Tables;
using LoggerService;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Repositories.Repositories;
using System.Security.Cryptography;

namespace Repositories.Models;

public class BranchRepository : RepositoryBase<Branch, BranchDto, BranchCreateDto, BranchUpdateDto>, IBranchRepository
{
    private readonly PasswordHasher<User> _passwordHasher = new();
    private readonly ILoggerManager _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BranchRepository(
        ILoggerManager logger,
        RepositoryContext repositoryContext,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
        : base(logger, repositoryContext, httpContextAccessor, mapper)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Branch?> GetByIdAsync(int id)
    {
        return await RepositoryContext.Branches
            .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);
    }

    public async Task<Branch?> GetByCodeAsync(string code)
    {
        return await RepositoryContext.Branches
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Code.ToLower() == code.ToLower());
    }

    public async Task<SingleObjectResponseModel> GetStatsAsync(int branchId)
    {
        try
        {
            var branch = await RepositoryContext.Branches
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == branchId);

            if (branch == null)
            {
                return new SingleObjectResponseModel
                {
                    ErrorCode = ErrorCatalog.ObjectNotFound,
                    IsDone = false,
                    ReturnMessage = "Branch not found"
                };
            }

            var totalUsers = await RepositoryContext.Users
                .IgnoreQueryFilters()
                .CountAsync(u => u.BranchId == branchId && !u.IsDeleted);

            var totalProducts = await RepositoryContext.Products
                .IgnoreQueryFilters()
                .CountAsync(p => p.BranchId == branchId && !p.IsDeleted);

            var totalItemsInStock = await RepositoryContext.ProductItems
                .IgnoreQueryFilters()
                .CountAsync(pi => pi.BranchId == branchId && pi.Status == ProductItemStatus.InStock && !pi.IsDeleted);

            var totalItemsExited = await RepositoryContext.ProductItems
                .IgnoreQueryFilters()
                .CountAsync(pi => pi.BranchId == branchId && pi.Status == ProductItemStatus.Exited && !pi.IsDeleted);

            var totalExitRequests = await RepositoryContext.ProductExitRequests
                .IgnoreQueryFilters()
                .CountAsync(r => r.BranchId == branchId && !r.IsDeleted);

            var totalEntryRequests = await RepositoryContext.ProductEntryRequests
                .IgnoreQueryFilters()
                .CountAsync(r => r.BranchId == branchId && !r.IsDeleted);

            var stats = new BranchStatsDto
            {
                BranchId = branch.Id,
                BranchName = branch.Name,
                BranchCode = branch.Code,
                TotalUsers = totalUsers,
                TotalProducts = totalProducts,
                TotalItemsInStock = totalItemsInStock,
                TotalItemsExited = totalItemsExited,
                TotalExitRequests = totalExitRequests,
                TotalEntryRequests = totalEntryRequests,
                MaxUsers = branch.MaxUsers,
                MaxProducts = branch.MaxProducts
            };

            return new SingleObjectResponseModel<BranchStatsDto>
            {
                ErrorCode = ErrorCatalog.noError,
                IsDone = true,
                ReturnMessage = "Stats loaded successfully",
                SingleObject = stats
            };
        }
        catch (Exception ex)
        {
            return new SingleObjectResponseModel
            {
                ErrorCode = ErrorCatalog.DataBaseFauiler,
                IsDone = false,
                ReturnMessage = ex.Message
            };
        }
    }

    public async Task<ListOfObjectsResponseModel<BranchStatsDto>> GetAllBranchStatsAsync()
    {
        try
        {
            var branches = await RepositoryContext.Branches
                .AsNoTracking()
                .Where(b => !b.IsDeleted)
                .OrderBy(b => b.Name)
                .ToListAsync();

            if (!branches.Any())
            {
                return new ListOfObjectsResponseModel<BranchStatsDto>
                {
                    ErrorCode = ErrorCatalog.noError,
                    IsDone = true,
                    ReturnMessage = "Branch stats loaded successfully",
                    Objects = new List<BranchStatsDto>(),
                    TotalCount = 0
                };
            }

            var branchIds = branches.Select(b => b.Id).ToList();
            var userCounts = await GetActiveUserCountsByBranchIdsAsync(branchIds);
            var productCounts = await GetActiveProductCountsByBranchIdsAsync(branchIds);

            var statsList = branches.Select(b => new BranchStatsDto
            {
                BranchId = b.Id,
                BranchName = b.Name,
                BranchCode = b.Code,
                TotalUsers = userCounts.TryGetValue(b.Id, out int uc) ? uc : 0,
                TotalProducts = productCounts.TryGetValue(b.Id, out int pc) ? pc : 0,
                MaxUsers = b.MaxUsers,
                MaxProducts = b.MaxProducts
            }).ToList();

            return new ListOfObjectsResponseModel<BranchStatsDto>
            {
                ErrorCode = ErrorCatalog.noError,
                IsDone = true,
                ReturnMessage = "Branch stats loaded successfully",
                Objects = statsList,
                TotalCount = statsList.Count
            };
        }
        catch (Exception ex)
        {
            return new ListOfObjectsResponseModel<BranchStatsDto>
            {
                ErrorCode = ErrorCatalog.DataBaseFauiler,
                IsDone = false,
                ReturnMessage = ex.Message
            };
        }
    }

    public async Task<SingleObjectResponseModel<BranchCreatedResultDto>> CreateWithInitialAdminAsync(BranchCreateDto dto)
    {
        try
        {
            // 1. Verify Code Uniqueness
            var codeExists = await RepositoryContext.Branches
                .IgnoreQueryFilters()
                .AnyAsync(b => b.Code.ToLower() == dto.Code.Trim().ToLower());

            if (codeExists)
            {
                return new SingleObjectResponseModel<BranchCreatedResultDto>
                {
                    ErrorCode = ErrorCatalog.missingValues,
                    IsDone = false,
                    ReturnMessage = $"Branch with code '{dto.Code}' already exists."
                };
            }

            // 2. Create Branch Entity
            var branch = new Branch
            {
                Name = dto.Name.Trim(),
                Code = dto.Code.Trim().ToUpper(),
                IndustryTemplate = string.IsNullOrWhiteSpace(dto.IndustryTemplate) ? "General" : dto.IndustryTemplate.Trim(),
                DefaultLanguage = string.IsNullOrWhiteSpace(dto.DefaultLanguage) ? "ar" : dto.DefaultLanguage.Trim(),
                Currency = string.IsNullOrWhiteSpace(dto.Currency) ? "SAR" : dto.Currency.Trim(),
                TimeZone = string.IsNullOrWhiteSpace(dto.TimeZone) ? "Asia/Riyadh" : dto.TimeZone.Trim(),
                ContactName = dto.ContactName,
                ContactEmail = dto.ContactEmail,
                ContactPhone = dto.ContactPhone,
                MaxUsers = dto.MaxUsers > 0 ? dto.MaxUsers : 10,
                MaxProducts = dto.MaxProducts > 0 ? dto.MaxProducts : 1000,
                MaxStorageMB = dto.MaxStorageMB > 0 ? dto.MaxStorageMB : 1024,
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };

            await RepositoryContext.Branches.AddAsync(branch);
            await RepositoryContext.SaveChangesAsync();

            // 3. Create Branch Manager User Group for this Branch
            var adminGroup = new UserGroup
            {
                BranchId = branch.Id,
                Name = "مدير الفرع",
                Description = $"مجموعة صلاحيات مدير فرع {branch.Name}"
            };
            await RepositoryContext.UserGroups.AddAsync(adminGroup);
            await RepositoryContext.SaveChangesAsync();

            // Grant operational branch pages to Branch Manager (excluding SuperAdmin platform management pages)
            var branchPages = await RepositoryContext.Pages
                .Where(p => !p.IsDeleted && !p.Path.ToLower().Contains("/branches"))
                .AsNoTracking()
                .ToListAsync();

            var groupPermissions = branchPages.Select(page => new GroupPagePermission
            {
                BranchId = branch.Id,
                UserGroupId = adminGroup.Id,
                PageId = page.Id
            }).ToList();

            await RepositoryContext.GroupPagePermissions.AddRangeAsync(groupPermissions);
            await RepositoryContext.SaveChangesAsync();

            // 4. Generate Branch Admin Account with Default Secure Password (P@ssw0rd)
            const string defaultPassword = "P@ssw0rd";
            var militaryNumber = dto.AdminMilitaryNumber ?? 0;

            if (militaryNumber <= 0)
            {
                int baseNumber = (branch.Id * 10000) + 1;
                int maxInRange = (branch.Id + 1) * 10000;
                var maxNum = await RepositoryContext.Users
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .Where(u => u.MilitaryNumber >= baseNumber && u.MilitaryNumber < maxInRange)
                    .MaxAsync(u => (int?)u.MilitaryNumber);
                militaryNumber = (maxNum ?? (baseNumber - 1)) + 1;
            }

            var adminUser = new User
            {
                MilitaryNumber = militaryNumber,
                Username = dto.AdminUsername.Trim(),
                Email = dto.AdminEmail.Trim(),
                Role = UserRole.Admin,
                PersonName = dto.AdminPersonName ?? $"مدير فرع {branch.Name}",
                BranchId = branch.Id,
                UserGroupId = adminGroup.Id,
                MustChangePassword = true
            };
            adminUser.PasswordHash = _passwordHasher.HashPassword(adminUser, defaultPassword);

            await RepositoryContext.Users.AddAsync(adminUser);
            await RepositoryContext.SaveChangesAsync();

            // 5. Seed Industry Lookups for this new Branch
            await SeedIndustryLookupsAsync(branch.Id, branch.IndustryTemplate);

            var branchDto = branch.Adapt<BranchDto>();
            branchDto.CurrentUserCount = 1;
            branchDto.CurrentProductCount = 0;

            return new SingleObjectResponseModel<BranchCreatedResultDto>
            {
                ErrorCode = ErrorCatalog.noError,
                IsDone = true,
                ReturnMessage = "Branch and Initial Admin created successfully with default password",
                SingleObject = new BranchCreatedResultDto
                {
                    Branch = branchDto,
                    AdminUsername = adminUser.Username,
                    AdminMilitaryNumber = adminUser.MilitaryNumber,
                    TemporaryPassword = defaultPassword
                }
            };
        }
        catch (Exception ex)
        {
            return new SingleObjectResponseModel<BranchCreatedResultDto>
            {
                ErrorCode = ErrorCatalog.DataBaseFauiler,
                IsDone = false,
                ReturnMessage = ex.Message
            };
        }
    }

    private async Task SeedIndustryLookupsAsync(int branchId, string template)
    {
        // Standard product states for the branch
        var states = new List<ProductState>
        {
            new() { BranchId = branchId, Name = "جديد / ممتاز", Code = "NEW" },
            new() { BranchId = branchId, Name = "مستخدم - بحالة جيدة", Code = "GOOD" },
            new() { BranchId = branchId, Name = "تحت الصيانة", Code = "MAINT" },
            new() { BranchId = branchId, Name = "تالف / رجيع", Code = "DAMAGED" }
        };
        await RepositoryContext.ProductStates.AddRangeAsync(states);

        // Standard categories based on template
        var categories = template.ToLower() switch
        {
            "clinic" or "healthcare" => new List<Category>
            {
                new() { BranchId = branchId, Name = "أجهزة ومعدات تشخيصية", Description = "أجهزة السونار والضغط والتخطيط" },
                new() { BranchId = branchId, Name = "مستهلكات ومستلزمات طبية", Description = "ضمادات وشاش وأدوات تعقيم" },
                new() { BranchId = branchId, Name = "أثاث عيادات ومستشفيات", Description = "أسرة كراسي فحص وطاولات" }
            },
            "retail" => new List<Category>
            {
                new() { BranchId = branchId, Name = "بضائع وأصناف معروضة", Description = "السلع الجاهزة للبيع المباشر" },
                new() { BranchId = branchId, Name = "معدات نقاط البيع", Description = "أجهزة كاشير وطابعات فواتير" },
                new() { BranchId = branchId, Name = "تجهيزات العرض والتخزين", Description = "أرفف وستاندات عرض" }
            },
            _ => new List<Category>
            {
                new() { BranchId = branchId, Name = "أجهزة حاسب آلي وإلكترونيات", Description = "أجهزة مكتبية ومحمولة وملحقاتها" },
                new() { BranchId = branchId, Name = "أثاث وتجهيزات مكتبية", Description = "مكاتب وكراسي وخزائن" },
                new() { BranchId = branchId, Name = "معدات وعدد تشغيلية", Description = "معدات صيانة وتشغيل عامة" }
            }
        };
        await RepositoryContext.Categories.AddRangeAsync(categories);

        // Standard warehouse bins
        var defaultDept = new Department
        {
            BranchId = branchId,
            Name = "المستودع الرئيسي",
            Description = "المستودع الرئيسي للفرع"
        };
        await RepositoryContext.Departments.AddAsync(defaultDept);
        await RepositoryContext.SaveChangesAsync();

        var bin = new WarehouseBin
        {
            BranchId = branchId,
            DepartmentId = defaultDept.Id,
            Code = "A-01-01",
            Name = "الرف الرئيسي 01",
            Aisle = "A",
            Shelf = "01",
            IsActive = true
        };
        await RepositoryContext.WarehouseBins.AddAsync(bin);
        await RepositoryContext.SaveChangesAsync();
    }

    public async Task<int> GetActiveUserCountAsync(int branchId)
    {
        return await RepositoryContext.Users
            .IgnoreQueryFilters()
            .CountAsync(u => u.BranchId == branchId && !u.IsDeleted);
    }

    public async Task<int> GetActiveProductCountAsync(int branchId)
    {
        return await RepositoryContext.Products
            .IgnoreQueryFilters()
            .CountAsync(p => p.BranchId == branchId && !p.IsDeleted);
    }

    public async Task<Dictionary<int, int>> GetActiveUserCountsByBranchIdsAsync(IEnumerable<int> branchIds)
    {
        var ids = branchIds.Distinct().ToList();
        if (!ids.Any()) return new Dictionary<int, int>();

        return await RepositoryContext.Users
            .IgnoreQueryFilters()
            .Where(u => u.BranchId.HasValue && ids.Contains(u.BranchId.Value) && !u.IsDeleted)
            .GroupBy(u => u.BranchId!.Value)
            .Select(g => new { BranchId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.BranchId, x => x.Count);
    }

    public async Task<Dictionary<int, int>> GetActiveProductCountsByBranchIdsAsync(IEnumerable<int> branchIds)
    {
        var ids = branchIds.Distinct().ToList();
        if (!ids.Any()) return new Dictionary<int, int>();

        return await RepositoryContext.Products
            .IgnoreQueryFilters()
            .Where(p => ids.Contains(p.BranchId) && !p.IsDeleted)
            .GroupBy(p => p.BranchId)
            .Select(g => new { BranchId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.BranchId, x => x.Count);
    }

    private static string GenerateSecureTempPassword()
    {
        const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lower = "abcdefghijkmnopqrstuvwxyz";
        const string digits = "23456789";
        const string special = "!@#$%&*";

        var bytes = new byte[8];
        RandomNumberGenerator.Fill(bytes);

        return $"{upper[bytes[0] % upper.Length]}{lower[bytes[1] % lower.Length]}{lower[bytes[2] % lower.Length]}{digits[bytes[3] % digits.Length]}{special[bytes[4] % special.Length]}{digits[bytes[5] % digits.Length]}{lower[bytes[6] % lower.Length]}";
    }

    public override async Task<SingleObjectResponseModel> Delete(object key)
    {
        try
        {
            int branchId = Convert.ToInt32(key);
            var branch = await RepositoryContext.Branches
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(b => b.Id == branchId && !b.IsDeleted);

            if (branch == null)
            {
                return new SingleObjectResponseModel
                {
                    ErrorCode = ErrorCatalog.ObjectNotFound,
                    IsDone = false,
                    ReturnMessage = "الفرع غير موجود"
                };
            }

            var now = DateTime.UtcNow;
            var userCode = _httpContextAccessor.HttpContext?.User?.Identity?.Name;

            // 1. Soft delete the branch
            branch.IsDeleted = true;
            branch.DeleteDate = now;
            branch.DeleteUserCode = userCode;

            // 2. Cascade soft delete UserGroups belonging to this branch
            var groups = await RepositoryContext.UserGroups
                .IgnoreQueryFilters()
                .Where(g => g.BranchId == branchId && !g.IsDeleted)
                .ToListAsync();

            foreach (var g in groups)
            {
                g.IsDeleted = true;
                g.DeleteDate = now;
                g.DeleteUserCode = userCode;
            }

            // 3. Cascade soft delete Users belonging to this branch
            var users = await RepositoryContext.Users
                .IgnoreQueryFilters()
                .Where(u => u.BranchId == branchId && !u.IsDeleted)
                .ToListAsync();

            foreach (var u in users)
            {
                u.IsDeleted = true;
                u.DeleteDate = now;
                u.DeleteUserCode = userCode;
            }

            await RepositoryContext.SaveChangesAsync();

            return new SingleObjectResponseModel
            {
                ErrorCode = ErrorCatalog.noError,
                IsDone = true,
                ReturnMessage = "تم حذف الفرع وكافة مجموعاته ومستخدميه بنجاح"
            };
        }
        catch (Exception ex)
        {
            _logger.logErrorWithException(ex, "BranchRepository ===> Delete");
            return new SingleObjectResponseModel
            {
                ErrorCode = ErrorCatalog.DataBaseFauiler,
                IsDone = false,
                ReturnMessage = ex.Message
            };
        }
    }
}
