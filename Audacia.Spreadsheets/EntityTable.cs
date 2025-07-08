using System.Collections.Generic;
using System.Linq;
using Audacia.Spreadsheets.Extensions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Audacia.Spreadsheets
{
    public class EntityTable<TEntity> : Table where TEntity : class
    {
        private const int DefaultMaxCharacterWidth = 75;

        public EntityTable() { }

        public EntityTable(IEnumerable<TEntity> source)
        { 
            // Generate the column data if not user provided.
            Columns = Extensions.Tables.GetColumns<TEntity>().ToList();
            Data = source;
            IncludeHeaders = true;
        }

        public IEnumerable<TEntity> Data { get; set; } = new List<TEntity>();

        public virtual TableRow FromEntity(TEntity entity)
        {
            // Generate the row if not user defined.
            return entity.GetRow(Columns);
        }

        public override CellReference Write(SharedDataTable sharedData, OpenXmlWriter writer)
        {
            var rowReference = new CellReference(StartingCellRef);

            // Write SubTotal above headers.
            WriteSubTotalHeaders(sharedData, writer, rowReference);

            // Write Headers above Data.
            WriteHeaders(sharedData, writer, rowReference);
            WriteDataRows(sharedData, writer, rowReference);

            // Return the cell ref at end of the table.
            return rowReference;
        }

        /// <summary>
        /// Enumerate over all rows and write them using an OpenXmlWriter
        /// This puts them into a MemoryStream, to improve this we would
        /// need to update the OpenXml library which we use.
        /// </summary>
        private void WriteDataRows(SharedDataTable sharedData, OpenXmlWriter writer, CellReference rowReference)
        {
            foreach (var entity in Data)
            {
                var row = FromEntity(entity);
                var rowCellReference = rowReference.Clone();
                row.Write(rowCellReference, Columns, sharedData, writer);
                rowReference.NextRow();
            }
        }

#pragma warning disable ACL1002
        private void WriteHeaders(SharedDataTable sharedData, OpenXmlWriter writer, CellReference rowReference)
#pragma warning restore ACL1002
        {
            if (IncludeHeaders)
            {
                var headerCellReference = rowReference.Clone();
                var row = new Row();
                writer.WriteStartElement(row);

                foreach (var column in Columns)
                {
                    var isFirstColumn = column == Columns.ElementAt(0);
                    var isLastColumn = column == Columns.ElementAt(Columns.Count - 1);
                    column.Write(HeaderStyle, headerCellReference, isFirstColumn, isLastColumn, sharedData, writer);
                    headerCellReference.NextColumn();
                }

                writer.WriteEndElement();
                rowReference.NextRow();
            }
        }

#pragma warning disable ACL1002
        private void WriteSubTotalHeaders(SharedDataTable sharedData, OpenXmlWriter writer, CellReference rowReference)
#pragma warning restore ACL1002
        {
            if (IncludeHeaders && Columns.Any(c => c.DisplaySubtotal))
            {
                var rowCount = Data.Count();
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
        }

        public override int GetMaxCharacterWidth(int columnIndex)
        {
            var column = Columns[columnIndex];
            return column.Width ?? DefaultMaxCharacterWidth;
        }
    }
}