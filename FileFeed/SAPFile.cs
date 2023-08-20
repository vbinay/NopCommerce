using FileFeed.SAP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileFeed
{
    public class SAPFile
    {


        public SAPFile()
        {

        }

        public StringBuilder Header { get; set; }
            
        public StringBuilder Builder { get; set; }

        public int TrailerCount { get; set; }

        public decimal TrailerTotal { get; set; }

        public SAPTrailer Trailer { get; set; }


    }
}
