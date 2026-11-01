using System.IdentityModel.Tokens.Jwt;
using Entities.Models.Databases;
using Microsoft.EntityFrameworkCore;
using Service_API.Extensions;
using Service_API.Helpers;
using Service_API.Middleware;

namespace Service_API;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("==================================================");
        Console.WriteLine("         OHDA INVENTORY SYSTEM - SERVICE API      ");
        Console.WriteLine("==================================================");

        try
        {
            var builder = WebApplication.CreateBuilder(args);

            // Clear default claim mapping so standard JWT claim names match ClaimTypes
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            // Add services to the container
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new NullableIntConverter());
                    options.JsonSerializerOptions.Converters.Add(new NullableDecimalConverter());
                    options.JsonSerializerOptions.Converters.Add(new NullableLongConverter());
                });

            builder.Services.AddMemoryCache();

            builder.Services.ConfigureSwaggerGen();
            builder.Services.ConfigureAutoMapper();
            builder.Services.ConfigureLogger(builder.Configuration);
            builder.Services.ConfigureHttpContext();
            builder.Services.ConfigureDataBase(builder.Configuration);
            builder.Services.ConfigureRepositoryWrapper();
            builder.Services.ConfigureJwtBearer(builder.Configuration);

            builder.Services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
            });

            builder.Services.AddCors(options =>
                options.AddDefaultPolicy(b => b.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

            var app = builder.Build();

            // Output connection string info for debugging
            var connStr = app.Configuration.GetConnectionString("SqlServerConnection");
            Console.WriteLine($"[Config] Provider: {app.Configuration["DatabaseProvider"] ?? "SqlServer"}");
            Console.WriteLine($"[Config] SQL Connection: {connStr}");

            // Apply EF Core migrations and initial seeding
            Console.WriteLine("[Database] Initializing database & running migrations...");
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<RepositoryContext>();
                try
                {
                    dbContext.Database.SetCommandTimeout(30);
                    await dbContext.Database.MigrateAsync();
                    Console.WriteLine("[Database] EF Migrations applied successfully.");
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[Database Migration Warning]: {ex.Message}");
                    Console.ResetColor();
                }

                try
                {
                    await DatabaseSeeder.SeedAsync(dbContext);
                    Console.WriteLine("[Database] Initial data seeding completed.");
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[Database Seeder Warning]: {ex.Message}");
                    Console.ResetColor();
                }
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
            });

            // Centralized performance and request timing middleware
            app.UseMiddleware<PerformanceMonitoringMiddleware>();
            app.UseMiddleware<ExceptionMiddleware>();
            app.UseResponseCompression();

            app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapGet("/", context =>
            {
                context.Response.Redirect("/swagger");
                return Task.CompletedTask;
            }).AllowAnonymous();

            app.MapControllers();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("--> Application configured successfully. Starting server...");
            Console.ResetColor();

            await app.RunAsync();
        }
        catch (Exception fatalEx)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("==================================================");
            Console.WriteLine($"[FATAL CRASH ON STARTUP]: {fatalEx.Message}");
            Console.WriteLine(fatalEx.ToString());
            Console.WriteLine("==================================================");
            Console.ResetColor();
            throw;
        }
    }
}

