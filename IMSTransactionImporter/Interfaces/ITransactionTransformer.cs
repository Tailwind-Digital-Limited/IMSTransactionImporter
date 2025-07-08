using IMSTransactionImporter.Classes;

namespace IMSTransactionImporter.Interfaces;

public interface ITransactionTransformer
{
    IMSTransactionImport Transform(string fileContents);
}
