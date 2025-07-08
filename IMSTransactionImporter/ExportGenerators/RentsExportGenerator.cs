using System.Text;
using IMSTransactionImporter.Classes;
using IMSTransactionImporter.Interfaces;
using LocalGovIMSClient;
using LocalGovIMSClient.Api.ProcessedTransactions;
using LocalGovIMSClient.Models;

namespace IMSTransactionImporter.ExportGenerators;

public class RentsExportGenerator : IExportGenerator
{
    public async Task Generate(IMSExport export, LocalGovIMSAPIClient client)
    {
        var fundsCodesForExport = new[] {"8"};

        // Get Processed transactions for the export period
        var processedTransactions = await GetProcessedTransactions(export, client, fundsCodesForExport!);

        var rows = new List<RentExportRow>();

        if (processedTransactions != null)
        {
            // Exclude refs between 97000000 - 97999999 These are for SME Professional
            processedTransactions = processedTransactions
                .Where(x => !string.IsNullOrWhiteSpace(x.AccountReference) 
                            && !x.AccountReference.StartsWith("97"))
                .ToList();
            
            rows = processedTransactions.Select(ToRentExportRow).ToList();
        }

        // Create the text output
        CreateTextFile(rows, export.FileName);
    }

    private static void CreateTextFile(List<RentExportRow> rows, string exportFileName)
    {
        var sb = new StringBuilder();
        foreach (var row in rows)
        {
            sb.AppendLine(
                $"{row.AccountNumber}{row.SubAccountNumber}{row.TransDate}{row.MethodOfPayment}{row.WeekNumber}{row.ReceiptNumber}{row.Amount}");
        }

        File.WriteAllText(exportFileName, sb.ToString());
    }


    private RentExportRow ToRentExportRow(ProcessedTransactionModel transaction)
    {
        var rentExportRow = new RentExportRow
        {
            AccountNumber = CalcAccountNumber(transaction.AccountReference),
            SubAccountNumber = "0",
            TransDate = transaction.TransactionDate!.Value.ToString("dd.MM.yyyy"),
            MethodOfPayment = CalcMethodOfPayment(transaction.MopCode),
            WeekNumber = "00",
            ReceiptNumber = CalcAccountNumber(transaction.AccountReference),
            Amount = CalcAmount(transaction.Amount)
        };
        return rentExportRow;
    }


    private static string CalcAccountNumber(string? accountReference)
    {
        if (string.IsNullOrWhiteSpace(accountReference))
            return "00000000"; // Return default value if account reference is null or empty

        // Remove any whitespace
        var cleanReference = accountReference.Trim();

        // Take first 8 characters if length is sufficient
        return cleanReference.Length >= 8
            ? cleanReference[..8]
            :
            // Pad with zeros if less than 8 characters
            cleanReference.PadRight(8, '0');
    }

    private static string CalcMethodOfPayment(string? transactionMopCode)
    {
        if (string.IsNullOrWhiteSpace(transactionMopCode))
        {
            return "00  "; // Return default value if MOP code is null or empty
        }

        var code = transactionMopCode switch
        {
            "51" => "SA",
            "46" => "IP",
            "47" => "IP",
            "1" => "KS",
            "5" => "ME",
            "22" => "TC",
            "48" => "CO",
            "4" => "CA",
            "19" => "IB",
            "12" => "PP",
            _ => transactionMopCode // default case: return the original Pay Code
        };
        return code.PadRight(4, ' '); // pad with spaces to make sure the length is 4 characters
    }
    
    private string CalcAmount(double? amount)
    {
        if (!amount.HasValue)
        {
            return "         0";
        }

        try
        {
            // Round to 2 decimal places to avoid floating point issues
            var roundedAmount = Math.Round(amount.Value, 2);
        
            // Multiply by 100 to remove decimals and convert to pennies
            var amountInPennies = Math.Round(roundedAmount * 100);
        
            // Convert to positive integer string padded with zeros
            return Math.Abs((long)amountInPennies).ToString().PadLeft(10, ' ');
        }
        catch (OverflowException)
        {
            // Handle extreme values
            return "9999999999";
        }
        catch (Exception)
        {
            // Handle any other unexpected errors
            return "         0";
        }
    }

    private async Task<List<ProcessedTransactionModel>?> GetProcessedTransactions(IMSExport export, LocalGovIMSAPIClient client, string[] fundsCodesForExport)
    {
        var parameters = new ProcessedTransactionsRequestBuilder.ProcessedTransactionsRequestBuilderGetQueryParameters
        {
            StartDate = export.StartDateTime,
            EndDate = export.EndDateTime,
            FundCodes = fundsCodesForExport
        };

        var response = await client.Api.ProcessedTransactions.GetAsync(r => r.QueryParameters = parameters);

        return response;
    }

    public class RentExportRow
    {
        public required string AccountNumber { get; set; }
        public required string SubAccountNumber { get; set; }
        public required string TransDate { get; set; }
        public required string MethodOfPayment { get; set; }
        public required string WeekNumber { get; set; }
        public required string ReceiptNumber { get; set; }
        public required string Amount { get; set; }
    }
}