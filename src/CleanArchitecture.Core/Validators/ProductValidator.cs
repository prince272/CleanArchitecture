using CleanArchitecture.Core.Entities;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Validators
{
    public class ProductValidator : AbstractValidator<Product>
    {
        public ProductValidator()
        {
            RuleFor(entity => entity.Name).NotEmpty();
            RuleFor(entity => entity.Price).InclusiveBetween(decimal.Zero, decimal.MaxValue);
        }
    }
}
