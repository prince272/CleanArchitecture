namespace CleanArchitecture.Server.Models.Account
{
    public class AuthenticateModel
    {
        public string Username { get; set; } = null!;

        public string Password { get; set; } = null!;
    }
}
