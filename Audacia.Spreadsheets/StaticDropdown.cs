using System.Collections.Generic;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Audacia.Spreadsheets
{
    public class StaticDropdown
    {
        private string FormulaText { get; set; }

        public bool AllowBlanks { get; set; }

        public int StartingRow { get; set; } = 2;

        public int EndingRow { get; set; } = 1048576;

        public string Column { get; set; } = string.Empty;

        public StaticDropdown(IEnumerable<string> options)
        {
            FormulaText = $"\"{string.Join(",", options)}\"";
        }

        public StaticDropdown(string formula)
        {
            FormulaText = formula;
        }

        public void Write(DataValidations dataValidations)
        {
            var dataValidation = new DataValidation()
            {
                Type = DataValidationValues.List,
                AllowBlank = AllowBlanks,
                SequenceOfReferences = new ListValue<StringValue>() { InnerText = $"{Column}{StartingRow}:{Column}{EndingRow}" }
            };
            var formula = new Formula1
            {
                Text = FormulaText
            };

            dataValidation.Append(formula);
            dataValidations.Append(dataValidation);
        }
    }
}
