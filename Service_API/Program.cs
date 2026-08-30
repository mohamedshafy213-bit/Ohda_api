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

        builder.Services.AddCors(options =>
            options.AddDefaultPolicy(b => b.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

        var app = builder.Build();

        // Seed initial system data (Users, Categories, Suppliers, Products, Inventories)
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<RepositoryContext>();
            // Force recreation of database to apply military number key changes cleanly
            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();
            await DatabaseSeeder.SeedAsync(dbContext);
        }

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        });

        app.UseMiddleware<ExceptionMiddleware>();

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
