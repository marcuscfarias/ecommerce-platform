using System.Globalization;
using Ecommerce.Auth.Application.Auth.EmailConfirmation;
using Ecommerce.Auth.Application.Auth.RegisterCustomer;
using Ecommerce.Auth.Application.Auth.Security;
using Ecommerce.Auth.Domain.Entities;
using Ecommerce.Auth.Domain.Enums;
using Ecommerce.Auth.Domain.Repositories;
using Ecommerce.Kernel.Application.Notifications;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;

namespace Ecommerce.Auth.UnitTests.Application.Auth.RegisterCustomer;

public class RegisterCustomerHandlerTests
{
    private readonly IAuthRepository _repository = Substitute.For<IAuthRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IEmailConfirmationTokenFactory _tokenFactory = Substitute.For<IEmailConfirmationTokenFactory>();
    private readonly IEmailConfirmationLinks _links = Substitute.For<IEmailConfirmationLinks>();
    private readonly IEmailSender _emailSender = Substitute.For<IEmailSender>();
    private readonly FakeLogger<RegisterCustomerHandler> _logger = new();
    private readonly Faker _faker = new();
    private readonly List<EmailMessage> _sentEmails = [];
    private readonly RegisterCustomerCommand _command;
    private readonly string _passwordHash;
    private readonly RegisterCustomerHandler _handler;

