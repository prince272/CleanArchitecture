using CleanArchitecture.Core.Entities;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Validators
{
    public class UserValidator : AbstractValidator<User>
    {
        public UserValidator()
        {
            RuleFor(entity => entity.FirstName).NotEmpty();
            RuleFor(entity => entity.LastName).NotEmpty();
            RuleFor(entity => entity.RegisteredOn).NotEmpty();
        }
    }
}
