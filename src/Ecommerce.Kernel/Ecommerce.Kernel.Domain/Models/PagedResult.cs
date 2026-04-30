namespace Ecommerce.Kernel.Domain.Models;

public record PagedResult<T>(IReadOnlyList<T> Data, int Page, int TotalCount, int TotalPages);
