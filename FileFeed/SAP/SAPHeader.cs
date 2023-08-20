using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlatFile.FixedLength;
using FlatFile.FixedLength.Attributes;

namespace FileFeed.SAP
{
    [FixedLengthFile]
    public class SAPHeader
    {
        //[FixedLengthField(1, 5, PaddingChar = '0')] //1-8 - 8
        public string HeaderLiteral
        {
            get { return "H"; }
        }

        //[FixedLengthField(1, 5, PaddingChar = '0')] //9-16 - 8
        public string FileDate { get; set; }

    }
}