    public RegisterCustomerHandlerTests()
    {
        _command = new RegisterCustomerCommand(_faker.Person.FullName, _faker.Internet.Email(), _faker.Internet.Password());
        _passwordHash = _faker.Random.Hash();

        _passwordHasher.Hash(_command.Password).Returns(_passwordHash);
        _repository.GetRoleByNameAsync(RoleName.Customer, Arg.Any<CancellationToken>())
            .Returns(new Role(nameof(RoleName.Customer)));
        _repository.GetActiveUserTokensAsync(default, default, default).ReturnsForAnyArgs([]);
        _tokenFactory.Create().Returns(new EmailConfirmationTokenPair(_faker.Random.AlphaNumeric(43), _faker.Random.Hash()));
        _tokenFactory.Lifetime.Returns(TimeSpan.FromHours(24));
        _links.ConfirmationLink(Arg.Any<string>())
            .Returns(ci => new Uri($"https://{_faker.Internet.DomainName()}/api/v1/auth/confirm-email?token={ci.Arg<string>()}"));
        _emailSender
            .SendAsync(Arg.Do<EmailMessage>(_sentEmails.Add), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var dispatcher = new EmailConfirmationDispatcher(
            _repository, _tokenFactory, _links, _emailSender, new FakeTimeProvider(),
            new FakeLogger<EmailConfirmationDispatcher>());

        _handler = new RegisterCustomerHandler(_repository, _passwordHasher, dispatcher, _logger);
    }

    [Fact]
    public async Task Handle_WhenEmailIsNew_ShouldAddUnconfirmedCustomerWithHashedPassword()
    {
        // Arrange
        User? added = null;
        _repository.Add(Arg.Do<User>(u => added = u));

        // Act
        await _handler.Handle(_command, CancellationToken.None);

        // Assert
        added.ShouldNotBeNull();
        added.Email.ShouldBe(_command.Email);
        added.Name.ShouldBe(_command.Name);
        added.PasswordHash.ShouldBe(_passwordHash);
        added.EmailConfirmed.ShouldBeFalse();
        added.IsCustomer().ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_WhenEmailIsNew_ShouldSaveUserBeforeIssuingTokenAndSendOneEmail()
    {
        // Act
        await _handler.Handle(_command, CancellationToken.None);

        // Assert
        Received.InOrder(() =>
        {
            _repository.Add(Arg.Any<User>());
            _repository.SaveChangesAsync(Arg.Any<CancellationToken>());
            _repository.AddUserToken(Arg.Any<UserToken>());
            _repository.SaveChangesAsync(Arg.Any<CancellationToken>());
        });
        _sentEmails.ShouldHaveSingleItem().To.ShouldBe(_command.Email);
    }

    [Fact]
    public async Task Handle_WhenEmailIsPending_ShouldSendOneEmailAndKeepNameAndHash()
    {
        // Arrange
        var existing = FakeUser(emailConfirmed: false);
        var originalName = existing.Name;
        var originalHash = existing.PasswordHash;
        _repository.GetByEmailAsync(_command.Email, Arg.Any<CancellationToken>()).Returns(existing);

        // Act
        await _handler.Handle(_command, CancellationToken.None);

        // Assert
        _sentEmails.ShouldHaveSingleItem().To.ShouldBe(existing.Email);
        _repository.Received(1).AddUserToken(Arg.Is<UserToken>(t => t.UserId == existing.Id));
        existing.Name.ShouldBe(originalName);
        existing.PasswordHash.ShouldBe(originalHash);
    }

    [Fact]
    public async Task Handle_WhenEmailIsConfirmed_ShouldSendNoEmailAndKeepTheUser()
    {
        // Arrange
        var existing = FakeUser(emailConfirmed: true);
        var originalName = existing.Name;
        var originalHash = existing.PasswordHash;
        _repository.GetByEmailAsync(_command.Email, Arg.Any<CancellationToken>()).Returns(existing);

        // Act
        await _handler.Handle(_command, CancellationToken.None);

        // Assert
        _sentEmails.ShouldBeEmpty();
        existing.Name.ShouldBe(originalName);
        existing.PasswordHash.ShouldBe(originalHash);
        existing.EmailConfirmed.ShouldBeTrue();
    }

    [Theory]
    [InlineData(null)]  // new address
    [InlineData(false)] // existing address, email pending
    [InlineData(true)]  // existing address, email confirmed
    public async Task Handle_WhenCalled_ShouldHashThePasswordExactlyOnce(bool? existingEmailConfirmed)
    {
        // Arrange
        if (existingEmailConfirmed is { } confirmed)
        {
            _repository.GetByEmailAsync(_command.Email, Arg.Any<CancellationToken>())
                .Returns(FakeUser(confirmed));
        }

        // Act
        await _handler.Handle(_command, CancellationToken.None);

        // Assert
        _passwordHasher.Received(1).Hash(_command.Password);
    }

    [Fact]
    public async Task Handle_WhenEmailIsNew_ShouldLogCustomerRegisteredWithUserIdAndNoPii()
    {
        // Act
        await _handler.Handle(_command, CancellationToken.None);

        // Assert
        var record = _logger.Collector.GetSnapshot().ShouldHaveSingleItem();
        record.Level.ShouldBe(LogLevel.Information);
        record.Id.Id.ShouldBe(1);
        record.StructuredState.ShouldNotBeNull().ShouldContain(e => e.Key == "UserId");
        record.Message.ShouldNotContain(_command.Email);
    }

    [Theory]
    [InlineData(false)] // email pending
    [InlineData(true)]  // email confirmed
    public async Task Handle_WhenEmailExists_ShouldLogExistingAccountHitWithUserIdAndNoPii(bool emailConfirmed)
    {
        // Arrange
        var existing = FakeUser(emailConfirmed);
        _repository.GetByEmailAsync(_command.Email, Arg.Any<CancellationToken>()).Returns(existing);

        // Act
        await _handler.Handle(_command, CancellationToken.None);

        // Assert
        var record = _logger.Collector.GetSnapshot().ShouldHaveSingleItem();
        record.Level.ShouldBe(LogLevel.Information);
        record.Id.Id.ShouldBe(2);
        record.StructuredState.ShouldNotBeNull()
            .ShouldContain(new KeyValuePair<string, string?>("UserId", existing.Id.ToString(CultureInfo.InvariantCulture)));
        record.Message.ShouldNotContain(existing.Email);
    }

    private User FakeUser(bool emailConfirmed) =>
        new(_command.Email, _faker.Random.Hash(), _faker.Person.FullName, emailConfirmed: emailConfirmed);
}
