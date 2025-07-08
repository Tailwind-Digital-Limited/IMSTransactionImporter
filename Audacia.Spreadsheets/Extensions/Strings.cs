using System.Text.RegularExpressions;

namespace Audacia.Spreadsheets.Extensions
{
#pragma warning disable AV1745
    internal static class Strings
#pragma warning restore AV1745
    {
        public static string PascalToTitleCase(this string input)
        {
            return Regex.Replace(input, "([a-z](?=[A-Z]|[0-9])|[A-Z](?=[A-Z][a-z]|[0-9])|[0-9](?=[^0-9]))", "$1 ");
        }
    }
}