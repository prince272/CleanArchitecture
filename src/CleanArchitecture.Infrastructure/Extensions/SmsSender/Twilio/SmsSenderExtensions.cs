using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Infrastructure.Extensions.SmsSender.Twilio
{
    public static class SmsSenderExtensions
    {
        public static IServiceCollection AddSmsSender(this IServiceCollection services, Action<SmsSenderOptions> configure)
        {
            services.Configure(configure);
            services.AddScoped<ISmsSender, SmsSender>();
            return services;
        }
    }
}
