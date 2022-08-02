namespace CleanArchitecture.Server.Extensions.Authentication
{
    public class AuthenticationTokenObject
    {
        public string TokenType { get; set; } = null!;

        public string AccessToken { get; set; } = null!;

        public double AccessTokenExpiresIn { get; set; }

        public string RefreshToken { get; set; } = null!;

        public double RefreshTokenExpiresIn { get; set; }
    }
}
