using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Audacia.Spreadsheets.Extensions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using OpenXmlCellFormat = DocumentFormat.OpenXml.Spreadsheet.CellFormat;

namespace Audacia.Spreadsheets
{
    public class TableRow
    {
        public int? Id { get; set; }

        public List<TableCell> Cells { get; } = new List<TableCell>();

#pragma warning disable ACL1002
        public void Write(CellReference cellReference, IList<TableColumn> columns, SharedDataTable sharedData, OpenXmlWriter writer)
#pragma warning restore ACL1002
        {
            var newRow = new Row();
            writer.WriteStartElement(newRow);

            for (var columnIndex = 0; columnIndex < columns.Count; columnIndex++)
            {
                var column = columns[columnIndex];
                var cell = Cells.Count > columnIndex
                    ? Cells[columnIndex]
                    : new TableCell(hasBorders: Cells.Count > 0 && Cells.Last().HasBorders);

                var cellStyle = cell.CellStyle(column);

                SetCellColours(sharedData, cell, cellStyle);

                var styleIndex = sharedData.GetOrCreateCellFormat(cellStyle).Index;

                cell.Write(styleIndex, column.Format, cellReference, writer);

                cellReference.NextColumn();
            }

            writer.WriteEndElement();
        }

        private static void SetCellColours(SharedDataTable sharedData, TableCell cell, CellStyle cellStyle)
        {
            if (!string.IsNullOrWhiteSpace(cell.FillColour))
            {
                cellStyle.BackgroundColour = sharedData.FillColours[cell.FillColour!];
            }

            if (!string.IsNullOrWhiteSpace(cell.TextColour))
            {
                cellStyle.TextColour = sharedData.TextColours[cell.TextColour!];
            }
        }

