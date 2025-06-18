using Api.Models;

namespace Api.Contracts;

public record PaginatedPaymentsResponse(
    List<Payment> Payments,
    int Page,
    int PageSize,
    int TotalCount);