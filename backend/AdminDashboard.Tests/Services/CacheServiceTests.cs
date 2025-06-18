using Api.Services;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using System.Text.Json;

namespace Tests.Services;

public class CacheServiceTests
{
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly Mock<ILogger<CacheService>> _loggerMock;
    private readonly CacheService _cacheService;

    public CacheServiceTests()
    {
        _cacheMock = new Mock<IDistributedCache>();
        _loggerMock = new Mock<ILogger<CacheService>>();
        _cacheService = new CacheService(_cacheMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAsync_WithCachedData_ShouldReturnDeserializedObject()
    {
        // Arrange
        var testObject = new { Name = "Test", Value = 123 };
        var serialized = JsonSerializer.Serialize(testObject);
        var bytes = Encoding.UTF8.GetBytes(serialized);

        _cacheMock.Setup(x => x.GetAsync("testKey", default))
            .ReturnsAsync(bytes);

        // Act
        var result = await _cacheService.GetAsync<object>("testKey");

        // Assert
        result.Should().NotBeNull();

        _loggerMock.Verify(x => x.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Cache hit for key testKey")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()));
    }

    [Fact]
    public async Task GetAsync_WithNoCachedData_ShouldReturnDefault()
    {
        // Arrange
        _cacheMock.Setup(x => x.GetAsync("testKey", default))
            .ReturnsAsync((byte[])null);

        // Act
        var result = await _cacheService.GetAsync<object>("testKey");

        // Assert
        result.Should().BeNull();

        _loggerMock.Verify(x => x.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Cache miss for key testKey")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()));
    }

    [Fact]
    public async Task GetAsync_WithCacheError_ShouldLogErrorAndReturnDefault()
    {
        // Arrange
        _cacheMock.Setup(x => x.GetAsync("testKey", default))
            .ThrowsAsync(new Exception("Cache error"));

        // Act
        var result = await _cacheService.GetAsync<object>("testKey");

        // Assert
        result.Should().BeNull();

        _loggerMock.Verify(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error getting data from cache for key testKey")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()));
    }

    [Fact]
    public async Task SetAsync_ShouldSerializeAndCacheData()
    {
        // Arrange
        var testObject = new { Name = "Test", Value = 123 };
        var options = new DistributedCacheEntryOptions();

        // Act
        await _cacheService.SetAsync("testKey", testObject, options);

        // Assert
        _cacheMock.Verify(x => x.SetAsync(
            "testKey",
            It.IsAny<byte[]>(),
            options,
            default), Times.Once);

        _loggerMock.Verify(x => x.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Data cached for key testKey")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()));
    }

    [Fact]
    public async Task RemoveAsync_ShouldRemoveCacheEntry()
    {
        // Act
        await _cacheService.RemoveAsync("testKey");

        // Assert
        _cacheMock.Verify(x => x.RemoveAsync("testKey", default), Times.Once);

        _loggerMock.Verify(x => x.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Cache removed for key testKey")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()));
    }
}