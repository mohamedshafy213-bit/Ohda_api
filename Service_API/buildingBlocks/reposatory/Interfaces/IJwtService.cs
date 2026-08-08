using Entities.Models.Tables;

namespace BuildingBlocks.Shared.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
    bool ValidateToken(string token);
    string GetUserIdFromToken(string token);
    string GetUsernameFromToken(string token);

}