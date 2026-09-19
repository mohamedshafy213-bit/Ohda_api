using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Entities.Models.Databases;
using Entities.Models.Databases.SqlDb;
using Entities.Models.Tables;
using Entities.Models.Enums;
using Entities.Models.Extensions;
using Entities.Models.Services;
using Microsoft.Extensions.Configuration;

namespace Ohda_Tests
{
    public class MultiBranchIsolationTests
    {
        private (RepositoryContext context, Mock<IHttpContextAccessor> httpMock) CreateInMemoryContext(int? branchId = null, bool isSuperAdmin = false, bool hasSupportAccess = false)
        {
            var mockHttp = new Mock<IHttpContextAccessor>();
            var claims = new List<Claim>();

            if (branchId.HasValue)
            {
                claims.Add(new Claim("branch_id", branchId.Value.ToString()));
            }

            if (isSuperAdmin)
            {
                claims.Add(new Claim(ClaimTypes.Role, "SuperAdmin"));
            }

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext();
            httpContext.User = principal;
            if (hasSupportAccess)
            {
                httpContext.Request.Headers["X-Support-Access"] = "true";
            }

            mockHttp.Setup(h => h.HttpContext).Returns(httpContext);

            var options = new DbContextOptionsBuilder<RepositoryContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var mockConfig = new Mock<IConfiguration>();

            var context = new TestSqlContext(mockConfig.Object, mockHttp.Object, options);
            return (context, mockHttp);
        }

        private class TestSqlContext : RepositoryContext
        {
            public TestSqlContext(IConfiguration config, IHttpContextAccessor http, DbContextOptions<RepositoryContext> options)
                : base(options, config, http, null, null)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);
                modelBuilder.ApplyGlobalFilters(_httpContextAccessor);
            }
        }

        [Fact]
        public async Task TenantIsolation_BranchA_CannotQuery_BranchB_Products()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<RepositoryContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            var mockConfig = new Mock<IConfiguration>();
            var httpAccessor = new HttpContextAccessor();

            // Seed under background/unauthenticated context (httpAccessor.HttpContext == null -> bypasses tenant filter)
            httpAccessor.HttpContext = null;
            var seedCtx = new TestSqlContext(mockConfig.Object, httpAccessor, options);
            await seedCtx.Products.AddAsync(new Product { Id = 1, BranchId = 101, Name = "Product Alpha", SKU = "SKU-A", Barcode = "BAR-A" });
            await seedCtx.Products.AddAsync(new Product { Id = 2, BranchId = 102, Name = "Product Beta", SKU = "SKU-B", Barcode = "BAR-B" });
            await seedCtx.SaveChangesAsync();

            // Act: Switch context to authenticated Branch 101 user
            var claims = new List<Claim> { new Claim("branch_id", "101") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            httpAccessor.HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };

            var queryCtx = new TestSqlContext(mockConfig.Object, httpAccessor, options);
            var visibleProducts = await queryCtx.Products.ToListAsync();

            // Assert
            Assert.Single(visibleProducts);
            Assert.Equal("Product Alpha", visibleProducts[0].Name);
            Assert.Equal(101, visibleProducts[0].BranchId);
        }

        [Fact]
        public async Task TenantIsolation_SuperAdminWithSupportAccess_CanViewAllBranches()
        {
            // Arrange: Seed products
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<RepositoryContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            var mockConfig = new Mock<IConfiguration>();
            var httpAccessor = new HttpContextAccessor();

            httpAccessor.HttpContext = null;
            var seedCtx = new TestSqlContext(mockConfig.Object, httpAccessor, options);
            await seedCtx.Products.AddAsync(new Product { Id = 10, BranchId = 201, Name = "Hospital Device", SKU = "MED-01", Barcode = "MED-BAR" });
            await seedCtx.Products.AddAsync(new Product { Id = 20, BranchId = 202, Name = "Retail Item", SKU = "RET-01", Barcode = "RET-BAR" });
            await seedCtx.SaveChangesAsync();

            // Act: Query as SuperAdmin with X-Support-Access
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "superadmin"),
                new Claim(ClaimTypes.Role, "SuperAdmin"),
                new Claim("role", "SuperAdmin")
            };
            var identity = new ClaimsIdentity(claims, "Bearer");
            var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
            httpContext.Request.Headers["X-Support-Access"] = "true";
            httpAccessor.HttpContext = httpContext;

            var queryCtx = new TestSqlContext(mockConfig.Object, httpAccessor, options);
            var allProducts = await queryCtx.Products.ToListAsync();

            // Assert
            Assert.Equal(2, allProducts.Count);
        }

        [Fact]
        public async Task SaveChanges_AutoStamps_BranchId_OnNewTenantEntity()
        {
            // Arrange
            var (context, _) = CreateInMemoryContext(branchId: 305);

            var newCategory = new Category { Name = "IT Hardware", BranchId = 0 };
            await context.Categories.AddAsync(newCategory);
            await context.SaveChangesAsync();

            // Assert
            Assert.Equal(305, newCategory.BranchId);
        }

        [Fact]
        public async Task SaveChanges_ThrowsException_WhenAltering_BranchId_OfExistingRecord()
        {
            // Arrange
            var (context, _) = CreateInMemoryContext(branchId: 400);

            var item = new Department { Id = 55, Name = "Security Dept", BranchId = 400 };
            await context.Departments.AddAsync(item);
            await context.SaveChangesAsync();

            // Act & Assert
            item.BranchId = 999;
            await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
        }
    }
}
