using System;
using System.IO;
using System.Linq;
using Entities.Models.Databases;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Ohda_Tests;

public class UnitTest1
{
    [Fact]
    public void CheckDatabaseDepartments()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(new[]
        {
            new KeyValuePair<string, string?>("ConnectionStrings:SqlServerConnection", "Server=localhost;Database=OhdaDb3;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;")
        });
        var config = configBuilder.Build();

        var optionsBuilder = new DbContextOptionsBuilder<RepositoryContext>();
        optionsBuilder.UseSqlServer("Server=localhost;Database=OhdaDb3;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;");

        using var context = new RepositoryContext(optionsBuilder.Options, config);

        var count = context.Departments.IgnoreQueryFilters().Count();
        var activeCount = context.Departments.IgnoreQueryFilters().Count(d => !d.IsDeleted);
        var branches = context.Departments.IgnoreQueryFilters().GroupBy(d => d.BranchId).Select(g => new { BranchId = g.Key, Count = g.Count() }).ToList();

        var latest5 = context.Departments.IgnoreQueryFilters().OrderByDescending(d => d.Id).Take(5).Select(d => new { d.Id, d.Name, d.BranchId, d.IsDeleted }).ToList();

        Console.WriteLine($"TOTAL DEPARTMENTS: {count}");
        Console.WriteLine($"ACTIVE DEPARTMENTS: {activeCount}");
        foreach (var b in branches)
        {
            Console.WriteLine($"BranchId: {b.BranchId} -> Count: {b.Count}");
        }
        Console.WriteLine("--- LATEST 5 ---");
        foreach (var d in latest5)
        {
            Console.WriteLine($"Id: {d.Id}, Name: {d.Name}, BranchId: {d.BranchId}, IsDeleted: {d.IsDeleted}");
        }

        Assert.True(true);
    }
}
