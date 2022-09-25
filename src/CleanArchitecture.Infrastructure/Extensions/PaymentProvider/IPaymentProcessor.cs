using CleanArchitecture.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Infrastructure.Extensions.PaymentProvider
{
    public interface IPaymentProcessor
    {
        Task<PaymentResult> ProcessAsync(Payment payment, IDictionary<string, object> details);
    }
}
