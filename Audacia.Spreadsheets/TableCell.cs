using System;
using System.Collections.Generic;
using System.Globalization;
using Audacia.Spreadsheets.Extensions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Audacia.Spreadsheets
{
    public class TableCell
    {
        public TableCell() { }

        public TableCell(object? value)
        {
            Value = value;
        }
        
#pragma warning disable AV1564
        public TableCell(object? value, bool isFormula)
#pragma warning restore AV1564
        {
            Value = value;
            IsFormula = isFormula;
        }
        
        public TableCell(
            object? value = null,
#pragma warning disable AV1564
            bool isFormula = false, 
            bool hasBorders = true,
            bool isBold = false)
#pragma warning restore AV1564
        {
            Value = value;
            IsFormula = isFormula;
            HasBorders = hasBorders;
            IsBold = isBold;
        }

        public object? Value { get; set; }

        public string? FillColour { get; set; }

        public string? TextColour { get; set; }

        public bool IsFormula { get; set; }

        public bool HasBorderTop { get; set; } = true;
        
        public bool HasBorderRight { get; set; } = true;
        
        public bool HasBorderBottom { get; set; } = true;
        
        public bool HasBorderLeft { get; set; } = true;

        // making still usable despite separating the border into 4 sections as may be required
        public bool HasBorders
        {
            get => HasBorderTop && HasBorderRight && HasBorderBottom && HasBorderLeft;
            set 
            { 
                HasBorderTop = value;
                HasBorderRight = value;
                HasBorderBottom = value;
                HasBorderLeft = value;
            }
        }

        public bool IsBold { get; set; }

        public CellStyle CellStyle(TableColumn column)
        {
            return new CellStyle
            {
                TextColour = IsBold ? 1U : 0U,
                BackgroundColour = 0U,
                BorderBottom = HasBorderBottom,
                BorderTop = HasBorderTop,
                BorderLeft = HasBorderLeft,
                BorderRight = HasBorderRight,
                Format = column.Format,
                HasWordWrap = Value is string && !IsFormula,
                IsEditable = IsEditable
            };
        }

        public bool IsEditable { get; set; }

        public void Write(UInt32Value styleIndex, CellFormat format, string reference, OpenXmlWriter writer)
        {
            (DataType dataType, string value) = GetDataTypeFormattedValueTuple(format);
            WriteCell(writer, styleIndex, reference, dataType, value, IsFormula);
        }

#pragma warning disable ACL1003
        public static void WriteCell(
            OpenXmlWriter writer,
            UInt32Value? styleIndex,
#pragma warning restore ACL1003
            string reference,
            DataType dataType,
            string value,
#pragma warning disable AV1564
            bool isFormula)
#pragma warning restore AV1564
        {
            if (styleIndex?.Value == default)
            {
                throw new ArgumentNullException(nameof(styleIndex));
            }

            var attributes = new List<OpenXmlAttribute>
            {
                //RS: This pragma is required due to the suggested alternative actually changing the name space in open xml and becomes a breaking change
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                new OpenXmlAttribute("r", null, reference),

                new OpenXmlAttribute("s", null, styleIndex),
                new OpenXmlAttribute("t", null, dataType)
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            };
            var newCell = new Cell();
            writer.WriteStartElement(newCell, attributes);

            if (!string.IsNullOrWhiteSpace(value))
            {
                if (isFormula)
                {
                    var cellFormula = new CellFormula(value);
                    writer.WriteElement(cellFormula);
                }
                else
                {
                    var cellValue = new CellValue(value);
                    writer.WriteElement(cellValue);
                }
            }

            writer.WriteEndElement();
        }

#pragma warning disable ACL1002
        private Tuple<DataType, string> GetDataTypeFormattedValueTuple(CellFormat format)
#pragma warning restore ACL1002
        {
            switch (Value)
            {
                case DateTime date:
                    var dateString = FormatDateTimeAsString(date);
                    return new Tuple<DataType, string>(DataType.Number, dateString);
                case DateTimeOffset date:
                    var dateTimeOffsetString = FormatDateTimeAsString(date.LocalDateTime);
                    return new Tuple<DataType, string>(DataType.Number, dateTimeOffsetString);
                case TimeSpan t:
                    if (format == CellFormat.Text)
                    {
                        var text = Value.ToString();
                        return new Tuple<DataType, string>(DataType.String, text);
                    }
                    
                    // Use the provided number format
                    var timeString = t.ToOADatePrecise().ToString(CultureInfo.CurrentCulture);
                    return new Tuple<DataType, string>(DataType.Number, timeString);
                case decimal dec:
                    var decimalString = dec.ToString(CultureInfo.CurrentCulture);
                    return new Tuple<DataType, string>(DataType.Number, decimalString);
                case double d:
                    var doubleString = d.ToString(CultureInfo.CurrentCulture);
                    return new Tuple<DataType, string>(DataType.Number, doubleString);
                case float f:
                    var floatString = f.ToString(CultureInfo.CurrentCulture);
                    return new Tuple<DataType, string>(DataType.Number, floatString);
                case int i:
                    var integerString = i.ToString(CultureInfo.CurrentCulture);
                    return new Tuple<DataType, string>(DataType.Number, integerString);
                case bool b:
                    var booleanString = FormatBooleanAsString(format, b);
                    return new Tuple<DataType, string>(DataType.String, booleanString);
                case Enum e:
                    var enumString = FormatEnumAsString(format, e);
                    return new Tuple<DataType, string>(DataType.String, enumString);
                default:
                    return new Tuple<DataType, string>(DataType.String, Value?.ToString() ?? string.Empty);
            }
        }

        private static string FormatBooleanAsString(CellFormat format, bool value)
        {
            switch (format)
            {
                case CellFormat.BooleanYN: return value ? "Y" : "N";
                case CellFormat.BooleanYesNo: return value ? "Yes" : "No";
                case CellFormat.BooleanOneZero: return value ? "1" : "0";
                case CellFormat.BooleanTrueFalse: return value ? "True" : "False";
                default: return value.ToString();
            }
        }

        private static string FormatEnumAsString(CellFormat format, object value)
        {
            switch (format)
            {
                case CellFormat.EnumDescription:
                    return EnumMember.GetDescription(value) ?? value.ToString();
                case CellFormat.EnumMember:
                    return EnumMember.GetEnumMemberValue(value) ?? value.ToString();
                case CellFormat.EnumName:
                    return EnumMember.GetName(value) ?? value.ToString();
                case CellFormat.EnumValue:
                    return EnumMember.GetValue(value) ?? value.ToString();
                default:
                    return EnumMember.GetOption(value) ?? value.ToString();
            }
        }

        private static string FormatDateTimeAsString(DateTime value)
        {
            return !value.Equals(DateTime.MinValue)
                ? value.ToOADatePrecise().ToString(CultureInfo.CurrentCulture)
                : string.Empty;
        }
    }
}
