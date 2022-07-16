using CleanArchitecture.Core.Helpers;
using CleanArchitecture.Infrastructure.Identity;
using FluentValidation;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Server.Utilities
{
    public static class RuleBuilderExtensions
    {
        public static IRuleBuilder<T, string> Username<T>(this IRuleBuilder<T, string> ruleBuilder, UserManager userManager)
        {
            ruleBuilder.CustomAsync(async (value, context, cancellationToken) =>
            {
                var contactType = ContactHelper.Switch(value);
                switch (contactType)
                {
                    case ContactType.EmailAddress:
                        {
                            if (!ContactHelper.TryValidateEmailAddress(value, out var _))
                                context.AddFailure($"'{contactType.Humanize()}' is not valid.");

                            if (await userManager.Users.AnyAsync(user => user.Email == value, cancellationToken))
                                context.AddFailure($"'{contactType.Humanize()}' is not registered.");
                        }
                        break;

                    case ContactType.PhoneNumber:
                        {
                            if (!ContactHelper.TryValidatePhoneNumber(value, out var _))
                                context.AddFailure($"'{contactType.Humanize()}' is not valid.");

                            if (await userManager.Users.AnyAsync(user => user.PhoneNumber == value, cancellationToken))
                                context.AddFailure($"'{contactType.Humanize()}' is not registered.");
                        }
                        break;

                    default: context.AddFailure("'Username' is not valid."); break;
                }
            });
            return ruleBuilder;
        }

        // How can I create strong passwords with FluentValidation?
        // source: https://stackoverflow.com/questions/63864594/how-can-i-create-strong-passwords-with-fluentvalidation
        public static IRuleBuilder<T, string> Password<T>(this IRuleBuilder<T, string> ruleBuilder, int minimumLength = 6)
        {
            var options = ruleBuilder.NotEmpty()
                .MinimumLength(minimumLength)
                .Matches("[A-Z]").WithMessage("'{PropertyName}' must contain at least 1 upper case.")
                .Matches("[a-z]").WithMessage("'{PropertyName}' must contain at least 1 lower case.")
                .Matches("[0-9]").WithMessage("'{PropertyName}' must contain at least 1 digit.")
                .Matches("[^a-zA-Z0-9]").WithMessage("'{PropertyName}' must contain at least 1 special character.");

            return options;
        }
    }
}
