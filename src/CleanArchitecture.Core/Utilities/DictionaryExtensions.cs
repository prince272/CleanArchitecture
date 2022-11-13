namespace CleanArchitecture.Core.Utilities
{
    /// <summary>
    /// Dictionary extensions.
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// A Dictionary&lt;TKey,TValue&gt; extension method that attempts to
        /// remove a key from the dictionary.
        /// </summary>
        /// <typeparam name="TKey">Type of the key.</typeparam>
        /// <typeparam name="TValue">Type of the value.</typeparam>
        /// <param name="dictionary">The dictionary to act on.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">[out] The value.</param>
        /// <returns>true if it succeeds, false if it fails.</returns>
        /// <remarks>https://github.com/zzzprojects/Eval-SQL.NET/blob/master/src/Z.Expressions.SqlServer.Eval/Extensions/Dictionary%602/TryRemove.cs</remarks>
        public static bool TryRemove<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, out TValue? value) where TKey : notnull
        {
            var isRemoved = dictionary.TryGetValue(key, out value);
            if (isRemoved)
            {
                dictionary.Remove(key);
            }

            return isRemoved;
        }

        /// <summary>
        /// Gets the value, if available, or <paramref name="ifNotFound"/>.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="self">The dictionary to search.</param>
        /// <param name="key">The item key.</param>
        /// <param name="ifNotFound">The fallback value.</param>
        /// <returns>
        /// Returns the item in <paramref name="self"/> that matches <paramref name="key"/>,
        /// falling back to the value of <paramref name="ifNotFound"/> if the item is unavailable.
        /// </returns>
        public static TValue? GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue? ifNotFound = default) where TKey : notnull
        {
            return dictionary.TryGetValue(key, out TValue? value) ? value : ifNotFound;
        }

        /// <summary>
        /// Tries to obtain the given key, otherwise returns null.
        /// </summary>
        /// <param name="values">The dictionary for the lookup.</param>
        /// <param name="key">The key to look for.</param>
        /// <returns>An object instance or null.</returns>
        /// <remarks>https://github.com/AngleSharp/AngleSharp/blob/master/src/AngleSharp/Common/ObjectExtensions.cs#L123</remarks>
        public static object? GetValueOrDefault(this IDictionary<string, object> values, string key)
        {
            values.TryGetValue(key, out var value);
            return value;
        }
    }
}