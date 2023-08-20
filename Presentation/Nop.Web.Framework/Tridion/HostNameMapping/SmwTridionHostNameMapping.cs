using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Nop.Web.Framework.Tridion.HostNameMapping
{
    /// <summary>
    /// Serializable list of host anme mappings SODMYWAY-2956
    /// </summary>
    [Serializable]
    public class SmwMappingSerializableList : List<SmwTridionHostNameMapping>
    {
        //Class for using XmlSerializer from generated assembly using sgen.exe
    }

    /// <summary>
    /// Serializable Tridion Host Name Mapping Info SODMYWAY-2956
    /// </summary>
    [Serializable]
    public class SmwTridionHostNameMapping : IPublicationInfo
    {
        public SmwTridionHostNameMapping()
        {
            //Parameterless constructor for serialization
        }
        public SmwTridionHostNameMapping(string hostName, int publicationId, string folderPath, string multimediaPath)
        {
            HostName = hostName;
            PublicationId = publicationId;
            PublicationFolder = folderPath;
            MultimediaFolder = multimediaPath;
        }

        [XmlAttribute]
        public string HostName
        {
            get;
            set;
        }
        [XmlAttribute]
        public int PublicationId
        {
            get;
            set;
        }
        [XmlAttribute]
        public string PublicationFolder
        {
            get;
            set;
        }

        [XmlAttribute]
        public string MultimediaFolder
        {
            get;
            set;
        }
    }
}
