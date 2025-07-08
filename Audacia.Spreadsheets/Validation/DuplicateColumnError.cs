using System.Collections.Generic;
using System.Linq;

namespace Audacia.Spreadsheets.Validation
{
    /// <summary>
    /// An import error for when there are multiple columns with the same name.
    /// </summary>
    public class DuplicateColumnError : IImportError
    {
        public ICollection<string> ColumnNames { get; }

        public DuplicateColumnError(IEnumerable<string> duplicateColumns)
        {
            ColumnNames = new HashSet<string>(duplicateColumns);
        }

        public string GetMessage()
        {
            var columnNames = ColumnNames.Distinct();
            return $"The following columns are duplicated; {string.Join(", ", columnNames)}";
        }
    }
}