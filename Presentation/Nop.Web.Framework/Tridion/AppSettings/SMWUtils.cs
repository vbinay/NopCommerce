using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc.Html;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Nop.Web.Framework.Tridion.HostNameMapping.Services;
using Nop.Web.Framework.Tridion.Profile;

namespace Nop.Web.Framework.Tridion.AppSettings
{
    /// <summary>
    /// This class gives us the ability to load Web.config values or specific site configurations from the AppSettings.config SODMYWAY-2956
    /// </summary>
    public sealed class SmwUtils
    {

        #region Constants
        //private static string DefaultMobilePrefix = "m-";
        private const string CacheKey = "SMWUtilsCachedKey"; 
        #endregion

        #region Constructors
        public static SmwUtils Instance
        {
            //This can no longer be a true singleton -- must use the cache since there are multiple configurations
            get
            {
                SmwUtils instance = HostNameContext.CacheManager.Get<SmwUtils>(CacheKey) ??
                                     HostNameContext.CacheManager.AddOrGetExisting<SmwUtils>(CacheKey, new SmwUtils());
                return instance;
            }
        }

        private SmwUtils()
        {
            PublicationId = HostNameContext.ConfigurationManager.PublicationId;
            EcommerceSiteKey = GetConfigValue("EcommerceSiteKey");
            MainSiteHost = HostNameContext.TridionSiteHost;
            TridionLoginPagePath = GetGlobalConfigValue("TridionLoginPagePath");
            TridionLogoutPagePath = GetGlobalConfigValue("TridionLogoutPagePath");
            ExcludeLoginLogoutReturnList = GetGlobalConfigValue("ExcludeLoginLogoutReturn").Split(new string[]{" , ", " ,", ", ", ","} , StringSplitOptions.None).ToList();
            TridionHeaderTemplateId = Convert.ToInt32(GetGlobalConfigValue("TridionHeaderTemplateId"));
            TridionHeaderComponentId = Convert.ToInt32(GetConfigValue("HeaderID"));
        } 
        #endregion

        #region Public Methods
        /// <summary>
        /// Get the Tridion profile information stored in the shared cookie
        /// </summary>
        /// <returns>Found profile or null</returns>
        public static SmwProfile GetProfileFromTempDataCookie()
        {
            HttpCookie cookie = HttpContext.Current.Request.Cookies["_smw_session_global"];
            IDictionary<string, object> tempData = new Dictionary<string, object>();

            if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
            {
                tempData = JsonConvert.DeserializeObject<Dictionary<string, object>>(cookie.Value, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            }

            if (tempData != null && tempData.ContainsKey("profile"))
            {
                var jProfile = tempData["profile"] as JObject;
                if (jProfile != null)
                {
                    var profile = jProfile.ToObject<SmwProfile>();
                    if (profile != null && profile.PublicationId == SmwUtils.Instance.PublicationId)
                    {
                        return profile;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// check if the give profile is registered or not
        /// </summary>
        /// <param name="profile">Profile to check</param>
        /// <returns>true for registered profile, false for not registered profile</returns>
        public static bool IsRegisteredUser(SmwProfile profile)
        {
            if (profile != null)
            {
                if (!String.IsNullOrEmpty(profile.Email) &&
                    !String.IsNullOrEmpty(profile.Firstname) &&
                    !String.IsNullOrEmpty(profile.Lastname) &&
                    !String.IsNullOrEmpty(profile.Id) &&
                    profile.AudienceTypeId != 0)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region Properties

        public int PublicationId { get; private set; }
        public string EcommerceSiteKey { get; private set; }
        public string MainSiteHost { get; private set; }
        public string TridionLoginPagePath { get; private set; }
        public string TridionLogoutPagePath { get; private set; }
        public List<string> ExcludeLoginLogoutReturnList { get; private set; }
        public int TridionHeaderTemplateId { get; private set; }
        public int TridionHeaderComponentId { get; private set; }

        #endregion

        #region Protected Methods

        /// <summary>
        /// returns configuration value by looking forst int he App Settings for the given site and then in the base Web.config for the application
        /// </summary>
        /// <param name="name">Parameter your looking for</param>
        /// <returns>Found Value or empty string</returns>
        private String GetConfigValue(String name)
        {
            String result = String.Empty;
            try
            {
                result = HostNameContext.ConfigurationManager.AppSettings[name];
                if (String.IsNullOrEmpty(result))
                {
                    result = GetGlobalConfigValue(name); //fall back on global value
                }
            }
            catch (Exception)
            {
                // ignored
            }
            return result;
        }

        /// <summary>
        /// Gets a value from the global web.config
        /// </summary>
        /// <param name="name">Parameter your looking for</param>
        /// <returns>Found Value or empty string</returns>
        private String GetGlobalConfigValue(String name)
        {
            String result = String.Empty;
            try
            {
                result = WebConfigurationManager.AppSettings[name];
            }
            catch (Exception)
            {
                // ignored
            }
            return result;
        }



        #endregion
    }
}
