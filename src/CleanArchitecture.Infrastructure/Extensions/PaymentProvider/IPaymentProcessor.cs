using CleanArchitecture.Infrastructure.Entities;

namespace CleanArchitecture.Infrastructure.Extensions.PaymentProvider
{
    public interface IPaymentProcessor
    {
        string Gateway { get; }

        PaymentMethod Method { get; }

        string GenerateTransactionId();

        Task<PaymentIssuer?> GetMobileIssuerAsync(string mobileNumber);

        Task<PaymentResult> ProcessAsync(Payment payment, CancellationToken cancellationToken = default);

        Task VerifyAsync(Payment payment, CancellationToken cancellationToken = default);
    }
}
