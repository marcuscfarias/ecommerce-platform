using Ecommerce.Kernel.Domain.Exceptions;

namespace Ecommerce.Catalog.Application.Exceptions;

public sealed class SkuConflictException(string sku)
    : Exception($"A product with SKU '{sku}' already exists."), IExceptionContract
{
    public int StatusCode => 409;
}
