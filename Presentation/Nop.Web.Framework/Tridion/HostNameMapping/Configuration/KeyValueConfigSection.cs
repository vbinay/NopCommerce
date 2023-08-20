using System;
using System.Configuration;

namespace Nop.Web.Framework.Tridion.HostNameMapping.Configuration
{
    /// <summary>
    /// Configuration Class for helping stroe and retrieve cahced values. JIRS 2956
    /// </summary>
    class KeyValueConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("", IsDefaultCollection = true, IsKey = false, IsRequired = true)]
        public KeyValueCollection KeyValuePairs
        {
            get
            {
                return base[""] as KeyValueCollection;
            }

            set
            {
                base[""] = value;
            }
        }
    }

    class KeyValueCollection : ConfigurationElementCollection
    {

        protected override ConfigurationElement CreateNewElement()
        {
            return new KeyValueElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((KeyValueElement)element).Key;
        }

        protected override string ElementName
        {
            get
            {
                return "pair";
            }
        }

        protected override bool IsElementName(string elementName)
        {
            return !String.IsNullOrEmpty(elementName) && elementName == "pair";
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }


        public KeyValueElement this[int index]
        {
            get
            {
                return base.BaseGet(index) as KeyValueElement;
            }
        }
    }

    class KeyValueElement : ConfigurationElement
    {
        [ConfigurationProperty("key", IsRequired = true, IsKey = true)]
        public string Key
        {
            get { return base["key"] as string; }
            set { base["key"] = value; }
        }

        [ConfigurationProperty("value", IsRequired = false, IsKey = false)]
        public string Value
        {
            get { return base["value"] as string; }
            set { base["value"] = value; }
        }
    }
}
