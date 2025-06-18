using Api.Data;
using Api.Exceptions;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class RateService : IRateService
{
    private readonly AppDbContext _context;
    private readonly ILogger<RateService> _logger;

    public RateService(AppDbContext context, ILogger<RateService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Rate> GetRate()
    {
        return await _context.Rates
            .OrderByDescending(r => r.UpdatedAt)
            .FirstOrDefaultAsync()
            ?? throw new EntityNotFoundException("Rate", "Current rate not found");
    }

    public async Task<Rate> UpdateRate(decimal newRate)
    {
        if (newRate <= 0)
        {
            throw new EntityValidationException(new Dictionary<string, string>
            {
                [nameof(newRate)] = "Rate must be greater than 0"
            });
        }

        var rate = new Rate { Value = newRate };
        _context.Rates.Add(rate);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated rate to {NewRate}", newRate);
        return rate;
    }
}