using IMSTransactionImporter.Classes;
using LocalGovIMSClient;

namespace IMSTransactionImporter.Interfaces;

public interface IExportGenerator
{
    Task Generate(IMSExport export, LocalGovIMSAPIClient client);
}
