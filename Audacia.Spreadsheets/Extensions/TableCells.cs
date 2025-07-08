namespace Audacia.Spreadsheets.Extensions
{
#pragma warning disable AV1745
    public static class TableCells
#pragma warning restore AV1745
    {
        public static string? GetValue(this TableCell cell)
        {
            return cell.Value?.ToString().Trim();
        }
    }
}