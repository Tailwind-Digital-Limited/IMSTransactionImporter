using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Audacia.Spreadsheets.Attributes;

namespace Audacia.Spreadsheets.Extensions
{
    public static class Tables
    {
        /// <summary>
        /// Reads all properties on the provided class and returns a list of Table Columns.
        /// </summary>
        public static IReadOnlyCollection<TableColumn> GetColumns<TEntity>(params string[] ignoreProperties)
            where TEntity : class
        {
            var tableColumns = new List<TableColumn>();
            
            foreach (var property in typeof(TEntity).GetProps(ignoreProperties))
            {
                var column = CreateColumnFromClassProperty<TEntity>(property);

                tableColumns.Add(column);
            }
            
            return tableColumns;
        }

        private static TableColumn CreateColumnFromClassProperty<TEntity>(PropertyInfo property) where TEntity : class
        {
            var cellFormat = property.GetCustomAttributes<CellFormatAttribute>(false).FirstOrDefault();
            var cellHeader = property.GetCustomAttributes<CellHeaderNameAttribute>(false).FirstOrDefault();
            var hideCellHeader = property.GetCustomAttributes<HideHeaderAttribute>(false).Any();
            var displaySubtotal = property.GetCustomAttributes<SubtotalHeaderAttribute>(false).Any() &&
                                  property.PropertyType.IsNumeric();
            var backgroundColour = property.GetCustomAttributes<CellBackgroundColourAttribute>(false).FirstOrDefault();
            var textColour = property.GetCustomAttributes<CellTextColourAttribute>(false).FirstOrDefault();

            // Get the column width from the property or the class
            var columnWidth = property.GetCustomAttributes<ColumnWidthAttribute>(false).FirstOrDefault()
                              ?? property.DeclaringType?.GetCustomAttributes<ColumnWidthAttribute>(false).FirstOrDefault();

            // The column name is either the cell header, the Display attribute or the property name.
            // Please do not alter this statement.
            var columnName = cellHeader?.Name?.PascalToTitleCase()
                             ?? property.GetDataAnnotationDisplayName();

            // Use the provided CellFormat or calculate it based on the property type
            var cellFormatValue = cellFormat?.CellFormat
                                  ?? property.PropertyType.GetCellFormat();

            return new TableColumn
            {
                PropertyInfo = property,
                Name = hideCellHeader ? string.Empty : columnName,
                DisplaySubtotal = displaySubtotal,
                CellBackgroundFormat = backgroundColour,
                CellTextFormat = textColour,
                Width = columnWidth?.Width,
                Format = cellFormatValue
            };
        }

        /// <summary>
        /// Converts the provided entity with attributes into a table row.
        /// </summary>
        public static TableRow GetRow<TEntity>(this TEntity entry, IReadOnlyCollection<TableColumn> columns)
            where TEntity : class
        {
            var cells = columns.Select(column =>
            {
                var cell = new TableCell
                {
                    Value = column.PropertyInfo?.GetValue(entry) ?? string.Empty,
                    FillColour = column.CellBackgroundFormat?.Colour,
                    TextColour = column.CellTextFormat?.Colour
                };

                // Get FillColour from property
                if (!string.IsNullOrEmpty(column.CellBackgroundFormat?.ReferenceField))
                {
                    cell.FillColour = typeof(TEntity)
                        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Where(property => string.Equals(
                            property.Name, 
                            column.CellBackgroundFormat?.ReferenceField, 
                            StringComparison.OrdinalIgnoreCase))
                        .Select(property => property.GetValue(entry, null) as string)
                        .FirstOrDefault();
                }
                    
                // Get TextColour from property
                if (!string.IsNullOrEmpty(column.CellTextFormat?.ReferenceField))
                {
                    cell.TextColour = typeof(TEntity)
                        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Where(property => string.Equals(
                            property.Name, 
                            column.CellTextFormat?.ReferenceField,
                            StringComparison.OrdinalIgnoreCase))
                        .Select(property => property.GetValue(entry, null) as string)
                        .FirstOrDefault();
                }

                return cell;
            });
                
            return TableRow.FromCells(cells, null);
        }

        /// <summary>
        /// Converts the provided enumerable into an enumerable of table rows.
        /// </summary>
        public static IEnumerable<TableRow> GetRows<TEntity>(IEnumerable<TEntity> source, IReadOnlyCollection<TableColumn> columns)
            where TEntity : class
        {
            return source.Select(entry => GetRow(entry, columns)).ToList();
        }

        /// <summary>
        /// Creates a spreadsheet table from an enumerable.
        /// </summary>
        public static Table ToTable<TEntity>(
            this IEnumerable<TEntity> source,
#pragma warning disable AV1564
            bool includeHeaders = true,
#pragma warning restore AV1564
            TableHeaderStyle? headerStyle = null,
            params string[] ignoreProperties)
            where TEntity : class
        {
            var columns = GetColumns<TEntity>(ignoreProperties);
            return new Table(includeHeaders)
            {
                HeaderStyle = headerStyle ?? new TableHeaderStyle(),
                Columns = columns.ToList(),
                Rows = GetRows(source, columns)
            };
        }
    }
}