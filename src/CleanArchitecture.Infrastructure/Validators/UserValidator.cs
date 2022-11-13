using CleanArchitecture.Infrastructure.Entities;
using FluentValidation;

namespace CleanArchitecture.Infrastructure.Validators
{
    public class UserValidator : AbstractValidator<User>
    {
        public UserValidator()
        {
            RuleFor(entity => entity.FirstName).NotEmpty();
            RuleFor(entity => entity.LastName).NotEmpty();
            RuleFor(entity => entity.RegisteredAt).NotEmpty();
        }
    }
}
