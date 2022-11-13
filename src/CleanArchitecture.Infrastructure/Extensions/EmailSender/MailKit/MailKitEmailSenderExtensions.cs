using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Infrastructure.Extensions.EmailSender.MailKit
{
    public static class MailKitEmailSenderExtensions
    {
        public static IServiceCollection AddMailKitEmailSender(this IServiceCollection services, Action<MailKitEmailSenderOptions> configure)
        {
            services.Configure(configure);
            services.AddScoped<IEmailSender, MailKitEmailSender>();
            return services;
        }
    }
}
