using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using OpenXmlCellFormat = DocumentFormat.OpenXml.Spreadsheet.CellFormat;

namespace Audacia.Spreadsheets.Extensions
{
#pragma warning disable AV1708, AV1745
    public static class SharedDataTables
#pragma warning restore AV1708, AV1745
    {
        public static CellStyle? GetCellFormat(this SharedDataTable sharedData, CellStyle cellStyle)
        {
            return sharedData.CellFormats.SingleOrDefault(sharedDataCellStyle =>
                HasMatchingBorders(cellStyle, sharedDataCellStyle) &&
                HasMatchingColours(cellStyle, sharedDataCellStyle) &&
                HasMatchingFormat(cellStyle, sharedDataCellStyle));
        }

        public static CellStyle GetOrCreateCellFormat(this SharedDataTable sharedData, CellStyle cellStyle)
        {
            var matchingStyle = sharedData.GetCellFormat(cellStyle);

            if (matchingStyle != default(CellStyle))
            {
                return matchingStyle;
            }

            var borders = AssignBordersPresent(cellStyle);
            var cellFormatsElement = CreateCellFormatElement(sharedData, cellStyle, borders);
            cellStyle.Index = UInt32Value.FromUInt32((uint)cellFormatsElement.ChildElements.Count) - 1;
            sharedData.CellFormats.Add(cellStyle);

            return cellStyle;
        }

        private static List<CellBorder> AssignBordersPresent(CellStyle cellStyle)
        {
            var borders = new List<CellBorder>();
            if (cellStyle.BorderTop)
            {
                borders.Add(CellBorder.Top);
            }

            if (cellStyle.BorderRight)
            {
                borders.Add(CellBorder.Right);
            }

            if (cellStyle.BorderBottom)
            {
                borders.Add(CellBorder.Bottom);
            }

            if (cellStyle.BorderLeft)
            {
                borders.Add(CellBorder.Left);
            }

            return borders;
        }

        private static CellFormats CreateCellFormatElement(
            SharedDataTable sharedData,
            CellStyle cellStyle,
            List<CellBorder> borders)
        {
            var borderSum = (uint)borders.Sum(border => (int)border);
            var borderSumXmlValue = UInt32Value.FromUInt32(borderSum);
            // Ignore custom boolean formats, see CellFormat.cs
            UInt32Value numberFormat = (uint)cellStyle.Format > 999U
                ? UInt32Value.FromUInt32((uint)CellFormat.Text)
                : UInt32Value.FromUInt32((uint)cellStyle.Format);

            var cellFormat = new OpenXmlCellFormat
            {
                FontId = cellStyle.TextColour,
                FillId = cellStyle.BackgroundColour,
                BorderId = borderSumXmlValue,
                NumberFormatId = numberFormat,
                ApplyFont = true,
                ApplyFill = true,
                ApplyBorder = true,
                ApplyNumberFormat = true,
                Protection = cellStyle.IsEditable
                    ? new Protection
                    {
                        Locked = false
                    }
                    : default,
                Alignment = new Alignment
                {
                    Horizontal = HorizontalAlignmentValues.Left,
                    Vertical = VerticalAlignmentValues.Top,
                    TextRotation = 0U,
                    WrapText = cellStyle.HasWordWrap,
                    ReadingOrder = 1U
                }
            };

            var cellFormatsElement = sharedData.Stylesheet.CellFormats!;
            cellFormatsElement.Append(cellFormat);
            return cellFormatsElement;
        }

        private static bool HasMatchingBorders(CellStyle leftHandObject, CellStyle rightHandObject)
        {
            return leftHandObject.BorderTop == rightHandObject.BorderTop &&
                   leftHandObject.BorderRight == rightHandObject.BorderRight &&
                   leftHandObject.BorderBottom == rightHandObject.BorderBottom &&
                   leftHandObject.BorderLeft == rightHandObject.BorderLeft;
        }

        private static bool HasMatchingColours(CellStyle leftHandObject, CellStyle rightHandObject)
        {
            return leftHandObject.TextColour == rightHandObject.TextColour &&
                   leftHandObject.BackgroundColour == rightHandObject.BackgroundColour;
        }

        private static bool HasMatchingFormat(CellStyle leftHandObject, CellStyle rightHandObject)
        {
            return leftHandObject.Format == rightHandObject.Format &&
                   leftHandObject.HasWordWrap == rightHandObject.HasWordWrap &&
                   leftHandObject.IsEditable == rightHandObject.IsEditable;
        }
    }
}