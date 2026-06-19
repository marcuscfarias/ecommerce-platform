namespace Ecommerce.Auth.IntegrationTests.Base.RateLimited;

[CollectionDefinition(nameof(RateLimitedAuthTestCollection))]
public class RateLimitedAuthTestCollection : ICollectionFixture<RateLimitedAuthIntegrationFixture>;
