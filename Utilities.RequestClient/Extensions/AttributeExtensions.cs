using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Utilities.RequestClient.Extensions
{
    /// <summary>
    /// Attribute Extensions
    /// </summary>
    public static class AttributeExtensions
    {
        /// <summary>
        /// Get description from enum's attribute
        /// </summary>
        /// <param name="value">Enum value</param>
        /// <returns>Description attribute's value</returns>
        public static string GetDescription(this Enum value)
        {
            try
            {
                return value.GetType()
                    .GetMember(value.ToString())
                    .First()
                    .GetCustomAttribute<DescriptionAttribute>()
                    .Description;
            }
            catch
            {
                return value.ToString();
            }
        }
    }
}
