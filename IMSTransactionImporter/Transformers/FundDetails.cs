namespace IMSTransactionImporter.Transformers;

public class FundDetails
{
    
    public required string VatCode { get; set; }
    public float VatRate { get; set; }
    public string? FundCode { get; set; }
}