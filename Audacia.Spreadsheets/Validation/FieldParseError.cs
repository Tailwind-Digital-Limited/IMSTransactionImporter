namespace Audacia.Spreadsheets.Validation
{
    /// <summary>
    /// An import error for when a cell value cannot be parsed.
    /// </summary>
    public class FieldParseError : RowImportError, IImportError
    {
        public string FieldName { get; }
        
        public string? Value { get; }
        
        public string? PossibleValues { get; }

        public FieldParseError(int rowNumber, string fieldName)
            : base(rowNumber)
        {
            FieldName = fieldName;
        }

        public FieldParseError(int rowNumber, string fieldName, string value, params string?[] possibleValues) 
            : base(rowNumber)
        {
            FieldName = fieldName;
            Value = value;
            PossibleValues = possibleValues.Length > 0
                ? string.Join(", ", possibleValues)
                : null;
        }

        public string GetMessage()
        {
            if (Value == null && PossibleValues == null)
            {
                return $"Unable to parse field \"{FieldName}\".";
            }

            return $"Unable to parse field \"{FieldName}\" of \"{Value}\"{(!string.IsNullOrEmpty(PossibleValues) ? $", please use {PossibleValues}" : string.Empty)}.";
        }
    }
}