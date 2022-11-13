using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace CleanArchitecture.Infrastructure.Data.Converters
{

    /// <summary>
    /// Converts complex field to/from JSON string.
    /// </summary>
    /// <typeparam name="T">Model field type.</typeparam>
    /// <remarks>See more: https://docs.microsoft.com/en-us/ef/core/modeling/value-conversions </remarks>
    public class JsonValueConverter<T> : ValueConverter<T?, string> where T : class
    {
        public JsonValueConverter(ConverterMappingHints? hints = default) :
          base(v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null), v => JsonSerializer.Deserialize<T?>(v, (JsonSerializerOptions?)null), hints)
        { }
    }

}
