using Ecommerce.Auth.Application.Auth.EmailConfirmation;
using Ecommerce.Auth.Application.Auth.ResendEmailConfirmation;
using Ecommerce.Auth.Application.Auth.Security;
using Ecommerce.Auth.Domain.Entities;
using Ecommerce.Auth.Domain.Repositories;
using Ecommerce.Kernel.Application.Notifications;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;

namespace Ecommerce.Auth.UnitTests.Application.Auth.ResendEmailConfirmation;

public class ResendEmailConfirmationHandlerTests
{
    private readonly IAuthRepository _repository = Substitute.For<IAuthRepository>();
    private readonly IEmailConfirmationTokenFactory _tokenFactory = Substitute.For<IEmailConfirmationTokenFactory>();
    private readonly IEmailConfirmationLinks _links = Substitute.For<IEmailConfirmationLinks>();
    private readonly IEmailSender _emailSender = Substitute.For<IEmailSender>();
    private readonly Faker _faker = new();
    private readonly List<EmailMessage> _sentEmails = [];
    private readonly ResendEmailConfirmationCommand _command;
    private readonly ResendEmailConfirmationHandler _handler;

    public ResendEmailConfirmationHandlerTests()
    {
        _command = new ResendEmailConfirmationCommand(_faker.Internet.Email());

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

        _handler = new ResendEmailConfirmationHandler(_repository, dispatcher);
    }

    [Fact]
    public async Task Handle_WhenEmailIsPending_ShouldIssueTokenAndSendOneEmail()
    {
        // Arrange
        var user = FakeUser(emailConfirmed: false);
        _repository.GetByEmailAsync(_command.Email, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        await _handler.Handle(_command, CancellationToken.None);

        // Assert
        _repository.Received(1).AddUserToken(Arg.Is<UserToken>(t => t.UserId == user.Id));
        _sentEmails.ShouldHaveSingleItem().To.ShouldBe(user.Email);
    }

    [Fact]
    public async Task Handle_WhenEmailIsUnknown_ShouldSendNoEmail()
    {
        // Arrange
        _repository.GetByEmailAsync(_command.Email, Arg.Any<CancellationToken>()).Returns((User?)null);

        // Act
        await _handler.Handle(_command, CancellationToken.None);

        // Assert
        _sentEmails.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenEmailIsConfirmed_ShouldSendNoEmail()
    {
        // Arrange
        _repository.GetByEmailAsync(_command.Email, Arg.Any<CancellationToken>()).Returns(FakeUser(emailConfirmed: true));

        // Act
        await _handler.Handle(_command, CancellationToken.None);

        // Assert
        _sentEmails.ShouldBeEmpty();
    }

    private User FakeUser(bool emailConfirmed) =>
        new(_command.Email, _faker.Random.Hash(), _faker.Person.FullName, emailConfirmed: emailConfirmed);
}
