using BuildingBlocks.Shared.Interfaces;
using Entities.Models.Tables;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BuildingBlocks.Shared.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(User user)
    {
        var secretKey = _configuration["JwtSettings:SecretKey"] ?? "SuperSecretKeyForOhdaInventoryApiSystem2026!";
        var issuer = _configuration["JwtSettings:Issuer"] ?? "OhdaInventoryApi";
        var audience = _configuration["JwtSettings:Audience"] ?? "OhdaInventoryUsers";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.MilitaryNumber.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(1440),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public bool ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var secretKey = _configuration["JwtSettings:SecretKey"] ?? "SuperSecretKeyForOhdaInventoryApiSystem2026!";
            var key = Encoding.UTF8.GetBytes(secretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["JwtSettings:Issuer"] ?? "OhdaInventoryApi",
                ValidateAudience = true,
                ValidAudience = _configuration["JwtSettings:Audience"] ?? "OhdaInventoryUsers",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            tokenHandler.ValidateToken(token, validationParameters, out _);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public string GetUserIdFromToken(string token)
    {
        var claims = GetClaimsFromToken(token);
        return claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
    }

    public string GetUsernameFromToken(string token)
    {
        var claims = GetClaimsFromToken(token);
        return claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? string.Empty;
    }

    private IEnumerable<Claim>? GetClaimsFromToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jsonToken = tokenHandler.ReadToken(token) as JwtSecurityToken;
            return jsonToken?.Claims;
        }
        catch
        {
            return null;
        }
    }
}