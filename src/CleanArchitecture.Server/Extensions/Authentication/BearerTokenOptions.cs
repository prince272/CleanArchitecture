namespace CleanArchitecture.Server.Extensions.Authentication
{
    public class BearerTokenOptions
    {
        public string Secret { set; get; } = null!;

        public string Issuer { set; get; } = null!;

        public string Audience { set; get; } = null!;

        public TimeSpan AccessTokenExpiresIn { set; get; }

        public TimeSpan RefeshTokenExpiresIn { set; get; }

        public bool MultipleAuthentication { set; get; }
    }
}
