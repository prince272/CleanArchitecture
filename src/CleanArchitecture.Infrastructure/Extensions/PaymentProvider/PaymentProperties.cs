using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Infrastructure.Extensions.PaymentProvider
{
    public class PaymentProperties
    {
        public const string PaymentId = nameof(PaymentId);

        public const string PaymentMethod = nameof(PaymentMethod);

        public const string Description = nameof(Description);

        public const string Email = nameof(Email);

        public const string PhoneNumber = nameof(PhoneNumber);
        
        public const string Amount = nameof(Amount);
    }
}
