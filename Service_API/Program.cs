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

        // Seed initial system data (Users, Categories, Suppliers, Products, Inventories)
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<RepositoryContext>();
            // Apply any pending EF Core migrations, then seed initial data
            await dbContext.Database.MigrateAsync();
            await DatabaseSeeder.SeedAsync(dbContext);
        }

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        });

        // Slow request logging middleware (logs requests exceeding 500ms)
        app.Use(async (context, next) =>
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            await next();
            sw.Stop();
            if (sw.ElapsedMilliseconds > 500)
            {
                var logger = context.RequestServices.GetService<ILogger<Program>>();
                logger?.LogWarning("[SLOW REQUEST] {Method} {Path} returned {StatusCode} in {Elapsed}ms",
                    context.Request.Method, context.Request.Path, context.Response.StatusCode, sw.ElapsedMilliseconds);
            }
        });

        app.UseMiddleware<ExceptionMiddleware>();

        app.UseResponseCompression();
        app.UseHttpsRedirection();
        app.UseCors();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapGet("/", context =>
        {
            context.Response.Redirect("/swagger");
            return Task.CompletedTask;
        }).AllowAnonymous();

        app.MapControllers();

        await app.RunAsync();
    }
}
