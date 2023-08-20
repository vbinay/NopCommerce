using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using Nop.Web.Framework.Tridion.AppSettings;

namespace Nop.Web.Tridion
{
    public class TridionComponentFactory
    {
        public TridionHeader GetHeaderComponent(int publicationId)
        {
            string queryString =
                "SELECT TOP 1 [CONTENT]" +
                "FROM [Tridion_Broker_Live].[dbo].[COMPONENT_PRESENTATIONS]" + 
                "WHERE PUBLICATION_ID = @publicationId and COMPONENT_ID = @componentId and TEMPLATE_ID = @templateId";

            // Create and open the connection in a using block. This
            // ensures that all resources will be closed and disposed
            // when the code exits.
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["TridionBroker"].ToString()))
            {
                // Create the Command and Parameter objects.
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@publicationId", publicationId);
                command.Parameters.AddWithValue("@componentId", SmwUtils.Instance.TridionHeaderComponentId);
                command.Parameters.AddWithValue("@templateId", SmwUtils.Instance.TridionHeaderTemplateId);

                // Open the connection in a try/catch block. 
                // Create and execute the DataReader, writing the result
                // set to the console window.
                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    List<string> results = new List<string>();
                    while (reader.Read())
                    {
                        results.Add(reader.GetString(0));
                    }
                    reader.Close();
                    string result = results.First();
                    string iconPath = string.Empty;
                    string tagLine = string.Empty;

                    //Parse xml String to XML doc
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(result);

                    XmlNodeList items = doc.SelectNodes("/Component/Fields/item");
                    if (items != null)
                    {
                        //iterate through each item
                        foreach (XmlNode item in items)
                        {
                            XmlNode itemName = item.SelectSingleNode("./key/string");

                            //if logo is found get the path
                            if (itemName != null && itemName.LastChild.Value == "Logo")
                            {
                                XmlNodeList innerItems = item.SelectNodes("./value/Field/EmbeddedValues/FieldSet/item");
                                if (innerItems != null)
                                {
                                    foreach (XmlNode innerItem in innerItems)
                                    {
                                        XmlNode innerItemName = innerItem.SelectSingleNode("./key/string");
                                        if (innerItemName != null && innerItemName.LastChild.Value == "BGImage")
                                        {
                                            XmlNode innerItemPath = innerItem.SelectSingleNode("./value/Field/LinkedComponentValues/Component/Multimedia/Url");
                                            if (innerItemPath != null)
                                                iconPath = innerItemPath.LastChild.Value;
                                        }
                                    }
                                }
                            }
                            //if tagline is found return the value
                            else if (itemName != null && itemName.LastChild.Value == "Tagline")
                            {
                                XmlNode taglineNode = item.SelectSingleNode("./value/Field/Values/string");
                                if (taglineNode != null)
                                    tagLine = taglineNode.LastChild.Value;
                            }
                        }
                    }

                    return new TridionHeader(iconPath, tagLine);
                }
                catch (Exception)
                {
                    // ignored
                }
                return null;
            }
        }
    }

    /// <summary>
    /// Class to return the path and tagline to be displayed in the header
    /// </summary>
    public class TridionHeader
    {
        #region Const

        public TridionHeader(string iconPath, string tagLine)
        {
            IconPath = iconPath;
            TagLine = tagLine;
        } 

        #endregion

        #region Properties

        public string IconPath { get; private set; }
        public string TagLine { get; private set; } 

        #endregion
    }
}