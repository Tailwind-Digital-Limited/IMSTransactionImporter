using System;
using System.Text.RegularExpressions;

namespace Audacia.Spreadsheets
{
    public class NamedRangeModel
    {
        private static readonly Regex _regex = new Regex("^[a-zA-Z0-9_]*$");
        
        private string _name = string.Empty;

        /// <summary>
        /// Gets or sets the name for Named Range. Must only contain alpha numerical characters
        /// </summary>
        public string Name
        {
            get { return _name; }

            set
            {
                if (_regex.IsMatch(value))
                {
                    _name = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(Name), "Named Range names must be alphanumeric");
                }
            }
        }

        public string SheetName { get; set; } = null!;
        
        public string StartCell { get; set; } = null!;
        
        public string EndCell { get; set; } = null!;

        // ReSharper disable once UnusedMember.Global
        public static void Write()
        {
        }
    }
}