using CsvHelper;
using CsvHelper.Configuration.Attributes;
using System.Globalization;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using IMSTransactionImporter.Classes;
using IMSTransactionImporter.Extensions;
using IMSTransactionImporter.Interfaces;
using LocalGovIMSClient.Models;

namespace IMSTransactionImporter.Transformers;

public class PIPTransformer : ITransactionTransformer
{
    public TransactionImportModel Transform(string fileContents)
    {
        // Parse CSV file
        var pipTransactions = ParseCsv(fileContents);
        
        // Convert rows to IMSTransactionImport
        var rows = pipTransactions.Select(Convert).ToList();
        
        // Create IMSTransactionImport
        var import = new TransactionImportModel
        {
            Rows = rows,
            ImportTypeId = 1,
            Notes = "Imported from PIPostOffice File"
        };
        
        return import;
    }

    private ProcessedTransactionModel Convert(PIPTransaction pip)
    {
        var processedTransaction = new ProcessedTransactionModel
        {
            Reference = pip.ReferenceNumber,
            Amount = pip.Amount,
            MopCode = "12",
            InternalReference = pip.PspReference,
            PspReference = $"PIP-{DateTime.Now:yyyyMMdd}-{pip.ContinuousAuditNumber}",
            OfficeCode = "S",
            EntryDate = DateTimeOffset.Now,
            TransactionDate = pip.TransactionDate,
            VatCode = "2",
            VatRate = 0,
            VatAmount = 0,
            Narrative = $"{pip.PaymentSource} - {pip.PaymentMethod}"
        };
        
        SetFundCodeAndAccountReference(processedTransaction);

        return processedTransaction;
    }

    private static readonly string[] FundCode1References = ["01", "02", "03", "04", "05"];

    public void SetFundCodeAndAccountReference(ProcessedTransactionModel processedTransaction)
    {
        var accountReference = "";
        var fundCode = "";
            
        // Early return if refNo is null or too short
        if (string.IsNullOrEmpty(processedTransaction.Reference) || processedTransaction.Reference.Length < 8)
            return;

        // Check if reference number starts with '98265029'
        if (processedTransaction.Reference.StartsWith("98265029"))
        {
            // Get positions with null checking
            var pos12 = processedTransaction.Reference.Length >= 12 ? processedTransaction.Reference[11].ToString() : "";
            var pos16 = processedTransaction.Reference.Length >= 16 ? processedTransaction.Reference[15].ToString() : "";
            var pos1617 = processedTransaction.Reference.Length >= 17 ? processedTransaction.Reference.Substring(15, 2) : "";

            

            // Fund Code logic
            if (pos12 is "7" or "6" && (pos16 == "6" || pos1617 == "06"))
            {
                fundCode = "6";
            }
            else if (pos12 == "7" && FundCode1References.Contains(pos1617))
            {
                fundCode = "1";
            }
            else if (int.TryParse(pos12, out int pos12Value))
            {
                fundCode = pos12Value.ToString();
            }

            // Reference Number transformation logic
            if (pos12 is "2" or "5" && processedTransaction.Reference.Length >= 22)
            {
                accountReference = ReplaceLettersWithZero(processedTransaction.Reference.Substring(16, 6));
                accountReference = accountReference.AddCouncilTaxCheckDigit(); // Logic is the same for Non Domestic Rates
            }
            else if ((pos12 == "6" || pos12 == "7") && processedTransaction.Reference.Length >= 22)
            {
                accountReference = ReplaceLettersWithZero(processedTransaction.Reference.Substring(15, 7));
                accountReference = accountReference.AddHousingBenefitsOverpayment7CheckDigit(); // Logic is the same for Sundry Debtors
            }
            else if (pos12 == "8" && processedTransaction.Reference.Length >= 22)
            {
                accountReference = ReplaceLettersWithZero(processedTransaction.Reference.Substring(14, 8));
                accountReference = accountReference.AddHousingRentsCheckDigit();
            }

            processedTransaction.FundCode = fundCode;
            processedTransaction.AccountReference = accountReference;
        }
    }

    

    private static string ReplaceLettersWithZero(string input)
    {
        // Assuming this method exists - implementing it for completeness
        return string.Concat(input.Select(c => char.IsLetter(c) ? '0' : c));
    }
    
