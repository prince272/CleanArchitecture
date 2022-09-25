using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Infrastructure.Extensions.PaymentProvider.PaySwitch
{
    public class PaySwitchPaymentProcessorOptions
    {
        public string ClientId { get; set; } = null!;

        public string ClientSecret { get; set; } = null!;

        public string MerchantId { get; set; } = null!;

        public string MerchantSecret { get; set; } = null!;
    }
}
