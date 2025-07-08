using System.Text;
using Audacia.Spreadsheets;
using IMSTransactionImporter.Classes;
using IMSTransactionImporter.Interfaces;
using LocalGovIMSClient;
using LocalGovIMSClient.Api.ProcessedTransactions;
using LocalGovIMSClient.Models;

namespace IMSTransactionImporter.ExportGenerators;

public class GeneralLedgerExportGenerator : IExportGenerator
{
    private List<FundModel>? _allFunds = [];
    private List<MethodOfPaymentModel>? _methodsOfPayment = [];
    private List<AccountHolderModel>? _accountHolders = [];

    public async Task Generate(IMSExport export, LocalGovIMSAPIClient client)
    {
        // Get the funds and MOPS from the API - this includes all the metadata for the funds
        _allFunds = await client.Api.Funds.GetAsync();
        _methodsOfPayment = await client.Api.MethodOfPayments.GetAsync();
        _accountHolders = await client.Api.AccountHolders.GetAsync(ah => ah.QueryParameters.FundCode = "10");

        if (_allFunds != null)
        {
            var fundsCodesForExport = _allFunds
                .Where(f => f.Metadata != null
                            && f.Metadata.Exists(m => m.Key == "ExportToLedger" && m.Value == "True")
                            && f.FundCode != null)
                .Select(f => f.FundCode)
                .ToArray();

            // Get Processed transactions for the export period
            var processedTransactions = await GetProcessedTransactions(export, client, fundsCodesForExport!);

            var rows = new List<LedgerRow>();


            if (processedTransactions != null)
            {
                // Part 1 Side 1 format - negative net amounts
                foreach (var transaction in processedTransactions)
                {
                    var ledgerRow = ConvertToLedgerRowSide1Part1(transaction);
                    rows.Add(ledgerRow);
                }

                // Part 1 Side 2 format - negative vat amounts for fund 10 
                foreach (var transaction in processedTransactions.Where(pt => pt.VatAmount != null && pt.VatAmount != 0))
                {
                    var ledgerRow = ConvertToLedgerRowSide1Part2(transaction);
                    rows.Add(ledgerRow);
                }

                // Part 1 Side 3 format - sum of transactions grouped by mop and date, excluding mops 19 and 22
                AddLedgerRowsForPart1Side3(rows, processedTransactions);

                // Part 2 Side 1 format - positive full amounts
                foreach (var transaction in processedTransactions)
                {
                    var ledgerRow = ConvertToLedgerRowSide2Part1(transaction);
                    rows.Add(ledgerRow);
                }

                // Part 2 Side 2 format - sum of transactions grouped by mop and transaction date, excluding mops 19 and 22
                AddLedgerRowsForPart2Side2(rows, processedTransactions);
            }

            // Create the excel file and write to disc
            CreateExcelFile(rows, export.FileName);
        }
    }

    private static void CreateExcelFile(List<LedgerRow> ledgerRows, string exportFilePath)
    {
        // Make Rows 
        var tableRows = ledgerRows.Select(FromLedgerRow);
        
        // Make Table, define columns, add rows
        var table = new Table(includeHeaders: true)
        {
            Columns = DefineColumns,
            Rows = tableRows
        };

        // Make Worksheet and add table
        var worksheet = new Worksheet
        {
            SheetName = "Sheet1",
            Table = table
        };

        // Export the spreadsheet
        var spreadsheet = Spreadsheet.FromWorksheets(worksheet);
        spreadsheet.Export(exportFilePath);
    }

    private static List<TableColumn> DefineColumns =>
    [
        new("Year"),
        new("Period"),
        new("Date"),
        new("Code"),
        new("Amount"),
        new("Reference"),
        new("Analysis"),
        new("Narrative")
    ];