    private static List<PIPTransaction> ParseCsv(string fileContents)
    {
        // Split the content into lines
        var lines = fileContents.Split(["\r\n", "\n"], StringSplitOptions.None);

        // Skip first and last 2 lines and rejoin
        var processedContent = string.Join(Environment.NewLine, lines.Skip(1).SkipLast(2));

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = false,
            TrimOptions = TrimOptions.Trim,
            Mode = CsvMode.RFC4180,
            MissingFieldFound = null // Ignore missing fields
        };
        using var reader = new StringReader(processedContent);
        using var csv = new CsvReader(reader, config);

        // Register custom date converter
        csv.Context.RegisterClassMap<PIPTransactionMap>();

        return csv.GetRecords<PIPTransaction>().ToList();
    }
}

public class PIPTransaction
{
    public string TransactionStatus { get; set; }
    public string TransactionType { get; set; }
    public DateTimeOffset TransactionDate { get; set; }
    public int ContinuousAuditNumber { get; set; }
    public int GroupNumber { get; set; }
    public string ClientId { get; set; }
    public string LineId { get; set; }
    public string ReferenceNumber { get; set; }
    public double Amount { get; set; }
    public float VATAmount { get; set; }
    public int PartialBankAccount { get; set; }
    public int BankSortCode { get; set; }
    public string BACSReference { get; set; }
    public string PartialCardNumber { get; set; }
    public string PaymentDescription { get; set; }
    public string CardHolderName { get; set; }
    public string PspReference { get; set; }
    public string PaymentSource { get; set; }
    public string PaymentMethod { get; set; }
    public string FundCode { get; set; }
    public string CRN { get; set; }

    [Name("External Transaction Reference")]
    public string ExternalTransactionReference { get; set; }

    public string ExternalTerminalReference { get; set; }

    [Name("ExternalPayment LocationCode")]
    public string ExternalPaymentLocationCode { get; set; }

    public string LocationName { get; set; }
    public string LocationAddress1 { get; set; }
    public string LocationAddress2 { get; set; }
    public string LocationAddress3 { get; set; }
    public string LocationAddress4 { get; set; }
    public string Postcode { get; set; }
    public string Reference2 { get; set; }
    public string Reference3 { get; set; }
}

public sealed class PIPTransactionMap : ClassMap<PIPTransaction>
{
    public PIPTransactionMap()
    {
        Map(m => m.TransactionStatus);
        Map(m => m.TransactionType);
        Map(m => m.TransactionDate).TypeConverter<CustomDateConverter>();
        Map(m => m.ContinuousAuditNumber);
        Map(m => m.GroupNumber);
        Map(m => m.ClientId);
        Map(m => m.LineId);
        Map(m => m.ReferenceNumber);
        Map(m => m.Amount);
        Map(m => m.VATAmount);
        Map(m => m.PartialBankAccount);
        Map(m => m.BankSortCode);
        Map(m => m.BACSReference);
        Map(m => m.PartialCardNumber);
        Map(m => m.PaymentDescription);
        Map(m => m.CardHolderName);
        Map(m => m.PspReference);
        Map(m => m.PaymentSource);
        Map(m => m.PaymentMethod);
        Map(m => m.FundCode);
        Map(m => m.CRN);
        Map(m => m.ExternalTransactionReference).Name("External Transaction Reference");
        Map(m => m.ExternalTerminalReference);
        Map(m => m.ExternalPaymentLocationCode).Name("ExternalPayment LocationCode");
        Map(m => m.LocationName);
        Map(m => m.LocationAddress1);
        Map(m => m.LocationAddress2);
        Map(m => m.LocationAddress3);
        Map(m => m.LocationAddress4);
        Map(m => m.Postcode);
        Map(m => m.Reference2);
        Map(m => m.Reference3);
    }
}

public class CustomDateConverter : DefaultTypeConverter
{
    public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
    {
        if (string.IsNullOrEmpty(text)) return null;

        // Parse date in format "ddMMyyy HHmmss"
        if (DateTimeOffset.TryParseExact(text, "ddMMyyyy HHmmss",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTimeOffset result))
        {
            return result;
        }

        return null;
    }
}