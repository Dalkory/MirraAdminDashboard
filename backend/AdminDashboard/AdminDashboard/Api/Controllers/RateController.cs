using Api.Contracts;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public class RateController : ApiControllerBase
{
    private readonly IRateService _rateService;

    public RateController(IRateService rateService)
    {
        _rateService = rateService;
    }

    [HttpGet]
    public async Task<ActionResult<Rate>> GetRate()
    {
        return await _rateService.GetRate();
    }

    [HttpPost]
    public async Task<ActionResult<Rate>> UpdateRate([FromBody] UpdateRateRequest request)
    {
        return await _rateService.UpdateRate(request.Value);
    }
}