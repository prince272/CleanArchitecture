using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Server.Extensions.Authentication
{
    public static class BearerTokenExtensions
    {
        public static AuthenticationBuilder AddBearerTokenProvider(this AuthenticationBuilder identityBuilder, Action<BearerTokenOptions> configure)
        {
            identityBuilder.Services.Configure(configure);
            identityBuilder.Services.AddTransient<BearerTokenProvider>();
            return identityBuilder;
        }
    }
}
