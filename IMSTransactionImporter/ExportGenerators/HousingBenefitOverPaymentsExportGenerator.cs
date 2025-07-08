using System.Text;
using IMSTransactionImporter.Classes;
using IMSTransactionImporter.Interfaces;
using LocalGovIMSClient;
using LocalGovIMSClient.Api.ProcessedTransactions;
using LocalGovIMSClient.Models;

namespace IMSTransactionImporter.ExportGenerators;

public class HousingBenefitOverPaymentExportGenerator : IExportGenerator
{
    public async Task Generate(IMSExport export, LocalGovIMSAPIClient client)
    {
        var fundsCodesForExport = new[] {"6"};

        // Get Processed transactions for the export period
        var processedTransactions = await GetProcessedTransactions(export, client, fundsCodesForExport!);

        var rows = new List<HousingBenefitOverPaymentsExportRow>();

        if (processedTransactions != null)
        {
            rows = processedTransactions.Select(ToExportRow).ToList();
        }

        // Create the text output
        CreateTextFile(rows, export.FileName);
    }

    private static void CreateTextFile(List<HousingBenefitOverPaymentsExportRow> rows, string exportFileName)
    {
        var sb = new StringBuilder();
        foreach (var row in rows)
        {
            sb.AppendLine(
                $"{Filler(4)}{row.ICMRef}{Filler(8)}{row.TransDate}{row.Amount}{row.CreditIndicator}{Filler(7)}{row.AccountNumber}{Filler(36)}");
        }

        File.WriteAllText(exportFileName, sb.ToString());
    }


    private HousingBenefitOverPaymentsExportRow ToExportRow(ProcessedTransactionModel transaction)
    {
        var rentExportRow = new HousingBenefitOverPaymentsExportRow
        {
            ICMRef = CalculateReference(transaction.PspReference),
            AccountNumber = CalcAccountNumber(transaction.AccountReference),
            TransDate = transaction.TransactionDate!.Value.ToString("ddMMyy"),
            Amount = CalcAmount(transaction.Amount),
            CreditIndicator = CalculateCreditIndicator(transaction.Amount)
        };
        return rentExportRow;
    }

    private static string CalculateReference(string? transactionPspReference)
    {
        if (string.IsNullOrWhiteSpace(transactionPspReference))
        {
            return "".PadRight(12, ' ');
        }

        var cleanReference = transactionPspReference.Trim();

        return cleanReference.Length switch
        {
            > 12 => cleanReference[..12], // Chop to first 12 characters
            _ => cleanReference.PadRight(12, ' ') // Pad with spaces to make sure the length is 12 characters
        };
    }

    private static string CalculateCreditIndicator(double? transactionAmount)
    {
        return transactionAmount switch
        {
            null => " ",
            < 0 => "Y", // Credit
            _ => " "
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
            return "00000000"; // Return default value if account reference is null or empty
        }

        // Remove any whitespace
        var cleanReference = accountReference.Trim();

        // return padded account number
        return cleanReference.PadLeft(11, ' ');
    }


    private string CalcAmount(double? amount)
    {
        if (!amount.HasValue)
        {
            return "          0";
        }

        try
        {
            // Round to 2 decimal places to avoid floating point issues
            var roundedAmount = Math.Round(amount.Value, 2);

            // Multiply by 100 to remove decimals and convert to pennies
            var amountInPennies = Math.Round(roundedAmount * 100);

            // Convert to positive integer string padded with zeros
            return Math.Abs((long) amountInPennies).ToString().PadLeft(11, ' ');
        }
        catch (OverflowException)
        {
            // Handle extreme values
            return "99999999999";
        }
        catch (Exception)
        {
            // Handle any other unexpected errors
            return "          0";
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

    public class HousingBenefitOverPaymentsExportRow
    {
        public required string ICMRef { get; set; }
        public required string TransDate { get; set; }
        public required string CreditIndicator { get; set; }
        public required string Amount { get; set; }
        public required string AccountNumber { get; set; }
    }
}