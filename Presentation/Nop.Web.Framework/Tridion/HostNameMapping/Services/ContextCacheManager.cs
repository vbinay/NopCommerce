using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Caching;

namespace Nop.Web.Framework.Tridion.HostNameMapping.Services
{
    //TODO: Could implement a base abstract class and add hook for modifying the cache key

    /// <summary>
    /// Implementation of Cache Manager that organizes cache values into regions based on the host name context of the request SODMYWAY-2956
    /// </summary>
    internal class ContextCacheManager : IContextCacheManager
    {
        private object cacheLock1 = new object();
        private object cacheLock2 = new object();
        private ITridionHostNameMappingService service;
        public ContextCacheManager(ITridionHostNameMappingService service)
        {
            this.service = service;
        }
        #region public methods
        public T Get<T>(string key) where T : class
        {
            key = GetRegionizedKey(key);
            object cached = MemoryCache.Default.Get(key);
            if (cached != null && cached is T)
                return cached as T;
            else return null;
        }

        public bool Contains(string key)
        {
            return MemoryCache.Default.Contains(GetRegionizedKey(key));
        }
        public T AddOrGetExisting<T>(string key, T cacheObject, bool dependentOnAppConfig = true, int cacheMinutes = 0) where T : class
        {
            key = GetRegionizedKey(key);
            lock (cacheLock2)
            {
                object cached = MemoryCache.Default.Get(key);
                if (cached != null && cached is T)
                    return cached as T;
                else
                {
                    if (dependentOnAppConfig)
                    {
                        AddOrReplaceWithDependency(key, cacheObject, new List<string>() { GetRegionizedKey(Constants.AppSettingsCacheKey) }); //Must regionize the ConfigCacheKey inside this class
                    }
                    else
                    {
                        cacheMinutes = cacheMinutes > 0 ? cacheMinutes : 100;
                        cacheObject = AddOrGetExisting<T>(key, cacheObject, cacheMinutes);
                    }

                    return cacheObject;
                }
            }
        }
        public void Remove(string key)
        {
            key = GetRegionizedKey(key);
            MemoryCache.Default.Remove(key);
        }
        /// <summary>
        /// Remove all keys that contain this general key from the current region
        /// </summary>
        /// <param name="key">The generalized key to be removed</param>
        public void RemoveSubset(string key)
        {
            string region = GetRegionizedKey(String.Empty); //get just the "region" part of the key by passing in empty string
            foreach (var item in MemoryCache.Default)
            {
                if (item.Key.Contains(region) && item.Key.Contains(key))
                    MemoryCache.Default.Remove(item.Key);
            }
        }
        public void Clear()
        {
            string region = GetRegionizedKey(String.Empty); //get just the "region" part of the key by passing in empty string
            foreach (var item in MemoryCache.Default)
            {
                if (item.Key.Contains(region))
                    MemoryCache.Default.Remove(item.Key);
            }
        }
        public T AddOrGetExisting<T>(string key, T cacheObject, FileInfo file) where T : class
        {
            //This method is internal so it can be used to cache each application configuration
            key = GetRegionizedKey(key);
            lock (cacheLock2)
            {
                object cached = MemoryCache.Default.Get(key);
                if (cached != null && cached is T)
                    return cached as T;
                else
                {
                    AddOrReplaceWithFile(key, cacheObject, file);
                    return cacheObject;
                }
            }
        }
        #endregion

        #region Private methods
        protected T AddOrGetExisting<T>(string key, T mapping, int cacheMinutes) where T : class
        {
            //key = GetRegionizedKey(key);
            lock (cacheLock1)
            {
                object cached = MemoryCache.Default.Get(key);
                if (cached != null && cached is T)
                    return cached as T;
                else
                {
                    MemoryCache.Default.Add(key, mapping, DateTimeOffset.Now.AddMinutes(cacheMinutes));
                    return mapping;
                }
            }
        }

        protected T AddOrGetExisting<T>(string key, T cacheObject, IEnumerable<string> dependentOn) where T : class
        {
            //key = GetRegionizedKey(key);
            lock (cacheLock2)
            {
                object cached = MemoryCache.Default.Get(key);
                if (cached != null && cached is T)
                    return cached as T;
                else
                {
                    AddOrReplaceWithDependency(key, cacheObject, dependentOn);
                    return cacheObject;
                }
            }
        }

        protected string GetRegionizedKey(string originalKey)
        {
            if (service.Mapping != null)
                return String.Format("{0};PublicationId={1}", originalKey, service.Mapping.PublicationId.ToString());
            else return originalKey;
        }

        protected void AddOrReplaceWithDependency(string key, object cacheObject, IEnumerable<string> dependentOn)
        {
            if (MemoryCache.Default.Contains(key)) MemoryCache.Default.Remove(key);
            CacheItemPolicy dependentPolicy = new CacheItemPolicy();
            ChangeMonitor monitor = MemoryCache.Default.CreateCacheEntryChangeMonitor(dependentOn);
            dependentPolicy.ChangeMonitors.Add(monitor);
            MemoryCache.Default.Add(key, cacheObject, dependentPolicy);
        }

