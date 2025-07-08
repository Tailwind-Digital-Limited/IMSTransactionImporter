using System.Globalization;
using System.Text;
using IMSTransactionImporter.Classes;
using IMSTransactionImporter.Interfaces;
using LocalGovIMSClient;
using LocalGovIMSClient.Api.ProcessedTransactions;
using LocalGovIMSClient.Models;

namespace IMSTransactionImporter.ExportGenerators;

public class SMEProfessionalExportGenerator : IExportGenerator
{
    public async Task Generate(IMSExport export, LocalGovIMSAPIClient client)
    {
        var fundsCodesForExport = new[] {"8"};

        // Get Processed transactions for the export period
        var processedTransactions = await GetProcessedTransactions(export, client, fundsCodesForExport!);

        var rows = new List<SMEProfessionalExportRow>();

        if (processedTransactions != null)
        {
            // Only include refs between 97000000 - 97999999 These are for SME Professional
            processedTransactions = processedTransactions
                .Where(x => !string.IsNullOrWhiteSpace(x.AccountReference) 
                            && x.AccountReference.StartsWith("97"))
                .ToList();
            
            rows = processedTransactions.Select(ToSMEProfessionalExportRow).ToList();
        }

        // Create the text output
        CreateTextFile(rows, export.FileName);
    }

    private static void CreateTextFile(List<SMEProfessionalExportRow> rows, string exportFileName)
    {
        var sb = new StringBuilder();
        foreach (var row in rows)
        {
            sb.AppendLine($"{row.TransDate},{row.Amount},1,GBP,{row.AccountNumber}");
        }

        File.WriteAllText(exportFileName, sb.ToString());
    }


    private SMEProfessionalExportRow ToSMEProfessionalExportRow(ProcessedTransactionModel transaction)
    {
        var smeProfessionalExportRow = new SMEProfessionalExportRow
        {
            AccountNumber = !string.IsNullOrWhiteSpace(transaction.AccountReference) ? transaction.AccountReference.Trim() : "000000000",
            TransDate = transaction.TransactionDate!.Value.ToString("dd-MMM-yy"),
            Amount = transaction.Amount.HasValue ? transaction.Amount.Value.ToString("F2") : "0.00" // Round to 2 decimal places to avoid floating point issues
            
        };
        return smeProfessionalExportRow;
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

    public class SMEProfessionalExportRow
    {
        public required string AccountNumber { get; set; }
        public required string TransDate { get; set; }
        public required string Amount { get; set; }
    }
}