using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Infrastructure.Extensions.SmsSender.Twilio
{
    public static class SmsSenderExtensions
    {
        public static IServiceCollection AddSmsSender(this IServiceCollection services, Action<SmsSenderOptions> configure)
        {
            services.Configure(configure);
            services.AddScoped<ISmsSender, SmsSender>();
            return services;
        }
    }
}
