using System.Collections.Concurrent;
using System.Reflection;

namespace CleanArchitecture.Core.Utilities
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


        static readonly ConcurrentDictionary<Type, bool> IsSimpleTypeCache = new ConcurrentDictionary<Type, bool>();

        // How To Test if Type is Primitive
        // source: https://stackoverflow.com/questions/2442534/how-to-test-if-type-is-primitive
        public static bool IsPrimitiveType(Type type)
        {
            return IsSimpleTypeCache.GetOrAdd(type, t =>
                type.IsPrimitive ||
                type.IsEnum ||
                type == typeof(string) ||
                type == typeof(decimal) ||
                type == typeof(DateTime) ||
                type == typeof(DateTimeOffset) ||
                type == typeof(TimeSpan) ||
                type == typeof(Guid) ||
                IsNullableSimpleType(type));

            static bool IsNullableSimpleType(Type t)
            {
                var underlyingType = Nullable.GetUnderlyingType(t);
                return underlyingType != null && IsPrimitiveType(underlyingType);
            }
        }
    }
}
