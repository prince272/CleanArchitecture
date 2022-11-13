using Microsoft.AspNetCore.Authentication;

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
