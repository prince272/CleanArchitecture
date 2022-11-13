using CleanArchitecture.Core.Utilities;
using CleanArchitecture.Infrastructure.Data.Converters;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text.Json.Serialization;

namespace CleanArchitecture.Infrastructure.Data
{
    public static class ModelBuilderExtensions
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

        /// <summary>
        /// Add fields marked with <see cref="JsonPropertyAttribute"/> to be converted using <see cref="JsonValueConverter{T}"/>.
        /// </summary>
        /// <param name="modelBuilder">Model builder instance. Cannot be null.</param>
        /// <param name="skipConventionalEntities">
        ///   Skip trying to initialize properties for entity types found by EF conventions.
        ///   EF conventions treats complex fields as possible entity types. This can easily cause issues if we are cross referencing types utilizing
        ///   JsonAttribute while not registering them as actual entities in our db context.
        /// </param>
        /// <remarks>
        /// Adapted from https://www.tabsoverspaces.com/233708-using-value-converter-for-custom-encryption-of-field-on-entity-framework-core-2-1
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "EF1001:Internal EF Core API usage.", Justification = "<Pending>")]
        public static void AddJsonProperties(this ModelBuilder modelBuilder, bool skipConventionalEntities = true)
        {
            static bool HasJsonAttribute(PropertyInfo propertyInfo) => propertyInfo != null && propertyInfo.CustomAttributes.Any(a => a.AttributeType == typeof(JsonPropertyNameAttribute));

            if (modelBuilder == null)
                throw new ArgumentNullException(nameof(modelBuilder));

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {

                var typeBase = typeof(TypeBase);
                if (skipConventionalEntities)
                {
                    var typeConfigurationSource = typeBase.GetField("_configurationSource", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(entityType)?.ToString();
                    if (Enum.TryParse(typeConfigurationSource, out ConfigurationSource typeSource) && typeSource == ConfigurationSource.Convention) continue;
                }

                var ignoredMembers = (Dictionary<string, ConfigurationSource>)typeBase.GetField("_ignoredMembers", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(entityType)!;
                bool NotIgnored(PropertyInfo property) =>
                  property != null && !ignoredMembers.ContainsKey(property.Name) && !property.CustomAttributes.Any(a => a.AttributeType == typeof(NotMappedAttribute));

                foreach (var clrProperty in entityType.ClrType.GetProperties().Where(x => NotIgnored(x) && HasJsonAttribute(x)))
                {
                    var property = modelBuilder.Entity(entityType.ClrType).Property(clrProperty.PropertyType, clrProperty.Name);
                    var modelType = clrProperty.PropertyType;

                    var converterType = typeof(JsonValueConverter<>).MakeGenericType(modelType);
                    var converter = (ValueConverter)Activator.CreateInstance(converterType, new object?[] { null })!;
                    property.Metadata.SetValueConverter(converter);

                    var valueComparer = typeof(JsonValueComparer<>).MakeGenericType(modelType);
                    property.Metadata.SetValueComparer((ValueComparer)Activator.CreateInstance(valueComparer, new object[0])!);
                }
            }
        }
    }
}
