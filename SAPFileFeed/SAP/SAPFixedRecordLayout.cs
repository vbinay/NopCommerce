using FlatFile.FixedLength.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPFileFeed.SAP
{
    class SAPFixedRecordLayout : FixedLayout<SAPRecord>
    {
        public SAPFixedRecordLayout()
        {
            this.WithMember(x => x.RecordIndicator, c => c.WithLength(1).WithLeftPadding(' '))
                .WithMember(x => x.OrderNumber, c => c.WithLength(10).WithLeftPadding(' '))
                .WithMember(x => x.Status, c => c.WithLength(1).WithLeftPadding(' '))
                .WithMember(x => x.LineNumber, c => c.WithLength(3).WithLeftPadding(' '))
                .WithMember(x => x.TransactionDate, c => c.WithLength(8).WithLeftPadding(' '))
                .WithMember(x => x.CostCenterNumber, c => c.WithLength(10).WithLeftPadding(' '))
                .WithMember(x => x.GLAccount, c => c.WithLength(10).WithLeftPadding(' '))
                .WithMember(x => x.AmountSign, c => c.WithLength(1).WithLeftPadding(' '))
                .WithMember(x => x.ProductAmount, c => c.WithLength(17).WithLeftPadding(' '));
        }
    }

    class SAPFixedTrailerLayout : FixedLayout<SAPTrailer>
    {
        public SAPFixedTrailerLayout()
        {
            this.WithMember(x => x.TrailerLiteral, c => c.WithLength(1).WithLeftPadding(' '))
                .WithMember(x => x.Count, c => c.WithLength(10).WithLeftPadding(' '))
                .WithMember(x => x.AmountSign, c => c.WithLength(1).WithLeftPadding(' '))
                .WithMember(x => x.Amount, c => c.WithLength(17).WithLeftPadding(' '));


        }
    }

    class SAPFixedHeaderLayout : FixedLayout<SAPHeader>
    {
        public SAPFixedHeaderLayout()
        {
            this.WithMember(x => x.HeaderLiteral, c => c.WithLength(1).WithLeftPadding(' '))
                .WithMember(x => x.FileDate, c => c.WithLength(8).WithLeftPadding(' '));



        }
    }
}
