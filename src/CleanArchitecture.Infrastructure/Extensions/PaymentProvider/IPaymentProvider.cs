using CleanArchitecture.Infrastructure.Entities;

namespace CleanArchitecture.Infrastructure.Extensions.PaymentProvider
{

    public interface IPaymentProvider
    {
        Task<PaymentResult> MapAsync(Payment payment, IDictionary<string, string> details, CancellationToken cancellationToken = default);

        Task<PaymentResult> ProcessAsync(Payment payment, CancellationToken cancellationToken = default);

        Task VerifyAsync(Payment payment, CancellationToken cancellationToken = default);
    }
}
