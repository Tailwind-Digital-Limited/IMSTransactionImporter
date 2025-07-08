namespace Audacia.Spreadsheets
{
    public class WorksheetProtection
    {
        public bool CanAddOrDeleteColumns { get; set; }
        
        public bool CanAddOrDeleteRows { get; set; }
        
        public string? Password { get; set; }
        
        public bool AllowSort { get; set; }
        
        public bool AllowAutoFilter { get; set; }

        public bool AllowFormatCells { get; set; }
        
        public bool AllowFormatRows { get; set; }
        
        public bool AllowFormatColumns { get; set; }
    }
}
