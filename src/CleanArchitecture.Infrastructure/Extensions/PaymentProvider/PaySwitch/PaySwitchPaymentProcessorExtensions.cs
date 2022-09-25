using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Infrastructure.Extensions.PaymentProvider.PaySwitch
{
    public static class PaySwitchPaymentProcessorExtensions
    {
        public static void AddPaySwitchProvider(this IServiceCollection services, Action<PaySwitchPaymentProcessorOptions> configure)
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
        }
    }
}
