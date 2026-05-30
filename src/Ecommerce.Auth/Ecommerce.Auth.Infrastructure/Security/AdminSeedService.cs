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

internal sealed partial class AdminSeedService(
    IServiceProvider serviceProvider,
    ILogger<AdminSeedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var settings = scope.ServiceProvider.GetRequiredService<IOptions<AdminSeedSettings>>().Value;

        var normalizedEmail = settings.Email.Trim().ToLowerInvariant();

        var exists = await context.Users.AnyAsync(u => u.Email == normalizedEmail, cancellationToken);
        if (exists)
        {
            LogAdminAlreadyExists(logger);
            return;
        }

        var adminRole = await context.Roles.SingleAsync(r => r.Name == nameof(RoleName.Admin), cancellationToken);

        var passwordHash = passwordHasher.Hash(settings.Password);
        var user = new User(normalizedEmail, passwordHash, settings.Name);
        user.AssignRole(adminRole);

        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);

        LogAdminSeeded(logger);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    [LoggerMessage(Level = LogLevel.Information, Message = "Admin user already exists, skipping seed")]
    private static partial void LogAdminAlreadyExists(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Admin user seeded successfully")]
    private static partial void LogAdminSeeded(ILogger logger);
}
