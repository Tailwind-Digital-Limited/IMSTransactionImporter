using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Audacia.Spreadsheets.Extensions;

namespace Audacia.Spreadsheets
{
    /// <summary>
    /// Provides a set of functions for converting enums to display name strings and back again.
    /// Supported attributes are;
    /// <see cref="DisplayNameAttribute"/>,
    /// <see cref="DisplayAttribute"/>,
    /// <see cref="DescriptionAttribute"/>,
    /// <see cref="EnumMemberAttribute"/>.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "AV1710:Member name includes the name of its containing type",
        Justification = "This helper class includes its own name in a number of it's members, this is reference to the 'EnumMemberAttribute' as is chose to clearly indicate the source of the value.")]
    public static class EnumMember
    {
        /// <summary>
        /// Retrieves the description for all fields on the provided enum type.
        /// For fields without a <see cref="DescriptionAttribute"/>, <see langword="null"/> will be returned.
        /// </summary>
        /// <param name="enumType">Enum type.</param>
        /// <returns>A list of descriptions from the provided enum type.</returns>
        public static IEnumerable<string?> Descriptions(Type enumType)
        {
            enumType = ValidateEnumType(enumType);

            foreach (var value in Enum.GetValues(enumType))
            {
                yield return GetDescription(value);
            }
        }

        /// <summary>
        /// Retrieves the description for all fields on the provided enum type.
        /// For fields without a <see cref="DescriptionAttribute"/>, <see langword="null"/> will be returned.
        /// </summary>
        /// <typeparam name="TEnum">Enum type.</typeparam>
        /// <returns>A list of descriptions from the provided enum type.</returns>
        public static IEnumerable<string?> Descriptions<TEnum>() where TEnum : struct
        {
            return Descriptions(typeof(TEnum));
        }

        /// <summary>
        /// Retrieves the enum member value for all fields on the provided enum type.
        /// For fields without a <see cref="EnumMemberAttribute"/>, <see langword="null"/> will be returned.
        /// </summary>
        /// <param name="enumType">Enum type.</param>
        /// <returns>A list of values from the provided enum type.</returns>
        public static IEnumerable<string?> EnumMemberValues(Type enumType)
        {
            enumType = ValidateEnumType(enumType);

            foreach (var value in Enum.GetValues(enumType))
            {
                yield return GetEnumMemberValue(value);
            }
        }

        /// <summary>
        /// Retrieves the enum member value for all fields on the provided enum type.
        /// For fields without a <see cref="EnumMemberAttribute"/>, <see langword="null"/> will be returned.
        /// </summary>
        /// <typeparam name="TEnum">Enum type.</typeparam>
        /// <returns>A list of values from the provided enum type.</returns>
        public static IEnumerable<string?> EnumMemberValues<TEnum>() where TEnum : struct
        {
            return EnumMemberValues(typeof(TEnum));
        }

        /// <summary>
        /// Retrieves the field name for all fields on the provided enum type.
        /// </summary>
        /// <param name="enumType">Enum type.</param>
        /// <returns>A list of names from the provided enum type.</returns>
        public static IEnumerable<string?> Names(Type enumType)
        {
            enumType = ValidateEnumType(enumType);

            foreach (var value in Enum.GetValues(enumType))
            {
                yield return GetName(value);
            }
        }

        /// <summary>
        /// Retrieves the field name for all fields on the provided enum type.
        /// </summary>
        /// <typeparam name="TEnum">Enum type.</typeparam>
        /// <returns>A list of names from the provided enum type.</returns>
        public static IEnumerable<string?> Names<TEnum>() where TEnum : struct
        {
            return Names(typeof(TEnum));
        }

        /// <summary>
        /// Retrieves the first available human readable name for all fields in the provided enum type.
        /// </summary>
        /// <param name="enumType">Enum type.</param>
        /// <returns>A list of human readble names from the provided enum type.</returns>
        public static IEnumerable<string?> Options(Type enumType)
        {
            enumType = ValidateEnumType(enumType);

            foreach (var value in Enum.GetValues(enumType))
            {
                yield return GetOption(value);
            }
        }

        /// <summary>
        /// Retrieves the first available human readable name for all fields in the provided enum type.
        /// </summary>
        /// <typeparam name="TEnum">Enum type.</typeparam>
        /// <returns>A list of human readble names from the provided enum type.</returns>
        public static IEnumerable<string?> Options<TEnum>() where TEnum : struct
        {
            return Options(typeof(TEnum));
        }

        /// <summary>
        /// Returns the value set on the <see cref="DescriptionAttribute"/>.
        /// For fields without a <see cref="DescriptionAttribute"/>, <see langword="null"/> will be returned.
        /// </summary>
        /// <param name="enumValue">Enum value.</param>
        /// <returns>The description string.</returns>
        /// <returns>The value of <see cref="DescriptionAttribute"/> from the provided <paramref name="enumValue"/>.</returns>
        /// <exception cref="ArgumentNullException">enumType or value is null.</exception>
        public static string? GetDescription(object enumValue)
        {
            if (enumValue == null)
            {
                throw new ArgumentNullException(nameof(enumValue));
            }

            var enumType = enumValue.GetType().GetUnderlyingTypeIfNullable();

            enumType = ValidateEnumType(enumType);

            return GetFieldInfo(enumValue)
                ?.GetCustomAttribute<DescriptionAttribute>(false)
                ?.Description;
        }

        /// <summary>
        /// Returns the value set on the <see cref="EnumMemberAttribute"/>.
        /// For fields without a <see cref="EnumMemberAttribute"/>, <see langword="null"/> will be returned.
        /// </summary>
        /// <param name="enumValue">Enum value.</param>
        /// <returns>The enummember value string.</returns>
        /// <exception cref="ArgumentNullException">enumType or value is null.</exception>
        public static string? GetEnumMemberValue(object enumValue)
        {
            if (enumValue == null)
            {
                throw new ArgumentNullException(nameof(enumValue));
            }

            var enumType = enumValue.GetType().GetUnderlyingTypeIfNullable();

            enumType = ValidateEnumType(enumType);

            return GetFieldInfo(enumValue)
                ?.GetCustomAttribute<EnumMemberAttribute>(false)
                ?.Value;
        }

        /// <summary>
        /// Returns the field name for the enum value.
        /// </summary>
        /// <param name="enumValue">Enum value.</param>
        /// <returns>The field name.</returns>
        /// <exception cref="ArgumentNullException">enumType or value is null.</exception>
        public static string? GetName(object enumValue)
        {
            if (enumValue == null)
            {
                throw new ArgumentNullException(nameof(enumValue));
            }

            var enumType = enumValue.GetType().GetUnderlyingTypeIfNullable();

            enumType = ValidateEnumType(enumType);

            return GetFieldInfo(enumValue)?.Name;
        }

        /// <summary>
        /// Returns the first available human readable name for the enum value.
        /// </summary>
        /// <param name="enumValue">Enum value.</param>
        /// <returns>A human readable name for the enum value.</returns>
        public static string? GetOption(object enumValue)
        {
            return GetDescription(enumValue) ??
                   GetEnumMemberValue(enumValue) ??
                   GetName(enumValue) ??
                   enumValue?.ToString();
        }

        /// <summary>
        /// Returns the enum value as a text integer.
        /// </summary>
        /// <param name="enumValue">Enum value.</param>
        /// <returns>The field name.</returns>
        /// <exception cref="ArgumentNullException">enumType or value is null.</exception>
        public static string? GetValue(object enumValue)
        {
            if (enumValue == null)
            {
                throw new ArgumentNullException(nameof(enumValue));
            }

            var enumType = enumValue.GetType().GetUnderlyingTypeIfNullable();

            ValidateEnumType(enumType);

            return Convert.ToInt32(enumValue, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Gets the value of an enum which corresponds to the specified enum member, display name, or description.
        /// </summary>
        /// <typeparam name="TEnum">Enum type.</typeparam>
        /// <param name="value">Input value.</param>
        /// <exception cref="ArgumentNullException">enumType or value is null.</exception>
        /// <exception cref="ArgumentException">
        ///     enumType is not an System.Enum.
        ///     Or value is either an empty string, or only contains whitespace.
        ///     Or value is a name, but not one of the named constants defined for the enumeration.
        /// </exception>
        /// <exception cref="OverflowException">value is outside the range of the underlying type of enumType.</exception>
        /// <returns>The enum value as an object.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "AV1551:Method overload should call another overload", Justification = "Method calls Parse(Type enumType, string value)")]
        public static TEnum? Parse<TEnum>(string value) where TEnum : struct
        {
            return (TEnum?)Parse(typeof(TEnum?), value ?? string.Empty);
        }

        /// <summary>
        /// Gets the value of an enum which corresponds to the specified enum member, display name, or description.
        /// </summary>
        /// <param name="enumType">Enum type.</param>
        /// <param name="value">Input value.</param>
        /// <exception cref="ArgumentNullException">enumType or value is null.</exception>
        /// <exception cref="ArgumentException">
        ///     enumType is not an System.Enum.
        ///     Or value is either an empty string, or only contains whitespace.
        ///     Or value is a name, but not one of the named constants defined for the enumeration.
        /// </exception>
        /// <exception cref="OverflowException">value is outside the range of the underlying type of enumType.</exception>
        /// <returns>The enum value as an object.</returns>
        public static object? Parse(Type enumType, string value)
        {
            enumType = ValidateEnumType(enumType);

            value = SanitizeDisplayNameString(value) ?? string.Empty;
            ValidateValueString(value);
            
            if (TryParseEnumAsString(enumType, value, out var enumValue))
            {
                return enumValue;
            }

            // Get an enumerable of enum options
            var fields = enumType.GetFields().Where(f => f.IsStatic);

            // Attempt to match to each possible display value
            foreach (var field in fields)
            {
                if (MatchesEnumMember(field, value) ||
                    MatchesDescription(field, value) ||
                    MatchesName(field, value) ||
                    MatchesDisplay(field, value))
                {
                    return field.GetValue(null) ?? default!;
                }
            }

            throw new ArgumentException($"Requested value '{value}' was not found.");
        }

        /// <summary>
        /// Attempts to parse the enum as a string and return the relevant member.
        /// </summary>
        /// <param name="type">Type of Enum.</param>
        /// <param name="value">Value to convert.</param>
        /// <param name="enumValue">Return value of the enum if parsed.</param>
        /// <returns>The parsed enum.</returns>
        /// <exception cref="OverflowException"> Throws when the enum member is out of range.</exception>
        private static bool TryParseEnumAsString(Type type, string value, out object? enumValue)
        {
            bool enumDefined;

            try
            {
                // Match by number value or field name
                enumValue = Enum.Parse(type, value, ignoreCase: true);
                enumDefined = Enum.IsDefined(type, enumValue);
            }
            catch (ArgumentException)
            {
                // Ignore enum value not found error
                enumValue = null;
                enumDefined = false;
            }
            
            /*When an enum isnt defined, but still has a value, this is an error state and due to backwards
            //compatability reasons (see unit test Check_that_overflow_exception_is_correctly_called()) an exception is
            thrown*/
            
            if (!enumDefined && enumValue != null)
            {
                throw new OverflowException($"Value '{value}' is outside the range of '{(type == null ? "the provided enum type" : type.Name)}'.");
            }

            return enumDefined;
        }

        /// <summary>
        /// Gets the value of an enum which corresponds to the specified enum member, display name, or description.
        /// </summary>
        /// <param name="enumType">Enum type.</param>
        /// <param name="value">Input value.</param>
        /// <param name="enumValue">The enum value as an object.</param>
        /// <returns><see langword="true" /> if parsed successfully.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Matters more if there *is* an error more than *what* error.")]
        public static bool TryParse(Type enumType, string value, out object? enumValue)
        {
            try
            {
                enumValue = Parse(enumType, value);
                return true;
            }
            catch
            {
                enumValue = null;
            }

            return false;
        }

        /// <summary>
        /// Gets the value of an enum which corresponds to the specified enum member, display name, or description.
        /// </summary>
        /// <typeparam name="TEnum">Enum type.</typeparam>
        /// <param name="value">Input value.</param>
        /// <param name="enumValue">The enum value as an object.</param>
        /// <returns><see langword="true" /> if parsed successfully.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "AV1551:Method overload should call another overload", Justification = "Method calls TryParse(Type enumType, string value, out object? enumValue)")]
        public static bool TryParse<TEnum>(string value, out TEnum? enumValue) where TEnum : struct
        {
            if (TryParse(typeof(TEnum), value, out var enumObj))
            {
                enumValue = (TEnum?)enumObj;
                return true;
            }

            enumValue = default(TEnum);
            return false;
        }

        /// <summary>
        /// Retrieves the value for all fields on the provided enum type.
        /// </summary>
        /// <param name="enumType">Enum type.</param>
        /// <returns>An integer array of enum values.</returns>
        public static IEnumerable<int> Values(Type enumType)
        {
            enumType = ValidateEnumType(enumType);

            return (int[])Enum.GetValues(enumType);
        }

        /// <summary>
        /// Retrieves the value for all fields on the provided enum type.
        /// </summary>
        /// <typeparam name="TEnum">Enum type.</typeparam>
        /// <returns>An array of enum values.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "AV1551:Method overload should call another overload", Justification = "Other 'overload' method has different return type.")]
        public static IEnumerable<TEnum> Values<TEnum>() where TEnum : struct
        {
            var enumType = typeof(TEnum);
            enumType = ValidateEnumType(enumType);

            return (TEnum[])Enum.GetValues(typeof(TEnum));
        }

        private static FieldInfo? GetFieldInfo(object enumValue)
        {
            return enumValue.GetType().GetField(enumValue.ToString() ?? string.Empty);
        }

        private static bool MatchesDescription(MemberInfo member, string? value)
        {
            var attribute = member.GetCustomAttribute<DescriptionAttribute>(false);

            if (attribute != null)
            {
                return string.Equals(attribute.Description, value, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        private static bool MatchesEnumMember(MemberInfo member, string? value)
        {
            var attribute = member.GetCustomAttribute<EnumMemberAttribute>(false);

            if (attribute != null)
            {
                return string.Equals(attribute.Value, value, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        private static bool MatchesDisplay(MemberInfo member, string? value)
        {
            var attribute = member.GetCustomAttribute<DisplayAttribute>(false);

            if (attribute != null)
            {
                return string.Equals(attribute.Name, value, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        private static bool MatchesName(MemberInfo member, string? value)
        {
            return string.Equals(member.Name, value, StringComparison.OrdinalIgnoreCase);
        }

        private static string? SanitizeDisplayNameString(string? value)
        {
            // Ignore long dashes from user input
            return value?.Replace("–", "-", StringComparison.InvariantCulture).Trim();
        }

        private static Type ValidateEnumType(Type enumType)
        {
            if (enumType == null)
            {
                throw new ArgumentNullException(nameof(enumType));
            }

            if (enumType.IsNullable())
            {
                enumType = enumType.GetUnderlyingTypeIfNullable();
            }

            if (!enumType.IsEnum)
            {
                throw new ArgumentException("Type must be an enum type.", nameof(enumType));
            }

            return enumType;
        }

        private static void ValidateValueString(string? value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Value cannot be empty or whitespace.", nameof(value));
            }
        }
    }
}