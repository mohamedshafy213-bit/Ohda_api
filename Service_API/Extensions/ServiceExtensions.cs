using Contracts.interfaces.Repository;
using Entities.Models.Databases;
using Entities.Models.Databases.OracleDb;
using Entities.Models.Databases.PostgresDb;
using Entities.Models.Databases.SqlDb;
using LoggerService;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Repositories.Repositories;
using Service_API.Helpers;
using System.Security.Claims;
using System.Text;

namespace Service_API.Extensions;

public static class ServiceExtensions
{
    public static void ConfigureAutoMapper(this IServiceCollection services)
    {
        TypeAdapterConfig<Contracts.DTOs.User.UserCreateDto, Entities.Models.Tables.User>
            .NewConfig()
            .Map(dest => dest.PasswordHash, src => src.Password);

        TypeAdapterConfig<Entities.Models.Tables.User, Contracts.DTOs.User.UserDto>
            .NewConfig()
            .Map(dest => dest.UserGroupName, src => src.UserGroup != null ? src.UserGroup.Name : null);

        TypeAdapterConfig<Entities.Models.Tables.ApprovalConfig, Contracts.DTOs.ApprovalConfig.ApprovalConfigDto>
            .NewConfig()
            .Map(dest => dest.UserGroupName, src => src.UserGroup != null ? src.UserGroup.Name : null);

        TypeAdapterConfig<Entities.Models.Tables.ProductExitRequest, Contracts.DTOs.ProductExitRequest.ProductExitRequestDto>
            .NewConfig()
            .Map(dest => dest.DepartmentName, src => src.Department != null ? src.Department.Name : null);

        TypeAdapterConfig<Entities.Models.Tables.ProductExitRequestItem, Contracts.DTOs.ProductExitRequest.ProductExitRequestItemDto>
            .NewConfig()
            .Map(dest => dest.ProductName, src => src.Product != null ? src.Product.Name : null)
            .Map(dest => dest.ProductSKU, src => src.Product != null ? src.Product.SKU : null);

        TypeAdapterConfig<Entities.Models.Tables.ProductEntryRequest, Contracts.DTOs.ProductEntryRequest.ProductEntryRequestDto>
            .NewConfig()
            .Map(dest => dest.DepartmentName, src => src.Department != null ? src.Department.Name : null);

        TypeAdapterConfig<Entities.Models.Tables.ProductEntryRequestItem, Contracts.DTOs.ProductEntryRequest.ProductEntryRequestItemDto>
            .NewConfig()
            .Map(dest => dest.ProductName, src => src.Product != null ? src.Product.Name : null)
            .Map(dest => dest.ProductSKU, src => src.Product != null ? src.Product.SKU : null)
            .Map(dest => dest.ProductStateName, src => src.ProductState != null ? src.ProductState.Name : null);

        TypeAdapterConfig<Entities.Models.Tables.Compass, Contracts.DTOs.Compass.CompassDto>
            .NewConfig()
            .Map(dest => dest.DepartmentName, src => src.Department != null ? src.Department.Name : null)
            .Map(dest => dest.ProductStateName, src => src.ProductState != null ? src.ProductState.Name : null);

        services.AddMapster();
    }

    public static void ConfigureLogger(this IServiceCollection services, IConfiguration Configuration)
    {
        switch (Configuration["DatabaseProvider"])
        {
            case "Oracle":
                services.AddSingleton<ILoggerManager>(logger => new LoggerManager(Configuration["DatabaseProvider"],
                    Configuration.GetConnectionString("OracleConnection")));
                break;
            case "Postgres":
                services.AddSingleton<ILoggerManager>(logger => new LoggerManager(Configuration["DatabaseProvider"],
                    Configuration.GetConnectionString("PostgresConnection")));
                break;
            case "SqlServer":
            default:
                services.AddSingleton<ILoggerManager>(logger => new LoggerManager(Configuration["DatabaseProvider"] ?? "SqlServer",
                    Configuration.GetConnectionString("SqlServerConnection")));
                break;
        }
    }

    public static void ConfigureHttpContext(this IServiceCollection services)
    {
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
    }

    public static void ConfigureDataBase(this IServiceCollection services, IConfiguration Configuration)
    {
        switch (Configuration["DatabaseProvider"])
        {
            case "Oracle":
                services.AddDbContext<RepositoryContext, OracleContext>((serviceProvider, options) =>
                {
                    options.UseOracle(Configuration.GetConnectionString("OracleConnection"), b => b.MigrationsAssembly("Entities"));
                });
                break;
            case "Postgres":
                AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
                services.AddDbContext<RepositoryContext, PostgresContext>((serviceProvider, options) =>
                {
                    options.UseNpgsql(Configuration.GetConnectionString("PostgresConnection"), b => b.MigrationsAssembly("Entities"))
                           .UseSnakeCaseNamingConvention();
                });
                break;
            case "SqlServer":
            default:
                services.AddDbContext<RepositoryContext, SqlServerContext>((serviceProvider, options) =>
                {
                    options.UseSqlServer(Configuration.GetConnectionString("SqlServerConnection"), b =>
                    {
                        b.MigrationsAssembly("Entities");
                        b.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null);
                    });
                });
                break;
        }
    }

    public static void ConfigureRepositoryWrapper(this IServiceCollection services)
    {
        services.AddScoped<IRepositoryWrapper, RepositoryWrapper>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
    }

    public static void ConfigureJwtBearer(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? "SuperSecretKeyForOhdaInventoryApiSystem2026!";
        var issuer = jwtSettings["Issuer"] ?? "OhdaInventoryApi";
        var audience = jwtSettings["Audience"] ?? "OhdaInventoryUsers";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = key,
                RoleClaimType = ClaimTypes.Role,
                NameClaimType = ClaimTypes.NameIdentifier,
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .Build();
        });
    }

    public static void ConfigureSwaggerGen(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.CustomSchemaIds(type => type.FullName);

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                In = ParameterLocation.Header,
                Description = "Enter JWT Bearer token: 'Bearer YOUR_TOKEN'",
                Type = SecuritySchemeType.ApiKey,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
    }
}
