using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Audacia.Spreadsheets.Extensions
{
    public static class MemberInfoExtensions
    {
        /// <summary>
        /// Get <see cref="DisplayNameAttribute"/> from a property, event or public void method.
        /// </summary>
        /// <param name="memberInfo">Member metadata.</param>
        /// <returns>The display name.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="memberInfo"/> is null.</exception>
        public static string? GetDataAnnotationDisplayName(this MemberInfo memberInfo)
        {
            if (memberInfo == null)
            {
                throw new ArgumentNullException(nameof(memberInfo));
            }

            var displayNameAttributes = (DisplayNameAttribute[])memberInfo.GetCustomAttributes(typeof(DisplayNameAttribute), false);

            if (displayNameAttributes.Any())
            {
                return displayNameAttributes[0].DisplayName;
            }

            var descriptionAttributes = (DescriptionAttribute[])memberInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (descriptionAttributes.Any())
            {
                return descriptionAttributes[0].Description;
            }

            return memberInfo.Name;
        }
    }
}