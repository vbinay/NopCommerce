using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace Nop.Web.Framework.Tridion.HostNameMapping.Services
{
    /// <summary>
    /// Sevice to help retrieve host anme specific values  SODMYWAY-2956
    /// </summary>
    internal class SmwTridionHostNameMappingSerivce : ITridionHostNameMappingService
    {
        private readonly IHostNameMappingConfiguration _configuration;
        private const string CACHE_KEY = "AllTridionMappingsCached";
        public SmwTridionHostNameMappingSerivce(IHostNameMappingConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IPublicationInfo Mapping
        {
            //Consider changing to Get/Set methods for clarity
            get
            {
                //This is the ONLY place that this object should be retrieved from HttpContext
                IPublicationInfo mapping = HttpContext.Current.Items[Constants.TridionHostNameMappingKey] as IPublicationInfo;
                if (mapping == null)
                {
                    string hostName = HttpContext.Current.Request.Url.Host;
                
                    try
                    {
                        mapping = HostNameContext.Service.AllMappings[hostName];
                    }
                    catch (KeyNotFoundException)
                    {
                        throw new HttpException(400, "Host name '" + hostName + "' was not found. Check the Host Name Mapping configuration.");
                        //throw new HostNameNotFoundException(hostName);
                    }
                    HttpContext.Current.Items.Add(Constants.TridionHostNameMappingKey, mapping);

                }
                return mapping;
            }
            //set
            //{
            //    //This is the ONLY place that this object should be set in HttpContext
            //    HttpContext.Current.Items.Add(Constants.TridionHostNameMappingKey, value);
            //}
        }


        public IDictionary<string, IPublicationInfo> AllMappings
        {
            get
            {
                IDictionary<string, IPublicationInfo> mappings = HostNameContext.NoContextCacheManager.Get<IDictionary<string, IPublicationInfo>>(CACHE_KEY);
                if (mappings == null)
                {
                    XmlDocument doc = new XmlDocument();
                    //Load in the global mapping file
                    doc.Load(_configuration.MappingXmlPath);

                    List<SmwTridionHostNameMapping> mappingsFromXml = null; //set up variable to use
                    XmlSerializer serializer = new XmlSerializer(typeof(SmwMappingSerializableList)); //Anyway to avoid "hard coding" the implementation of the serialized object?
                    using (var reader = new XmlNodeReader(doc.DocumentElement))
                    {
                        //Deserialize the list of Publication Info from the XML in the file
                        mappingsFromXml = (SmwMappingSerializableList)serializer.Deserialize(reader);
                    }
                    //Load list of objects into a dictionary for easier lookup
                    mappings = mappingsFromXml.ToDictionary(x => x.HostName, x => (IPublicationInfo)x);
                    //Store the result in cache (or get an existing one if one was added since the first check)
                    mappings = HostNameContext.NoContextCacheManager.AddOrGetExisting<IDictionary<string, IPublicationInfo>>(CACHE_KEY, mappings, new FileInfo(_configuration.MappingXmlPath));
                }
                return mappings;
            }
        }
    }
}
