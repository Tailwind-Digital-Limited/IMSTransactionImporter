using System.Collections.Generic;
using System.Linq;

namespace Audacia.Spreadsheets.Extensions
{
#pragma warning disable AV1745
    public static class TableColumns
#pragma warning restore AV1745
    {
        /// <summary>
        /// Creates a dictionary that maps column header to column index.
        /// </summary>
        /// <param name="columns">Worksheet table columns</param>
#pragma warning disable AV1130
        public static IDictionary<string, int> ToDictionary(this IEnumerable<TableColumn> columns)
#pragma warning restore AV1130
        {
            return columns
                .Where(col => !string.IsNullOrEmpty(col.Name))
                .Select((col, index) => (index, col: col.Name!.Trim()))
                .ToDictionary(c => c.col, c => c.index);
        }
    }
}