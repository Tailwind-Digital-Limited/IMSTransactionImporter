using System.Globalization;
using System.Text;
using IMSTransactionImporter.Classes;
using IMSTransactionImporter.Interfaces;
using LocalGovIMSClient;
using LocalGovIMSClient.Api.ProcessedTransactions;
using LocalGovIMSClient.Models;

namespace IMSTransactionImporter.ExportGenerators;

public class ParkingFineExportGenerator : IExportGenerator
{
    public async Task Generate(IMSExport export, LocalGovIMSAPIClient client)
    {
        var fundsCodesForExport = new[] {"9"};

        // Get Processed transactions for the export period
        var processedTransactions = await GetProcessedTransactions(export, client, fundsCodesForExport!);

        if (processedTransactions != null)
        {
            // only include transactions where the account code starts GG6
            processedTransactions = processedTransactions
                .Where(pt => pt.AccountReference != null 
                             && pt.AccountReference.StartsWith("GG6"))
                .ToList();
            
            var rows = processedTransactions
                .Select(ToParkingFineRecord)
                .ToList();

            // Create the text output
            CreateTextFile(rows, export.FileName);
        }
    }

    private static void CreateTextFile(List<ParkingFineRecord> records, string exportFileName)
    {
        var sb = new StringBuilder();

        // create run number 
        var runNumber = DaysSinceDate(new DateTime(2015, 1, 1)).ToString().PadLeft(6, '0');

        // header record
        sb.AppendLine(runNumber);
        sb.AppendLine("0001");
        sb.AppendLine(records.Count.ToString().PadLeft(4, '0')); // 0009 if 9 records in export
        sb.AppendLine(records.Sum(r => r.FinePaidAmountValue).ToString("F2").PadLeft(9, '0')); // if sum of fines is £432.15 = 000432.15 
        AddFillerRows(sb, 23); // add 23 blank rows
        sb.AppendLine($"{DateTime.Now:dd/MM/yy}");

        foreach (var record in records)
        {
            sb.AppendLine(runNumber);
            sb.AppendLine("0001");
            sb.AppendLine(record.PCNSerialNumber);
            sb.AppendLine(record.FinePaidAmount);
            sb.AppendLine(record.ReceiptDate);
            sb.AppendLine(record.ReceiptTime);
            sb.AppendLine(record.ReceiptNumber);
            AddFillerRows(sb, 14); // 14 blank rows
            sb.AppendLine(record.PaymentMethod);
            sb.AppendLine(record.FinePaidAmount);
            sb.AppendLine("000000.00");
            sb.AppendLine("000000.00");
            sb.AppendLine("000000.00");
            sb.AppendLine("000000.00");
            sb.AppendLine(record.ReceiptDate);
        }

        File.WriteAllText(exportFileName, sb.ToString());
    }

    private static int DaysSinceDate(DateTime startDate)
    {
        return (DateTime.Now - startDate).Days;
    }

    private ParkingFineRecord ToParkingFineRecord(ProcessedTransactionModel transaction)
    {
        var smeProfessionalExportRow = new ParkingFineRecord
        {
            PCNSerialNumber = transaction.AccountReference!,
            ReceiptDate = transaction.TransactionDate!.Value.ToString("dd/MM/yy"),
            ReceiptTime = transaction.TransactionDate.Value.ToString("HH:mm"),
            PaymentMethod = CalcPaymentMethod(transaction.MopCode),
            ReceiptNumber = !string.IsNullOrEmpty(transaction.PspReference) ? transaction.PspReference : "",
            FinePaidAmount = transaction.Amount!.Value.ToString("F2").PadLeft(9, '0'),
            FinePaidAmountValue = transaction.Amount.Value
        };
        return smeProfessionalExportRow;
    }

    private static string CalcPaymentMethod(string? transactionMopCode) => transactionMopCode switch
    {
        "19" => "BAI",
        "20" => "BAI",
        _ => string.IsNullOrEmpty(transactionMopCode) ? "" : transactionMopCode
    };

    private static void AddFillerRows(StringBuilder sb, int rowsToAdd)
    {
        for (var i = 0; i < rowsToAdd; i++)
        {
            sb.AppendLine();
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

    public class ParkingFineRecord
    {
        public required string PCNSerialNumber { get; set; }
        public required string ReceiptDate { get; set; }
        public required string ReceiptTime { get; set; }
        public required string ReceiptNumber { get; set; }
        public required string PaymentMethod { get; set; }
        public required string FinePaidAmount { get; set; }
        public double FinePaidAmountValue { get; set; }
    }
}