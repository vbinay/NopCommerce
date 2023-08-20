using System;

namespace Nop.Web.Framework.Tridion.Profile
{
    /// <summary>
    /// SMW Profile Attribute CLass SODMYWAY-2956
    /// </summary>
    [Serializable]
    public class SmwProfileAttribute
    {
        public String AttributeTitle { get; set; }
        public String AttributeId { get; set; }
        public String CategoryId { get; set; }
    }
}
