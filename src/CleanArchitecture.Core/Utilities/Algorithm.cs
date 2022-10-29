using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Utilities
{
    public static class Algorithm
    {
        public static async Task<string> GenerateSlugAsync(string text, string separator, Func<string, Task<bool>> exists)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            if (separator == null)
                throw new ArgumentNullException(nameof(text));

            string slug = null!;
            int count = 1;

            do
            {
                slug = GenerateSlug($"{text}{(count == 1 ? "" : $" {count}")}".Trim(), separator);
                count += 1;
            } while (await exists(slug));

            return slug;
        }

        // URL Slugify algorithm in C#?
        // source: https://stackoverflow.com/questions/2920744/url-slugify-algorithm-in-c/2921135#2921135
        public static string GenerateSlug(string text, string separator)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            if (separator == null)
                throw new ArgumentNullException(nameof(text));

            static string RemoveDiacritics(string text)
            {
                var normalizedString = text.Normalize(NormalizationForm.FormD);
                var stringBuilder = new StringBuilder();

                foreach (var c in normalizedString)
                {
                    var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                    if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                    {
                        stringBuilder.Append(c);
                    }
                }

                return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
            }

            // remove all diacritics.
            text = RemoveDiacritics(text);

            // Remove everything that's not a letter, number, hyphen, dot, whitespace or underscore.
            text = Regex.Replace(text, @"[^a-zA-Z0-9\-\.\s_]", string.Empty, RegexOptions.Compiled).Trim();

            // replace symbols with a hyphen.
            text = Regex.Replace(text, @"[\-\.\s_]", separator, RegexOptions.Compiled);

            // replace double occurrences of hyphen.
            text = Regex.Replace(text, @"(-){2,}", "$1", RegexOptions.Compiled).Trim('-');

            return text;
        }

        public static string GenerateHash(string input)
        {
            var byteValue = Encoding.UTF8.GetBytes(input);
            var byteHash = SHA256.HashData(byteValue);
            return Convert.ToBase64String(byteHash);
        }

        public static string GenerateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            // Convert the byte array to hexadecimal string
            var sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString();
        }

        public static string GenerateMD5(Stream input)
        {
            // Use input string to calculate MD5 hash
            using MD5 md5 = MD5.Create();
            byte[] hashBytes = md5.ComputeHash(input);

            // Convert the byte array to hexadecimal string
            var sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString();
        }

        public static Guid CreateCryptographicallySecureGuid()
        {
            return new Guid(Generate128BitsOfRandomEntropy());
        }

        private static byte[] Generate128BitsOfRandomEntropy()
        {
            var randomBytes = new byte[16]; // 16 Bytes will give us 128 bits.
            using (var rngCsp = RandomNumberGenerator.Create())
            {
                // Fill the array with cryptographically secure random bytes.
                rngCsp.GetBytes(randomBytes);
            }
            return randomBytes;
        }
    }
}
