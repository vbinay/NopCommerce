using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace FeedCSVToDB
{
    public class FileFeedCls
    {
        public void TransferCSVToDB(string Filepath)
        {
            FileInfo file = new FileInfo(Filepath);
            DataTable datatbl = new DataTable("FeedData");
            try
            {
               OleDbConnection con =
                        new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + file.DirectoryName + ";Extended Properties ='text;HDR=No;FMT = CSVDelimited'");
               OleDbCommand cmd = new OleDbCommand(string.Format("SELECT * FROM [{0}]", file.Name), con);
               con.Open();
                // Using a DataTable to process the data
               OleDbDataAdapter adp = new OleDbDataAdapter(cmd);                        
               adp.Fill(datatbl);
               con.Close();
                                               
                if(datatbl.Rows.Count> 0)
                {
                    string consString = ConfigurationSettings.AppSettings["FeedData"].ToString();
                    SqlConnection scon = new SqlConnection(consString);
                    SqlCommand scmd = new SqlCommand("Trunc_InsertBBCust", scon);
                    scmd.CommandType = CommandType.StoredProcedure; 
                    scmd.Parameters.AddWithValue("@TableVar", datatbl);
                    scon.Open();
                    scmd.ExecuteNonQuery();
                    scon.Close();
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
    }
}
