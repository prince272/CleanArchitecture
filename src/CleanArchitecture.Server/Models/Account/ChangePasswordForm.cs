using FluentValidation;

namespace CleanArchitecture.Server.Models.Account
{
    public class ChangePasswordForm
    {
        public string CurrentPassword { get; set; } = null!;

        public string NewPassword { get; set; } = null!;
    }

    public class ChangePasswordValidator : AbstractValidator<ChangePasswordForm>
    {
        public ChangePasswordValidator()
        {
            RuleFor(_ => _.CurrentPassword).NotEmpty();
            RuleFor(_ => _.NewPassword).NotEmpty().Password();
        }
    }
}
