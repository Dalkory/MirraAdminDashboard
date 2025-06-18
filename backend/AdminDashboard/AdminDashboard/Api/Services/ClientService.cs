using Api.Data;
using Api.Exceptions;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class ClientService : IClientService
{
    private readonly AppDbContext _context;

    public ClientService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Client>> GetClients()
    {
        return await _context.Clients
            .Include(c => c.Tags)
            .Select(c => new Client
            {
                Id = c.Id,
                Name = c.Name,
                Email = c.Email,
                Balance = c.Balance,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                Tags = c.Tags.Select(t => new Tag
                {
                    Id = t.Id,
                    Name = t.Name,
                    Color = t.Color
                }).ToList()
            })
            .ToListAsync();
    }

    public async Task<Client> GetClient(int id)
    {
        var client = await _context.Clients
            .Include(c => c.Tags)
            .Select(c => new Client
            {
                Id = c.Id,
                Name = c.Name,
                Email = c.Email,
                Balance = c.Balance,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                Tags = c.Tags.Select(t => new Tag
                {
                    Id = t.Id,
                    Name = t.Name,
                    Color = t.Color
                }).ToList()
            })
            .FirstOrDefaultAsync(c => c.Id == id);

        if (client == null)
        {
            throw new EntityNotFoundException(nameof(Client), id);
        }

        return client;
    }

    public async Task<Client> CreateClient(Client client)
    {
        await ValidateClient(client);

        client.CreatedAt = DateTime.UtcNow;
        client.UpdatedAt = DateTime.UtcNow;

        _context.Clients.Add(client);
        await _context.SaveChangesAsync();
        return client;
    }

    public async Task<Client> UpdateClient(int id, Client client)
    {
        var existingClient = await _context.Clients
            .Include(c => c.Tags)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new EntityNotFoundException(nameof(Client), id);

        await ValidateClient(client, id);

        existingClient.Name = client.Name;
        existingClient.Email = client.Email;
        existingClient.Balance = client.Balance;
        existingClient.UpdatedAt = DateTime.UtcNow;

        var existingTags = existingClient.Tags.ToList();

        await _context.SaveChangesAsync();

        return await GetClient(id);
    }

    public async Task DeleteClient(int id)
    {
        var client = await _context.Clients.FindAsync(id)
            ?? throw new EntityNotFoundException(nameof(Client), id);

        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();
    }

    private async Task ValidateClient(Client client, int? id = null)
    {
        var errors = new Dictionary<string, string>();

        if (await _context.Clients.AnyAsync(c =>
            c.Name == client.Name && (!id.HasValue || c.Id != id.Value)))
        {
            errors[nameof(client.Name)] = $"Client with name '{client.Name}' already exists";
        }

        if (await _context.Clients.AnyAsync(c =>
            c.Email == client.Email && (!id.HasValue || c.Id != id.Value)))
        {
            errors[nameof(client.Email)] = $"Client with email '{client.Email}' already exists";
        }

        if (errors.Any())
        {
            throw new EntityValidationException(errors);
        }
    }
}