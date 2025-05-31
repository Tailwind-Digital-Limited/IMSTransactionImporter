// See https://aka.ms/new-console-template for more information

using System.Text;
using System.Text.Json;
using IMSTransactionImporter.Interfaces;
using IMSTransactionImporter.InternalClasses;
using IMSTransactionImporter.Settings;
using IMSTransactionImporter.Transformers;
using Microsoft.Extensions.Configuration;

var builder = new ConfigurationBuilder().AddUserSecrets<ApiSettings>();
var configuration = builder.Build();

var apiSettings = configuration.GetSection(ApiSettings.SectionName).Get<ApiSettings>() 
    ?? throw new InvalidOperationException("API settings are not configured");

var imports = new List<IMSImport>
{
    new()
    {
        ImportType = ImportType.PIP,
        FilePath = "CSS_PIP_510_250516081048.csv"
    }
};

foreach (var imsImport in imports)
{
    // Get File Contents
    Console.WriteLine($"Getting file contents from {imsImport.FilePath}");
    var fileContents = GetFileContents(imsImport.FilePath);
    
    // Get Transformer for the import type
    Console.WriteLine($"Getting transform for {imsImport.ImportType}");
    var transformer = GetTransformer(imsImport.ImportType);
    
    // Transform Data
    Console.WriteLine($"Transforming data");
    var imsTransactionImport = transformer.Transform(fileContents);
    
    // Send request to IMS
    Console.WriteLine($"Sending data to IMS API");
    var success = await SendDataToIMS(apiSettings.ApiUrl, apiSettings.IMSApiKey, imsTransactionImport);
    
    // Print result
    if (success)
    {
        Console.WriteLine($"Transaction import successful for {imsImport.ImportType}");
    }
    else
    {
        Console.WriteLine($"Transaction import failed for {imsImport.ImportType}");
    }
}
Console.ReadKey();
return;


string GetFileContents(string filePath)
{
    var currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
    var path = Path.Combine(currentDirectory, "Imports", filePath);
    var fileContents = File.ReadAllText(path);
    return fileContents;
}

static ITransactionTransformer GetTransformer(ImportType importType) => importType switch
{
    ImportType.PIP => new PIPTransformer(),
    ImportType.Bailiff => throw new NotImplementedException(),
    ImportType.BankFile => throw new NotImplementedException(),
    // Add additional transformers here
    _ => throw new NotImplementedException()
};

async Task<bool> SendDataToIMS(string url, string apiKey, IMSTransactionImport import)
{
    // Create the content
    var jsonContent = JsonSerializer.Serialize(import, new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    });
    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
    
    // Create the HTTP client
    var httpClient = new HttpClient();
    httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    httpClient.DefaultRequestHeaders.Add("X-API-Key", apiKey);
    
    // Send POST request
    var response = await httpClient.PostAsync(url, content);
    return response.IsSuccessStatusCode;
}