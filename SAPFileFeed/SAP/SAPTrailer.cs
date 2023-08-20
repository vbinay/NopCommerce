using FlatFile.FixedLength.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPFileFeed.SAP
{
    [FixedLengthFile]
    public class SAPTrailer
    {
        //[FixedLengthField(1, 5, PaddingChar = '0')] //1-8 - 8
        public string TrailerLiteral
        {
            get { return "T"; }
        }

        //[FixedLengthField(1, 5, PaddingChar = '0')] //9-16 - 8
        public string Count { get; set; }

        //[FixedLengthField(9, 5, PaddingChar = ' ')]//17-17 - 1
        public String AmountSign { get; set; }

        //[FixedLengthField(1, 5, PaddingChar = '0')]//18-29 - 12
        public string Amount { get; set; }

    }
}
