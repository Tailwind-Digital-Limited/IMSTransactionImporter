using System.Collections.Generic;
using System.Linq;

namespace Audacia.Spreadsheets.Extensions
{
#pragma warning disable AV1745
    public static class TableRows
#pragma warning restore AV1745
    {
        /// <summary>
        /// Returns an array of values for the provided rows.
        /// </summary>
#pragma warning disable AV1130
        public static string?[][] GetFields(this IEnumerable<TableRow> rows)
#pragma warning restore AV1130
        {
            return rows.Select(GetFields).ToArray();
        }
        
        /// <summary>
        /// Returns an array of cell values for the provided row.
        /// </summary>
#pragma warning disable AV1130
        public static string?[] GetFields(this TableRow row)
#pragma warning restore AV1130
        {
            return row.Cells.Select(TableCells.GetValue).ToArray();
        }

        /// <summary>
        /// Returns true if the row contains no data.
        /// </summary>
        public static bool IsEmpty(this TableRow row)
        {
            return row.Cells.Select(TableCells.GetValue).All(string.IsNullOrEmpty);
        }
    }
}