    private static TableRow FromLedgerRow(LedgerRow ledgerRow)
    {
        return TableRow.FromCells([
            new TableCell(ledgerRow.Year),
            new TableCell(ledgerRow.Period),
            new TableCell(ledgerRow.Date),
            new TableCell(ledgerRow.Code),
            new TableCell(ledgerRow.Amount),
            new TableCell(ledgerRow.Reference),
            new TableCell(ledgerRow.Analysis),
            new TableCell(ledgerRow.Narrative)
        ], null);
    }

    private LedgerRow ConvertToLedgerRowSide1Part1(ProcessedTransactionModel transaction)
    {
        var netAmount = (decimal) (transaction.Amount!.Value - transaction.VatAmount!.Value);
        var useGeneralLedgerCode = _allFunds?.FirstOrDefault(f => f.FundCode == transaction.FundCode)?.Metadata?.Exists(m => m.Key == "UseGeneralLedgerCode" && m.Value == "True");
        var generalLedgerCode = "";
        if (useGeneralLedgerCode == true)
        {
            generalLedgerCode = _allFunds?.FirstOrDefault(f => f.FundCode == transaction.FundCode)?.Metadata?.FirstOrDefault(m => m.Key == "GeneralLedgerCode")?.Value!;
        }
        else
        {
            // Look up the GL Code from the AccountHolderTable
            if (transaction.FundCode == "10")
            {
                var generalLedgerAccount = _accountHolders!.FirstOrDefault(ah => ah.AccountReference == transaction.AccountReference);
                if (generalLedgerAccount != null)
                {
                    generalLedgerCode = generalLedgerAccount.UserField1;
                }
            }
        }

        var ledgerRow = new LedgerRow
        {
            Year = CalculateYear(),
            Period = CalculatePeriod(),
            Date = transaction.TransactionDate!.Value.ToString("dd/MM/yyyy"),
            Code = generalLedgerCode!,
            // net amount = amount - vat amount
            Amount = -netAmount,
            Reference = transaction.AccountReference!.Trim(),
            Analysis = GetFundName(transaction.FundCode!),
            Narrative = MakeNarrative(transaction)
        };
        return ledgerRow;
    }

    private LedgerRow ConvertToLedgerRowSide1Part2(ProcessedTransactionModel transaction)
    {
        var ledgerRow = new LedgerRow
        {
            Year = CalculateYear(),
            Period = CalculatePeriod(),
            Date = transaction.TransactionDate!.Value.ToString("dd/MM/yyyy"),
            Code = "Z840/L0013",
            // net amount = amount - vat amount
            Amount = (decimal) -transaction.VatAmount!.Value,
            Reference = transaction.AccountReference!.Trim(),
            Analysis = GetFundName(transaction.FundCode!),
            Narrative = MakeNarrative(transaction)
        };
        return ledgerRow;
    }

    private void AddLedgerRowsForPart1Side3(List<LedgerRow> ledgerRows, List<ProcessedTransactionModel> processedTransactions)
    {
        var excludedMOPCodes = new List<string> {"19", "22"};
        // Group transactions by MOP and date, exclude mops 19 and 22
        var transactionGroups = processedTransactions
            .Where(pt => !excludedMOPCodes.Contains(pt.MopCode!))
            .GroupBy(pt => new {transactiondate = pt.TransactionDate!.Value.Date, pt.MopCode});

        foreach (var transactionGroup in transactionGroups)
        {
            ledgerRows.Add(new LedgerRow
            {
                Year = CalculateYear(),
                Period = CalculatePeriod(),
                Date = DateTime.Now.ToString("dd/MM/yyyy"),
                Code = "Z001/L0030",
                Amount = (decimal) -transactionGroup.Sum(t => t.Amount!.Value),
                Reference = "",
                Analysis = "",
                Narrative = $"MOP: ({transactionGroup.Key.MopCode}); TrDate:{transactionGroup.Key.transactiondate:dd/MM/yyyy}"
            });
        }
    }

