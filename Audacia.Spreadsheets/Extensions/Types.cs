using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Audacia.Spreadsheets.Attributes;

namespace Audacia.Spreadsheets.Extensions
{
#pragma warning disable AV1745
    internal static class Types
#pragma warning restore AV1745
    {
        /// <summary>
        /// Returns an enumerable of properties that should display on the worksheet.
        /// </summary>
        public static IEnumerable<PropertyInfo> GetProps(this Type classType, params string[] ignoreProperties)
        {
            return classType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property => WantedProperty(ignoreProperties, property))
                .Where(p =>
            {
                // And the property is a value type not a class
                var underlyingType = p.PropertyType.GetUnderlyingTypeIfNullable();
                return underlyingType.IsValueType || underlyingType == typeof(string);
            }).ToList();
        }

#pragma warning disable RCS1213
        private static bool WantedProperty(string[] ignoreProperties, PropertyInfo property)
#pragma warning restore RCS1213
        {
            var ignoreDataMemberAttrs = property.GetCustomAttributes(typeof(IgnoreDataMemberAttribute), false);
            if (ignoreDataMemberAttrs.Any())
            {
                return false;
            }

            // Property should ony be ignored by the spreadsheet exporter
            var cellIgnoreAttrs = property.GetCustomAttributes(typeof(CellIgnoreAttribute), false);
            if (cellIgnoreAttrs.Any())
            {
                return false;
            }

            // Property is a primary key and should be ignored (Legacy support)
            var primaryKeyAttrs = property.GetCustomAttributes(typeof(IdColumnAttribute), false);
            if (primaryKeyAttrs.Any())
            {
                return false;
            }

            // Property should be ignored if it's in the list of ignoreProperties
            return !ignoreProperties?.Contains(property.Name) ?? true;
        }

        /// <summary>
        /// Gets the default cell format for a property.
        /// This is intended to be used to auto configure datetime columns that have not been explicitly setup.
        /// </summary>
        /// <exception cref="ArgumentNullException">Occurs when <see cref="propertyType"/> is <see langword="null"/>.</exception>
        internal static CellFormat GetCellFormat(this Type propertyType)
        {
            if (propertyType == null)
            {
                throw new ArgumentNullException(nameof(propertyType));
            }

            var underlyingType = propertyType.GetUnderlyingTypeIfNullable();

            if (underlyingType == typeof(DateTime) ||
                underlyingType == typeof(DateTimeOffset)) 
            {
                return CellFormat.DateTime;
            }

            if (underlyingType == typeof(TimeSpan)) 
            {
                return CellFormat.TimeSpanFull;
            }

            // It's fine for booleans, numbers, and enums to be text, altering this would cause breaking changes to other projects.
            return CellFormat.Text;
        }
    }
}