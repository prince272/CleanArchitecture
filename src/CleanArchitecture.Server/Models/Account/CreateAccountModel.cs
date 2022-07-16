using CleanArchitecture.Core;
using CleanArchitecture.Infrastructure.Identity;
using CleanArchitecture.Server.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Server.Models.Account
{
    public class CreateAccountModel
    {
        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string Username { get; set; } = null!;

        public string Password { get; set; } = null!;
    }

    public class CreateAccountValidator : AbstractValidator<CreateAccountModel>
    {
        public CreateAccountValidator()
        {
            RuleFor(model => model.FirstName).NotEmpty();
            RuleFor(model => model.LastName).NotEmpty();
            RuleFor(model => model.Username).NotEmpty();
            RuleFor(model => model.Password).NotEmpty().Password();
        }
    }
}
