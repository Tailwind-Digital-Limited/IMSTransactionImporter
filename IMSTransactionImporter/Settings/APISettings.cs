namespace IMSTransactionImporter.Settings;

public class ApiSettings
{
    public const string SectionName = "APISettings";
    
    public required string IMSApiKey { get; init; }
    
    public required string ApiUrl { get; init; }
}