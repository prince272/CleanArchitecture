using CleanArchitecture.Core.Utilities;
using CleanArchitecture.Server.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace CleanArchitecture.Server.Controllers
{
    public class HomeController : ApiController
    {
        [Authorize]
        [HttpGet("protected")]
        public IActionResult Protected()
        {
            return Ok(HttpContext.GetTempData());
        }

        [AllowAnonymous]
        [ApiExplorerSettings(IgnoreApi = true)]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("error/{statusCode}")]
        public IActionResult Error(int statusCode)
        {
            var exceptionHandler = HttpContext.Features.Get<IExceptionHandlerFeature>();
            return Problem(statusCode: statusCode);
        }
    }

    [Route("")]
    public abstract class ApiController : ControllerBase
    {
        /// <summary>
        /// Creates an Microsoft.AspNetCore.Mvc.BadRequestObjectResult that produces a Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest.
        /// </summary>
        /// <param name="errors">One or more validation errors.</param>
        /// <param name="detail">The value for <see cref="ProblemDetails.Detail" />.</param>
        /// <param name="instance">The value for <see cref="ProblemDetails.Instance" />.</param>
        /// <param name="statusCode">The status code.</param>
        /// <param name="title">The value for <see cref="ProblemDetails.Title" />. Defaults to "One or more validation errors occurred."</param>
        /// <param name="type">The value for <see cref="ProblemDetails.Type" />.</param>
        /// <param name="extensions">The value for <see cref="ProblemDetails.Extensions" />.</param>
        /// <returns>The created <see cref="IResult"/> for the response.</returns>
        [NonAction]
        public IActionResult ValidationProblem(
        IDictionary<string, string[]> errors,
        string? detail = null,
        string? instance = null,
        int? statusCode = null,
        string? title = null,
        string? type = null,
        IDictionary<string, object?>? extensions = null)
        {
            var problemDetailsFactory = (AppProblemDetailsFactory)HttpContext.RequestServices.GetServices<ProblemDetailsFactory>().First(_ => _.GetType() == typeof(AppProblemDetailsFactory));
            return ValidationProblem(problemDetailsFactory.CreateValidationProblemDetails(HttpContext, errors, statusCode, title, type, detail, instance, extensions));
        }
    }
}