        public static TableRow FromCells(IEnumerable<TableCell> cells, int? id)
        {
            var row = new TableRow
            {
                Id = id
            };

            foreach (var cell in cells)
            {
                row.Cells.Add(cell);
            }

            return row;
        }

#pragma warning disable ACL1002
#pragma warning disable CA1502
        public static IEnumerable<TableRow> FromOpenXml(
            WorksheetPart worksheetPart,
            SpreadsheetDocument spreadSheet,
            int columnsCount,
            int startingRowIndex = 0)
#pragma warning restore ACL1002
#pragma warning restore CA1502
        {
            var stylesheet = spreadSheet.WorkbookPart?.WorkbookStylesPart?.Stylesheet;
            var cellFormats = stylesheet?.CellFormats;
            var dateFormatIds = GetDateFormatsInFile(spreadSheet.WorkbookPart?.WorkbookStylesPart!);

            // Read each row and add to table
            var rows = worksheetPart.Worksheet.Elements<SheetData>().First().Elements<Row>().ToList();
            var stringTable = spreadSheet.WorkbookPart?.GetPartsOfType<SharedStringTablePart>().First();

            var rowPointer = new CellReference("A1").MutateBy(0, startingRowIndex);

#pragma warning disable ACL1011
            foreach (var row in rows.Skip(startingRowIndex))
            {
                var cellRef = rowPointer.Clone();
                var cells = row.Elements<Cell>().ToArray();
                var cellData = new List<TableCell>();

                for (var columnIndex = 0; columnIndex < columnsCount; columnIndex++)
                {
                    var cellReference = cellRef.ToString();
                    var matchedCells = cells.Where(c =>
                        string.Compare(cellReference, c.CellReference?.Value, StringComparison.OrdinalIgnoreCase) == 0)
                        .ToList();

                    if (!matchedCells.Any() || matchedCells.First().CellValue == null)
                    {
                        var newCell = new TableCell(null);
                        cellData.Add(newCell);
                    }
                    else
                    {
                        var matchedCell = matchedCells.First();
                        if (matchedCell.DataType is { Value: CellValues.SharedString } &&
                            !string.IsNullOrEmpty(matchedCell.CellValue?.Text))
                        {
                            var newCell = CreateCellAsSharedString(spreadSheet, matchedCell, stringTable, cellFormats, stylesheet);
                            cellData.Add(newCell);
                        }
                        else
                        {
                            // Read value from worksheet
                            var valueAdded = false;
                            var newCell = new TableCell(null);
                            // If a cell format is defined
                            if (matchedCell.StyleIndex?.Value != null)
                            {
                                var styleIndex = (int)matchedCell.StyleIndex!.Value;
                                var cellFormat = (OpenXmlCellFormat)cellFormats.ElementAt(styleIndex);

                                var fill = (Fill)stylesheet!.Fills!.ChildElements[(int)cellFormat!.FillId!.Value];
                                var patternFill = fill.PatternFill;

                                // Parse DateTime
                                if (IsDateFormat(cellFormat.NumberFormatId!.Value) ||
                                    dateFormatIds.Contains(cellFormat.NumberFormatId))
                                {
                                    if (double.TryParse(matchedCell.CellValue?.InnerXml, out var parsedValue))
                                    {
                                        ParseNewCellAsDate(parsedValue, cellFormat, newCell);
                                        cellData.Add(newCell);
                                        valueAdded = true;
                                    }
                                } // Parse Numbers
                                else if (IsNumberFormat(cellFormat.NumberFormatId))
                                {
                                    if (!valueAdded && decimal.TryParse(matchedCell.CellValue!.Text, out var value))
                                    {
                                        // Round using the highest degree of precision to prevent floating point errors.
                                        newCell.Value = decimal.Round(value, 28);
                                        cellData.Add(newCell);
                                        valueAdded = true;
                                    }
                                }

                                // Read cell colour
                                newCell.FillColour = GetColor(spreadSheet, patternFill!);
                            }

                            // Read cell value as string
                            if (!valueAdded)
                            {
                                var cellText = matchedCell.CellValue!.Text;

                                if (int.TryParse(cellText, out var _))
                                {
                                    newCell.Value = cellText;
                                }
                                else if (decimal.TryParse(cellText, NumberStyles.Any, CultureInfo.InvariantCulture, out var fallbackValue))
                                {
                                    // Round using the highest degree of precision supported in floating point notation and trim trailing zeros.
                                    newCell.Value = decimal.Round(fallbackValue, 15).ToString("G29");
                                }
                                else
                                {
                                    newCell.Value = cellText;
                                }

                                cellData.Add(newCell);
                            }
                        }
                    }

                    cellRef.NextColumn();
                }
#pragma warning restore ACL1011

                var rowId = Convert.ToInt32(rowPointer.RowNumber);
                rowPointer.NextRow();

                if (cellData.All(c => c.Value == null || (c.Value is string s && string.IsNullOrWhiteSpace(s))))
                {
                    continue;
                }

                yield return FromCells(cellData, rowId);
            }
        }

        private static void ParseNewCellAsDate(double parsedValue, OpenXmlCellFormat cellFormat, TableCell newCell)
        {
            var date = DateTime.FromOADate(parsedValue);

            // Breaking Change: Cut down to timespan if required
            if (IsTimespanFormat(cellFormat.NumberFormatId!.Value))
            {
                newCell.Value = date.TimeOfDay;
            }
            else
            {
                newCell.Value = date;
            }
        }

#pragma warning disable ACL1003
#pragma warning disable ACL1002
        private static TableCell CreateCellAsSharedString(
            SpreadsheetDocument spreadSheet,
            Cell matchedCell,
            SharedStringTablePart? stringTable,
            CellFormats? cellFormats,
            Stylesheet? stylesheet)
#pragma warning restore ACL1003
#pragma warning restore ACL1002
        {
            // Read value from shared string table
            var newCell = new TableCell(null);
            var integerCellValue = int.Parse(matchedCell.CellValue!.Text, NumberFormatInfo.InvariantInfo);
            newCell.Value = stringTable?.SharedStringTable.ElementAt(integerCellValue).InnerText;

            // Read cell colour
            if (matchedCell.StyleIndex?.HasValue != null)
            {
                var styleIndex = (int)matchedCell.StyleIndex.Value;
                var cellFormat = (OpenXmlCellFormat)cellFormats.ElementAt(styleIndex);

                var fill = (Fill)stylesheet!.Fills!.ChildElements[(int)cellFormat.FillId!.Value];
                var patternFill = fill.PatternFill;

                if (patternFill != null)
                {
                    newCell.FillColour = GetColor(spreadSheet, patternFill);
                }
            }

            return newCell;
        }

