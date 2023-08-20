using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Xsl;
using System.IO;
using Nop.Web.Framework.Tridion.HostNameMapping.Services;
using Nop.Web.Framework.Tridion.AppSettings;

namespace Nop.Web.Tridion
{
    public enum NavigationType
    {
        Top/*, Side, Footer, Breadcrumb, Mobile*/
    }

    public interface INavigationRenderer
    {
        MvcHtmlString RenderNavigation(HtmlHelper helper, NavigationType navigationType);
    }
    
    public static class NavigationHelper
    {
        private const string _navigationXmlFile = "navigation.xml";
        private static Dictionary<NavigationType, string> _xslPath = new Dictionary<NavigationType, string>();
        private const string CACHE_KEY = "NavigationRendererCacheKey";
        private static INavigationRenderer Renderer
        {
            get
            {
                INavigationRenderer _instance = HostNameContext.CacheManager.Get<INavigationRenderer>(CACHE_KEY);
                if (_instance == null) //nothing was found
                {
                    //Adds an entry to the cache which is dependent on the Cached Context Configuration
                    _instance = HostNameContext.CacheManager.AddOrGetExisting<INavigationRenderer>(CACHE_KEY, new NavigationRenderer(_navigationXmlFile, _xslPath));
                }
                return _instance;
            }
        }
        public static MvcHtmlString RenderNavigation(this HtmlHelper helper, NavigationType navigationType)
        {
            return Renderer.RenderNavigation(helper, navigationType);
        }
        static NavigationHelper()
        {
            _xslPath.Add(NavigationType.Top, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Tridion\TridionNav.xslt"));
            //_xslPath.Add(NavigationType.Side, "RHSNavigation.xslt");
            //_xslPath.Add(NavigationType.Breadcrumb, "BreadCrumb.xslt");
            //_xslPath.Add(NavigationType.Footer, "FooterNav.xslt");
            //_xslPath.Add(NavigationType.Mobile, "MobileNav.xslt");
        }

        private class NavigationRenderer : INavigationRenderer
        {
            private readonly string navigationXmlFile;
            private readonly IDictionary<NavigationType, string> xslFiles;
            internal NavigationRenderer(string navigationXmlFile, IDictionary<NavigationType, string> xslFiles)
            {
                this.navigationXmlFile = GetPath(navigationXmlFile);
                this.xslFiles = xslFiles.ToDictionary(x => x.Key, x => x.Value);
            }
            private static string GetPath(string fileName)
            {
                return HostNameContext.ConfigurationManager.SystemFolderPath + fileName;
            }
            public MvcHtmlString RenderNavigation(HtmlHelper helper, NavigationType navigationType)
            {
                MvcHtmlString result = MvcHtmlString.Empty;
                string cacheKey = string.Format("CachedMenu-{0}", navigationType.ToString());
                result = HostNameContext.CacheManager.Get<MvcHtmlString>(cacheKey);
                if (result == null)
                {
                    XmlDocument document = new XmlDocument();
                    document.LoadXml(ReadFileContents(navigationXmlFile));

                    XslCompiledTransform xslt = new XslCompiledTransform();
                    using (StringReader reader = new StringReader(ReadFileContents(xslFiles[navigationType])))
                    {
                        xslt.Load(XmlReader.Create(reader));
                    }
                    using (StringWriter writer = new StringWriter())
                    {
                        XsltArgumentList args = new XsltArgumentList();
                        args.AddParam("TridionHost", "", "http://" + SmwUtils.Instance.MainSiteHost);


                        xslt.Transform(document, args, writer);
                        result = MvcHtmlString.Create(writer.ToString());

                        result = HostNameContext.CacheManager.AddOrGetExisting<MvcHtmlString>(cacheKey, result, new FileInfo(navigationXmlFile)); //Tie the nav to the app settings file instead of timed expiration 
                        //, false, SMWUtils.Instance.NavigationCacheMinutes);
                    }
                }

                return result;
            }

            internal static string ReadFileContents(string path, bool forceRefresh = false)
            {
                HttpContext current = HttpContext.Current;
                string cacheItemName = path;
                string result = null;
                if (!forceRefresh && !string.IsNullOrWhiteSpace(cacheItemName))
                {
                    //object cachedObject = current.Cache.Get(cacheItemName) ?? string.Empty;
                    result = HostNameContext.CacheManager.Get<string>(cacheItemName) ?? String.Empty;
                }
                if (string.IsNullOrWhiteSpace(result))
                {
                    //path = current.Server.MapPath(path);
                    if (File.Exists(path))
                    {
                        StreamReader reader = new StreamReader(path);
                        result = reader.ReadToEnd();
                        //current.Cache.Add(cacheItemName, CachedData, new CacheDependency(path), Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.High, null);
                        result = HostNameContext.CacheManager.AddOrGetExisting<string>(cacheItemName, result, new FileInfo(path));
                    }
                }
                return result;
            }
        }
    }
}