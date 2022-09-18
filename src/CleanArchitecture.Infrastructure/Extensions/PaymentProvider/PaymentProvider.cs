using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Infrastructure.Extensions.PaymentProvider
{
    public class PaymentProvider : IPaymentProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public PaymentProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<PaymentResult> ProcessAsync(IDictionary<string, object> details)
        {
            details = details.ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.InvariantCultureIgnoreCase);

            var paymentMethod = details[PaymentProperties.PaymentMethod].To<PaymentMethod>();
            var paymentProcessor = GetPaymentProcessor(paymentMethod);
            var paymentResult = await paymentProcessor.ProcessAsync(details);

            if (paymentResult.Success)
            {
                var payment = new Payment();
            }

            return paymentResult;
        }

        IPaymentProcessor GetPaymentProcessor(PaymentMethod paymentMethod)
        {
            return _serviceProvider.GetServices<IPaymentProcessor>().First(_ => ((IPaymentMethod)_).SupportedMethods.HasFlag(paymentMethod));
        }
    }
}