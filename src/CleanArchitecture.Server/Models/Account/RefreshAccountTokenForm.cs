using FluentValidation;

namespace CleanArchitecture.Server.Models.Account
{
    public class RefreshAccountTokenForm
    {
        public string RefreshToken { get; set; } = null!;
    }

    public class RefreshAccountTokenValidator : AbstractValidator<RefreshAccountTokenForm>
    {
        public RefreshAccountTokenValidator()
        {
            RuleFor(model => model.RefreshToken).NotEmpty();
        }
    }
}
