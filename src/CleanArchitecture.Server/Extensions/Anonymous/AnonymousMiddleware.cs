using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace CleanArchitecture.Server.Extensions.AnonymousId
{
    public class AnonymousMiddleware
    {
        private RequestDelegate nextDelegate;
        private AnonymousCookieOptionsBuilder cookieOptionsBuilder;

        public AnonymousMiddleware(RequestDelegate nextDelegate, IOptions<AnonymousCookieOptionsBuilder> cookieOptionsBuilder)
        {
            this.nextDelegate = nextDelegate;
            this.cookieOptionsBuilder = cookieOptionsBuilder.Value;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            HandleRequest(httpContext);
            await nextDelegate.Invoke(httpContext);
        }

        public void HandleRequest(HttpContext httpContext)
        {
            var cookieOptions = cookieOptionsBuilder.Build(httpContext);
            var encodedData = httpContext.Request.Cookies[cookieOptions.Name];

            // Handle secure cookies over an unsecured connection.
            if (cookieOptions.Secure && !httpContext.Request.IsHttps)
            {
                if (!string.IsNullOrWhiteSpace(encodedData))
                    httpContext.Response.Cookies.Delete(cookieOptions.Name);

                // Adds the feature to request collection.
                httpContext.Features.Set<IAnonymousFeature>(new AnonymousFeature());
            }
            else
            {
                // Gets the value and anonymous Id data from the cookie, if available.
                var decodedData = AnonymousEncoder.Decode(encodedData);

                if (decodedData != null && !string.IsNullOrWhiteSpace(decodedData.AnonymousId))
                {
                    // Adds the feature to request collection.
                    httpContext.Features.Set<IAnonymousFeature>(new AnonymousFeature() { AnonymousId = decodedData.AnonymousId });
                }
                else
                {
                    var data = new AnonymousData(Guid.NewGuid().ToString(), cookieOptions.Expires.Value.DateTime);
                    encodedData = AnonymousEncoder.Encode(data);

                    httpContext.Response.Cookies.Append(cookieOptions.Name, encodedData, cookieOptions);

                    httpContext.Features.Set<IAnonymousFeature>(new AnonymousFeature() { AnonymousId = data.AnonymousId });
                }
            }
        }
    }
}