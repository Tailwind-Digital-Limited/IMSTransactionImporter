using System;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Audacia.Spreadsheets
{
    public class Worksheet : WorksheetBase
    {
        public Table Table { get; set; } = null!;

        protected override void WriteSheetData(SharedDataTable sharedData, OpenXmlWriter writer)
        {
            Table.Write(sharedData, writer);
        }
        
#pragma warning disable ACL1002
#pragma warning disable AV1564
        public static Worksheet FromOpenXml(Sheet worksheet, SpreadsheetDocument spreadSheet, bool includeHeaders, bool hasSubtotals)
#pragma warning restore AV1564
#pragma warning restore ACL1002
        {
            if (spreadSheet.WorkbookPart == null || string.IsNullOrEmpty(worksheet.Id?.Value))
            {
                throw new InvalidOperationException(
                    $"{nameof(spreadSheet.WorkbookPart)} and {nameof(worksheet)} must be provided.");
            }

            var worksheetPart = (WorksheetPart?)spreadSheet.WorkbookPart?.GetPartById(worksheet.Id!.Value!);
            var table = new Table
            {
                StartingCellRef = "A1",
                IncludeHeaders = includeHeaders,
                HeaderStyle = null
            };

            var startingRowIndex = 0;
            if (hasSubtotals && includeHeaders)
            {
                // Rows start at i = 2 if this sheet was also exported with subtotals 
                startingRowIndex += 2;
            } 
            else if (includeHeaders)
            {
                // Rows start at i = 1 to skip header row IF headers are included
                startingRowIndex++;
            }

            if (includeHeaders)
            {
                var columns = TableColumn.FromOpenXml(worksheetPart, spreadSheet, hasSubtotals);
                table.Columns.AddRange(columns);
            }

            var maxRowWidth = includeHeaders ? table.Columns.Count : GetMaxRowWidth(worksheetPart!);

            // Force enumeration of the content when reading the worksheet, otherwise the spreadsheet is disposed before we can read the data.
            table.Rows = TableRow.FromOpenXml(worksheetPart!, spreadSheet, maxRowWidth, startingRowIndex).ToArray();

            return new Worksheet
            {
                SheetName = worksheet.Name!,
                Table = table,
                Visibility = worksheet.State ?? SheetStateValues.Visible
            };
        }
    }
}
