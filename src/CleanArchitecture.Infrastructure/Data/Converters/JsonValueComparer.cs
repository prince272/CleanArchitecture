using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace CleanArchitecture.Infrastructure.Data.Converters
{

    /// <summary>
    /// Compares two objects.
    /// Required to make EF Core change tracking work for complex value converted objects.
    /// </summary>
    /// <remarks>
    /// For objects that implement <see cref="ICloneable"/> and <see cref="IEquatable{T}"/>,
    /// those implementations will be used for cloning and equality.
    /// For plain objects, fall back to deep equality comparison using JSON serialization
    /// (safe, but inefficient).
    /// </remarks>
    internal class JsonValueComparer<T> : ValueComparer<T> where T : class
    {
        private static T DoGetSnapshot(T instance)
        {
            if (instance is ICloneable cloneable)
                return (T)cloneable.Clone();

            var result = JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(instance))!;
            return result;

        }

        private static int DoGetHashCode(T instance)
        {
            if (instance is IEquatable<T>)
                return instance.GetHashCode();

            return JsonSerializer.Serialize(instance).GetHashCode();

        }

        private static bool DoEquals(T? left, T? right)
        {
            if ((left == null) || (right == null)) return false;

            if (left is IEquatable<T> equatable)
                return equatable.Equals(right);

            var result = JsonSerializer.Serialize(left).Equals(JsonSerializer.Serialize(right));
            return result;

        }

        public JsonValueComparer() : base(
          (t1, t2) => DoEquals(t1, t2),
          t => DoGetHashCode(t),
          t => DoGetSnapshot(t))
        {
        }
    }
}
