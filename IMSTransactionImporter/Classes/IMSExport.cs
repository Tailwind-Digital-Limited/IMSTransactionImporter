namespace IMSTransactionImporter.Classes;

public class IMSExport
{
    public ExportType ExportType { get; set; }
    public required string FileName { get; set; }

    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
}