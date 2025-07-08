using System.Collections.Generic;
using System.Linq;
using Audacia.Spreadsheets.Validation;

namespace Audacia.Spreadsheets
{
    public class ImportRow<TRowModel>
    {
        /// <summary>
        /// Gets or sets the row number from the spreadsheet, defaults to zero for a column parsing errors.
        /// </summary>
        public int RowId { get; set; }

        /// <summary>
        /// Gets or sets the parsed row data as a <see cref="{TRowModel}"/>.
        /// </summary>
        public TRowModel Data { get; set; } = default!;

        /// <summary>
        /// Gets or sets an array of validation errors from attempting to parse all expected cells.
        /// </summary>
        public IReadOnlyCollection<IImportError> ImportErrors { get; set; } = new List<IImportError>();

        /// <summary>
        /// Gets a value indicating whether there are no <see cref="ImportErrors"/>.
        /// </summary>
        public bool IsValid => !ImportErrors.Any();
    }
}
