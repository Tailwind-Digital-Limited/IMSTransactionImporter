using System.Collections.Generic;
#pragma warning disable AV1704

namespace Audacia.Spreadsheets.Validation
{
    /// <summary>
    /// To be used when the referenced record cannot be associated.
    /// </summary>
    public class RecordAssociationError : RowGroupImportError, IImportError
    {
        private string TypeOne { get; }
        
        private string TypeTwo { get; }
        
        private string ValueOne { get; }
        
        private string ValueTwo { get; }

#pragma warning disable ACL1003
        public RecordAssociationError(string typeOne, string typeTwo, string valueOne, string valueTwo, IEnumerable<int> rowNumbers) 
#pragma warning restore ACL1003
            : base(rowNumbers)
        {
            TypeOne = typeOne;
            TypeTwo = typeTwo;
            ValueOne = valueOne;
            ValueTwo = valueTwo;
        }

        public string GetMessage()
        {
            return $"{TypeOne} of {ValueOne} is not associated with {TypeTwo} of {ValueTwo}.";
        }
    }
}