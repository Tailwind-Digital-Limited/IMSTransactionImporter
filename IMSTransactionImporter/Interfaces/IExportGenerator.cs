using IMSTransactionImporter.Classes;

namespace IMSTransactionImporter.Interfaces;

public interface IExportGenerator
{
    Task Generate(IMSExport export, string apiKey);
}
