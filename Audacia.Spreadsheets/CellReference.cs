using System;
using Audacia.Spreadsheets.Extensions;

namespace Audacia.Spreadsheets
{
    public class CellReference
    {
        public CellReference()
        {
            ColumnLetter = "A";
            RowNumber = 1;
        }
        
        public CellReference(string cellRef)
        {
            ColumnLetter = cellRef.GetColumnLetter();
            RowNumber = cellRef.GetRowNumber();
        }

        public CellReference(string columnLetter, uint rowNumber)
        {
            ColumnLetter = columnLetter;
            RowNumber = rowNumber;
        }
        
        public static implicit operator string(CellReference source)
        {
            return source.ToString();
        }
                
        /// <summary>
        /// Gets or sets the Column Letter ie; A, B, C, D.
        /// </summary>
        public string ColumnLetter { get; set; }
        
        /// <summary>
        /// Gets or sets the row number ie; 1, 2, 3, 4.
        /// </summary>
        public uint RowNumber { get; set; }

        /// <summary>
        /// Returns a new Object with the same values.
        /// </summary>
        public CellReference Clone()
        {
            return new CellReference(ColumnLetter, RowNumber);
        }

        /// <summary>
        /// Returns a new object modified by the given values.
        /// </summary>
        public CellReference MutateBy(int columns, int rows)
        {
            var uintValue = Convert.ToUInt32(rows);
            var nextRowNumber = RowNumber + uintValue;

            if (columns != 0)
            {
                var nextColumnNumber = ColumnLetter.ToColumnNumber() + columns;
                var nextColumnLetter = nextColumnNumber.ToColumnLetter();
                return new CellReference(nextColumnLetter, nextRowNumber);
            }

            return new CellReference(ColumnLetter, nextRowNumber);
        }
        
        /// <summary>
        /// Increments the column value by one.
        /// </summary>
        public void NextColumn()
        {
            var nextColumnNumber = ColumnLetter.ToColumnNumber() + 1;
            ColumnLetter = nextColumnNumber.ToColumnLetter();
        }
        
        /// <summary>
        /// Increments the row value by one.
        /// </summary>
        public void NextRow()
        {
            RowNumber++;
        }
        
        public override string ToString()
        {
            return $"{ColumnLetter}{RowNumber}";
        }
    }
}