using CleanArchitecture.Infrastructure.Entities.Store;
using FluentValidation;

namespace CleanArchitecture.Infrastructure.Validators
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
