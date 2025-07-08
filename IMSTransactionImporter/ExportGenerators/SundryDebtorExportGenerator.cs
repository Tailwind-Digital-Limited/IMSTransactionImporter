using System.Globalization;
using System.Text;
using IMSTransactionImporter.Classes;
using IMSTransactionImporter.Interfaces;
using LocalGovIMSClient;
using LocalGovIMSClient.Api.ProcessedTransactions;
using LocalGovIMSClient.Models;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;

namespace IMSTransactionImporter.ExportGenerators;

public class SundryDebtorExportGenerator : IExportGenerator
{
    public async Task Generate(IMSExport export, LocalGovIMSAPIClient client)
    {
        var fundsCodesForExport = new[] {"7"};

        // Get Processed transactions for the export period
        var processedTransactions = await GetProcessedTransactions(export, client, fundsCodesForExport!);

        var rows = new List<SundryDebtorRow>();

        if (processedTransactions != null)
        {
            rows = processedTransactions.Select(ToSundryDebtorRow).ToList();
        }

        // Create the text output
        CreateTextFile(rows, export.FileName);
    }

    private static void CreateTextFile(List<SundryDebtorRow> rows, string exportFileName)
    {
        var sb = new StringBuilder();
        foreach (var row in rows)
        {
            sb.AppendLine(
                $"{row.ICMRef},{row.MethodOfPayment},{row.ExportDate},{row.AccountRef1},{row.TransDate},{row.Filler},{row.Amount},{row.AccountRef2},{row.TransactionDate}");
        }

        File.WriteAllText(exportFileName, sb.ToString());
    }


    private SundryDebtorRow ToSundryDebtorRow(ProcessedTransactionModel transaction)
    {
        var sundryDebtorRow = new SundryDebtorRow
        {
            ICMRef = transaction.PspReference!.Trim(),
            MethodOfPayment = transaction.MopCode!,
            ExportDate = DateTime.Now.ToString("dd/MM/yyyy"),
            AccountRef1 = transaction.AccountReference!.Trim(),
            AccountRef2 = transaction.AccountReference!.Trim(),
            TransDate = transaction.TransactionDate!.Value.ToString("dd MMM yy "),
            TransactionDate = transaction.TransactionDate!.Value.ToString("dd/MM/yyyy"),
            Amount = transaction.Amount!.Value.ToString("F2", CultureInfo.InvariantCulture),
            Filler = " "
        };
        return sundryDebtorRow;
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

    public class SundryDebtorRow
    {
        public required string ICMRef { get; set; }
        public required string MethodOfPayment { get; set; }
        public required string ExportDate { get; set; }
        public required string AccountRef1 { get; set; }
        public required string TransDate { get; set; }
        public required string Filler { get; set; }
        public required string Amount { get; set; }
        public required string AccountRef2 { get; set; }
        public required string TransactionDate { get; set; }
    }
}