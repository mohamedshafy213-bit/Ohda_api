using System.IdentityModel.Tokens.Jwt;
using Entities.Models.Databases;
using Service_API.Extensions;

namespace Service_API;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Clear default claim mapping so standard JWT claim names match ClaimTypes
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        // Add services to the container
        builder.Services.AddControllers();

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
            await DatabaseSeeder.SeedAsync(dbContext);
        }

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        });

        app.UseHttpsRedirection();
        app.UseCors();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        await app.RunAsync();
    }
}
