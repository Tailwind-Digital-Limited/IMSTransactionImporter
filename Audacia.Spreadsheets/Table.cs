using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Audacia.Spreadsheets.Extensions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Audacia.Spreadsheets
{
    public class Table
    {
        public Table() { }

#pragma warning disable AV1564
        public Table(bool includeHeaders) => IncludeHeaders = includeHeaders;
#pragma warning restore AV1564

        public string StartingCellRef { get; set; } = "A1";

        public TableHeaderStyle? HeaderStyle { get; set; }

        public bool IncludeHeaders { get; set; }

        public List<TableColumn> Columns { get; set; } = new List<TableColumn>();

        public IEnumerable<TableRow> Rows { get; set; } = new List<TableRow>();

#pragma warning disable ACL1002
        public virtual CellReference Write(SharedDataTable sharedData, OpenXmlWriter writer)
#pragma warning restore ACL1002
        {
            var rowReference = new CellReference(StartingCellRef);

            // Write Subtotals above headers
            if (IncludeHeaders && Columns.Any(c => c.DisplaySubtotal))
            {
                var rowCount = Rows.Count();
                var subtotalCellRef = rowReference.Clone();
                var newRow = new Row();
                writer.WriteStartElement(newRow);

                foreach (var column in Columns)
                {
                    var isFirstColumn = column == Columns.ElementAt(0);
                    var isLastColumn = column == Columns.ElementAt(Columns.Count - 1);
                    column.WriteSubtotal(subtotalCellRef, isFirstColumn, isLastColumn, rowCount, sharedData, writer);
                    subtotalCellRef.NextColumn();
                }

                writer.WriteEndElement();
                rowReference.NextRow();
            }

            // Write headers above data
            if (IncludeHeaders && Columns.Any())
            {
                var headerCellRef = rowReference.Clone();
                var newRow = new Row();
                writer.WriteStartElement(newRow);
                WriteHeaders(sharedData, writer, headerCellRef);
                writer.WriteEndElement();
                rowReference.NextRow();
            }
            
            // Enumerate over all rows and write them using an OpenXMLWriter
            // This puts them into a MemoryStream, to improve this we would need to update the OpenXML library we are using
            foreach (var row in Rows)
            {
                var clonedRowReference = rowReference.Clone();
                row.Write(clonedRowReference, Columns, sharedData, writer);
                rowReference.NextRow();
            }

            // Return the cell ref at end of the table
            return rowReference;
        }

        private void WriteHeaders(SharedDataTable sharedData, OpenXmlWriter writer, CellReference headerCellRef)
        {
            foreach (var column in Columns)
            {
                var isFirstColumn = column == Columns.ElementAt(0);
                var isLastColumn = column == Columns.ElementAt(Columns.Count - 1);
                //HeaderStyle below is fine to be null, adding a check here for it will possibly break existing implementations
                //See OverallConversionTests.cs NoErrorOnEmptyArray
                column.Write(HeaderStyle, headerCellRef, isFirstColumn, isLastColumn, sharedData, writer);
                headerCellRef.NextColumn();
            }
        }

#pragma warning disable ACL1002
        public virtual int GetMaxCharacterWidth(int columnIndex)
#pragma warning restore ACL1002
        {
            var column = Columns[columnIndex];

            if (column.Width != null)
            {
                return column.Width.Value;
            }

            //  Get all of the cells for this column to find the widest cell and make that width of the column
            var cells = Rows.Select(r => r.Cells.Count > columnIndex ? r.Cells[columnIndex] : null).Where(c => c != null).ToList();

            if (IncludeHeaders)
            {
                var tableCell = new TableCell(column.Name);
                cells.Add(tableCell);
            }

            // Create a Cell for Rollup if necessary
            if (column.DisplaySubtotal)
            {
                var total = Rows
                    .Where(r => r.Cells.Count > columnIndex)
                    .Select(r =>
                    {
                        var value = r.Cells.ElementAt(columnIndex).Value;
                        var isNumeric = value?.GetType().IsNumeric() ?? false;
                        return isNumeric ? Convert.ToDecimal(value, NumberFormatInfo.InvariantInfo) : 0;
                    })
                    .DefaultIfEmpty(0)
                    .Sum(v => v);
                var totalCell = new TableCell
                {
                    // Format as currency because the number value alone just isn't long enough
                    Value = $"{total:C}"
                };

                cells.Add(totalCell);
            }

            // Find the max cell width of supplied column           
            var current = 0;
            for (var i = 0; i < cells.Count; i++)
            {
                var cell = cells[i];
                var cellValue = cell?.Value?.ToString() ?? string.Empty;
                var cellTextLength = cellValue.Length;

                if (cellTextLength > current)
                {
                    current = cellTextLength;
                }
            }

            //  While arbitrary, 75 has been tested to be a sufficient max width for columns.
            if (current > 75)
            {
                current = 75;
            }

            return current;
        }
    }
}