    private LedgerRow ConvertToLedgerRowSide2Part1(ProcessedTransactionModel transaction)
    {
        var ledgerRow = new LedgerRow
        {
            Year = CalculateYear(),
            Period = CalculatePeriod(),
            Date = transaction.TransactionDate!.Value.ToString("dd/MM/yyyy"),
            Code = "Z001/L0030", // if bank account number is 64787060 then code should be Z010/L0030
            // TODO: Check the import to see if we are capturing the bank account being used

            // net amount = amount - vat amount
            Amount = (decimal) transaction.Amount!.Value,
            Reference = transaction.AccountReference!.Trim(),
            Analysis = GetFundName(transaction.FundCode!),
            Narrative = MakeNarrative(transaction)
        };
        return ledgerRow;
    }

    private void AddLedgerRowsForPart2Side2(List<LedgerRow> ledgerRows, List<ProcessedTransactionModel> processedTransactions)
    {
        var excludedMOPCodes = new List<string> {"19", "22"};
        // Group transactions by MOP and date, exclude mops 19 and 22
        var transactionGroups = processedTransactions
            .Where(pt => !excludedMOPCodes.Contains(pt.MopCode!))
            .GroupBy(pt => new {transactiondate = pt.TransactionDate!.Value.Date, pt.MopCode});

        foreach (var transactionGroup in transactionGroups)
        {
            ledgerRows.Add(new LedgerRow
            {
                Year = CalculateYear(),
                Period = CalculatePeriod(),
                Date = DateTime.Now.ToString("dd/MM/yyyy"),
                Code = CalculateGlCodePart2Side2(transactionGroup.Key.MopCode!),
                Amount = (decimal) transactionGroup.Sum(t => t.Amount!.Value),
                Reference = "",
                Analysis = "",
                Narrative = $"MOP: ({transactionGroup.Key.MopCode}); TrDate:{transactionGroup.Key.transactiondate:dd/MM/yyyy}"
            });
        }
    }

    private static string CalculateGlCodePart2Side2(string mopCode)
    {
        switch (mopCode.Length)
        {
            case 1:
                return $"X70{mopCode}/L9820";
            case 2:
                return $"X7{mopCode}/L9820";
        }

        return $"X7{mopCode}/L9820";
    }

    private string MakeNarrative(ProcessedTransactionModel transaction)
    {
        var sb = new StringBuilder();

        sb.Append($"PayRef:{transaction.PspReference}; ");
        // TODO: sb.Append($"ExtRef:{transaction.ExternalReference}; ");
        sb.Append($"FundCode:{transaction.FundCode}; ");
        sb.Append($"MOP:{transaction.MopCode} ");

        var mopName = _methodsOfPayment?.FirstOrDefault(m => m.Code == transaction.MopCode)?.Name;

        sb.Append($"{mopName}({transaction.MopCode}); ");
        sb.Append($"TrDate:{transaction.TransactionDate!.Value.ToString("dd/MM/yyyy")}; ");
        sb.Append($"PostDate:{transaction.EntryDate!.Value.ToString("dd/MM/yyyy")}; ");

        return sb.ToString();
    }

    private string GetFundName(string transactionFundCode)
    {
        var fund = _allFunds?.FirstOrDefault(f => f.FundCode == transactionFundCode);
        return fund?.FundName!;
    }

    private static int CalculatePeriod()
    {
        var month = DateTime.Now.Month;
        if (month < 4)
        {
            return month + 9;
        }

        return month - 3;
    }

    private static int CalculateYear()
    {
        if (DateTime.Now.Month < 4)
        {
            return DateTime.Now.Year;
        }

        return DateTime.Now.Year + 1;
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

    public class LedgerRow
    {
        public int Year { get; set; }
        public int Period { get; set; }
        public required string Date { get; set; }
        public required string Code { get; set; }
        public decimal Amount { get; set; }
        public required string Reference { get; set; }
        public required string Analysis { get; set; }
        public required string Narrative { get; set; }
    }
}