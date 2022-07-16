using CleanArchitecture.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IdentityBuilder AddBearerTokenManager(this IdentityBuilder identityBuilder, Action<BearerTokenOptions> configure)
        {
            identityBuilder.Services.Configure(configure);
            identityBuilder.Services.AddTransient<BearerTokenManager>();
            return identityBuilder;
        }
    }
}
