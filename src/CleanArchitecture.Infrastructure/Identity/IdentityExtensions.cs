using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Infrastructure.Identity
{
    public static class IdentityExtensions
    {
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
