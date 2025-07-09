using IMSTransactionImporter.Classes;
using LocalGovIMSClient.Models;

namespace IMSTransactionImporter.Interfaces;

public interface ITransactionTransformer
{
    TransactionImportModel Transform(string fileContents);
}
