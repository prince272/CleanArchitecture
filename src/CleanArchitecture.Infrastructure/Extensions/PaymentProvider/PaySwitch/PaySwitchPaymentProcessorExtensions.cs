using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Infrastructure.Extensions.PaymentProvider.PaySwitch
{
    public static class PaySwitchPaymentProcessorExtensions
    {
        public static void AddPaySwitchProvider(this IServiceCollection services, Action<PaySwitchPaymentProcessorOptions> configure)
        {
            services.TryAddTransient<IPaymentProvider, PaymentProvider>();
            services.Configure(configure);
            services.AddTransient<IPaymentProcessor, PaySwitchPaymentProcessor>();
        }
    }
}
