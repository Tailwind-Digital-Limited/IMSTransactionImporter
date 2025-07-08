using System.Collections.Generic;

namespace Audacia.Spreadsheets.Validation
{
    /// <summary>
    /// An import error referencing a collection of Rows.
    /// </summary>
    public abstract class RowGroupImportError
    {
        public ICollection<int> RowNumbers { get; }

        protected RowGroupImportError(IEnumerable<int> rowNumbers)
        {
            RowNumbers = new HashSet<int>(rowNumbers);
        }
    }
}