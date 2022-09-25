using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Utilities;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Server.Extensions.Authentication
{
    public static class IdentityExtensions
    {
        public static async Task<User?> FindByUsernameAsync(this UserManager<User> userManager, string username)
        {
            User? user = null;

            switch (ContactHelper.GetContactType(username))
            {
                case ContactType.EmailAddress:
                    {
                        var emailAddress = ContactHelper.ParseEmailAddress(username).Address;
                        user = await userManager.FindByEmailAsync(emailAddress);
                    }
                    break;
                case ContactType.PhoneNumber:
                    {
                        var phoneNumber = ContactHelper.ParsePhoneNumber(username).Number;
                        user = await userManager.Users.SingleOrDefaultAsync(user => user.PhoneNumber == phoneNumber);
                    }; 
                    break;
                default: throw new InvalidOperationException();
            }

            return user;
        }

        public static IdentityResult ThrowIfFailed(this IdentityResult result)
        {
            if (!result.Succeeded)
            {
                throw new ValidationException(result.Errors.Select(error => new ValidationFailure(error.Code, error.Description)).ToArray());
            }

            return result;
        }
    }
}
