using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Server.Utilities
{
    public static class Extensions
    {
        public static ModelStateDictionary With(this ModelStateDictionary modelState, ValidationResult result)
        {
            foreach (var error in result.Errors)
                modelState.AddModelError(error.PropertyName, error.ErrorMessage);

            return modelState;
        }
    }
}
