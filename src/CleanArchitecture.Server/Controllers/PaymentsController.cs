using CleanArchitecture.Core;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Infrastructure.Data;
using CleanArchitecture.Infrastructure.Extensions.PaymentProvider;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Text.Json;

namespace CleanArchitecture.Server.Controllers
{
    public class PaymentsController : ApiController
    {
        private readonly IPaymentProvider _paymentProvider;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;

        public PaymentsController(IPaymentProvider paymentProvider, IUnitOfWork unitOfWork, UserManager<User> userManager)
        {
            _paymentProvider = paymentProvider ?? throw new ArgumentNullException(nameof(paymentProvider));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        [HttpPost("payments/{paymentId}/checkout/{transactionId}")]
        public async Task<IActionResult> Checkout(long paymentId, string transactionId, [FromBody] IDictionary<string, object> form)
        {
            var payment = await _unitOfWork.FindAsync<Payment>(paymentId);
            if (payment == null || payment.TransactionId != transactionId)
            {
                return NotFound();
            }

            var result = await _paymentProvider.ProcessAsync(payment, form);

            if (!result.Success)
                return ValidationProblem(result.Errors!, title: result.Message);

            return Ok(result.Data);
        }

        [HttpGet("payments/{paymentId}/checkout/{transactionId}")]
        public async Task<IActionResult> Checkout(long paymentId, string transactionId)
        {
            var payment = await _unitOfWork.FindAsync<Payment>(paymentId);
            if (payment == null || payment.TransactionId != transactionId)
            {
                return NotFound();
            }

            return Ok(payment);
        }
    }
}
