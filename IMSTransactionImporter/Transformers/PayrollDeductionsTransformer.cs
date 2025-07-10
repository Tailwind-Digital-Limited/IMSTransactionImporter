using CsvHelper;
using System.Globalization;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using IMSTransactionImporter.Interfaces;
using LocalGovIMSClient.Models;

namespace IMSTransactionImporter.Transformers;

public class PayrollDeductionsTransformer : ITransactionTransformer
{
    public TransactionImportModel Transform(string fileContents)
    {
        // Parse CSV file
        var payrollDeductionTransactions = ParseCsv(fileContents);
        
        // Convert rows to IMSTransactionImport
        var rows = payrollDeductionTransactions.Select(Convert).ToList();
        
        // Create IMSTransactionImport
        var import = new TransactionImportModel()
        {
            Rows = rows,
            ImportTypeId = 4,
            Notes = "Imported from Payroll Deductions File"
        };
        
        return import;
    }

    public static ProcessedTransactionModel Convert(PayrollDeductionTransaction payrollDeduction)
    {
        var processedTransaction = new ProcessedTransactionModel
        {
            Amount = (double)payrollDeduction.Amount,
            AccountReference = payrollDeduction.CustomerReference,
            MopCode = "51",
            InternalReference = GenerateRandom16CharString(),
            PspReference = $"PYD-{DateTime.Now:yyMMdd}-{payrollDeduction.RowNumber}",
            OfficeCode = "S",
            EntryDate = DateTimeOffset.Now,
            TransactionDate = payrollDeduction.TransactionDate,
            Narrative = $"{payrollDeduction.EmployeeNameNumber}",
            // Set defaults
            VatRate = 0,
            VatAmount = 0,
            VatCode = "1",
        };
        var fundDetails = GetFundDetails(payrollDeduction.FundName);
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
        "HB Overpayment" => new FundDetails{FundCode = "6", VatCode = "3", VatRate = 0},
        "Housing Rents" => new FundDetails{FundCode = "8", VatCode = "3", VatRate = 0},
        "Income" => new FundDetails{FundCode = "10", VatCode = "3", VatRate = 0},
        
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
    
    private static List<PayrollDeductionTransaction> ParseCsv(string fileContents)
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
        csv.Context.RegisterClassMap<PayrollDeductionsMap>();

        var transactions = new List<PayrollDeductionTransaction>();
        var rowNumber = 1;
    
        foreach (var record in csv.GetRecords<PayrollDeductionTransaction>())
        {
            record.RowNumber = rowNumber++;
            transactions.Add(record);
        }

        return transactions;
    }
}

public class PayrollDeductionsDateTimeConverter : DateTimeConverter
{
    public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
    {
        return DateTimeOffset.ParseExact(text, "dd/MM/yyyy", CultureInfo.InvariantCulture);
    }
}
public sealed class PayrollDeductionsMap : ClassMap<PayrollDeductionTransaction>
{
    public PayrollDeductionsMap()
    {
        Map(m => m.TransactionDate).Index(0).TypeConverter<PayrollDeductionsDateTimeConverter>();
        Map(m => m.CustomerReference).Index(1);
        Map(m => m.Amount).Index(2).TypeConverter<DecimalConverter>();
        Map(m => m.FundName).Index(3);
        Map(m => m.PayElement).Index(4);
        Map(m => m.EmployeeNameNumber).Index(5);
    }
}

public class PayrollDeductionTransaction
{
    public DateTimeOffset TransactionDate { get; set; }
    public string? CustomerReference { get; set; }
    public decimal Amount { get; set; }
    public string? FundName { get; set; }
    public string? PayElement { get; set; }
    public string? EmployeeNameNumber { get; set; }
    public int RowNumber { get; set; }
}