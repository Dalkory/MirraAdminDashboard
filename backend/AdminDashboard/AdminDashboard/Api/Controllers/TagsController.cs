using Api.Contracts;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public class TagsController : ApiControllerBase
{
    private readonly ITagService _tagService;

    public TagsController(ITagService tagService)
    {
        _tagService = tagService;
    }

    [HttpGet]
    public async Task<ActionResult<List<TagResponse>>> GetTags()
    {
        var tags = await _tagService.GetTags();
        return Ok(tags.ConvertAll(ToResponse));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TagResponse>> GetTag(int id)
    {
        var tag = await _tagService.GetTag(id);
        return Ok(ToResponse(tag));
    }

    [HttpPost]
    public async Task<ActionResult<TagResponse>> CreateTag([FromBody] CreateTagRequest request)
    {
        var tag = new Tag { Name = request.Name, Color = request.Color };
        var createdTag = await _tagService.CreateTag(tag);
        return CreatedAtAction(nameof(GetTag), new { id = createdTag.Id }, ToResponse(createdTag));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTag(int id, [FromBody] UpdateTagRequest request)
    {
        var tag = new Tag { Id = id, Name = request.Name, Color = request.Color };
        await _tagService.UpdateTag(id, tag);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTag(int id)
    {
        await _tagService.DeleteTag(id);
        return NoContent();
    }

    [HttpPut("clients/{clientId}")]
    public async Task<IActionResult> UpdateClientTags(int clientId, [FromBody] UpdateClientTagsRequest request)
    {
        await _tagService.UpdateClientTags(clientId, request.TagIds);
        return NoContent();
    }

    private static TagResponse ToResponse(Tag tag) =>
        new(tag.Id, tag.Name, tag.Color);
}