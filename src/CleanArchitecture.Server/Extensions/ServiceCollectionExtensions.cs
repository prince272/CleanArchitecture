using CleanArchitecture.Server.Extensions.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Server.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IdentityBuilder AddAuthenticationManager(this IdentityBuilder identityBuilder, Action<AuthenticationOptions> configure)
        {
            identityBuilder.Services.Configure(configure);
            identityBuilder.Services.AddTransient<AuthenticationManager>();
            return identityBuilder;
        }
    }
}
