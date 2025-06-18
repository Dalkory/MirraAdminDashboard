using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public class ClientsController : ApiControllerBase
{
    private readonly IClientService _clientService;

    public ClientsController(IClientService clientService)
    {
        _clientService = clientService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Client>>> GetClients() =>
        await _clientService.GetClients();

    [HttpGet("{id}")]
    public async Task<ActionResult<Client>> GetClient(int id) =>
        await _clientService.GetClient(id);

    [HttpPost]
    public async Task<ActionResult<Client>> CreateClient([FromBody] Client client)
    {
        var createdClient = await _clientService.CreateClient(client);
        return CreatedAtAction(nameof(GetClient), new { id = createdClient.Id }, createdClient);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Client>> UpdateClient(int id, [FromBody] Client client)
    {
        var updatedClient = await _clientService.UpdateClient(id, client);
        return Ok(updatedClient);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteClient(int id)
    {
        await _clientService.DeleteClient(id);
        return NoContent();
    }
}