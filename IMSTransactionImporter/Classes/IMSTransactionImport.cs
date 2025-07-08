namespace IMSTransactionImporter.Classes;

public class IMSTransactionImport
{
    public int ImportTypeId { get; set; }
    public string? Notes { get; set; }
    public int NumberOfRows => Rows.Count;
    public IList<IMSProcessedTransaction> Rows { get; set; } = [];
    public IList<string>? Errors { get; set; }
}