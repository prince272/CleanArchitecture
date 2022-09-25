using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Utilities;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Infrastructure.Extensions.PaymentProvider.PaySwitch
{
    public class PaySwitchPaymentProcessor : IPaymentMethod, IPaymentProcessor
    {
        private readonly PaySwitchPaymentProcessorOptions _paySwitchPaymentMethodOptions;
        private readonly IHttpClientFactory _httpClientFactory;

        public PaySwitchPaymentProcessor(IOptions<PaySwitchPaymentProcessorOptions> paySwitchPaymentMethodOptions, IHttpClientFactory httpClientFactory)
        {
            _paySwitchPaymentMethodOptions = paySwitchPaymentMethodOptions.Value ?? throw new ArgumentNullException(nameof(paySwitchPaymentMethodOptions));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public PaymentMethod SupportedMethods => PaymentMethod.PaySwitch | PaymentMethod.MobileMoney;

        HttpClient CreateHttpClient()
        {
            var httpClient = _httpClientFactory.CreateClient(nameof(PaySwitchPaymentProcessor));
            httpClient.BaseAddress = new Uri("https://prod.theteller.net");
            var authenticationToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_paySwitchPaymentMethodOptions.ClientId}:{_paySwitchPaymentMethodOptions.ClientSecret}"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authenticationToken);
            httpClient.DefaultRequestHeaders.Add("Merchant-Id", _paySwitchPaymentMethodOptions.MerchantId);
            return httpClient;
        }

        public async Task<PaymentResult> ProcessAsync(Payment payment, IDictionary<string, object> details)
        {
            var httpClient = CreateHttpClient();

            var paymentMethod = details[PaymentProperties.Method].To<PaymentMethod>();

            if (paymentMethod == PaymentMethod.MobileMoney)
            {
                var mobileNumber = details[PaymentProperties.MobileNumber].ToString()!;
                var mobileIssuer = details[PaymentProperties.MobileIssuer].ToString()!;

                var paymentTransactionId = Algorithm.GenerateText(12, Algorithm.WHOLE_NUMERIC_CHARS);
                var requestHeaders = new Dictionary<string, string>();
                var requestData = new Dictionary<string, string>
                {
                    { "merchant_id", _paySwitchPaymentMethodOptions.MerchantId },
                    { "transaction_id", paymentTransactionId },
                    { "amount", (payment.Amount * 100).ToString("000000000000") },
                    { "processing_code", "000200" },
                    { "r-switch", mobileIssuer },
                    { "desc", payment.Description },
                    { "subscriber_number", mobileNumber.TrimStart('+') },
                };

                foreach (var requestHeader in requestHeaders) httpClient.DefaultRequestHeaders.Add(requestHeader.Key, requestHeader.Value);
                var response = await httpClient.PostAsJsonAsync("/v1.1/transaction/process", requestData);
                response.EnsureSuccessStatusCode();
                var responseData = await response.Content.ReadFromJsonAsync<IDictionary<string, string>>();
                return PaymentResult.Succeeded(new Dictionary<string, object>());

            }
            return PaymentResult.Failed(message: "Unable to process payment.");
        }
    }
}