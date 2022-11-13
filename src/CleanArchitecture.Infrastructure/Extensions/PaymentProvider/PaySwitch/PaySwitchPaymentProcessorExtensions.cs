using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net;

namespace CleanArchitecture.Infrastructure.Extensions.PaymentProvider.PaySwitch
{
    public static class PaySwitchPaymentProcessorExtensions
    {
        public static IServiceCollection AddPaySwitchProcessor(this IServiceCollection services, Action<PaySwitchPaymentProcessorOptions> configure)
        {
            services.TryAddTransient<IPaymentProvider, PaymentProvider>();
            services.AddHttpClient(nameof(PaySwitchPaymentProcessor))
                 .ConfigurePrimaryHttpMessageHandler(_ =>
                 {
                     var handler = new HttpClientHandler();
                     if (handler.SupportsAutomaticDecompression)
                     {
                         handler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                     }

                     return handler;
                 });
            services.Configure(configure);
            services.AddTransient<IPaymentProcessor, PaySwitchPaymentProcessor>();
            return services;
        }
    }
}
