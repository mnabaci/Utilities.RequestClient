using System.ComponentModel;

namespace Utilities.RequestClient.Types
{
    /// <summary>
    /// Media Types
    /// </summary>
    public enum MediaTypes
    {
        /// <summary>
        /// None
        /// </summary>
        [Description("none")]
        None = 0,

        /// <summary>
        /// Json
        /// </summary>
        [Description("application/json")]
        Json = 1,

        /// <summary>
        /// Xml
        /// </summary>
        [Description("application/xml")]
        Xml = 2,

        /// <summary>
        /// Bson
        /// </summary>
        [Description("application/bson")]
        Bson = 3
    }
}
