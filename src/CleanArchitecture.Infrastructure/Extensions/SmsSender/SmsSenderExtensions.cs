using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Infrastructure.Extensions.SmsSender
{
    public static class SmsSenderExtensions
    {
        public static void AddSmsSender(this IServiceCollection services, Action<SmsSenderOptions> configure)
        {
            services.Configure(configure);
            services.AddScoped<ISmsSender, SmsSender>();
        }
    }
}
