using System.Collections.Generic;

namespace Audacia.Spreadsheets.Validation
{
    /// <summary>
    /// To be used when the referenced record doesn't exist in the system.
    /// </summary>
    public class RecordMissingError : RowGroupImportError, IImportError
    {
        private string Type { get; set; }

        private string MissingName { get; set; }

        public RecordMissingError(string type, string missingName, IEnumerable<int> rowNumbers) 
            : base(rowNumbers)
        {
            Type = type;
            MissingName = missingName;
        }

        public string GetMessage()
        {
            return $"Could not find a {Type} called {MissingName}.";
        }
    }
}