using CleanArchitecture.Core.Helpers;
using CleanArchitecture.Infrastructure.Extensions.PaymentProvider;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Text.Json;

namespace CleanArchitecture.Server.Controllers
{
    public class PaymentsController : ApiController
    {
        private readonly IPaymentProvider _paymentProvider;

        public PaymentsController(IPaymentProvider paymentProvider)
        {
            _paymentProvider = paymentProvider ?? throw new ArgumentNullException(nameof(paymentProvider));
        }

        [HttpPost("payments/process")]
        public async Task<IActionResult> Process([FromBody] IDictionary<string, object> form)
        {
            var result = await _paymentProvider.ProcessAsync(form);

            if (!result.Success)
                return ValidationProblem(result.Errors!, title: result.Message);

            return Ok(result.Data);
        }
    }
}