        protected void AddOrReplaceWithFile(string key, object cacheObject, FileInfo file)
        {
            if (MemoryCache.Default.Contains(key)) MemoryCache.Default.Remove(key);
            CacheItemPolicy filePolicy = new CacheItemPolicy();
            FileChangeMonitor monitor = new HostFileChangeMonitor(new List<string>() { file.FullName });
            filePolicy.ChangeMonitors.Add(monitor);
            MemoryCache.Default.Add(key, cacheObject, filePolicy);
        }
        #endregion

    }

    /// <summary>
    /// Implementation of Cache Manager that does NOT organize cache values into regions based on current context - use with care
    /// </summary>
    internal class ContextlessCacheManager : IContextCacheManager
    {
        private object cacheLock1 = new object();
        private object cacheLock2 = new object();

        #region public methods
        public T Get<T>(string key) where T : class
        {
            object cached = MemoryCache.Default.Get(key);
            if (cached != null && cached is T)
                return cached as T;
            else return null;
        }

        public bool Contains(string key)
        {
            return MemoryCache.Default.Contains(key);
        }
        public T AddOrGetExisting<T>(string key, T cacheObject, bool dependentOnAppConfig = true, int cacheMinutes = 0) where T : class
        {
            lock (cacheLock2)
            {
                object cached = MemoryCache.Default.Get(key);
                if (cached != null && cached is T)
                    return cached as T;
                else
                {
                    if (dependentOnAppConfig)
                    {
                        AddOrReplaceWithDependency(key, cacheObject, new List<string>() { Constants.AppSettingsCacheKey }); //this will be dependent on a non-regionized app config key
                    }
                    else
                    {
                        cacheMinutes = cacheMinutes > 0 ? cacheMinutes : 100;
                        cacheObject = AddOrGetExisting<T>(key, cacheObject, cacheMinutes);
                    }

                    return cacheObject;
                }
            }
        }
        public void Remove(string key)
        {
            MemoryCache.Default.Remove(key);
        }
        public void RemoveSubset(string key)
        {
            throw new NotImplementedException(); //don't want anyone clearing the entire cache, only want ot allow this on individual regions
        }
        public void Clear()
        {
            throw new NotImplementedException(); //don't want anyone clearing the entire cache
        }
        public T AddOrGetExisting<T>(string key, T cacheObject, FileInfo file) where T : class
        {
            lock (cacheLock2)
            {
                object cached = MemoryCache.Default.Get(key);
                if (cached != null && cached is T)
                    return cached as T;
                else
                {
                    AddOrReplaceWithFile(key, cacheObject, file);
                    return cacheObject;
                }
            }
        }
        #endregion

        #region Private methods
        protected T AddOrGetExisting<T>(string key, T mapping, int cacheMinutes) where T : class
        {
            //key = GetRegionizedKey(key);
            lock (cacheLock1)
            {
                object cached = MemoryCache.Default.Get(key);
                if (cached != null && cached is T)
                    return cached as T;
                else
                {
                    MemoryCache.Default.Add(key, mapping, DateTimeOffset.Now.AddMinutes(cacheMinutes));
                    return mapping;
                }
            }
        }

        protected T AddOrGetExisting<T>(string key, T cacheObject, IEnumerable<string> dependentOn) where T : class
        {
            //key = GetRegionizedKey(key);
            lock (cacheLock2)
            {
                object cached = MemoryCache.Default.Get(key);
                if (cached != null && cached is T)
                    return cached as T;
                else
                {
                    AddOrReplaceWithDependency(key, cacheObject, dependentOn);
                    return cacheObject;
                }
            }
        }

        protected void AddOrReplaceWithDependency(string key, object cacheObject, IEnumerable<string> dependentOn)
        {
            if (MemoryCache.Default.Contains(key)) MemoryCache.Default.Remove(key);
            CacheItemPolicy dependentPolicy = new CacheItemPolicy();
            ChangeMonitor monitor = MemoryCache.Default.CreateCacheEntryChangeMonitor(dependentOn);
            dependentPolicy.ChangeMonitors.Add(monitor);
            MemoryCache.Default.Add(key, cacheObject, dependentPolicy);
        }

        protected void AddOrReplaceWithFile(string key, object cacheObject, FileInfo file)
        {
            if (MemoryCache.Default.Contains(key)) MemoryCache.Default.Remove(key);
            CacheItemPolicy filePolicy = new CacheItemPolicy();
            FileChangeMonitor monitor = new HostFileChangeMonitor(new List<string>() { file.FullName });
            filePolicy.ChangeMonitors.Add(monitor);
            MemoryCache.Default.Add(key, cacheObject, filePolicy);
        }
        #endregion

    }
}
