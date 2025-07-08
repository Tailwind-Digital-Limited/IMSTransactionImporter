using System.Collections.Generic;

namespace Audacia.Spreadsheets.Validation
{
    /// <summary>
    /// To be used when a unique record is already in the system.
    /// </summary>
    public class RecordExistsError : RowGroupImportError, IImportError
    {
        public string KeyName { get; }

        public string DuplicateKey { get; }

        public RecordExistsError(int rowNumber, string keyName, string duplicateKey)
            : base(new[] { rowNumber })
        {
            KeyName = keyName;
            DuplicateKey = duplicateKey;
        }

        public RecordExistsError(IEnumerable<int> rowNumbers, string keyName, string duplicateKey) 
            : base(rowNumbers)
        {
            KeyName = keyName;
            DuplicateKey = duplicateKey;
        }

        public string GetMessage()
        {
            return $"The {KeyName} {DuplicateKey} already exists in the system.";
        }
    }
}