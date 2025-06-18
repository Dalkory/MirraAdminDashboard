using Api.Data;
using Api.Exceptions;
using Api.Models;
using Api.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Tests.Services;

public class ClientServiceTests
{
    private readonly AppDbContext _context;
    private readonly ClientService _clientService;

    public ClientServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "ClientServiceTests")
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        _clientService = new ClientService(_context);
    }

    [Fact]
    public async Task GetClients_ShouldReturnAllClients()
    {
        // Arrange
        var clients = new List<Client>
        {
            new Client { Name = "Client1", Email = "client1@test.com", Balance = 100 },
            new Client { Name = "Client2", Email = "client2@test.com", Balance = 200 }
        };
        await _context.Clients.AddRangeAsync(clients);
        await _context.SaveChangesAsync();

        // Act
        var result = await _clientService.GetClients();

        // Assert
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(clients, opt => opt.Excluding(c => c.Tags));
    }

    [Fact]
    public async Task GetClient_WithExistingId_ShouldReturnClient()
    {
        // Arrange
        var client = new Client { Name = "Test", Email = "test@test.com", Balance = 300 };
        await _context.Clients.AddAsync(client);
        await _context.SaveChangesAsync();

        // Act
        var result = await _clientService.GetClient(client.Id);

        // Assert
        result.Should().BeEquivalentTo(client, opt => opt.Excluding(c => c.Tags));
    }

    [Fact]
    public async Task GetClient_WithNonExistingId_ShouldThrowNotFoundException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _clientService.GetClient(999));
    }

    [Fact]
    public async Task CreateClient_WithValidData_ShouldCreateClient()
    {
        // Arrange
        var client = new Client { Name = "New", Email = "new@test.com", Balance = 400 };

        // Act
        var result = await _clientService.CreateClient(client);

        // Assert
        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be(client.Name);
        result.Email.Should().Be(client.Email);
        result.Balance.Should().Be(client.Balance);

        var dbClient = await _context.Clients.FindAsync(result.Id);
        dbClient.Should().BeEquivalentTo(result);
    }

    [Fact]
    public async Task CreateClient_WithDuplicateEmail_ShouldThrowValidationException()
    {
        // Arrange
        var existingClient = new Client { Name = "Existing", Email = "duplicate@test.com", Balance = 100 };
        await _context.Clients.AddAsync(existingClient);
        await _context.SaveChangesAsync();

        var newClient = new Client { Name = "New", Email = "duplicate@test.com", Balance = 200 };

        // Act & Assert
        await Assert.ThrowsAsync<EntityValidationException>(() => _clientService.CreateClient(newClient));
    }

    [Fact]
    public async Task UpdateClient_ShouldUpdateClient()
    {
        // Arrange
        var originalClient = new Client { Name = "Original", Email = "original@test.com", Balance = 100 };
        await _context.Clients.AddAsync(originalClient);
        await _context.SaveChangesAsync();

        var updatedClient = new Client { Name = "Updated", Email = "updated@test.com", Balance = 200 };

        // Act
        var result = await _clientService.UpdateClient(originalClient.Id, updatedClient);

        // Assert
        result.Name.Should().Be(updatedClient.Name);
        result.Email.Should().Be(updatedClient.Email);
        result.Balance.Should().Be(updatedClient.Balance);

        var dbClient = await _context.Clients.FindAsync(originalClient.Id);
        dbClient.Should().BeEquivalentTo(result);
    }

    [Fact]
    public async Task DeleteClient_ShouldRemoveClient()
    {
        // Arrange
        var client = new Client { Name = "ToDelete", Email = "delete@test.com", Balance = 100 };
        await _context.Clients.AddAsync(client);
        await _context.SaveChangesAsync();

        // Act
        await _clientService.DeleteClient(client.Id);

        // Assert
        var dbClient = await _context.Clients.FindAsync(client.Id);
        dbClient.Should().BeNull();
    }
}