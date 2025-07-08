using System.Collections.Generic;

namespace Audacia.Spreadsheets.Validation
{
    /// <summary>
    /// An import error for one or more missing columns.
    /// </summary>
    public class MissingColumnError : IImportError
    {
        public ICollection<string> MissingColumns { get; }

        public MissingColumnError(IEnumerable<string> missingColumns)
        {
            MissingColumns = new HashSet<string>(missingColumns);
        }

        public string GetMessage()
        {
            return $"The following columns are missing; {string.Join(", ", MissingColumns)}.";
        }
    }
}