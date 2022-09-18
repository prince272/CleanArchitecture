using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Infrastructure.Extensions.PaymentProvider.PaySwitch
{
    public class PaySwitchPaymentProcessor : IPaymentMethod, IPaymentProcessor
    {
        private readonly PaySwitchPaymentProcessorOptions _paySwitchPaymentMethodOptions;

        public PaySwitchPaymentProcessor(IOptions<PaySwitchPaymentProcessorOptions> paySwitchPaymentMethodOptions)
        {
            _paySwitchPaymentMethodOptions = paySwitchPaymentMethodOptions.Value ?? throw new ArgumentNullException(nameof(paySwitchPaymentMethodOptions));
        }

        public PaymentMethod SupportedMethods => PaymentMethod.PaySwitch;

        public async Task<PaymentResult> ProcessAsync(IDictionary<string, object> details)
        {
            return PaymentResult.Failed(message: "Unable to process payment.");
        }
    }
}
