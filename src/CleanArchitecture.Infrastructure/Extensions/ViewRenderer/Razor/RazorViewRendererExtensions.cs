using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CleanArchitecture.Infrastructure.Extensions.ViewRenderer.Razor
{
    public static class RazorViewRendererExtensions
    {
        public static IServiceCollection AddRazorViewRenderer(this IServiceCollection services, Action<RazorViewRendererOptions> configure)
        {
            services.Configure(configure);

            var builder = services.AddMvcCore();
            builder.AddRazorViewEngine();
            services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddScoped<IViewRenderer, RazorViewRenderer>();
            return services;
        }
    }
}
