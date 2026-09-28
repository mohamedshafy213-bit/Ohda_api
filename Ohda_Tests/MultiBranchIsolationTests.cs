using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Entities.Models.Databases;
using Entities.Models.Tables;
using Entities.Models.Extensions;
using Entities.Models.Interfaces;
using Entities.Models.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Service_API.Services;

namespace Ohda_Tests;

public class MultiBranchIsolationTests
{
    private class TestDbContext : RepositoryContext
    {
        public TestDbContext(
            DbContextOptions options,
            IConfiguration config,
            ICurrentTenant? currentTenant = null)
            : base(options, config, null, currentTenant, null, null)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyGlobalFilters(this);
        }
    }

    private (TestDbContext context, CurrentBranchService tenantService, IHttpContextAccessor httpAccessor) CreateTestEnvironment(
        int? branchId = null,
        bool isSuperAdmin = false,
        string? headerBranchId = null,
        string? headerSupportAccess = null,
        string? dbName = null)
    {
        dbName ??= Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<RepositoryContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        var httpAccessor = new HttpContextAccessor();
        var claims = new List<Claim>();

        if (branchId.HasValue)
        {
            claims.Add(new Claim("branch_id", branchId.Value.ToString()));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, "42"));
        }

        if (isSuperAdmin)
        {
            claims.Add(new Claim(ClaimTypes.Role, "SuperAdmin"));
            claims.Add(new Claim("role", "SuperAdmin"));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, "1"));
        }

        var httpContext = new DefaultHttpContext();
        if (claims.Count > 0)
        {
            var identity = new ClaimsIdentity(claims, "Bearer");
            httpContext.User = new ClaimsPrincipal(identity);
        }

        if (headerBranchId != null)
        {
            httpContext.Request.Headers["X-Branch-Id"] = headerBranchId;
        }

        if (headerSupportAccess != null)
        {
            httpContext.Request.Headers["X-Support-Access"] = headerSupportAccess;
        }

        httpAccessor.HttpContext = httpContext;

        var mockLogger = new Mock<ILogger<CurrentBranchService>>();
        var tenantService = new CurrentBranchService(httpAccessor, mockLogger.Object);
        var mockConfig = new Mock<IConfiguration>();

        var context = new TestDbContext(options, mockConfig.Object, tenantService);
        return (context, tenantService, httpAccessor);
    }

    [Fact]
    public async Task Security_BranchUser_With_XBranchId_Header_Gets_Only_Own_Branch_Data()
    {
        // Arrange: User belongs to Branch 101, but sends X-Branch-Id: 102
        var dbName = Guid.NewGuid().ToString();

        // Seed data using a direct seeding context
        var options = new DbContextOptionsBuilder<RepositoryContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        var mockConfig = new Mock<IConfiguration>();
        using (var seedCtx = new TestDbContext(options, mockConfig.Object, null))
        {
            await seedCtx.Products.AddRangeAsync(
                new Product { Id = 1, BranchId = 101, Name = "Branch 101 Item", SKU = "SKU-101", Barcode = "BAR-101" },
                new Product { Id = 2, BranchId = 102, Name = "Branch 102 Item", SKU = "SKU-102", Barcode = "BAR-102" }
            );
            await seedCtx.SaveChangesAsync();
        }

        // Act: Query as Branch 101 user attempting to override with header X-Branch-Id: 102
        var (queryCtx, tenant, _) = CreateTestEnvironment(branchId: 101, isSuperAdmin: false, headerBranchId: "102", dbName: dbName);
        var visibleProducts = await queryCtx.Products.ToListAsync();

        // Assert: Unauthorized override must be rejected; user gets only Branch 101 data
        Assert.Equal(101, tenant.BranchId);
        Assert.Single(visibleProducts);
        Assert.Equal(101, visibleProducts[0].BranchId);
        Assert.Equal("Branch 101 Item", visibleProducts[0].Name);
    }

    [Fact]
    public async Task Security_BranchUser_With_XSupportAccess_Header_Gets_Only_Own_Branch_Data()
    {
        // Arrange: Regular user attempts X-Support-Access: true header bypass
        var dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<RepositoryContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        var mockConfig = new Mock<IConfiguration>();

        using (var seedCtx = new TestDbContext(options, mockConfig.Object, null))
        {
            await seedCtx.Products.AddRangeAsync(
                new Product { Id = 10, BranchId = 101, Name = "Branch 101 Item", SKU = "SKU-A", Barcode = "BAR-A" },
                new Product { Id = 20, BranchId = 102, Name = "Branch 102 Item", SKU = "SKU-B", Barcode = "BAR-B" }
            );
            await seedCtx.SaveChangesAsync();
        }

        // Act: Query as Branch 101 with header X-Support-Access: true
        var (queryCtx, tenant, _) = CreateTestEnvironment(branchId: 101, isSuperAdmin: false, headerSupportAccess: "true", dbName: dbName);
        var visibleProducts = await queryCtx.Products.ToListAsync();

        // Assert: Header is ignored, IsSuperAdmin is false, user gets only Branch 101
        Assert.False(tenant.IsSuperAdmin);
        Assert.Single(visibleProducts);
        Assert.Equal(101, visibleProducts[0].BranchId);
    }

    [Fact]
    public async Task Security_User_Without_BranchClaim_Gets_Zero_Records_Never_Branch1()
    {
        // Arrange: Seed Branch 1 and Branch 2 products
        var dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<RepositoryContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        var mockConfig = new Mock<IConfiguration>();

        using (var seedCtx = new TestDbContext(options, mockConfig.Object, null))
        {
            await seedCtx.Products.AddRangeAsync(
                new Product { Id = 1, BranchId = 1, Name = "Branch 1 Default Item", SKU = "SKU-DEF", Barcode = "BAR-DEF" },
                new Product { Id = 2, BranchId = 2, Name = "Branch 2 Item", SKU = "SKU-2", Barcode = "BAR-2" }
            );
            await seedCtx.SaveChangesAsync();
        }

        // Act: Query as authenticated user with NO branch claim
        var (queryCtx, tenant, _) = CreateTestEnvironment(branchId: null, isSuperAdmin: false, dbName: dbName);
        var visibleProducts = await queryCtx.Products.ToListAsync();

        // Assert: Tenant BranchId is 0, zero records returned (never falls back to Branch 1)
        Assert.Equal(0, tenant.BranchId);
        Assert.Empty(visibleProducts);
    }

    [Fact]
    public async Task SuperAdmin_Can_Override_Branch_Or_Query_All_Branches()
    {
        // Arrange: Seed items in Branch 10 and 20
        var dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<RepositoryContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        var mockConfig = new Mock<IConfiguration>();

        using (var seedCtx = new TestDbContext(options, mockConfig.Object, null))
        {
            await seedCtx.Products.AddRangeAsync(
                new Product { Id = 1, BranchId = 10, Name = "Alpha", SKU = "SKU-10", Barcode = "BAR-10" },
                new Product { Id = 2, BranchId = 20, Name = "Beta", SKU = "SKU-20", Barcode = "BAR-20" }
            );
            await seedCtx.SaveChangesAsync();
        }

        // Case A: SuperAdmin with X-Branch-Id: 20 override gets Branch 20
        var (queryCtxA, tenantA, _) = CreateTestEnvironment(branchId: 10, isSuperAdmin: true, headerBranchId: "20", dbName: dbName);
        Assert.Equal(20, tenantA.BranchId);
        var branch20Products = await queryCtxA.Products.ToListAsync();
        Assert.Single(branch20Products);
        Assert.Equal(20, branch20Products[0].BranchId);

        // Case B: SuperAdmin querying with IgnoreQueryFilters() sees all branches (soft delete filtered)
        var allProducts = await queryCtxA.Products.IgnoreQueryFilters().Where(p => !p.IsDeleted).ToListAsync();
        Assert.Equal(2, allProducts.Count);
    }

    [Fact]
    public async Task BackgroundWorker_TenantIsolation_BranchA_Scope_Never_Sees_BranchB()
    {
        // Arrange: Setup DI container with queue and worker services
        var services = new ServiceCollection();
        var dbName = Guid.NewGuid().ToString();
        var mockConfig = new Mock<IConfiguration>();

        services.AddSingleton(mockConfig.Object);
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentTenant, CurrentBranchService>();
        services.AddDbContext<RepositoryContext, TestDbContext>(opts => opts.UseInMemoryDatabase(dbName));
        services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
        services.AddLogging();

        var serviceProvider = services.BuildServiceProvider();

        // Seed data in DB
        using (var scope = serviceProvider.CreateScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<RepositoryContext>();
            await ctx.Notifications.AddRangeAsync(
                new Notification { Id = 1, BranchId = 501, Title = "Branch 501 Alert", Message = "Msg 501", UserId = 1 },
                new Notification { Id = 2, BranchId = 502, Title = "Branch 502 Alert", Message = "Msg 502", UserId = 2 }
            );
            await ctx.SaveChangesAsync();
        }

        // Act: Enqueue work item for Branch 501
        var queue = serviceProvider.GetRequiredService<IBackgroundTaskQueue>();
        List<Notification>? capturedNotifications = null;

        await queue.QueueBackgroundWorkItemAsync(async (sp, ct) =>
        {
            var ctx = sp.GetRequiredService<RepositoryContext>();
            capturedNotifications = await ctx.Notifications.ToListAsync(ct);
        }, branchId: 501, userId: 99);

        // Dequeue and simulate QueuedHostedService execution
        var workItem = await queue.DequeueAsync(CancellationToken.None);
        using (var workerScope = serviceProvider.CreateScope())
        {
            var workerTenant = workerScope.ServiceProvider.GetRequiredService<ICurrentTenant>();
            workerTenant.SetTenant(workItem.BranchId, workItem.UserId, workItem.IsSuperAdmin);

            await workItem.WorkItem(workerScope.ServiceProvider, CancellationToken.None);
        }

        // Assert: Background task executed under Branch 501 scope only sees Branch 501 notification
        Assert.NotNull(capturedNotifications);
        Assert.Single(capturedNotifications);
        Assert.Equal(501, capturedNotifications[0].BranchId);
        Assert.Equal("Branch 501 Alert", capturedNotifications[0].Title);
    }

    [Fact]
    public async Task SaveChanges_AutoStamps_BranchId_And_Blocks_CrossTenant_Tampering()
    {
        var (context, _, _) = CreateTestEnvironment(branchId: 305);

        // Auto-stamp test: BranchId 0 -> 305
        var newCat = new Category { Name = "Hardware", BranchId = 0 };
        await context.Categories.AddAsync(newCat);
        await context.SaveChangesAsync();
        Assert.Equal(305, newCat.BranchId);

        // Cross-tenant create block test: Branch 305 user cannot create for Branch 999
        var rogueCat = new Category { Name = "Rogue", BranchId = 999 };
        await context.Categories.AddAsync(rogueCat);
        await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
    }
}
