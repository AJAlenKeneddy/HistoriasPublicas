using StoryBlazeServer.Models;
using System.Security.Claims;

namespace StoryBlazeServer.Services
{
    public interface IJwtService
    {
        string GenerateToken(Usuario usuario); 
        string GetUserIdFromToken(string token);
    }


}
