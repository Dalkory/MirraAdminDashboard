using Api.Data;
using Api.Exceptions;
using Api.Models;
using Api.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Services;

public class RateServiceTests
{
    private readonly AppDbContext _context;
    private readonly Mock<ILogger<RateService>> _loggerMock;
    private readonly RateService _rateService;

    public RateServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "RateServiceTests")
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        _loggerMock = new Mock<ILogger<RateService>>();
        _rateService = new RateService(_context, _loggerMock.Object);
    }

    [Fact]
    public async Task GetRate_WithNoRates_ShouldThrowNotFoundException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _rateService.GetRate());
    }

    [Fact]
    public async Task GetRate_WithRates_ShouldReturnLatestRate()
    {
        // Arrange
        var rates = new List<Rate>
        {
            new Rate { Value = 10, UpdatedAt = DateTime.UtcNow.AddDays(-1) },
            new Rate { Value = 20, UpdatedAt = DateTime.UtcNow }
        };
        await _context.Rates.AddRangeAsync(rates);
        await _context.SaveChangesAsync();

        // Act
        var result = await _rateService.GetRate();

        // Assert
        result.Value.Should().Be(20);
    }

    [Fact]
    public async Task UpdateRate_WithValidRate_ShouldCreateNewRate()
    {
        // Arrange
        var newRateValue = 15m;

        // Act
        var result = await _rateService.UpdateRate(newRateValue);

        // Assert
        result.Value.Should().Be(newRateValue);
        result.Id.Should().BeGreaterThan(0);

        _loggerMock.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Updated rate to {newRateValue}")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()));
    }

    [Fact]
    public async Task UpdateRate_WithInvalidRate_ShouldThrowValidationException()
    {
        // Arrange
        var invalidRate = 0m;

        // Act & Assert
        await Assert.ThrowsAsync<EntityValidationException>(() => _rateService.UpdateRate(invalidRate));
    }
}