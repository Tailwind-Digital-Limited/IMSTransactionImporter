using System;

namespace Audacia.Spreadsheets.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class CellTextColourAttribute : Attribute
    {
        public CellTextColourAttribute() { }
        
        public CellTextColourAttribute(string colour) => Colour = colour;
                
        public string? ReferenceField { get; set; }
        
        public string? Colour { get; }
    }
}
