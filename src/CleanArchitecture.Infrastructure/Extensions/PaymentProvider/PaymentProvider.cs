using CleanArchitecture.Core;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Utilities;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CleanArchitecture.Infrastructure.Extensions.PaymentProvider
{
    public class PaymentProvider : IPaymentProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public PaymentProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        private IPaymentProcessor[] GetPaymentProcessors()
        {
            return _serviceProvider.GetServices<IPaymentProcessor>().ToArray();
        }

        IPaymentProcessor? GetPaymentProcessor(string? paymentGateway)
        {
            return _serviceProvider.GetServices<IPaymentProcessor>().FirstOrDefault(_ => _.Gateway.Equals(paymentGateway, StringComparison.InvariantCultureIgnoreCase));
        }

        public async Task<PaymentResult> MapAsync(Payment payment, IDictionary<string, string> details, CancellationToken cancellationToken = default)
        {
            if (payment == null) throw new ArgumentNullException(nameof(payment));

            details = details.ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.InvariantCultureIgnoreCase);

            var paymentProcessors = GetPaymentProcessors();
            var paymentOption = details.GetValueOrDefault(PaymentProperties.PaymentOption);

            if (string.IsNullOrWhiteSpace(paymentOption))
            {
                return PaymentResult.Failed(new Dictionary<string, string[]>() { {
                       PaymentProperties.PaymentOption, new[] { $"'{PaymentProperties.PaymentOption.Humanize()}' must not be empty." } } });
            }

            if (!Enum.TryParse<PaymentMethod>(paymentOption, true, out var paymentMethod))
            {
                payment.Gateway = paymentProcessors.Where(_ => _.Gateway.Equals(paymentOption, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault()?.Gateway;
                payment.Method = PaymentMethod.Default;

                if (string.IsNullOrWhiteSpace(payment.Gateway))
                    return PaymentResult.Failed(new Dictionary<string, string[]>() { {
                       PaymentProperties.PaymentOption, new[] { $"'{PaymentProperties.PaymentOption.Humanize()}' is not supported." } } });
            }
            else
            {
                payment.Gateway = paymentProcessors.Where(_ => _.Method.HasFlag(paymentMethod)).First().Gateway;
                payment.Method = paymentMethod;
            }

            var paymentProcessor = GetPaymentProcessor(payment.Gateway);
            if (paymentProcessor == null) throw new InvalidOperationException($"The payment gateway has no registered service for the type '{typeof(IPaymentProcessor)}'.");

            if (payment.Method == PaymentMethod.MobileMoney)
            {
                payment.MobileNumber = details.GetValueOrDefault(nameof(PaymentProperties.MobileNumber))?.ToString();

                if (string.IsNullOrWhiteSpace(payment.MobileNumber))
                {
                    return PaymentResult.Failed(new Dictionary<string, string[]>() { {
                               nameof(PaymentProperties.MobileNumber), new[] { $"'{nameof(PaymentProperties.MobileNumber).Humanize()}' must not be empty." } } });
                }

                if (!ContactHelper.TryParsePhoneNumber(payment.MobileNumber, out var mobileDetails))
                {
                    return PaymentResult.Failed(new Dictionary<string, string[]>() { {
                               nameof(PaymentProperties.MobileNumber), new[] { $"'{nameof(PaymentProperties.MobileNumber).Humanize()}' is not valid." } } });
                }

                payment.MobileNumber = mobileDetails.Number;
                payment.MobileIssuer = await paymentProcessor.GetMobileIssuerAsync(mobileDetails.Number);

                if (payment.MobileIssuer == null)
                {
                    return PaymentResult.Failed(new Dictionary<string, string[]>() { {
                               nameof(PaymentProperties.MobileNumber), new[] { $"'{nameof(PaymentProperties.MobileNumber).Humanize()}' is not supported." } } });
                }
            }

            payment.TransactionId = paymentProcessor.GenerateTransactionId();
            return PaymentResult.Succeeded();
        }

        public Task<PaymentResult> ProcessAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            if (payment == null) throw new ArgumentNullException(nameof(payment));

            var paymentProcessor = GetPaymentProcessor(payment.Gateway);
            if (paymentProcessor == null) throw new InvalidOperationException($"The payment gateway has no registered service for the type '{typeof(IPaymentProcessor)}'.");

            return paymentProcessor.ProcessAsync(payment, cancellationToken);
        }

        public Task VerifyAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            if (payment == null) throw new ArgumentNullException(nameof(payment));

            var paymentProcessor = GetPaymentProcessor(payment.Gateway);
            if (paymentProcessor == null) throw new InvalidOperationException($"The payment gateway has no registered service for the type '{typeof(IPaymentProcessor)}'.");

            return paymentProcessor.VerifyAsync(payment, cancellationToken);
        }
    }
}