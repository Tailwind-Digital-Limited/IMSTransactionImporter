using System;
using System.Collections.Generic;
using System.Linq;

namespace Audacia.Spreadsheets.Extensions
{
#pragma warning disable AV1745
    public static class Worksheets
#pragma warning restore AV1745
    {
        /// <summary>
        /// Returns the first tables on the current worksheet.
        /// When importing a spreadsheet there will only be one table per worksheet.
        /// </summary>
        public static Table GetTable(this WorksheetBase worksheet)
        {
            return GetTables(worksheet).FirstOrDefault();
        }

        /// <summary>
        /// Returns all tables on the current worksheet. 
        /// </summary>
        public static IEnumerable<Table> GetTables(this WorksheetBase worksheet)
        {
            if (worksheet is Worksheet singleTableWorksheet)
            {
                yield return singleTableWorksheet.Table;
            }
            else if (worksheet is MultiTableWorksheet multiTableWorksheet)
            {
                foreach (var table in multiTableWorksheet.Tables)
                {
                    yield return table;
                }                
            }
            else
            {
                throw new NotSupportedException($"Type of {worksheet?.GetType()} is not supported by GetTables().");
            }
        }

        /// <summary>
        /// Returns all tables for the provided worksheets.
        /// </summary>
        public static IEnumerable<Table> GetTables(this IEnumerable<WorksheetBase> worksheets)
        {
            return worksheets.SelectMany(GetTables).ToList();
        }

        /// <summary>
        /// Creates a worksheet from an enumerable.
        /// </summary>
#pragma warning disable ACL1003
        public static Worksheet ToWorksheet<TEntity>(
            this IEnumerable<TEntity> source, 
#pragma warning restore ACL1003
#pragma warning disable AV1553
            string? sheetName = null, 
#pragma warning restore AV1553
#pragma warning disable AV1564
            bool includeHeaders = true,
#pragma warning restore AV1564
            TableHeaderStyle? headerStyle = null,
            params string[] ignoreProperties)
            where TEntity : class
        {
            var table = source.ToTable(includeHeaders, headerStyle, ignoreProperties);

            var freezePane = default(FreezePane);
            if (includeHeaders)
            {
                freezePane = new FreezePane();
                if (table.Columns.Any(c => c.DisplaySubtotal))
                {
                    freezePane.StartingCell = "A3";
                    freezePane.FrozenRows = 2;
                }
            }

            return new Worksheet
            {
                SheetName = sheetName,
                FreezePane = freezePane,
                Table = table,
                HasAutofilter = true
            };
        }
    }
}
