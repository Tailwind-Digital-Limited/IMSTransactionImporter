using System;
using System.Collections.Generic;
using System.Linq;

namespace Audacia.Spreadsheets
{
    public static class Bool
    {
        private static readonly ICollection<string> TrueValues = new[] { "TRUE", "Yes", "Y", "1" };
        
        private static readonly ICollection<string> FalseValues = new[] { "FALSE", "No", "N", "0" };
        
        /// <summary>
        /// Parses a string value to a boolean.
        /// </summary>
        /// <param name="input">source string</param>
        /// <returns>boolean value</returns>
        /// <exception cref="ArgumentNullException">Occurs when the input string is null.</exception>
        /// <exception cref="FormatException">Occurs when a boolean cannot be created from the input string.</exception>
        public static bool Parse(string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (!TryParse(input, out var result))
            {
                throw new FormatException($"Unknown format: \"{input}\".");
            }

            return result;
        }
        
        /// <summary>
        /// Tries to parse a string value to a boolean.
        /// </summary>
        /// <param name="input">source string</param>
        /// <param name="value">resultant boolean value</param>
        /// <returns>true if successful.</returns>
#pragma warning disable AV1564
#pragma warning disable AV1715
        public static bool TryParse(string input, out bool value)
#pragma warning restore AV1715
#pragma warning restore AV1564
        {
            if (!string.IsNullOrEmpty(input))
            {
                var trimmed = input.Trim();
                if (TrueValues.Contains(trimmed, StringComparer.OrdinalIgnoreCase))
                {
                    return value = true;
                }
                
                if (FalseValues.Contains(trimmed, StringComparer.OrdinalIgnoreCase))
                {
                    value = false;
                    return true;
                }
            }

            return value = false;
        }
    }
}