using Api.Contracts;
using Api.Data;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class PaymentService : IPaymentService
{
    private readonly AppDbContext _context;

    public PaymentService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Payment>> GetPayments(int take = 5)
    {
        return await _context.Payments
            .Include(p => p.Client)
            .OrderByDescending(p => p.CreatedAt)
            .Take(take)
            .ToListAsync();
    }

    public async Task<PaginatedPaymentsResponse> GetPaymentsPagedAsync(int page = 1, int pageSize = 5)
    {
        var query = _context.Payments
            .Include(p => p.Client)
            .OrderByDescending(p => p.CreatedAt);

        var totalCount = await query.CountAsync();
        var payments = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedPaymentsResponse(
            payments,
            page,
            pageSize,
            totalCount);
    }
}