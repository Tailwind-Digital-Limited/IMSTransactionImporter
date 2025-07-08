namespace IMSTransactionImporter.Classes;

public class IMSProcessedTransaction
{
    public string? Reference { get; set; }
    public string? InternalReference { get; set; }
    public string? PspReference { get; set; }
    public string? OfficeCode { get; set; }
    public string? EntryDate { get; set; }
    public string? TransactionDate { get; set; }
    public string? AccountReference { get; set; }
    public int UserCode { get; set; }
    public string? FundCode { get; set; }
    public string? MopCode { get; set; }
    public decimal Amount { get; set; }
    public string? VatCode { get; set; }
    public decimal VatRate { get; set; }
    public decimal VatAmount { get; set; }
    public string? Narrative { get; set; }
    public int ImportId { get; set; }
}