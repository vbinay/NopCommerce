using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;

namespace Nop.Web.Framework.Tridion.HostNameMapping
{
    /// <summary>
    /// Basic Tridion Publication information SODMYWAY-2956
    /// </summary>
    public interface IPublicationInfo
    {
        //Added public Sets to allow for serialization
        string HostName { get; set; } //This is obviously redundant
        int PublicationId { get; set; } //realistically don't even need this -- it will be in the individual configuration
        /// <summary>
        /// The publication path from the Tridion publication
        /// </summary>
        string PublicationFolder { get; set; }
        string MultimediaFolder { get; set; }
    }

    
    public interface IHealthCheckConfiguration
    {
        int PortNumber { get; }
        string HostName { get; }
    }
    public interface ITridionHostNameMappingService
    {
        /// <summary>
        /// The Publication Mapping of the current context
        /// </summary>
        IPublicationInfo Mapping { get; } //Possibly turn this into an actual Get/Set method to make it more clear
        /// <summary>
        /// Dictionary of all Host Name to Publication mappings
        /// Key: Host Name
        /// Value: Publication Info
        /// </summary>
        IDictionary<string, IPublicationInfo> AllMappings { get; }
    }

    /// <summary>
    /// Configuration values for the based on the current application context
    /// </summary>
    public interface IContextConfiguration
    {
        int PublicationId { get; }
        string PublicationPath { get; }
        string PublicationFolder { get; }
        NameValueCollection AppSettings { get; }
        string CssPath { get; }
        string JsPath { get; }
        string MultimediaPath { get; }
        //string OutboundEmailPath { get; }
        //string EmailPagesPath { get; }
        //string SiteMapPath { get; }
        string SystemFolderPath { get; }
        //string ViewsPath { get; }
    }

    public interface IHostNameMappingConfiguration : IContextConfiguration
    {
        string MappingXmlPath { get; }
        string AppSettingFilePath { get; }
    }
    
    public interface IContextCacheManager
    {
        T Get<T>(string key) where T : class;
        bool Contains(string key);
        /// <summary>
        /// Adds or gets an object from the Cache. Can be dependent on the configuration of the current context
        /// </summary>
        /// <typeparam name="T">Type of object to add or get</typeparam>
        /// <param name="key">Key of the cached item</param>
        /// <param name="cacheObject">The object to cache if none exists</param>
        /// <param name="dependentOnContextConfig">True to store this value in the cache with a dependency on the current context configuration</param>
        /// <param name="cacheMinutes">Number of minutes for the absolute expiration. Only used if it is a positive value</param>
        /// <returns></returns>
        T AddOrGetExisting<T>(string key, T cacheObject, bool dependentOnContextConfig = true, int cacheMinutes = 0) where T : class;
        void Remove(string key);
        void RemoveSubset(string key);
        void Clear();
        T AddOrGetExisting<T>(string key, T cacheObject, FileInfo file) where T : class;
    }
    internal static class Constants
    {
        internal const string TridionHostNameMappingKey = "TridionHostNameMapping";
        internal const string AppSettingsCacheKey = "AppSettingsCacheKey";
        internal const string SiteStyleCacheKey = "SiteStyleCacheKey";
        internal const string ParentPublicationsKey = "ParentPublicationsKey";
    }
}

