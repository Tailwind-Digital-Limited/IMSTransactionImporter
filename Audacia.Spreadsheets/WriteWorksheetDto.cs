using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Audacia.Spreadsheets
{
    public class WriteWorksheetDto
    {
        public Workbook Workbook { get; set; }

        public WorkbookPart WorkbookPart { get; set; }

        public SharedDataTable SharedData { get; set; }

        public Sheets Sheets { get; set; }

        public WriteWorksheetDto(Workbook workbook, WorkbookPart workbookPart, SharedDataTable sharedData,
            Sheets sheets)
        {
            Workbook = workbook;
            WorkbookPart = workbookPart;
            SharedData = sharedData;
            Sheets = sheets;
        }
    }
}