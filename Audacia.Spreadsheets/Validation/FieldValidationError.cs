using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Audacia.Spreadsheets.Validation
{
    /// <summary>
    /// An import error for member validation errors on a row.
    /// </summary>
    public class FieldValidationError : RowImportError, IImportError
    {
        public ICollection<ValidationResult> ValidationErrors { get; }

        public FieldValidationError(int rowNumber, ICollection<ValidationResult> validationErrors) 
            : base(rowNumber)
        {
            ValidationErrors = validationErrors;
        }
        
        public string GetMessage()
        {
            var builder = new StringBuilder();
            if (ValidationErrors.Any())
            {
                builder.AppendLine("The validation requirements were not met;");
            }

            foreach (var error in ValidationErrors)
            {
                builder.Append(error.DisplayName);
                builder.AppendLine(":");
                foreach (var message in error.Errors)
                {
                    builder.AppendLine(message);
                }

                builder.AppendLine();
            }

            return builder.ToString();
        }
    }
}