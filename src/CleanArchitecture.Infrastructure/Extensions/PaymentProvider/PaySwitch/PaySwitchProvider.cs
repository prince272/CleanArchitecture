using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Infrastructure.Extensions.PaymentProvider.PaySwitch
{
    public class PaySwitchProvider : IPaymentProvider
    {
        private readonly PaySwitchOptions _paySwitchOptions;

        public PaySwitchProvider(IOptions<PaySwitchOptions> paySwitchOptions)
        {
            _paySwitchOptions = paySwitchOptions.Value ?? throw new ArgumentNullException(nameof(paySwitchOptions));
        }

        public string Name => "PaySwitch";
    }
}
