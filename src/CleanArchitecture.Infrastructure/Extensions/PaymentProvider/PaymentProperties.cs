using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Infrastructure.Extensions.PaymentProvider
{
    public class PaymentProperties
    {
        public const string Id = nameof(Id);

        public const string Method = nameof(Method);

        public const string Description = nameof(Description);

        public const string EmailAddress = nameof(EmailAddress);

        public const string MobileNumber = nameof(MobileNumber);

        public static string MobileIssuer = nameof(MobileIssuer);

        public const string Amount = nameof(Amount);

        public const string Type = nameof(Type);
    }
}
