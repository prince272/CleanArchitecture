using CleanArchitecture.Core;
using CleanArchitecture.Core.Helpers;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Data
{
    public static class AppDbContextExtensions
    {
        public static ModelBuilder ApplyEntitiesFromAssembly(this ModelBuilder modelBuilder, Assembly assembly, Func<Type, bool>? predicate = null)
        {
            foreach (var type in TypeHelper.GetConstructibleTypes(assembly).OrderBy(t => t.FullName))
            {
                // Only accept types that contain a parameterless constructor, are not abstract and satisfy a predicate if it was used.
                if (type.GetConstructor(Type.EmptyTypes) == null
                    || (!predicate?.Invoke(type) ?? false))
                {
                    continue;
                }

                foreach (var @interface in type.GetInterfaces())
                {
                    if (@interface.IsGenericType)
                        continue;

                    if (@interface == typeof(IEntity))
                    {
                        modelBuilder.Entity(type);
                    }
                }
            }

            return modelBuilder;
        }

        public static void ValidateEntitiesFromAssembly(this DbContext dbContext, Assembly assembly)
        {
            var modifiedEntries = dbContext.ChangeTracker.Entries().Where(ee => (ee.State == EntityState.Added || ee.State == EntityState.Modified)).ToArray();
            var validatorTypes = TypeHelper.GetConstructibleTypes(assembly)
                 .Where(t => TypeHelper.IsAssignableToGenericType(t, typeof(FluentValidation.AbstractValidator<>)));

            foreach (var modifiedEntry in modifiedEntries)
            {
                var entity = modifiedEntry.Entity;
                var validatorType = validatorTypes.FirstOrDefault(t => t.BaseType != null && t.BaseType.GetGenericArguments()[0] == entity.GetType());

                if (validatorType != null)
                {
                    var validator = (IValidator)Activator.CreateInstance(validatorType)!;
                    var validationResult = validator.Validate(new ValidationContext<object>(entity));
                    if (!validationResult.IsValid) throw new ValidationException(validationResult.Errors);
                }
            }
        }
    }
}
