using System;

namespace Audacia.Spreadsheets.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class CellHeaderNameAttribute : Attribute
    {
        public string? Name { get; set; }
    }
}