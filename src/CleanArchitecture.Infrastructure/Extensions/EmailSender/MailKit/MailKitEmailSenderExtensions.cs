using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Infrastructure.Extensions.EmailSender.MailKit
{
    public static class MailKitEmailSenderExtensions
    {
        public static IServiceCollection AddMailKitEmailSender(this IServiceCollection services, Action<MailKitEmailSenderOptions> configure)
        {
            services.Configure(configure);
            services.AddScoped<IEmailSender, MailKitEmailSender>();
            return services;
        }
    }
}
