using Api.Models;

namespace Api.Services
{
    public interface IAuthService
    {
        Task<string> Login(string email, string password);
        Task<string> RefreshToken(string token);
    }
}