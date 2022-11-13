using CleanArchitecture.Core.Utilities;
using FluentValidation;
using Humanizer;
using System.Linq.Expressions;

namespace CleanArchitecture.Server.Models
{
    public static class ModelExtensions
    {
        public static void Add(this IDictionary<string, string[]> errors, LambdaExpression expression, params string[] errorMessages)
        {
            errors.Add(ExpressionHelper.GetSubName(expression), errorMessages);
        }

        public static IRuleBuilder<T, string> Username<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            ruleBuilder.Custom((value, context) =>
            {
                var usernameType = ContactTypeHelper.GetContactType(value);

                switch (usernameType)
                {
                    case ContactType.EmailAddress:
                        {
                            if (!ContactTypeHelper.TryParseEmailAddress(value, out _))
                            {
                                context.AddFailure($"'{usernameType.Humanize()}' is not valid.");
                                return;
                            }
                        }
                        break;

                    case ContactType.PhoneNumber:
                        {
                            if (!ContactTypeHelper.TryParsePhoneNumber(value, out _))
                            {
                                context.AddFailure($"'{usernameType.Humanize()}' is not valid.");
                                return;
                            }
                        }
                        break;

                    default: throw new InvalidOperationException();
                }
            });
            return ruleBuilder;
        }

        // How can I create strong passwords with FluentValidation?
        // source: https://stackoverflow.com/questions/63864594/how-can-i-create-strong-passwords-with-fluentvalidation
        public static IRuleBuilder<T, string> Password<T>(this IRuleBuilder<T, string> ruleBuilder, int minimumLength = 6)
        {
            var options = ruleBuilder
                .MinimumLength(minimumLength)
                .Matches("[A-Z]").WithMessage("'{PropertyName}' must contain at least 1 upper case.")
                .Matches("[a-z]").WithMessage("'{PropertyName}' must contain at least 1 lower case.")
                .Matches("[0-9]").WithMessage("'{PropertyName}' must contain at least 1 digit.")
                .Matches("[^a-zA-Z0-9]").WithMessage("'{PropertyName}' must contain at least 1 special character.");

            return options;
        }
    }
}