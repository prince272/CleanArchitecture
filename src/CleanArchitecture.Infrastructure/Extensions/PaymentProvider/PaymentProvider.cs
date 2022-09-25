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
        private readonly IUnitOfWork _unitOfWork;

        public PaymentProvider(IServiceProvider serviceProvider, IUnitOfWork unitOfWork)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        PaymentIssuer? GetMobileIssuer(string mobileNumber)
        {
            var mobileIssuers = new List<PaymentIssuer>()
            {
               new ("MTN", "^\\+233(24|54|55|59)", "MTN"),
               new ("MTN", "^\\+233(20|50)", "Vodafone"),
               new ("MTN", "^\\+233(27|57|26|56)", "AirtelTigo"),
            };

            return mobileIssuers.FirstOrDefault(_ => Regex.IsMatch($"{mobileNumber}", _.Pattern));
        }

        public async Task<PaymentResult> ProcessAsync(Payment payment, IDictionary<string, object> details)
        {
            var errors = new Dictionary<string, string[]>();
            details = details.ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.InvariantCultureIgnoreCase);

            // Payment Method Validation
            var paymentMethodValue = details.TryGetValue(PaymentProperties.Method)?.ToString();

            if (string.IsNullOrWhiteSpace(paymentMethodValue))
            {
                return PaymentResult.Failed(new Dictionary<string, string[]>() { {
                       PaymentProperties.Method, new[] { $"'{PaymentProperties.Method.Humanize()}' must not be empty." } } });
            }

            if (!paymentMethodValue.To(out PaymentMethod paymentMethod))
            {
                return PaymentResult.Failed(new Dictionary<string, string[]>() { {
                       PaymentProperties.Method, new[] { $"'{PaymentProperties.Method.Humanize()}' is not valid." } } });
            }

            details[PaymentProperties.Method] = paymentMethod;

            if (payment.Type == PaymentType.Debit)
            {
                if (paymentMethod == PaymentMethod.MobileMoney)
                {
                    // MobileNumber Validation
                    var mobileNumber = details.TryGetValue(PaymentProperties.MobileNumber)?.ToString();

                    if (string.IsNullOrWhiteSpace(mobileNumber))
                    {
                        return PaymentResult.Failed(new Dictionary<string, string[]>() { {
                               PaymentProperties.MobileNumber, new[] { $"'{PaymentProperties.MobileNumber.Humanize()}' must not be empty." } } });
                    }

                    if (!ContactHelper.TryParsePhoneNumber(mobileNumber, out var mobileDetails))
                    {
                        return PaymentResult.Failed(new Dictionary<string, string[]>() { {
                               PaymentProperties.MobileNumber, new[] { $"'{PaymentProperties.MobileNumber.Humanize()}' is not valid." } } });
                    }

                    mobileNumber = mobileDetails.Number;
                    var mobileIssuer = GetMobileIssuer(mobileNumber);

                    if (mobileIssuer == null)
                    {
                        return PaymentResult.Failed(new Dictionary<string, string[]>() { {
                               PaymentProperties.MobileNumber, new[] { $"'{PaymentProperties.MobileNumber.Humanize()}' is not supported." } } });
                    }

                    details[PaymentProperties.MobileNumber] = mobileNumber;
                    details[PaymentProperties.MobileIssuer] = mobileIssuer.Code;
                }
                else if (paymentMethod == PaymentMethod.PlasticMoney)
                {

                }
                else throw new InvalidOperationException();
            }
            else throw new InvalidOperationException();

            var paymentProcessor = GetPaymentProcessor(paymentMethod);
            var paymentResult = await paymentProcessor.ProcessAsync(payment, details);

            if (paymentResult.Success)
            {
                payment.Status = PaymentStatus.Processing;
                _unitOfWork.Update(payment);
                await _unitOfWork.CompleteAsync();
            }

            return paymentResult;
        }

        IPaymentProcessor GetPaymentProcessor(PaymentMethod paymentMethod)
        {
            return _serviceProvider.GetServices<IPaymentProcessor>().First(_ => ((IPaymentMethod)_).SupportedMethods.HasFlag(paymentMethod));
        }
    }
}