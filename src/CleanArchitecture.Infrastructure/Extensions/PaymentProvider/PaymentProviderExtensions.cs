using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Infrastructure.Extensions.PaymentProvider
{
    public static class PaymentProviderExtensions
    {
        public static IServiceCollection AddPaymentProvider(this IServiceCollection services)
        {
            services.AddHostedService<PaymentWorker>();
            services.AddTransient<IPaymentProvider, PaymentProvider>();
            return services;
        }
    }
}
