using Api.Data;
using Api.Exceptions;
using Api.Models;
using Api.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Services;

public class TagServiceTests
{
    private readonly AppDbContext _context;
    private readonly Mock<ILogger<TagService>> _loggerMock;
    private readonly TagService _tagService;

    public TagServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TagServiceTests")
        .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        _loggerMock = new Mock<ILogger<TagService>>();
        _tagService = new TagService(_context, _loggerMock.Object);
    }

    [Fact]
    public async Task GetTags_ShouldReturnAllTags()
    {
        // Arrange
        var tags = new List<Tag>
        {
            new Tag { Name = "Tag1", Color = "#111111" },
            new Tag { Name = "Tag2", Color = "#222222" }
        };
        await _context.Tags.AddRangeAsync(tags);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tagService.GetTags();

        // Assert
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(tags);
    }

    [Fact]
    public async Task GetTag_WithExistingId_ShouldReturnTag()
    {
        // Arrange
        var tag = new Tag { Name = "Test", Color = "#FFFFFF" };
        await _context.Tags.AddAsync(tag);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tagService.GetTag(tag.Id);

        // Assert
        result.Should().BeEquivalentTo(tag);
    }

    [Fact]
    public async Task GetTag_WithNonExistingId_ShouldThrowNotFoundException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _tagService.GetTag(999));
    }

    [Fact]
    public async Task CreateTag_WithUniqueName_ShouldCreateTag()
    {
        // Arrange
        var tag = new Tag { Name = "NewTag", Color = "#123456" };

        // Act
        var result = await _tagService.CreateTag(tag);

        // Assert
        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be(tag.Name);
        result.Color.Should().Be(tag.Color);

        var dbTag = await _context.Tags.FindAsync(result.Id);
        dbTag.Should().BeEquivalentTo(result);

        _loggerMock.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Created tag with ID {result.Id}")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()));
    }

    [Fact]
    public async Task CreateTag_WithDuplicateName_ShouldThrowConflictException()
    {
        // Arrange
        var existingTag = new Tag { Name = "Existing", Color = "#111111" };
        await _context.Tags.AddAsync(existingTag);
        await _context.SaveChangesAsync();

        var newTag = new Tag { Name = "Existing", Color = "#222222" };

        // Act & Assert
        await Assert.ThrowsAsync<EntityConflictException>(() => _tagService.CreateTag(newTag));
    }

    [Fact]
    public async Task UpdateTag_ShouldUpdateTag()
    {
        // Arrange
        var originalTag = new Tag { Name = "Original", Color = "#111111" };
        await _context.Tags.AddAsync(originalTag);
        await _context.SaveChangesAsync();

        var updatedTag = new Tag { Name = "Updated", Color = "#222222" };

        // Act
        await _tagService.UpdateTag(originalTag.Id, updatedTag);

        // Assert
        var dbTag = await _context.Tags.FindAsync(originalTag.Id);
        dbTag.Name.Should().Be(updatedTag.Name);
        dbTag.Color.Should().Be(updatedTag.Color);
    }

    [Fact]
    public async Task UpdateTag_WithDuplicateName_ShouldThrowConflictException()
    {
        // Arrange
        var tag1 = new Tag { Name = "Tag1", Color = "#111111" };
        var tag2 = new Tag { Name = "Tag2", Color = "#222222" };
        await _context.Tags.AddRangeAsync(tag1, tag2);
        await _context.SaveChangesAsync();

        var updatedTag = new Tag { Name = "Tag2", Color = "#333333" };

        // Act & Assert
        await Assert.ThrowsAsync<EntityConflictException>(() =>
            _tagService.UpdateTag(tag1.Id, updatedTag));
    }

    [Fact]
    public async Task DeleteTag_ShouldRemoveTag()
    {
        // Arrange
        var tag = new Tag { Name = "ToDelete", Color = "#111111" };
        await _context.Tags.AddAsync(tag);
        await _context.SaveChangesAsync();

        // Act
        await _tagService.DeleteTag(tag.Id);

        // Assert
        var dbTag = await _context.Tags.FindAsync(tag.Id);
        dbTag.Should().BeNull();

        _loggerMock.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Deleted tag with ID {tag.Id}")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()));
    }

    [Fact]
    public async Task UpdateClientTags_ShouldUpdateClientTags()
    {
        // Arrange
        var client = new Client { Name = "Test", Email = "test@test.com" };
        var tags = new List<Tag>
        {
            new Tag { Name = "Tag1", Color = "#111111" },
            new Tag { Name = "Tag2", Color = "#222222" }
        };

        await _context.Clients.AddAsync(client);
        await _context.Tags.AddRangeAsync(tags);
        await _context.SaveChangesAsync();

        // Act
        await _tagService.UpdateClientTags(client.Id, new[] { tags[0].Id });

        // Assert
        var dbClient = await _context.Clients
            .Include(c => c.Tags)
            .FirstOrDefaultAsync(c => c.Id == client.Id);

        dbClient.Tags.Should().HaveCount(1);
        dbClient.Tags.First().Id.Should().Be(tags[0].Id);
    }
}