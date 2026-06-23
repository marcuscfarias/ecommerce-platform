using Ecommerce.Auth.Application.Auth.Security;
using Ecommerce.Auth.Domain.Entities;
using Ecommerce.Auth.Domain.Enums;
using Ecommerce.Auth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ecommerce.Auth.Infrastructure.Security;

internal sealed partial class UserSeedService(
    IServiceProvider serviceProvider,
    ILogger<UserSeedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var seedUsers = scope.ServiceProvider.GetRequiredService<IOptions<SeedUsersSettings>>().Value;

        if (seedUsers.Users.Count == 0)
            return;

        var rolesByName = await context.Roles.ToDictionaryAsync(r => r.Name, cancellationToken);

        foreach (var seed in seedUsers.Users)
            await SeedUserAsync(context, passwordHasher, rolesByName, seed, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task SeedUserAsync(
        AuthDbContext context,
        IPasswordHasher passwordHasher,
        Dictionary<string, Role> rolesByName,
        SeedUserSettings seed,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = seed.Email.Trim().ToLowerInvariant();

        var exists = await context.Users.AnyAsync(u => u.Email == normalizedEmail, cancellationToken);
        if (exists)
        {
            LogUserAlreadyExists(logger, normalizedEmail);
            return;
        }

        if (!rolesByName.TryGetValue(seed.Role.ToString(), out var role))
            throw new InvalidOperationException($"Seed role '{seed.Role}' was not found.");

        var passwordHash = passwordHasher.Hash(seed.Password);
        var user = new User(normalizedEmail, passwordHash, seed.Name);
        user.AssignRole(role);

        context.Users.Add(user);

        LogUserSeeded(logger, normalizedEmail, seed.Role);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Seed user {Email} already exists, skipping")]
    private static partial void LogUserAlreadyExists(ILogger logger, string email);

    [LoggerMessage(Level = LogLevel.Information, Message = "Seed user {Email} created with role {Role}")]
    private static partial void LogUserSeeded(ILogger logger, string email, RoleName role);
}
