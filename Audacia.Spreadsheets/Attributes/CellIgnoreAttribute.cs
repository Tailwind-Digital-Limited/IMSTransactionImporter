using System;

namespace Audacia.Spreadsheets.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class CellIgnoreAttribute : Attribute
    {
    }
}