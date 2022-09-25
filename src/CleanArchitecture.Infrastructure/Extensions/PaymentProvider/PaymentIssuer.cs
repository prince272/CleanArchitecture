using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Infrastructure.Extensions.PaymentProvider
{
    public class PaymentIssuer
    {
        public PaymentIssuer(string code, string pattern, string name)
        {
            Code = code;
            Pattern = pattern;
            Name = name;
        }

        public string Code { get; }

        public string Pattern { get; }

        public string Name { get; }
    }
}
