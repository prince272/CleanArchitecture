using CleanArchitecture.Core;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Utilities;
using CleanArchitecture.Infrastructure.Data;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Infrastructure.Extensions.PaymentProvider.PaySwitch
{
    public class PaySwitchPaymentProcessor : IPaymentProcessor
    {
        private readonly PaySwitchPaymentProcessorOptions _paySwitchPaymentMethodOptions;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;

        public PaySwitchPaymentProcessor(IOptions<PaySwitchPaymentProcessorOptions> paySwitchPaymentMethodOptions, IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork)
        {
            _paySwitchPaymentMethodOptions = paySwitchPaymentMethodOptions.Value ?? throw new ArgumentNullException(nameof(paySwitchPaymentMethodOptions));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        private HttpClient CreateHttpClient()
        {
            var httpClient = _httpClientFactory.CreateClient(nameof(PaySwitchPaymentProcessor));
            httpClient.BaseAddress = new Uri("https://prod.theteller.net");
            var authenticationToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_paySwitchPaymentMethodOptions.ClientId}:{_paySwitchPaymentMethodOptions.ClientSecret}"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authenticationToken);
            httpClient.DefaultRequestHeaders.Add("Merchant-Id", _paySwitchPaymentMethodOptions.MerchantId);
            return httpClient;
        }

        private string? GetCurrntIPAddress()
        {
            if (_httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress is not IPAddress remoteIp)
                return null;

            if (remoteIp.Equals(IPAddress.IPv6Loopback))
                return IPAddress.Loopback.ToString();

            return remoteIp.MapToIPv4().ToString();
        }

        private string? GetCurrentUAString()
        {
            return _httpContextAccessor.HttpContext?.Request?.Headers.UserAgent;
        }

        public string GenerateTransactionId()
        {
            return Algorithm.GenerateText(12, Algorithm.WHOLE_NUMERIC_CHARS);
        }

        public string Gateway => "PaySwitch";

        public PaymentMethod Method => PaymentMethod.MobileMoney | PaymentMethod.PlasticMoney | PaymentMethod.Default;

        public async Task<PaymentResult> ProcessAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            if (payment == null) throw new ArgumentNullException(nameof(payment));
            if (payment.TransactionId == null) throw new InvalidOperationException();

            var httpClient = CreateHttpClient();

            if (payment.Method == PaymentMethod.MobileMoney)
            {
                if (payment.MobileIssuer == null) throw new InvalidOperationException();
                if (payment.MobileNumber == null) throw new InvalidOperationException();

                if (payment.Type == PaymentType.Debit)
                {
                    var requestContent = JsonContent.Create(new Dictionary<string, string>
                    {
                        { "merchant_id", _paySwitchPaymentMethodOptions.MerchantId },
                        { "transaction_id", payment.TransactionId },
                        { "amount", (payment.Amount * 100).ToString("000000000000") },
                        { "processing_code", "000200" },
                        { "r-switch", payment.MobileIssuer.Code },
                        { "desc", payment.Description },
                        { "subscriber_number", payment.MobileNumber.TrimStart('+') },
                    });

                    requestContent.Headers.ContentType!.CharSet = null;

                    try
                    {
                        // Note: We timeout and update the payment status because it doesn't seem appropriate for the request to wait for the user's approval.
                        var response = await httpClient.PostAsync("/v1.1/transaction/process", requestContent, cancellationToken).WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);
                        response.EnsureSuccessStatusCode();
                    }
                    catch (TimeoutException)
                    {
                        payment.Status = PaymentStatus.Processing;
                        payment.IPAddress = GetCurrntIPAddress();
                        payment.UAString = GetCurrentUAString();
                        payment.UpdatedAt = DateTimeOffset.UtcNow;
                        _unitOfWork.Update(payment);
                        await _unitOfWork.CompleteAsync();

                        return PaymentResult.Succeeded();
                    }

                    return PaymentResult.Failed(message: "Unable to process payment.");
                }
            }
            else if (payment.Method == PaymentMethod.PlasticMoney)
            {

            }
            else if (payment.Method == PaymentMethod.Default)
            {

            }
            else throw new InvalidOperationException();

            throw new NotImplementedException();
        }

        public async Task VerifyAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            if (payment == null) throw new ArgumentNullException(nameof(payment));

            if (payment.Status == PaymentStatus.Pending || payment.Status == PaymentStatus.Processing)
            {
                if ((DateTimeOffset.UtcNow - payment.UpdatedAt) > TimeSpan.FromMinutes(3))
                {
                    payment.Status = PaymentStatus.Expired;
                    payment.ExpiredAt = DateTimeOffset.UtcNow;
                    payment.UpdatedAt = DateTimeOffset.UtcNow;
                    _unitOfWork.Update(payment);
                    await _unitOfWork.CompleteAsync();
                }
                else
                {
                    if (payment.Status == PaymentStatus.Processing)
                    {
                        var httpClient = CreateHttpClient();
                        var response = await httpClient.GetAsync($"/v1.1/users/transactions/{payment.TransactionId}/status", cancellationToken);
                        response.EnsureSuccessStatusCode();

                        var responseData = (await response.Content.ReadFromJsonAsync<IDictionary<string, object>>(cancellationToken: cancellationToken))!.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.ToString()!);
                        if (responseData.GetValueOrDefault("code") == "000")
                        {
                            payment.Status = PaymentStatus.Completed;
                            payment.CompletedAt = DateTimeOffset.UtcNow;
                            payment.UpdatedAt = DateTimeOffset.UtcNow;
                            _unitOfWork.Update(payment);
                            await _unitOfWork.CompleteAsync();
                        }
                        else if (responseData.GetValueOrDefault("code") == "104")
                        {
                            payment.Status = PaymentStatus.Declined;
                            payment.DeclinedAt = DateTimeOffset.UtcNow;
                            payment.UpdatedAt = DateTimeOffset.UtcNow;
                            _unitOfWork.Update(payment);
                            await _unitOfWork.CompleteAsync();
                        }
                    }
                }
            }
        }

        public Task<PaymentIssuer?> GetMobileIssuerAsync(string mobileNumber)
        {
            if (mobileNumber == null) throw new ArgumentNullException(nameof(mobileNumber));

            var mobileIssuers = new List<PaymentIssuer>()
            {
               new PaymentIssuer { Code = "MTN", Pattern = "^\\+233(24|54|55|59)", Name = "MTN" },
               new PaymentIssuer { Code = "VDF", Pattern =  "^\\+233(20|50)", Name = "Vodafone"},
               new PaymentIssuer { Code = "ATL", Pattern = "^\\+233(26|56)", Name = "AirtelTigo"},
               new PaymentIssuer { Code = "TGO", Pattern = "^\\+233(27|57)", Name = "AirtelTigo"},
            };

            return Task.FromResult(mobileIssuers.FirstOrDefault(_ => Regex.IsMatch(mobileNumber, _.Pattern)));
        }
    }
}