        private static string? GetColor(SpreadsheetDocument sd, PatternFill fill)
        {
            var colour = fill.ForegroundColor;
            // Due to OpenXml limitations, colour.Rgb gives "FF######" so the first 2 characters are removed.
            return colour != null && colour.Auto?.HasValue == true && colour.Rgb?.HasValue == true
                    ? colour.Rgb?.Value?.Substring(2)
                    : null;
        }

#pragma warning disable AV1553
        private static bool IsDateFormat(uint? numberFormatId, string? formatCode = null)
#pragma warning restore AV1553
        {
            // Microsoft only gives limited format information, there isn't an entire list of format codes online
            // So first check the ones we do know.
            if ((numberFormatId >= (uint)CellFormat.Date
                 && numberFormatId <= (uint)CellFormat.DateTime)
                || numberFormatId == (uint)CellFormat.DateVariant
                || numberFormatId == (uint)CellFormat.TimeSpanMinutes)
            {
                return true;
            }

            // Then check if it contains date formatting or year formatting
            return formatCode != null && (formatCode.Contains("mmm") || formatCode.Contains("yy"));
        }

#pragma warning disable AV1553
        private static bool IsNumberFormat(uint? numberFormatId, string? formatCode = null)
#pragma warning restore AV1553
        {
            // Microsoft only give limited format information, there isn't an entire list of format codes online
            // So first check the ones we do know.
            if ((numberFormatId >= (uint)CellFormat.Integer
                 && numberFormatId <= (uint)CellFormat.Scientific)
                || numberFormatId == (uint)CellFormat.Currency
                || (numberFormatId >= (uint)CellFormat.AccountingGBP
                    && numberFormatId <= (uint)CellFormat.AccountingEUR))
            {
                return true;
            }

            // Then check if it contains date formatting or year formatting
            return formatCode != null && (formatCode.Contains("mmm") || formatCode.Contains("yy"));
        }

        public static bool IsTimespanFormat(uint numberFormatId)
        {
            return (numberFormatId >= (uint)CellFormat.Time
                    && numberFormatId <= (uint)CellFormat.TimeSpanFull)
                   || numberFormatId == (uint)CellFormat.TimeSpanMinutes;
        }

        private static ICollection<uint> GetDateFormatsInFile(WorkbookStylesPart stylePart)
        {
            if (stylePart == null)
            {
                throw new ArgumentNullException(nameof(stylePart));
            }

            var formatIds = new Collection<uint>();

            var numFormatsParentNodes = stylePart.Stylesheet.ChildElements.OfType<NumberingFormats>();

            foreach (var numFormatParentNode in numFormatsParentNodes)
            {
                AddFormatIds(numFormatParentNode, formatIds);
            }

            return formatIds;
        }

        private static void AddFormatIds(NumberingFormats numFormatParentNode, Collection<uint> formatIds)
        {
            var formatNodes = numFormatParentNode.ChildElements.OfType<NumberingFormat>();
            foreach (var formatNode in formatNodes)
            {
                if (IsDateFormat(formatNode.NumberFormatId?.Value, formatNode.FormatCode))
                {
                    formatIds.Add(formatNode.NumberFormatId!.Value);
                }
            }
        }
    }
}
