using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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
