using Api.Models;

namespace Api.Services
{
    public interface IRateService
    {
        Task<Rate> GetRate();
        Task<Rate> UpdateRate(decimal newRate);
    }
}