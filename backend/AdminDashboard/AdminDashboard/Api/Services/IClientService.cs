using Api.Models;

namespace Api.Services
{
    public interface IClientService
    {
        Task<List<Client>> GetClients();
        Task<Client> GetClient(int id);
        Task<Client> CreateClient(Client client);
        Task<Client> UpdateClient(int id, Client client);
        Task DeleteClient(int id);
    }
}