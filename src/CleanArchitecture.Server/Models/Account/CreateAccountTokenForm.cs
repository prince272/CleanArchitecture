using CleanArchitecture.Infrastructure.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace CleanArchitecture.Server.Models.Account
{
    public class CreateAccountTokenForm
    {
        public string Username { get; set; } = null!;

        public string Password { get; set; } = null!;
    }


    public class CreateAccountTokenValidator : AbstractValidator<CreateAccountTokenForm>
    {
        public CreateAccountTokenValidator(UserManager<User> userManager)
        {
            if (userManager == null) throw new ArgumentNullException(nameof(userManager));

            RuleFor(model => model.Username).NotEmpty().Username();
            RuleFor(model => model.Password).NotEmpty();
        }
    }
}
