using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Audacia.Spreadsheets.Extensions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Audacia.Spreadsheets
{
#pragma warning disable CA1724
    public class Spreadsheet
#pragma warning restore CA1724
    {
        public List<WorksheetBase> Worksheets { get; } = new List<WorksheetBase>();

        /// <summary>
        /// Gets the Ranges of a spreadsheet which have been defined.
        /// Must be defined after worksheets have been defined or the ranges will be moved by the addition of tables
        /// </summary>
        public List<NamedRangeModel> NamedRanges { get; } = new List<NamedRangeModel>();

        /// <summary>
        /// Writes the spreadsheet to a stream as an Excel Workbook (*.xlsx).
        /// </summary>
#pragma warning disable ACL1002
        public void Write(Stream stream)
#pragma warning restore ACL1002
        {
            using (var document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
            {
                var tables = Worksheets.GetTables();
                var sharedData = new StylesheetBuilder(tables).Build();

                var workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();
                var workbook = workbookPart.Workbook;
                var newSheets = new Sheets();
                var sheets = workbook.AppendChild(newSheets);

                workbook.CalculationProperties = new CalculationProperties();

                // Shared string table
                var sharedStringTablePart = workbookPart.AddNewPart<SharedStringTablePart>();
                sharedStringTablePart.SharedStringTable = new SharedStringTable();
                sharedStringTablePart.SharedStringTable.Save();

                // Stylesheet
                var workbookStylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();
                workbookStylesPart.Stylesheet = sharedData.Stylesheet;
                workbookStylesPart.Stylesheet.Save();

                for (var index = 0; index < Worksheets.Count; index++)
                {
                    var workSheetDto = new WriteWorksheetDto(workbook, workbookPart, sharedData, sheets);
                    WriteWorkSheet(index, workSheetDto);
                }

                AddDefinedNames(workbookPart);
            }
        }

        private void AddDefinedNames(WorkbookPart workbookPart)
        {
            var definedNames = new DefinedNames();
            if (NamedRanges != null && NamedRanges!.Any())
            {
                //  Adds DefinedNames To Workbook
                foreach (var namedRange in NamedRanges)
                {
                    var definedNameToPush = new DefinedName
                    {
                        Name = namedRange.Name,
                        Text = $"\'{namedRange.SheetName}\'!{namedRange.StartCell}:{namedRange.EndCell}"
                    };
                    definedNames.Append(definedNameToPush);
                }

                workbookPart.Workbook.DefinedNames = definedNames;
            }
        }
        
        //RS: Unable to reduce this more meaningfully
#pragma warning disable ACL1002
        private Sheets WriteWorkSheet(int index, WriteWorksheetDto dto)
#pragma warning restore ACL1002
        {
            var sheetNumber = index + 1;
            var worksheet = Worksheets[index];
            var worksheetPart = dto.WorkbookPart.AddNewPart<WorksheetPart>();
            using (var writer = OpenXmlWriter.Create(worksheetPart))
            {
                // Sanitize worksheet name
                const int maxSheetNameLength = 30;
                if (string.IsNullOrWhiteSpace(worksheet.SheetName))
                {
                    worksheet.SheetName = $"Sheet {sheetNumber}";
                }
                else if (worksheet.SheetName?.Length > maxSheetNameLength)
                {
                    worksheet.SheetName = worksheet.SheetName.Substring(0, maxSheetNameLength).Trim();
                }

                var sheet = new Sheet
                {
                    Id = dto.WorkbookPart.GetIdOfPart(worksheetPart),
                    SheetId = Convert.ToUInt32(sheetNumber),
                    State = worksheet.Visibility,
                    Name = worksheet.SheetName
                };

                dto.Sheets.Append(sheet);

                worksheet.Write(dto.SharedData, writer);
            }

            return dto.Sheets;
        }

        /// <summary>
        /// Writes the spreadsheet to a byte array as an Excel Workbook (*.xlsx).
        /// </summary>
#pragma warning disable AV1130
#pragma warning disable ACL1009
#pragma warning disable AV1551
        public byte[] Export()
#pragma warning restore AV1551
#pragma warning restore ACL1009
#pragma warning restore AV1130
        {
            using (var stream = new MemoryStream())
            {
                Write(stream);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Writes the spreadsheet to the specified filepath as an Excel Workbook (*.xlsx).
        /// </summary>
        public virtual void Export(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(filePath);

            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                throw new DirectoryNotFoundException(directory);
            }

            using (var fileStream = new FileStream(filePath, FileMode.OpenOrCreate))
            {
                Write(fileStream);
            }
        }

        /// <summary>
        /// Reads the spreadsheet from the provided file bytes, supports Excel Workbook (*.xlsx).
        /// </summary>
        /// <param name="bytes">Spreadsheet file bytes</param>
        /// <param name="includeHeaders">Declare if column header row is included on the spreadsheet</param>
        /// <param name="hasSubtotals">Declare if subtotal row exists above column header row</param>
        /// <param name="ignoreSheets">Declare if sheets should be skipped, filters by name</param>
#pragma warning disable AV1564
#pragma warning disable AV1553
        public static Spreadsheet FromBytes(byte[] bytes, bool includeHeaders = true, bool hasSubtotals = false, IEnumerable<string>? ignoreSheets = default)
#pragma warning restore AV1553
#pragma warning restore AV1564
        {
            using (var ms = new MemoryStream(bytes))
            {
                return FromStream(ms, includeHeaders, hasSubtotals, ignoreSheets);
            }
        }

        /// <summary>
        /// Reads the spreadsheet from the provided file location, supports Excel Workbook (*.xlsx).
        /// </summary>
        /// <param name="filePath">Path to spreadsheet file</param>
        /// <param name="includeHeaders">Declare if column header row is included on the spreadsheet</param>
        /// <param name="hasSubtotals">Declare if subtotal row exists above column header row</param>
        /// <param name="ignoreSheets">Declare if sheets should be skipped, filters by name</param>
#pragma warning disable AV1564
#pragma warning disable AV1553
        public static Spreadsheet FromFilePath(string filePath, bool includeHeaders = true, bool hasSubtotals = false, IEnumerable<string>? ignoreSheets = default)
#pragma warning restore AV1553
#pragma warning restore AV1564
        {
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                return FromStream(fs, includeHeaders, hasSubtotals, ignoreSheets);
            }
        }

        /// <summary>
        /// Creates a spreadsheet containing the provided worksheets.
        /// </summary>
        /// <param name="worksheets">Worksheets to include</param> 
        public static Spreadsheet FromWorksheets(params WorksheetBase[] worksheets)
        {
            var spreadsheet = new Spreadsheet();
            if (worksheets != null)
            {
                spreadsheet.Worksheets.AddRange(worksheets);
            }
            
            return spreadsheet;
        }

        /// <summary>
        /// Reads the spreadsheet from the provided stream, supports Excel Workbook (*.xlsx).
        /// </summary>
        /// <param name="stream">Spreadsheet file bytes</param>
        /// <param name="includeHeaders">Declare if column header row is included on the spreadsheet</param>
        /// <param name="hasSubtotals">Declare if subtotal row exists above column header row</param>
        /// <param name="ignoreSheets">Declare if sheets should be skipped, filters by name</param>
#pragma warning disable AV1564
#pragma warning disable AV1553
        public static Spreadsheet FromStream(Stream stream, bool includeHeaders = true, bool hasSubtotals = false, IEnumerable<string>? ignoreSheets = default)
#pragma warning restore AV1553
#pragma warning restore AV1564
        {
            using (var spreadSheet = SpreadsheetDocument.Open(stream, false))
            {
                var descendants = spreadSheet.WorkbookPart?.Workbook.Descendants<Sheet>();
                if (ignoreSheets != null && ignoreSheets!.Any())
                {
                    descendants = descendants.Where(d => !ignoreSheets.Contains(d.Name?.ToString()));
                }
                
                var worksheets = descendants
                    .Select(sheet => Worksheet.FromOpenXml(sheet, spreadSheet, includeHeaders, hasSubtotals))
                    .ToArray();

                return FromWorksheets(worksheets);
            }
        }
    }
}
