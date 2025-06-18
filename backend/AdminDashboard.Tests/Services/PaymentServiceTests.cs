using Api.Data;
using Api.Models;
using Api.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Tests.Services;

public class PaymentServiceTests
{
    private readonly AppDbContext _context;
    private readonly PaymentService _paymentService;

    public PaymentServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "PaymentServiceTests")
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        _paymentService = new PaymentService(_context);
    }

    [Fact]
    public async Task GetPayments_ShouldReturnLatestPayments()
    {
        // Arrange
        var client = new Client { Name = "Test", Email = "test@test.com" };
        await _context.Clients.AddAsync(client);

        var payments = new List<Payment>
        {
            new Payment { ClientId = client.Id, Amount = 100, CreatedAt = DateTime.UtcNow.AddDays(-2) },
            new Payment { ClientId = client.Id, Amount = 200, CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new Payment { ClientId = client.Id, Amount = 300, CreatedAt = DateTime.UtcNow }
        };
        await _context.Payments.AddRangeAsync(payments);
        await _context.SaveChangesAsync();

        // Act
        var result = await _paymentService.GetPayments(2);

        // Assert
        result.Should().HaveCount(2);
        result[0].Amount.Should().Be(300);
        result[1].Amount.Should().Be(200);
    }

    [Fact]
    public async Task GetPaymentsPagedAsync_ShouldReturnPaginatedResults()
    {
        // Arrange
        var client = new Client { Name = "Test", Email = "test@test.com" };
        await _context.Clients.AddAsync(client);

        var payments = Enumerable.Range(1, 10)
            .Select(i => new Payment
            {
                ClientId = client.Id,
                Amount = i * 100,
                CreatedAt = DateTime.UtcNow.AddDays(-i)
            })
            .OrderByDescending(p => p.CreatedAt)
            .ToList();

        await _context.Payments.AddRangeAsync(payments);
        await _context.SaveChangesAsync();

        // Act
        var result = await _paymentService.GetPaymentsPagedAsync(page: 2, pageSize: 3);

        // Assert
        result.Payments.Should().HaveCount(3);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(3);
        result.TotalCount.Should().Be(10);

        var expectedAmounts = new[] { 400, 500, 600 };
        result.Payments.Select(p => p.Amount).Should().BeEquivalentTo(expectedAmounts);
    }
}