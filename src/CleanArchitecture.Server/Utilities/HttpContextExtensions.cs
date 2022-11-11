using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace CleanArchitecture.Server.Utilities
{
    public static class HttpContextExtensions
    {
        public static ITempDataDictionary GetTempData(this HttpContext httpContext)
        {
            return httpContext.RequestServices.GetRequiredService<ITempDataDictionaryFactory>().GetTempData(httpContext);
        }
    }
}