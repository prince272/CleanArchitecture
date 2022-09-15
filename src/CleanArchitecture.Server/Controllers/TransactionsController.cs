using CleanArchitecture.Infrastructure.Extensions.PaymentProvider;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CleanArchitecture.Server.Controllers
{
    public class TransactionsController : ApiController
    {
        public TransactionsController()
        {
        }

        [HttpPost("transactions/process")]
        public async Task<IActionResult> Process([FromBody] Dictionary<string, object> form)
        {
            return Ok();
        }
    }
}
