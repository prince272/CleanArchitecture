using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.WebUtilities;

namespace CleanArchitecture.Server.Utilities
{
    /// <summary>
    /// Based on Microsoft's DefaultProblemDeatilsFactory
    /// https://github.com/aspnet/AspNetCore/blob/2e4274cb67c049055e321c18cc9e64562da52dcf/src/Mvc/Mvc.Core/src/Infrastructure/DefaultProblemDetailsFactory.cs
    /// </summary>
    public sealed class AppProblemDetailsFactory : ProblemDetailsFactory
    {
        private readonly ApiBehaviorOptions _options;

        public AppProblemDetailsFactory(IOptions<ApiBehaviorOptions> options)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public override ProblemDetails CreateProblemDetails(
            HttpContext httpContext,
            int? statusCode = null,
            string? title = null,
            string? type = null,
            string? detail = null,
            string? instance = null)
        {
            statusCode ??= 500;

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Type = type,
                Detail = detail,
                Instance = instance,
            };

            ApplyProblemDetailsDefaults(httpContext, problemDetails, statusCode.Value);

            return problemDetails;
        }

        public override ValidationProblemDetails CreateValidationProblemDetails(
            HttpContext httpContext,
            ModelStateDictionary modelStateDictionary,
            int? statusCode = null,
            string? title = null,
            string? type = null,
            string? detail = null,
            string? instance = null)
        {
            if (modelStateDictionary == null)
            {
                throw new ArgumentNullException(nameof(modelStateDictionary));
            }

            statusCode ??= 400;

            var problemDetails = new ValidationProblemDetails(modelStateDictionary)
            {
                Status = statusCode,
                Type = type,
                Detail = detail,
                Instance = instance,
            };

            if (title != null)
            {
                // For validation problem details, don't overwrite the default title with null.
                problemDetails.Title = title;
            }

            ApplyProblemDetailsDefaults(httpContext, problemDetails, statusCode.Value);

            return problemDetails;
        }

        public ValidationProblemDetails CreateValidationProblemDetails(
            HttpContext httpContext,
            IDictionary<string, string[]> errors,
            int? statusCode = null,
            string? title = null,
            string? type = null,
            string? detail = null,
            string? instance = null,
            IDictionary<string, object?>? extensions = null)
        {
            if (errors == null)
            {
                throw new ArgumentNullException(nameof(errors));
            }

            string ResolvePropertyNamingPolicy(string propertyName)
                => (httpContext.RequestServices.GetRequiredService<IOptions<JsonOptions>>()?.Value)?.JsonSerializerOptions.DictionaryKeyPolicy?.ConvertName(propertyName) ?? propertyName;

            errors = errors.ToDictionary(k => ResolvePropertyNamingPolicy(k.Key), k => k.Value);

            statusCode ??= 400;

            var problemDetails = new ValidationProblemDetails(errors)
            {
                Status = statusCode,
                Type = type,
                Detail = detail,
                Instance = instance,
            };

            if (title != null)
            {
                // For validation problem details, don't overwrite the default title with null.
                problemDetails.Title = title;
            }

            if (extensions is not null)
            {
                foreach (var extension in extensions)
                {
                    problemDetails.Extensions.Add(ResolvePropertyNamingPolicy(extension.Key), extension.Value);
                }
            }

            ApplyProblemDetailsDefaults(httpContext, problemDetails, statusCode.Value);

            return problemDetails;
        }

        private void ApplyProblemDetailsDefaults(HttpContext httpContext, ProblemDetails problemDetails, int statusCode)
        {
            problemDetails.Status ??= statusCode;

            if (_options.ClientErrorMapping.TryGetValue(statusCode, out var clientErrorData))
            {
                problemDetails.Type ??= clientErrorData.Link;
            }

            var traceId = Activity.Current?.Id ?? httpContext?.TraceIdentifier;
            if (traceId != null)
            {
                problemDetails.Extensions["traceId"] = traceId;
            }

            problemDetails.Extensions["reason"] = ReasonPhrases.GetReasonPhrase(statusCode);
        }
    }
}