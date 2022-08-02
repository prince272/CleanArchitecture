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
    public static class AuthenticationTokenExtensions
    {
        public static AuthenticationBuilder AddAuthenticationTokenProvider(this AuthenticationBuilder identityBuilder, Action<AuthenticationTokenOptions> configure)
        {
            identityBuilder.Services.Configure(configure);
            identityBuilder.Services.AddTransient<AuthenticationTokenProvider>();
            return identityBuilder;
        }
    }
}
