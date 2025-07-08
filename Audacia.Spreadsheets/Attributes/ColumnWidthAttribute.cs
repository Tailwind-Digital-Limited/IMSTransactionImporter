using System;

namespace Audacia.Spreadsheets.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class ColumnWidthAttribute : Attribute
    {
        public ColumnWidthAttribute() { }

        public ColumnWidthAttribute(int width) => Width = width;
        
        public int Width { get; }
    }
}