using System.Globalization;
using System.Text;
using IMSTransactionImporter.Classes;
using IMSTransactionImporter.Interfaces;
using LocalGovIMSClient;
using LocalGovIMSClient.Api.ProcessedTransactions;
using LocalGovIMSClient.Models;

namespace IMSTransactionImporter.ExportGenerators;

public class CouncilTaxNNDRExportGenerator : IExportGenerator
{
    public async Task Generate(IMSExport export, LocalGovIMSAPIClient client)
    {
        var fundsCodesForExport = new[] {"2", "5"};

        // Get Processed transactions for the export period
        var processedTransactions = await GetProcessedTransactions(export, client, fundsCodesForExport!);

        var rows = new List<CouncilTaxNNDRExportRow>();

        if (processedTransactions != null)
        {
            rows = processedTransactions.Select(ToExportRow).ToList();
        }

        // Create the text output
        CreateTextFile(rows, export.FileName);
    }

    private static void CreateTextFile(List<CouncilTaxNNDRExportRow> rows, string exportFileName)
    {
        var sb = new StringBuilder();
        // Header row
        sb.AppendLine($"{DateTime.Now:dd MMM yyyy}*{DateTime.Now:HH:mm:ss}00002{Filler(26)}");
        // Data rows
        foreach (var row in rows)
        {
            sb.AppendLine(
                $"{row.AccountNumber}{Filler(9)}{row.CheckDigit}{Filler(2)}{row.MethodOfPayment}{row.Amount}{row.Fund}{row.TransDate}{Filler(20)}{row.ICMRef}{Filler(65)}{row.LiabilityNumber}");
        }

        File.WriteAllText(exportFileName, sb.ToString());
    }


    private CouncilTaxNNDRExportRow ToExportRow(ProcessedTransactionModel transaction)
    {
        var rentExportRow = new CouncilTaxNNDRExportRow
        {
            ICMRef = CalcReference(transaction.PspReference),
            AccountNumber = CalcAccountNumber(transaction.AccountReference),
            TransDate = transaction.TransactionDate!.Value.ToString("dd-MMM-yyyy"),
            Amount = CalcAmount(transaction.Amount),
            Fund = CalcFund(transaction.FundCode),
            LiabilityNumber = CalcLiabilityNumber(transaction.Narrative),
            MethodOfPayment = CalcMethodOfPayment(transaction.MopCode),
            CheckDigit = CalcCheckDigit(transaction.AccountReference)
        };
        return rentExportRow;
    }

    private string CalcCheckDigit(string? transactionAccountReference)
    {
        // return last character of account reference
        return string.IsNullOrWhiteSpace(transactionAccountReference) ? "" : transactionAccountReference.Trim().Last().ToString();
    }

    private static string CalcLiabilityNumber(string? transactionNarrative)
    {
        if (string.IsNullOrWhiteSpace(transactionNarrative))
        {
            return "";
        }

        // If the transaction was collected by a bailiff the the narrative will contain the liability number
        return transactionNarrative.Contains("Liability") ? transactionNarrative[..7].PadLeft(10, ' ') : Filler(10);
    }

    private static string CalcFund(string? transactionFundCode) => transactionFundCode switch
    {
        "2" => "CT",
        "5" => "NN",
        _ => transactionFundCode ?? "" // default case: return the original Pay Code
    };

    private static string CalcReference(string? transactionPspReference)
    {
        if (string.IsNullOrWhiteSpace(transactionPspReference))
        {
            return "".PadRight(18, ' ');
        }

        var cleanReference = transactionPspReference.Trim();
        
        // Take first 18 characters if length is sufficient
        return cleanReference.Length >= 18
            ? cleanReference[..18]
            : cleanReference.PadRight(18, ' '); // Pad with spaces if less than 18 characters
    }

    private static string CalcMethodOfPayment(string? transactionMopCode)
    {
        if (string.IsNullOrWhiteSpace(transactionMopCode))
        {
            return "00  "; // Return default value if MOP code is null or empty
        }

        return transactionMopCode switch
        {
            "46" or "47" => "WS",
            "5" => "TP",
            "12" => "PP",
            "1" => "K1",
            "48" or "49" => "K2",
            _ => transactionMopCode // default case: return the original Pay Code
        };
    }

    private static string Filler(int length)
    {
        return new string(' ', length);
    }


    private static string CalcAccountNumber(string? accountReference)
    {
        if (string.IsNullOrWhiteSpace(accountReference))
        {
            return "000000"; // Return default value if account reference is null or empty
        }

        // Remove any whitespace
        var cleanReference = accountReference.Trim();

        // return first six characters if length is sufficient
        return cleanReference[..6];
    }


    private static string CalcAmount(double? amount)
    {
        return amount.HasValue ? amount.Value.ToString("F2").PadLeft(14, '0') : "00000000000.00"; // Pad with zeros to make sure the length is 14 characters
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

    public class CouncilTaxNNDRExportRow
    {
        public required string ICMRef { get; set; }
        public required string TransDate { get; set; }
        public required string Amount { get; set; }
        public required string AccountNumber { get; set; }
        public required string CheckDigit { get; set; }
        public required string MethodOfPayment { get; set; }
        public required string LiabilityNumber { get; set; }
        public required string Fund { get; set; }
    }
}