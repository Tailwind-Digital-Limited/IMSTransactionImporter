using CsvHelper;
using System.Globalization;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using IMSTransactionImporter.Interfaces;
using LocalGovIMSClient.Models;

namespace IMSTransactionImporter.Transformers;

public class BailiffTransformer : ITransactionTransformer
{
    public TransactionImportModel Transform(string fileContents)
    {
        // Parse CSV file
        var bailiffTransactions = ParseCsv(fileContents);
        
        // Convert rows to IMSTransactionImport
        var rows = bailiffTransactions.Select(Convert).ToList();
        
        // Create IMSTransactionImport
        var import = new TransactionImportModel()
        {
            Rows = rows,
            ImportTypeId = 3,
            Notes = "Imported from Bailiff File"
        };
        
        return import;
    }

    public static ProcessedTransactionModel Convert(BailiffTransaction bailiff)
    {
        var processedTransaction = new ProcessedTransactionModel
        {
            Reference = bailiff.CustomerReference,
            Amount = (double)bailiff.Amount,
            AccountReference = bailiff.CustomerReference,
            MopCode = "20",
            InternalReference = GenerateRandom16CharString(),
            PspReference = $"BLF-{DateTime.Now:yyMMdd}-{bailiff.RowNumber}",
            OfficeCode = "S",
            EntryDate = DateTimeOffset.Now,
            TransactionDate = bailiff.TransactionDate,
            Narrative = $"{bailiff.LiabilityOrderNumber} (Liability order number)",
            // Set defaults
            VatRate = 0,
            VatAmount = 0,
            VatCode = "1",
        };
        var fundDetails = GetFundDetails(bailiff.FundName);
        if (fundDetails != null)
        {
            processedTransaction.VatCode = fundDetails.VatCode;
            processedTransaction.VatRate = fundDetails.VatRate;
            processedTransaction.FundCode = fundDetails.FundCode;
            
            // Example Amount is £1.20 and Vat Rate is 20% (0.2). Vat amount is 1.20 - 1.20/(1+0.2) = 20p. 
            processedTransaction.VatAmount = processedTransaction.Amount - processedTransaction.Amount / (1 + processedTransaction.VatRate);
        }
        
        return processedTransaction;
    }

    

    // Leaving this for extensibility even though currently all funds have the same vat code and rate.
    private static FundDetails? GetFundDetails(string? bailiffFundName) => bailiffFundName switch
    {
        "Council Tax" => new FundDetails{FundCode = "2", VatCode = "3", VatRate = 0},
        "NDR" => new FundDetails{FundCode = "5", VatCode = "3", VatRate = 0},
        "Benefit Overpayment" => new FundDetails{FundCode = "6", VatCode = "3", VatRate = 0},
        "Sundry Debt" => new FundDetails{FundCode = "7", VatCode = "3", VatRate = 0},
        "PCN" => new FundDetails{FundCode = "9", VatCode = "3", VatRate = 0},
        _ => null
    };


    public static string GenerateRandom16CharString()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 16)
            .Select(s => s[random.Next(s.Length)])
            .ToArray());
    }
    
    private static List<BailiffTransaction> ParseCsv(string fileContents)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = false,
            TrimOptions = TrimOptions.Trim,
            Mode = CsvMode.RFC4180,
            MissingFieldFound = null // Ignore missing fields
        };
        using var reader = new StringReader(fileContents);
        using var csv = new CsvReader(reader, config);
        csv.Context.RegisterClassMap<BailiffTransactionMap>();

        var transactions = new List<BailiffTransaction>();
        var rowNumber = 1;
    
        foreach (var record in csv.GetRecords<BailiffTransaction>())
        {
            record.RowNumber = rowNumber++;
            transactions.Add(record);
        }

        return transactions;
    }
}

public class CustomDateTimeConverter : DateTimeConverter
{
    public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
    {
        return DateTimeOffset.ParseExact(text, "dd/MM/yyyy", CultureInfo.InvariantCulture);
    }
}
public sealed class BailiffTransactionMap : ClassMap<BailiffTransaction>
{
    public BailiffTransactionMap()
    {
        Map(m => m.TransactionDate).Index(0).TypeConverter<CustomDateTimeConverter>();
        Map(m => m.CustomerReference).Index(1);
        Map(m => m.Amount).Index(2).TypeConverter<DecimalConverter>();
        Map(m => m.FundName).Index(3);
        Map(m => m.LiabilityOrderNumber).Index(4);
    }
}

internal class FundDetails
{
    
    public required string VatCode { get; set; }
    public float VatRate { get; set; }
    public string? FundCode { get; set; }
}

public class BailiffTransaction
{
    public DateTimeOffset TransactionDate { get; set; }
    public string? CustomerReference { get; set; }
    public decimal Amount { get; set; }
    public string? FundName { get; set; }
    public string? LiabilityOrderNumber { get; set; }
    public int RowNumber { get; set; }
}



