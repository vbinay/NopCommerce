using System;
using System.Collections.Specialized;
using Nop.Web.Framework.Tridion.HostNameMapping.Services;

namespace Nop.Web.Framework.Tridion.HostNameMapping.Configuration
{
    /// <summary>
    /// Confiuration for the Tridion Host Name Mapping to retieve neeping site specific information SODMYWAY-2956
    /// </summary>
    internal class SmwTridionMappingConfiguration : IHostNameMappingConfiguration
    {
        internal SmwTridionMappingConfiguration()
        {
            MappingXmlPath = HostNameContext.GetGlobalAppSetting("TridionHostNameMappingXmlPath");
            AppSettingFilePath = HostNameContext.GetGlobalAppSetting("TridionAppSettingsRelativePath");
            PublicationPath = HostNameContext.GetGlobalAppSetting("TridionPublishFolderPath");
            CssPath = HostNameContext.GetGlobalAppSetting("TridionRelativeCssPath");
            JsPath = HostNameContext.GetGlobalAppSetting("TridionRelativeJavascriptPath");
            SystemFolderPath = HostNameContext.GetGlobalAppSetting("TridionRelativeSystemFolderPath");
        }

        public string MappingXmlPath
        {
            get;
            private set;
        }


        public string AppSettingFilePath
        {
            get;
            private set;
        }

        public string PublicationPath
        {
            get;
            private set;
        }

        public string CssPath
        {
            get;
            private set;
        }

        public string JsPath
        {
            get;
            private set;
        }

        public string MultimediaPath
        {
            get { throw new NotImplementedException(); }
        }

        public int PublicationId
        {
            get { throw new NotImplementedException(); }
        }


        public NameValueCollection AppSettings
        {
            get { throw new NotImplementedException(); }
        }


        public string PublicationFolder
        {
            get { throw new NotImplementedException(); }
        }

        public string SystemFolderPath
        {
            get;
            private set;
        }
    }
}
