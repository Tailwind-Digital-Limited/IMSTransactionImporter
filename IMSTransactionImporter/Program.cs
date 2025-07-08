// See https://aka.ms/new-console-template for more information

using System.Text;
using System.Text.Json;
using IMSTransactionImporter.Classes;
using IMSTransactionImporter.ExportGenerators;
using IMSTransactionImporter.Interfaces;
using IMSTransactionImporter.Settings;
using IMSTransactionImporter.Transformers;
using LocalGovIMSClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;

var builder = new ConfigurationBuilder().AddUserSecrets<ApiSettings>();
var configuration = builder.Build();

var apiSettings = configuration.GetSection(ApiSettings.SectionName).Get<ApiSettings>()
                  ?? throw new InvalidOperationException("API settings are not configured");

var apiClient = GetLocalGovIMSApiClient(apiSettings.IMSApiKey);

var imports = new List<IMSImport>
{
    // new()
    // {
    //     ImportType = ImportType.PIP,
    //     FilePath = "CSS_PIP_510_250516081048.csv"
    // }
    // new()
    // {
    //     ImportType = ImportType.Bailiff,
    //     FilePath = "Payments NDR.txt"
    // },
    // new()
    // {
    //     ImportType = ImportType.Bailiff,
    //     FilePath = "Payments CT.txt"
    // },
    // new()
    // {
    //     ImportType = ImportType.Bailiff,
    //     FilePath = "Payments PCN.txt"
    // }
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


var exports = new List<IMSExport>
{
    // new()
    // {
    //     ExportType = ExportType.GeneralLedger,
    //     FileName = $"GLINC{DateTime.Now:dd-MM-yyyy-HH-mm-ss}.xlsx",
    //     StartDateTime = new DateTime(2025, 07, 02, 17, 00, 00),
    //     EndDateTime = new DateTime(2025, 07, 04, 16, 59, 59),
    // },
    // new()
    // {
    //     ExportType = ExportType.SundryDebtors,
    //     FileName = $"SDPAY{DateTime.Now:dd}.txt",
    //     StartDateTime = new DateTime(2025, 07, 02, 17, 00, 00),
    //     EndDateTime = new DateTime(2025, 07, 04, 16, 59, 59),
    // },
    // new()
    // {
    //     ExportType = ExportType.HousingRents,
    //     FileName = $"CASH1.dat",
    //     StartDateTime = new DateTime(2025, 07, 02, 17, 00, 00),
    //     EndDateTime = new DateTime(2025, 07, 04, 16, 59, 59),
    // },
    // new()
    // {
    //     ExportType = ExportType.HousingBenefitsOverPayments,
    //     FileName = $"PAYMENTS.dat",
    //     StartDateTime = new DateTime(2025, 07, 02, 17, 00, 00),
    //     EndDateTime = new DateTime(2025, 07, 04, 16, 59, 59),
    // },
    // new()
    // {
    //     ExportType = ExportType.CouncilTaxNNDR,
    //     FileName = $"IWORLD.pay",
    //     StartDateTime = new DateTime(2025, 07, 02, 17, 00, 00),
    //     EndDateTime = new DateTime(2025, 07, 04, 16, 59, 59),
    // },
    new()
    {
        ExportType = ExportType.SMEProfessional,
        FileName = $"GBCLettings_{DateTime.Now:dd-MMM-yy}.csv",
        StartDateTime = new DateTime(2025, 07, 02, 17, 00, 00),
        EndDateTime = new DateTime(2025, 07, 04, 16, 59, 59),
    },
    
};

foreach (var imsExport in exports)
{
    // Get File Contents
    Console.WriteLine($"Extract data from IMS {imsExport.ExportType}");


    // Get Transformer for the import type
    Console.WriteLine($"Getting generator for {imsExport.ExportType}");
    var generator = GetGenerator(imsExport.ExportType);

    // Transform Data
    Console.WriteLine($"Generating export data");
    imsExport.FileName = GetExportFilePath(imsExport.FileName);
    await generator.Generate(imsExport, apiClient);
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

string GetExportFilePath(string fileName)
{
    var currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
    return Path.Combine(currentDirectory, "Exports", fileName);
}

static ITransactionTransformer GetTransformer(ImportType importType) => importType switch
{
    ImportType.PIP => new PIPTransformer(),
    ImportType.Bailiff => new BailiffTransformer(),
    ImportType.BankFile => throw new NotImplementedException(),
    // Add additional transformers here
    _ => throw new NotImplementedException()
};

static IExportGenerator GetGenerator(ExportType exportType) => exportType switch
{
    ExportType.GeneralLedger => new GeneralLedgerExportGenerator(),
    ExportType.SundryDebtors => new SundryDebtorExportGenerator(),
    ExportType.HousingRents => new RentsExportGenerator(),
    ExportType.HousingBenefitsOverPayments => new HousingBenefitOverPaymentExportGenerator(),
    ExportType.CouncilTaxNNDR => new CouncilTaxNNDRExportGenerator(),
    ExportType.SMEProfessional => new SMEProfessionalExportGenerator(),
    // Add additional transformers here
    _ => throw new NotImplementedException()
};

async Task<bool> SendDataToIMS(string url, string apiKey, IMSTransactionImport import)
{
    if (import.NumberOfRows > 0)
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

    return false;
}

static LocalGovIMSAPIClient GetLocalGovIMSApiClient(string apiKey)
{
    var authenticationProvider = new ApiKeyAuthenticationProvider(
        apiKey, "X-API-Key", ApiKeyAuthenticationProvider.KeyLocation.Header);

    var requestAdapter = new HttpClientRequestAdapter(authenticationProvider);

    return new LocalGovIMSAPIClient(requestAdapter);
}