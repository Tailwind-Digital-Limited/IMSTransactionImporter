using System.Collections.Generic;

namespace Audacia.Spreadsheets.Validation
{
    /// <summary>
    /// An import error for one or more missing worksheets.
    /// </summary>
    public class MissingWorksheetError : IImportError
    {
        public ICollection<string> SheetNames { get; }

        public MissingWorksheetError(IEnumerable<string> sheetNames)
        {
            SheetNames = new HashSet<string>(sheetNames);
        }
        
        public string GetMessage()
        {
            return $"The following worksheets are missing; {string.Join(", ", SheetNames)}.";
        }
    }
}