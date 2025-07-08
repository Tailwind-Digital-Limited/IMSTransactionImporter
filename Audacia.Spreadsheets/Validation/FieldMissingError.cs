namespace Audacia.Spreadsheets.Validation
{
    /// <summary>
    /// An import error for when a cell value is missing.
    /// </summary>
    public class FieldMissingError : RowImportError, IImportError
    {
        public string FieldName { get; }

        public FieldMissingError(int rowNumber, string fieldName) 
            : base(rowNumber)
        {
            FieldName = fieldName;
        }

        public string GetMessage()
        {
            return $"{FieldName} is missing on row {RowNumber}.";
        }
    }
}