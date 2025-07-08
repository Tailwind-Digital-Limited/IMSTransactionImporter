using System;

namespace Audacia.Spreadsheets.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class CellBackgroundColourAttribute : Attribute
    {
        public CellBackgroundColourAttribute() { }

        public CellBackgroundColourAttribute(string colour) => Colour = colour;

        public string? ReferenceField { get; set; }

        public string? Colour { get; }
    }
}
