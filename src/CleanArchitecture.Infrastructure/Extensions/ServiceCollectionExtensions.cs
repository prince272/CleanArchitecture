using CleanArchitecture.Infrastructure.Extensions.ViewRenderer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Server.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddRazorViewRenderer(this IServiceCollection services, Action<RazorViewRendererOptions> configure)
        {
            services.Configure(configure);

            var options = services.BuildServiceProvider().GetRequiredService<IOptions<RazorViewRendererOptions>>();

            services.AddMvcCore().AddRazorViewEngine();
            services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddScoped<IViewRenderer, RazorViewRenderer>();
        }
    }
}
