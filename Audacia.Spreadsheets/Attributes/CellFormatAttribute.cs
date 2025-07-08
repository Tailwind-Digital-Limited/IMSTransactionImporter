using System;

namespace Audacia.Spreadsheets.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class CellFormatAttribute : Attribute
    {
        public CellFormatAttribute(CellFormat cellFormat) => CellFormat = cellFormat;

        public CellFormat CellFormat { get; }
    }
}
