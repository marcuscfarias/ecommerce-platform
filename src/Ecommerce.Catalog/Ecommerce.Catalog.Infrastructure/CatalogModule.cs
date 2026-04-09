using Ecommerce.Catalog.Application;
using Ecommerce.Shared.Infrastructure;
using MediatR;

namespace Ecommerce.Catalog.Infrastructure;

internal sealed class CatalogModule(ISender sender) : Module(sender), ICatalogModule { }
