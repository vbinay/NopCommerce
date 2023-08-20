using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeedCSVToDB;
using System.Configuration;

namespace FeedCSVToDB
{
    class Program
    {
        static void Main(string[] args)
        {
            FileFeedCls feedobj = new FileFeedCls();
            string CSVpath = ConfigurationSettings.AppSettings["CSVPath"].ToString();
            feedobj.TransferCSVToDB(CSVpath);
        }
    }
}
