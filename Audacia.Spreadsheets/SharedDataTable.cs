using System.Collections.Generic;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Audacia.Spreadsheets
{
    /// <summary>
    /// Global data object that can be used by any worksheet.
    /// </summary>
#pragma warning disable AV1708
    public class SharedDataTable
#pragma warning restore AV1708
    {
        public List<CellStyle> CellFormats { get; } = new List<CellStyle>();

        public DefinedNames DefinedNames { get; } = new DefinedNames();

        public Stylesheet Stylesheet { get; set; } = new Stylesheet();

        public Dictionary<string, uint> FillColours { get; } = new Dictionary<string, uint>();

        public Dictionary<string, uint> TextColours { get; } = new Dictionary<string, uint>();

        public Dictionary<string, uint> Fonts { get; } = new Dictionary<string, uint>();
    }
}