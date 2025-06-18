using Api.Data;
using Api.Exceptions;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class TagService : ITagService
{
    private readonly AppDbContext _context;
    private readonly ILogger<TagService> _logger;

    public TagService(AppDbContext context, ILogger<TagService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Tag>> GetTags()
    {
        return await _context.Tags
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Tag> GetTag(int id)
    {
        return await _context.Tags
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new EntityNotFoundException(nameof(Tag), id);
    }

    public async Task<Tag> CreateTag(Tag tag)
    {
        await ValidateTagNameIsUnique(tag.Name);

        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created tag with ID {TagId}", tag.Id);
        return tag;
    }

    public async Task<Tag> UpdateTag(int id, Tag tag)
    {
        var existingTag = await GetExistingTag(id);
        await ValidateTagNameIsUnique(tag.Name, id);

        existingTag.Name = tag.Name;
        existingTag.Color = tag.Color;

        await _context.SaveChangesAsync();
        return existingTag;
    }

    public async Task DeleteTag(int id)
    {
        var tag = await GetExistingTag(id);
        _context.Tags.Remove(tag);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted tag with ID {TagId}", id);
    }

    public async Task UpdateClientTags(int clientId, int[] tagIds)
    {
        var client = await GetClientWithTags(clientId);
        var tags = await GetExistingTags(tagIds);

        client.Tags.Clear();
        foreach (var tag in tags)
        {
            client.Tags.Add(tag);
        }

        await _context.SaveChangesAsync();
    }

    private async Task<Tag> GetExistingTag(int id)
    {
        return await _context.Tags.FindAsync(id)
            ?? throw new EntityNotFoundException(nameof(Tag), id);
    }

    private async Task<Client> GetClientWithTags(int clientId)
    {
        return await _context.Clients
            .Include(c => c.Tags)
            .FirstOrDefaultAsync(c => c.Id == clientId)
            ?? throw new EntityNotFoundException(nameof(Client), clientId);
    }

    private async Task<List<Tag>> GetExistingTags(int[] tagIds)
    {
        var tags = await _context.Tags
            .Where(t => tagIds.Contains(t.Id))
            .ToListAsync();

        var notFoundIds = tagIds.Except(tags.Select(t => t.Id)).ToList();
        if (notFoundIds.Any())
        {
            throw new EntityNotFoundException(nameof(Tag), string.Join(",", notFoundIds));
        }

        return tags;
    }

    private async Task ValidateTagNameIsUnique(string name, int? id = null)
    {
        if (await _context.Tags.AnyAsync(t =>
            t.Name == name && (!id.HasValue || t.Id != id.Value)))
        {
            throw new EntityConflictException(nameof(Tag), nameof(Tag.Name), name);
        }
    }
}