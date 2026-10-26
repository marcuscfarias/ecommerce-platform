namespace Ecommerce.Orders.Domain.Catalog;

public interface ICatalogProducts
{
    Task<IReadOnlyDictionary<int, CatalogProduct>> GetAsync(
        IReadOnlyCollection<int> productIds, CancellationToken cancellationToken);
}
