using System.Collections.Generic;

namespace Audacia.Spreadsheets.Validation
{
    /// <summary>
    /// An import error for when multiple of the same unique constraint are imported.
    /// </summary>
    public class DuplicateKeyError : RowGroupImportError, IImportError
    {
        public string KeyName { get; }

        public string DuplicateKey { get; }

        public DuplicateKeyError(int rowNumber, string keyName, string duplicateKey)
            : base(new[] { rowNumber })
        {
            KeyName = keyName;
            DuplicateKey = duplicateKey;
        }

        public DuplicateKeyError(IEnumerable<int> rowNumbers, string keyName, string duplicateKey) 
            : base(rowNumbers)
        {
            KeyName = keyName;
            DuplicateKey = duplicateKey;
        }

        public string GetMessage()
        {
            return $"The {KeyName} {DuplicateKey} appears multiple times in the import.";
        }
    }
}