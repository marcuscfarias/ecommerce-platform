using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Ecommerce.Auth.Domain.Enums;
using Ecommerce.Auth.Infrastructure.Security;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Microsoft.IdentityModel.Tokens;

namespace Ecommerce.Auth.UnitTests.Infrastructure.Security;

public class JwtTokenGeneratorTests
{
    private static readonly Faker Faker = new();

    [Fact]
    public void Generate_WhenCalled_ShouldReturnNonEmptyToken()
    {
        // Arrange
        int userId = Faker.Random.Int(1, int.MaxValue);
        string email = Faker.Internet.Email();
        var sut = CreateSut().Sut;

        // Act
        var result = sut.Generate(userId, email, []);

        // Assert
        result.ShouldNotBeNull();
        result.Token.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Generate_WhenCalled_ShouldSetExpiresInSecondsFromAccessTokenMinutes()
    {
        // Arrange
        int userId = Faker.Random.Int(1, int.MaxValue);
        string email = Faker.Internet.Email();
        var (sut, settings, _) = CreateSut();

        // Act
        var result = sut.Generate(userId, email, []);

        // Assert
        result.ShouldNotBeNull();
        result.ExpiresInSeconds.ShouldBe(settings.AccessTokenMinutes * 60);
    }

    [Fact]
    public void Generate_WhenCalled_ShouldEmitSubClaimWithUserId()
    {
        // Arrange
        var userId = Faker.Random.Int(1, int.MaxValue);
        var email = Faker.Internet.Email();
        var (sut, _, _) = CreateSut();

        // Act
        var result = sut.Generate(userId, email, []);

        // Assert
        var jwt = ReadToken(result.Token);
        var sub = jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub);
        sub.Value.ShouldBe(userId.ToString(CultureInfo.InvariantCulture));
    }

