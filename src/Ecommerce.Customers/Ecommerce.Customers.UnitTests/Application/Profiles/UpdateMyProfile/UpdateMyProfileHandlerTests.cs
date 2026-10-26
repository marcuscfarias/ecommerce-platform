using Ecommerce.Customers.Application.Profiles;
using Ecommerce.Customers.Application.Profiles.UpdateMyProfile;
using Ecommerce.Customers.Domain.Entities;
using Ecommerce.Customers.Domain.Repositories;
using Ecommerce.Kernel.Application.Security;
using Microsoft.Extensions.Logging.Testing;

namespace Ecommerce.Customers.UnitTests.Application.Profiles.UpdateMyProfile;

public class UpdateMyProfileHandlerTests
{
    private readonly ICustomerRepository _repository = Substitute.For<ICustomerRepository>();
    private readonly IUserContext _userContext = Substitute.For<IUserContext>();
    private readonly Faker _faker = new();
    private readonly List<Customer> _added = [];
    private readonly int _userId;
    private readonly UpdateMyProfileCommand _command;
    private readonly UpdateMyProfileHandler _handler;

    public UpdateMyProfileHandlerTests()
    {
        _userId = _faker.Random.Int(1);
        _userContext.UserId.Returns(_userId);
        _repository.Add(Arg.Do<Customer>(_added.Add));
        _command = new UpdateMyProfileCommand(_faker.Name.FullName(), $"+55{_faker.Random.ReplaceNumbers("###########")}");

        var currentProfile = new CurrentCustomerProfile(
            _repository, _userContext, new FakeLogger<CurrentCustomerProfile>());

        _handler = new UpdateMyProfileHandler(currentProfile, _repository);
    }

    [Fact]
    public async Task Handle_WhenProfileExists_ShouldReplaceBothFieldsAndSave()
    {
        // Arrange
        var existing = new Customer(_userId, _faker.Name.FullName());
        _repository.GetByUserIdAsync(_userId, Arg.Any<CancellationToken>()).Returns(existing);

        // Act
        await _handler.Handle(_command, CancellationToken.None);

        // Assert
        existing.DisplayName.ShouldBe(_command.DisplayName);
        existing.Phone.ShouldBe(_command.Phone);
        _repository.Received(1).Update(existing);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenProfileIsMissing_ShouldCreateThenApplySubmittedValues()
    {
        // Arrange
        _userContext.Name.Returns(_faker.Name.FullName());

        // Act
        await _handler.Handle(_command, CancellationToken.None);

        // Assert
        var added = _added.ShouldHaveSingleItem();
        added.UserId.ShouldBe(_userId);
        added.DisplayName.ShouldBe(_command.DisplayName);
        added.Phone.ShouldBe(_command.Phone);
        Received.InOrder(() =>
        {
            _repository.Add(added);
            _repository.SaveChangesAsync(Arg.Any<CancellationToken>());
            _repository.Update(added);
            _repository.SaveChangesAsync(Arg.Any<CancellationToken>());
        });
    }
}
