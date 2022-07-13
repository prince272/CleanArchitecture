using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Helpers
{
    public static class TypeHelper
    {
        /// <summary>
        /// Checks whether <paramref name="givenType"/> implements/inherits <paramref name="genericType"/>.
        /// </summary>
        /// <param name="givenType">Type to check</param>
        /// <param name="genericType">Generic type</param>
        public static bool IsAssignableToGenericType(Type givenType, Type genericType)
        {
            var givenTypeInfo = givenType.GetTypeInfo();

            if (givenTypeInfo.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
            {
                return true;
            }

            foreach (var interfaceType in givenType.GetInterfaces())
            {
                if (interfaceType.GetTypeInfo().IsGenericType && interfaceType.GetGenericTypeDefinition() == genericType)
                {
                    return true;
                }
            }

            if (givenTypeInfo.BaseType == null)
            {
                return false;
            }

            return IsAssignableToGenericType(givenTypeInfo.BaseType, genericType);
        }

        /// <summary>
        /// Checks whether <paramref name="givenType"/> implements/inherits <paramref name="type"/>.
        /// </summary>
        /// <param name="givenType">Type to check</param>
        /// <param name="type">Type</param>
        public static bool IsAssignableToType(Type givenType, Type type)
        {
            var givenTypeInfo = givenType.GetTypeInfo();

            if (!givenTypeInfo.IsGenericType && givenType == type)
            {
                return true;
            }

            foreach (var interfaceType in givenType.GetInterfaces())
            {
                if (!interfaceType.GetTypeInfo().IsGenericType && interfaceType == type)
                {
                    return true;
                }
            }

            if (givenTypeInfo.BaseType == null)
            {
                return false;
            }

            return IsAssignableToGenericType(givenTypeInfo.BaseType, type);
        }

        public static IEnumerable<Type> GetConstructibleTypes(this Assembly assembly)
            => assembly.GetLoadableDefinedTypes().Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition);

        public static IEnumerable<Type> GetLoadableDefinedTypes(this Assembly assembly)
        {
            try
            {
                return assembly.DefinedTypes;
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(t => t != null).Select(IntrospectionExtensions.GetTypeInfo!);
            }
        }
    }
}
