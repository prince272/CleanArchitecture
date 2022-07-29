namespace CleanArchitecture.Server.Extensions.Identity
{
    public class AuthenticationData
    {
        public string TokenType { get; set; } = null!;

        public string AccessToken { get; set; } = null!;

        public double AccessTokenExpiresIn { get; set; }

        public string RefreshToken { get; set; } = null!;

        public double RefreshTokenExpiresIn { get; set; }
    }
}
