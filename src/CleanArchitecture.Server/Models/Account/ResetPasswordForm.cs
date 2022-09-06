using FluentValidation;

namespace CleanArchitecture.Server.Models.Account
{
    public interface IResetPasswordForm
    {
        string Username { get; set; }

        string Password { get; set; }

        string Code { get; set; }
    }

    public class SendResetPasswordForm : IResetPasswordForm
    {
        public SendResetPasswordForm()
        {
        }

        public string Username { get; set; } = null!;

        string IResetPasswordForm.Password { get; set; } = null!;

        string IResetPasswordForm.Code { get; set; } = null!;
    }

    public class VerifyResetPasswordForm : IResetPasswordForm
    {
        public VerifyResetPasswordForm()
        {
        }

        public string Username { get; set; } = null!;

        string IResetPasswordForm.Password { get; set; } = null!;

        public string Code { get; set; } = null!;
    }

    public class ResetPasswordForm : IResetPasswordForm
    {
        public string Username { get; set; } = null!;

        public string Password { get; set; } = null!;

        public string Code { get; set; } = null!;
    }

    public class SendResetPasswordValidator : AbstractValidator<SendResetPasswordForm>
    {
        public SendResetPasswordValidator()
        {
            RuleFor(_ => _.Username).NotEmpty();
        }
    }

    public class VerifyResetPasswordValidator : AbstractValidator<VerifyResetPasswordForm>
    {
        public VerifyResetPasswordValidator()
        {
            RuleFor(_ => _.Username).NotEmpty();
            RuleFor(_ => _.Code).NotEmpty();
        }
    }

    public class ResetPasswordValidator : AbstractValidator<ResetPasswordForm>
    {
        public ResetPasswordValidator()
        {
            RuleFor(_ => _.Username).NotEmpty();
            RuleFor(_ => _.Password).NotEmpty().Password();
            RuleFor(_ => _.Code).NotEmpty();
        }
    }
}