    [Fact]
    public void Generate_WhenCalled_ShouldEmitEmailClaim()
    {
        // Arrange
        var userId = Faker.Random.Int(1, int.MaxValue);
        var email = Faker.Internet.Email();
        var (sut, _, _) = CreateSut();

        // Act
        var result = sut.Generate(userId, email, []);

        // Assert
        var jwt = ReadToken(result.Token);
        var emailClaim = jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email);
        emailClaim.Value.ShouldBe(email);
    }

    [Fact]
    public void Generate_WhenCalled_ShouldEmitJtiClaim()
    {
        // Arrange
        var userId = Faker.Random.Int(1, int.MaxValue);
        var email = Faker.Internet.Email();
        var (sut, _, _) = CreateSut();

        // Act
        var result = sut.Generate(userId, email, []);

        // Assert
        var jwt = ReadToken(result.Token);
        var jti = jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti);
        jti.Value.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Generate_WhenCalled_ShouldEmitUniqueJtiPerCall()
    {
        // Arrange
        var userId = Faker.Random.Int(1, int.MaxValue);
        var email = Faker.Internet.Email();
        var (sut, _, _) = CreateSut();

        // Act
        var first = sut.Generate(userId, email, []);
        var second = sut.Generate(userId, email, []);

        // Assert
        var firstJti = ReadToken(first.Token).Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
        var secondJti = ReadToken(second.Token).Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
        firstJti.ShouldNotBe(secondJti);
    }

    [Fact]
    public void Generate_WhenCalled_ShouldUseConfiguredIssuerAndAudience()
    {
        // Arrange
        var userId = Faker.Random.Int(1, int.MaxValue);
        var email = Faker.Internet.Email();
        var (sut, settings, _) = CreateSut();

        // Act
        var result = sut.Generate(userId, email, []);

        // Assert
        var jwt = ReadToken(result.Token);
        jwt.Issuer.ShouldBe(settings.Issuer);
        jwt.Audiences.Single().ShouldBe(settings.Audience);
    }

    [Fact]
    public void Generate_WhenCalled_ShouldSetNotBeforeToCurrentTime()
    {
        // Arrange
        var userId = Faker.Random.Int(1, int.MaxValue);
        var email = Faker.Internet.Email();
        var (sut, _, clock) = CreateSut();

        // Act
        var result = sut.Generate(userId, email, []);

        // Assert
        var jwt = ReadToken(result.Token);
        jwt.ValidFrom.ShouldBe(clock.GetUtcNow().UtcDateTime);
    }

    [Fact]
    public void Generate_WhenCalled_ShouldSetExpiresToNowPlusAccessTokenMinutes()
    {
        // Arrange
        var userId = Faker.Random.Int(1, int.MaxValue);
        var email = Faker.Internet.Email();
        var (sut, settings, clock) = CreateSut();

        // Act
        var result = sut.Generate(userId, email, []);

        // Assert
        var jwt = ReadToken(result.Token);
        jwt.ValidTo.ShouldBe(clock.GetUtcNow().UtcDateTime.AddMinutes(settings.AccessTokenMinutes));
    }

    [Fact]
    public void Generate_WhenCalled_ShouldSignWithHmacSha256()
    {
        // Arrange
        var userId = Faker.Random.Int(1, int.MaxValue);
        var email = Faker.Internet.Email();
        var (sut, _, _) = CreateSut();

        // Act
        var result = sut.Generate(userId, email, []);

        // Assert
        var jwt = ReadToken(result.Token);
        jwt.SignatureAlgorithm.ShouldBe(SecurityAlgorithms.HmacSha256);
    }

    [Fact]
    public void Generate_WhenCalled_ShouldProduceTokenValidatableWithConfiguredSecret()
    {
        // Arrange
        var userId = Faker.Random.Int(1, int.MaxValue);
        var email = Faker.Internet.Email();
        var (sut, settings, _) = CreateSut();
        var validationParameters = new TokenValidationParameters
        {
            ValidIssuer = settings.Issuer,
            ValidAudience = settings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SecretKey)),
            ValidateLifetime = false,
        };

        // Act
        var result = sut.Generate(userId, email, []);

        // Assert
        Should.NotThrow(() => new JwtSecurityTokenHandler().ValidateToken(result.Token, validationParameters, out _));
    }


    [Fact]
    public void Generate_WhenRolesProvided_ShouldIncludeRoleClaimsInToken()
    {
        // Arrange
        var userId = Faker.Random.Int(1, int.MaxValue);
        var email = Faker.Internet.Email();
        var (sut, _, _) = CreateSut();

        // Act
        var result = sut.Generate(userId, email, [RoleName.Admin, RoleName.Manager]);

        // Assert
        var jwt = ReadToken(result.Token);
        var roleClaims = jwt.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
        roleClaims.ShouldContain(nameof(RoleName.Admin));
        roleClaims.ShouldContain(nameof(RoleName.Manager));
        roleClaims.Count.ShouldBe(2);
    }

    [Fact]
    public void Generate_WhenNoRoles_ShouldNotIncludeRoleClaims()
    {
        // Arrange
        var userId = Faker.Random.Int(1, int.MaxValue);
        var email = Faker.Internet.Email();
        var (sut, _, _) = CreateSut();

        // Act
        var result = sut.Generate(userId, email, []);

        // Assert
        var jwt = ReadToken(result.Token);
        var roleClaims = jwt.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();
        roleClaims.ShouldBeEmpty();
    }

    private static (JwtTokenGenerator Sut, JwtSettings Settings, FakeTimeProvider Clock) CreateSut(
        int accessTokenMinutes = 15)
    {
        DateTimeOffset dateTimeOffset = new(2026, 5, 14, 0, 0, 0, TimeSpan.Zero);
        var clock = new FakeTimeProvider(dateTimeOffset);

        var settings = new Faker<JwtSettings>()
            .RuleFor(s => s.Issuer, f => f.Internet.DomainName())
            .RuleFor(s => s.Audience, f => f.Internet.DomainName())
            .RuleFor(s => s.SecretKey, f => f.Random.String2(64))
            .RuleFor(s => s.AccessTokenMinutes, _ => accessTokenMinutes)
            .Generate();
        var options = Options.Create(settings);

        var sut = new JwtTokenGenerator(options, clock);
        return (sut, settings, clock);
    }

    private static JwtSecurityToken ReadToken(string token) =>
        new JwtSecurityTokenHandler().ReadJwtToken(token);
}
