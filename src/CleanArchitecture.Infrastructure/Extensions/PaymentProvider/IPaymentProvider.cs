using CleanArchitecture.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Infrastructure.Extensions.PaymentProvider
{

    public interface IPaymentProvider
    {
        Task<PaymentResult> MapAsync(Payment payment, IDictionary<string, string> details, CancellationToken cancellationToken = default);

        Task<PaymentResult> ProcessAsync(Payment payment, CancellationToken cancellationToken = default);

        Task VerifyAsync(Payment payment, CancellationToken cancellationToken = default);
    }
}
