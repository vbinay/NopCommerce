using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlatFile.FixedLength;
using FlatFile.FixedLength.Attributes;

namespace FileFeed
{
    [FixedLengthFile]
    public class FsuMealPlanRecord
    {
        [FixedLengthField(1, 5, PaddingChar = '0')]
        public string EmplId { get; set; }

        //[FixedLengthField(1, 5, PaddingChar = ' ')]
        public string Filler1
        {
            get { return " "; }
        }

        //[FixedLengthField(1, 5, PaddingChar = '0')]
        public String ItemType {
            get { return "200000029003"; }
        }

        //[FixedLengthField(1, 5, PaddingChar = ' ')]
        public string Filler2
        {
            get { return " "; }
        }

        //[FixedLengthField(2, 25, PaddingChar = ' ', Padding = Padding.Right, Converter = typeof(ConverterDecimalTwoDigs))]
        public string Amount { get; set; }

        public string Filler3
        {
            get { return " "; }
        }

        public string Term { get; set; }

        public string Filler4
        {
            get { return " "; }
        }

        public string ReferenceNumber { get; set; }

        public string Filler5
        {
            get { return " "; }
        }

        public string DueDate { get; set; }

        public string Filler6
        {
            get { return " "; }
        }

        public string AccountType
        {
            get { return "STU"; }
        }
    }
}
