using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nop.Core.Domain.Orders
{
    public enum MealPlanSortingEnum /// NU-16
    {

        MealPlanNameDesc = 0,

        MealPlanNameAsc = 1,

        RecipientNameAsc = 2,

        RecipientNameDesc = 3,

        RecipientAddressAsc = 4,

        RecipientAddressDesc = 5,

        RecipientPhoneAsc = 6,

        RecipientPhoneDesc = 7,

        RecipientEmailAsc = 8,

        RecipientEmailDesc = 9,

        RecipientAcctNumAsc = 10,

        RecipientAcctNumDesc = 11,

        IsProcessedAsc = 12,

        IsProcessedDesc = 13,

        CreatedOnLocalAsc = 14,

        CreatedOnLocalDesc = 15,

        CreatedOnUtcAsc = 14,

        CreatedOnUtcDesc = 15,

        AmountAsc = 16,

        AmountDesc = 17,
    }
}
