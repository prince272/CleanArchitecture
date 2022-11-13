using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace CleanArchitecture.Infrastructure
{
    public interface IExtendable
    {
        string? ExtensionData { get; set; }
    }

    public static class ExtendableExtensions
    {
        public static T? GetData<T>([NotNull] this IExtendable extendable, [NotNull] string name)
        {
            if (extendable.ExtensionData == null)
            {
                return default;
            }
            var json = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(extendable.ExtensionData)!;
            var prop = json[name];
            return prop.Deserialize<T>();
        }

        public static void SetData<T>([NotNull] this IExtendable extendable, [NotNull] string name, T? value)
        {
            if (extendable.ExtensionData == null)
            {
                if (EqualityComparer<T>.Default.Equals(value, default))
                {
                    return;
                }
                extendable.ExtensionData = "{}";
            }

            var json = JsonSerializer.Deserialize<Dictionary<string, object>>(extendable.ExtensionData)!;

            if (value == null || EqualityComparer<T>.Default.Equals(value, default))
            {
                if (json[name] != null)
                {
                    json.Remove(name);
                }
            }

            json.Add(name, value!);

            var data = JsonSerializer.Serialize(json);
            if (data == "{}")
            {
                data = null;
            }
            extendable.ExtensionData = data;
        }

        public static bool RemoveData([NotNull] this IExtendable extendable, string name)
        {
            if (extendable.ExtensionData == null)
            {
                return false;
            }

            var json = JsonSerializer.Deserialize<Dictionary<string, object>>(extendable.ExtensionData)!;
            var token = json[name];

            if (token == null)
            {
                return false;
            }

            json.Remove(name);

            var data = JsonSerializer.Serialize(json);
            if (data == "{}")
            {
                data = null;
            }

            extendable.ExtensionData = data;

            return true;
        }

        //TODO: string[] GetExtendedPropertyNames(...)
    }
}
