#nullable disable


namespace CleanArchitecture.Server.Extensions.Anonymous
{
    public static class AnonymousMiddlewareExtensions
    {
        public static IServiceCollection AddAnonymous(this IServiceCollection services, Action<AnonymousCookieOptionsBuilder> configure = null)
        {
            return services.Configure(configure);
        }

        public static IApplicationBuilder UseAnonymous(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AnonymousMiddleware>();
        }
    }
}