namespace CleanArchitecture.Server.Extensions.Authentication
{
    public class BearerTokenData
    {
        public string TokenType { get; set; } = null!;

        public string AccessToken { get; set; } = null!;

        public long AccessTokenExpiresIn { get; set; }

        public string RefreshToken { get; set; } = null!;

        public long RefreshTokenExpiresIn { get; set; }
    }
}
