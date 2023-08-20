using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPFileFeed
{
    public static class SAPLogger
    {
        public static void WriteLogs(string LogMessage)
        {
            var path = "";
            string FullPath = string.Empty;

            if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings["LogFolder"]))
            {
                path = ConfigurationManager.AppSettings["LogFolder"];
            }
            if (!Directory.Exists(path))
            {

                Directory.CreateDirectory(path);
                FullPath = Path.Combine(path, "Log.txt");
                File.Create(FullPath).Close();
            }
            else
            {
                FullPath = Path.Combine(path, "Log.txt");
                //File.Create(FullPath).Close();
            }


            StringBuilder completeFile = new StringBuilder();
            completeFile.AppendLine(LogMessage + Environment.NewLine);
            File.AppendAllText(FullPath, completeFile.ToString());

        }
    }
}
