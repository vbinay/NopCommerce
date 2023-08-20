using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using Nop.Web.Framework.Tridion.HostNameMapping.Configuration;

namespace Nop.Web.Framework.Tridion.HostNameMapping.Services
{
    /// <summary>
    /// A single point of access for all Host Name Context objects SODMYWAY-2956
    /// </summary>
    public static class HostNameContext
    {
        //Singletons - could use Dependency Injection instead of using "new" directive
        private static readonly IHostNameMappingConfiguration GlobalConfig = new SmwTridionMappingConfiguration();
        private static readonly string _tridionSiteHostCacheKey = "TridionSiteHostCacheKey";

        static HostNameContext()
        {
            Service = new SmwTridionHostNameMappingSerivce(GlobalConfig);
            CacheManager = new ContextCacheManager(Service);
            NoContextCacheManager = new ContextlessCacheManager();
        }

        /// <summary>
        /// The Tridion to Host Name Mapping Service
        /// </summary>
        internal static ITridionHostNameMappingService Service { get; private set; }

        /// <summary>
        /// The configuration specific to this host name
        /// </summary>
        public static IContextConfiguration ConfigurationManager
        {
            get
            {
                //HostNameMapping.Service.GetMapping(HttpContext.Current);
                IContextConfiguration result = CacheManager.Get<IContextConfiguration>(Constants.AppSettingsCacheKey);
                if (result == null)
                {
                    //Make this context config dependent on the app settings file
                    if (Service.Mapping != null)
                    {
                        string configPath = GlobalConfig.PublicationPath + Service.Mapping.PublicationFolder +
                                            GlobalConfig.AppSettingFilePath;
                        result = CacheManager.AddOrGetExisting<IContextConfiguration>(Constants.AppSettingsCacheKey,
                            new SmwContextConfiguration(GlobalConfig, Service.Mapping), new FileInfo(configPath));
                    }
                    else
                        result = CacheManager.AddOrGetExisting<IContextConfiguration>(Constants.AppSettingsCacheKey,
                            new SmwContextConfiguration(), false, 1440);
                }
                return result;
            }
        }

        /// <summary>
        /// A Cache Manager that organizes cached objects based on the context of the current request
        /// </summary>
        public static IContextCacheManager CacheManager { get; private set; }

        //no need to lazy load because constructor does nothing

        /// <summary>
        /// A Cache Manager that does not use the current context to organize the cache
        /// </summary>
        internal static IContextCacheManager NoContextCacheManager { get; private set; }

        //no need to lazy load because constructor does nothing

        /// <summary>
        /// Gets a Global (not context specific) app config setting 
        /// </summary>
        /// <param name="name">App Setting key</param>
        /// <returns></returns>
        internal static String GetGlobalAppSetting(String name)
        {
            String result = String.Empty;
            try
            {
                result = WebConfigurationManager.AppSettings[name];
                if (result == null) result = String.Empty;
            }
            catch (Exception)
            {
            }
            return result;
        }

        public static string TridionSiteHost
        {
            get
            {
                //HostNameMapping.Service.GetMapping(HttpContext.Current);
                string output = string.Empty;
                string result = CacheManager.Get<string>(_tridionSiteHostCacheKey);
                if (result == null)
                {
                    var pubMappings =
                        Service.AllMappings.Where(
                            x =>
                                x.Value.PublicationId == ConfigurationManager.PublicationId &&
                                !x.Value.HostName.Contains(".preview.") &&
                                x.Value.HostName != HttpContext.Current.Request.Url.Host).ToList();
                    if (pubMappings.Any())
                    {
                        string primary = GetGlobalAppSetting("TridionSiteDefaultHostname");
                        string secondary = GetGlobalAppSetting("TridionSiteSecondaryHostname");
                        Dictionary<int, string> rankedHostnames = new Dictionary<int, string>();

                        foreach (var mapping in pubMappings)
                        {
                            int rank = 0;
                            string host = mapping.Value.HostName;
                            if (host.Contains(secondary))
                            {
                                rank += 1;
                            }
                            if (host.Contains(primary))
                            {
                                rank += 2;
                            }
                            if (!host.Contains(primary) && !host.Contains(secondary))
                            {
                                rank += 3;
                            }
                            rankedHostnames.Add(rank, host);
                        }

                        rankedHostnames = rankedHostnames.OrderByDescending(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
                        var foundHost = rankedHostnames.First().Value;
                        output = CacheManager.AddOrGetExisting<string>(_tridionSiteHostCacheKey, foundHost);
                    }
                    else
                    {
                        output = CacheManager.AddOrGetExisting<string>(_tridionSiteHostCacheKey, string.Empty);
                    }
                }
                return output;
            }
        }
    }
}
