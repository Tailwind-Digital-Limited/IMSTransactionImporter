using System.Collections.Generic;
using Audacia.Spreadsheets.Extensions;

namespace Audacia.Spreadsheets.Validation
{
    public class ValidationResult
    {
        public ValidationResult(string memberName, string message)
        {
            MemberName = memberName;
            DisplayName = memberName;
            Errors.Add(message);
        }
        
        public ValidationResult(string memberName, string displayName, string message)
        {
            MemberName = memberName;
            DisplayName = displayName;
            Errors.Add(message);
        }

        public ValidationResult(string memberName, IEnumerable<string> messages)
        {
            MemberName = memberName;
            DisplayName = memberName;
            Errors.AddRange(messages);
        }
        
        public ValidationResult(string memberName, string displayName, IEnumerable<string> messages)
        {
            MemberName = memberName;
            DisplayName = displayName;
            Errors.AddRange(messages);
        }

        public string DisplayName { get; }
        
        public string MemberName { get; }
        
        public ICollection<string> Errors { get; } = new HashSet<string>();
    }
}