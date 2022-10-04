using CleanArchitecture.Core;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Infrastructure.Data;
using CleanArchitecture.Infrastructure.Extensions.PaymentProvider;
using CleanArchitecture.Server.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace CleanArchitecture.Server.Controllers
{
    public class PaymentsController : ApiController
    {
        private readonly IPaymentProvider _paymentProvider;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClientServer _clientServer;
        private readonly UserManager<User> _userManager;

        public PaymentsController(IPaymentProvider paymentProvider, IUnitOfWork unitOfWork, IClientServer clientServer, UserManager<User> userManager)
        {
            _paymentProvider = paymentProvider ?? throw new ArgumentNullException(nameof(paymentProvider));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _clientServer = clientServer ?? throw new ArgumentNullException(nameof(clientServer));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        [HttpPost("payments/checkout/{checkoutId}")]
        public async Task<IActionResult> Checkout(string checkoutId, [FromBody] IDictionary<string, object> form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));

            //if (string.IsNullOrWhiteSpace(returnUrl))
            //    return ValidationProblem(new Dictionary<string, string[]>() { { nameof(returnUrl), new[] { $"'{nameof(returnUrl)}' cannot be empty." } } });

            //if (!Uri.IsWellFormedUriString(returnUrl, UriKind.Absolute))
            //    return ValidationProblem(new Dictionary<string, string[]>() { { nameof(returnUrl), new[] { $"'{nameof(returnUrl)}' is not valid." } } });

            //if (_clientServer.ClientUrls.Any(origin => Uri.Compare(new Uri(origin), new Uri(returnUrl), UriComponents.SchemeAndServer, UriFormat.UriEscaped, StringComparison.InvariantCultureIgnoreCase) == 0))
            //    return ValidationProblem(new Dictionary<string, string[]>() { { nameof(returnUrl), new[] { $"'{nameof(returnUrl)}' is not allowed." } } });

            var payments = (await _unitOfWork.Query<Payment>().Where(_ => _.CheckoutId == checkoutId).ToArrayAsync());
            var payment = payments.LastOrDefault();

            if (payment == null)
            {
                return NotFound();
            }

            if (payment.UserId.HasValue)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null) throw new InvalidOperationException($"Value cannot be null.");

                if (payment.UserId.Value != currentUser.Id)
                    return Forbid();
            }

            if (payment.Status != PaymentStatus.Pending) 
            {
                payment = payment.Renew();
                _unitOfWork.Add(payment);
                await _unitOfWork.CompleteAsync();
            }

            var result = await _paymentProvider.MapAsync(payment, form.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.ToString()!));
            if (!result.Success) return ValidationProblem(result.Errors, title: result.Message);

            result = await _paymentProvider.ProcessAsync(payment);
            if (!result.Success) return ValidationProblem(result.Errors, title: result.Message);

            return Ok(result.Data);
        }

        [HttpGet("payments/checkout/{checkoutId}")]
        public async Task<IActionResult> Checkout(string checkoutId)
        {
            var payments = (await _unitOfWork.Query<Payment>().Where(_ => _.CheckoutId == checkoutId).ToArrayAsync());
            var payment  = payments.LastOrDefault();

            if (payment == null)
            {
                return NotFound();
            }

            if (payment.UserId.HasValue)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null) throw new InvalidOperationException($"Value cannot be null.");

                if (payment.UserId.Value != currentUser.Id)
                    return Forbid();
            }

            return Ok(new
            {
                payment.Id,
                payment.Description,
                payment.Amount,
                payment.Type,
                payment.Status,
                payment.UserId,
                payment.Method,
                payment.ReturnUrl
            });
        }
    }
}