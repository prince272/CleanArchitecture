using CleanArchitecture.Core;
using CleanArchitecture.Core.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Server.Models.Account
{
    public class CreateAccountForm
    {
        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string Username { get; set; } = null!;

        public string Password { get; set; } = null!;
    }

    public class CreateAccountValidator : AbstractValidator<CreateAccountForm>
    {
        public CreateAccountValidator(UserManager<User> userManager)
        {
            if (userManager == null) throw new ArgumentNullException(nameof(userManager));

            RuleFor(model => model.FirstName).NotEmpty();
            RuleFor(model => model.LastName).NotEmpty();
            RuleFor(model => model.Username).NotEmpty().Username();
            RuleFor(model => model.Password).NotEmpty().Password();
        }
    }
}
