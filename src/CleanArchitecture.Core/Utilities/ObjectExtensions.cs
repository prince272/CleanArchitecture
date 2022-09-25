#nullable disable

using CleanArchitecture;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Utilities
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Converts a value to a destination type.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="destinationType">The type to convert the value to.</param>
        /// <returns>The converted value.</returns>
        public static object To(this object value, Type destinationType)
        {
            return value.To(destinationType, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Tries to converts a value to a destination type.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="destinationType">The type to convert the value to.</param>
        /// <returns>The converted value.</returns>
        public static bool To(this object value, Type destinationType, out object destination)
        {
            try
            {
                destination = value.To(destinationType, CultureInfo.InvariantCulture);
                return true;
            }
            catch (Exception)
            {
                destination = null;
                return false;
            }
        }

        /// <summary>
        /// Converts a value to a destination type.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="destinationType">The type to convert the value to.</param>
        /// <param name="culture">Culture</param>
        /// <returns>The converted value.</returns>
        public static object To(this object value, Type destinationType, CultureInfo culture)
        {
            if (value != null)
            {
                var sourceType = value.GetType();

                if (!TypeHelper.IsPrimitiveType(sourceType))
                {
                    value = value.ToString();
                    sourceType = value.GetType();
                }

                var destinationConverter = TypeDescriptor.GetConverter(destinationType);
                if (destinationConverter != null && destinationConverter.CanConvertFrom(value.GetType()))
                    return destinationConverter.ConvertFrom(null, culture, value);

                var sourceConverter = TypeDescriptor.GetConverter(sourceType);
                if (sourceConverter != null && sourceConverter.CanConvertTo(destinationType))
                    return sourceConverter.ConvertTo(null, culture, value, destinationType);

                if (destinationType.IsEnum && value is int)
                    return Enum.ToObject(destinationType, (int)value);

                if (!destinationType.IsInstanceOfType(value))
                    return Convert.ChangeType(value, destinationType, culture);
            }
            return value;
        }

        /// <summary>
        /// Tries to converts a value to a destination type.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="destinationType">The type to convert the value to.</param>
        /// <param name="culture">Culture</param>
        /// <returns>The converted value.</returns>
        public static bool To(this object value, Type destinationType, CultureInfo culture, out object destination)
        {
            try
            {
                destination = value.To(destinationType, culture);
                return true;
            }
            catch (Exception)
            {
                destination = null;
                return false;
            }
        }

        /// <summary>
        /// Converts a value to a destination type.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <typeparam name="T">The type to convert the value to.</typeparam>
        /// <returns>The converted value.</returns>
        public static T To<T>(this object value)
        {
            return (T)value.To(typeof(T));
        }

        /// <summary>
        /// Tries to converts a value to a destination type.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <typeparam name="T">The type to convert the value to.</typeparam>
        /// <returns>The converted value.</returns>
        public static bool To<T>(this object value, out T destination)
        {
            try
            {
                destination = (T)value.To(typeof(T));
                return true;
            }
            catch (Exception)
            {
                destination = default;
                return false;
            }
        }
    }
}