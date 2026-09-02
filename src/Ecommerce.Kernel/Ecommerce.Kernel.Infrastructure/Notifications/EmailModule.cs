using System.Net.Http.Headers;
using Ecommerce.Kernel.Application.Notifications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Ecommerce.Kernel.Infrastructure.Notifications;

public static class EmailModule
{
    private const string ResendBaseAddress = "https://api.resend.com";

    private static readonly TimeSpan ResendTimeout = TimeSpan.FromSeconds(10);

    public static IServiceCollection AddEmailSender(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<EmailOptions>()
            .Bind(configuration.GetSection(EmailOptions.SectionName))
            .Validate(o => Enum.IsDefined(o.Provider), "Email:Provider must be 'Console' or 'Resend'.")
            .Validate(o => !string.IsNullOrWhiteSpace(o.FromAddress), "Email:FromAddress is required.")
            .Validate(
                o => o.Provider != EmailProvider.Resend || !string.IsNullOrWhiteSpace(o.Resend.ApiKey),
                "Email:Resend:ApiKey is required when Email:Provider is 'Resend'.")
            .ValidateOnStart();

        services.AddScoped<ConsoleEmailSender>();

        services.AddHttpClient<ResendEmailSender>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<EmailOptions>>().Value;

            client.BaseAddress = new Uri(ResendBaseAddress);
            client.Timeout = ResendTimeout;
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", options.Resend.ApiKey);
        });

        services.AddScoped<IEmailSender>(sp =>
            sp.GetRequiredService<IOptions<EmailOptions>>().Value.Provider switch
            {
                EmailProvider.Resend => sp.GetRequiredService<ResendEmailSender>(),
                _ => sp.GetRequiredService<ConsoleEmailSender>(),
            });

        return services;
    }
}
