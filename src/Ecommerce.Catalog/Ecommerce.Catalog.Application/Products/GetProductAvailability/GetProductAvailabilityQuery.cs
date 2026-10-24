using Ecommerce.Kernel.Application.CQRS;

namespace Ecommerce.Catalog.Application.Products.GetProductAvailability;

public sealed record GetProductAvailabilityQuery(IReadOnlyCollection<int> ProductIds) : IQuery<GetProductAvailabilityResult>;
