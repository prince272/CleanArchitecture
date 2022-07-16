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

namespace CleanArchitecture.Infrastructure.Data
{
    public static class AppDbContextExtensions
    {
        public static ModelBuilder ApplyEntitiesFromAssembly(this ModelBuilder modelBuilder, Assembly assembly, Func<Type, bool>? predicate = null)
        {
            foreach (var type in assembly.GetConstructibleTypes().OrderBy(t => t.FullName))
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
    }
}
