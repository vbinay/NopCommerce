using System;
using System.Collections.Specialized;
using System.Linq;
using System.Xml.Linq;

namespace Nop.Web.Framework.Tridion.HostNameMapping.Configuration
{
    /// <summary>
    /// Help load the values for the specific Tridion Context SODMYWAY-2956
    /// </summary>
    internal class SmwContextConfiguration : IContextConfiguration
    {
        internal SmwContextConfiguration(IHostNameMappingConfiguration config, IPublicationInfo mapping)
        {
            PublicationPath = config.PublicationPath + mapping.PublicationFolder;
            PublicationFolder = mapping.PublicationFolder;
            CssPath = PublicationPath + config.CssPath;
            JsPath = PublicationPath + config.JsPath;
            MultimediaPath = config.PublicationPath + mapping.MultimediaFolder + "\\"; //Tridion doesn't add trailing slash by default
            SystemFolderPath = PublicationPath + config.SystemFolderPath;
            //TODO: Move site-specific views into /Views/TridionSites folder and change Web.config value to reflect this. 
            //When that is done, switch this to config.ViewsPath + mapping.PublicationFolder i.e. /Views/TridionSites/albany/
            PublicationId = mapping.PublicationId;
            LoadAppSettings(config, mapping);
        }
        internal SmwContextConfiguration()
        {
            AppSettings = new NameValueCollection();
        }

       
        private void LoadAppSettings(IHostNameMappingConfiguration config, IPublicationInfo mapping)
        {
            //Don't worry about singleton-ing or Caching here -- should be taken care of by a static class
            if (AppSettings == null)
            {
                //Load config from file
                string configPath = PublicationPath + config.AppSettingFilePath;
                XDocument configDoc;
                try
                {
                    configDoc = XDocument.Load(configPath);
                }
                catch (Exception)
                {
                    throw new AppSettingsNotFoundException(mapping.HostName);
                }
                var appSettingElements =
                    configDoc.Element("configuration").Element("appSettings").Elements("add");
                //Using ToDictionary results in an Exception when duplicate Keys exist, as they often do due to user error
                //.ToDictionary(
                //a => a.Attribute("key").Value,
                //a => a.Attribute("value").Value); //alternatively could just get configDoc.Descendants("add")
                AppSettings = new NameValueCollection();
                foreach (var element in appSettingElements)
                {
                    string key = element.Attribute("key").Value;
                    if (!AppSettings.AllKeys.Contains(key)) //Prevent duplicates from being added -- only first one will get added
                    {
                        AppSettings.Add(key, element.Attribute("value").Value);
                    }
                }
            }
        }

        public NameValueCollection AppSettings
        {
            //Cached by static caller of this class
            get;
            private set;
        }

        public string PublicationPath
        {
            get;
            private set;
        }

        public int PublicationId
        {
            get;
            private set;
        }

        public string PublicationFolder
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
            get;
            private set;
        }

        public string SystemFolderPath
        {
            get;
            private set;
        }
    }
}
