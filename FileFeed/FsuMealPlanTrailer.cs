using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlatFile.FixedLength;
using FlatFile.FixedLength.Attributes;

namespace FileFeed
{
    [FixedLengthFile]
    public class FsuMealPlanTrailer
    {
        //[FixedLengthField(1, 5, PaddingChar = '0')] //1-8 - 8
        public string TrailerLiteral
        {
            get { return "TRAILER"; }
        }

        //[FixedLengthField(1, 5, PaddingChar = '0')] //9-16 - 8
        public int ControlCount { get; set; }

       //[FixedLengthField(9, 5, PaddingChar = ' ')]//17-17 - 1
        public String Filler1
        {
            get { return " "; }
        }

       //[FixedLengthField(1, 5, PaddingChar = '0')]//18-29 - 12
        public decimal ControlAmount { get; set; }

       //[FixedLengthField(2, 25, PaddingChar = ' ')] //30-20 - 1
        public string Filler2
        {
            get { return " "; }
        }


       //[FixedLengthField(2, 5, PaddingChar = '0')] //31-33 - 3
        public string AccountType
        {
            get { return "STU"; }
        }


       //[FixedLengthField(2, 5, PaddingChar = ' ')]//34-34 - 1
        public string Filler3
        {
            get { return " "; }
        }

       //[FixedLengthField(2, 5, PaddingChar = '0')]//35-39 - 5
        public string OriginId {
            get { return "00009"; }
        }

    }
}
