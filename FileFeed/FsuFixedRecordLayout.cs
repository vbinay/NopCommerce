using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using FlatFile.Core;
using FlatFile.FixedLength.Implementation;

namespace FileFeed
{
    class FsuFixedRecordLayout : FixedLayout<FsuMealPlanRecord>
    {
        public FsuFixedRecordLayout()
        {
            this.WithMember(x => x.EmplId, c => c.WithLength(10).WithRightPadding(' '))
                .WithMember(x => x.Filler1, c => c.WithLength(1))
                .WithMember(x => x.ItemType, c => c.WithLength(12))
                .WithMember(x => x.Filler2, c => c.WithLength(1))
                .WithMember(x => x.Amount, c => c.WithLength(11).WithLeftPadding(' '))
                .WithMember(x => x.Filler3, c => c.WithLength(1))
                .WithMember(x => x.Term, c => c.WithLength(4).WithLeftPadding(' '))
                .WithMember(x => x.Filler4, c => c.WithLength(1))
                .WithMember(x => x.ReferenceNumber, c => c.WithLength(30).WithRightPadding(' '))
                .WithMember(x => x.Filler5, c => c.WithLength(1))
                .WithMember(x => x.DueDate, c => c.WithLength(8))
                .WithMember(x => x.Filler6, c => c.WithLength(1))
                .WithMember(x => x.AccountType, c => c.WithLength(3));
        }
    }  

    class FsuFixedTrailerLayout : FixedLayout<FsuMealPlanTrailer>
    {
        public FsuFixedTrailerLayout()
        {
            this.WithMember(x => x.TrailerLiteral, c => c.WithLength(8).WithRightPadding(' '))
                .WithMember(x => x.ControlCount, c => c.WithLength(8).WithLeftPadding(' '))
                .WithMember(x => x.Filler1, c => c.WithLength(1))
                .WithMember(x => x.ControlAmount, c => c.WithLength(12).WithLeftPadding(' '))
                .WithMember(x => x.Filler2, c => c.WithLength(1))
                .WithMember(x => x.AccountType, c => c.WithLength(3))
                .WithMember(x => x.Filler3, c => c.WithLength(1))
                .WithMember(x => x.OriginId, c => c.WithLength(5));


        }
    }
}
