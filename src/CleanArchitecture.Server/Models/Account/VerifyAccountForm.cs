using FluentValidation;

namespace CleanArchitecture.Server.Models.Account
{
    public interface IVerifyAccountForm
    {
        string Username { get; set; }

        string Code { get; set; }
    }

    public class SendVerifyAccountForm : IVerifyAccountForm
    {
        public SendVerifyAccountForm()
        {     
        }

        public string Username { get; set; } = null!;

        public VerifyAccountReason Reason { get; set; }

        string IVerifyAccountForm.Code { get; set; } = null!;
    }

    public class VerifyAccountForm : IVerifyAccountForm
    {
        public string Username { get; set; } = null!;

        public VerifyAccountReason Reason { get; set; }

        public string Code { get; set; } = null!;
    }

    public enum VerifyAccountReason
    {
        Verify,
        Change
    }

    public class SendVerifyAccountValidator : AbstractValidator<SendVerifyAccountForm>
    {
        public SendVerifyAccountValidator()
        {
            RuleFor(_ => _.Username).NotEmpty().Username();
        }
    }

    public class VerifyAccountValidator : AbstractValidator<VerifyAccountForm>
    {
        public VerifyAccountValidator()
        {
            RuleFor(_ => _.Username).NotEmpty().Username();
            RuleFor(_ => _.Code).NotEmpty();
        }
    }
}
