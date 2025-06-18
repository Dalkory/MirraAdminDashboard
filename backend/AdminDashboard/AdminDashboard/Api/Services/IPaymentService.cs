using Api.Models;

namespace Api.Services
{
    public interface IPaymentService
    {
        Task<List<Payment>> GetPayments(int take = 5);
    }
}