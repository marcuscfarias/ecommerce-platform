namespace Ecommerce.Auth.IntegrationTests.Base;

[CollectionDefinition(nameof(RateLimitedAuthTestCollection))]
public class RateLimitedAuthTestCollection : ICollectionFixture<RateLimitedAuthIntegrationFixture>;
