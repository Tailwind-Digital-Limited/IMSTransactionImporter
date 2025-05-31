using IMSTransactionImporter.InternalClasses;

namespace IMSTransactionImporter.Interfaces;

public interface ITransactionTransformer
{
    IMSTransactionImport Transform(string fileContents);
}