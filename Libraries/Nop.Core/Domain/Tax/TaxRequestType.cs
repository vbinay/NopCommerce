using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Tax
{
    public enum TaxRequestType
    {
        /// <summary>
        /// First Tax Rate Request while adding Product to cart
        /// </summary>
        AddToCartQuotationType = 1,
        /// <summary>
        /// Second Tax Rate Request while final step of the check
        /// </summary>
        CheckinQuotationType = 2,
        /// <summary>
        /// Third tax rate request after confirming the order
        /// </summary>
        ConfirmDistributiveTax = 3,


        RequestDistributiveTax = 4,

        RequestQuotationTax = 5,


        RequestBulkQuotationTax = 6,

        ResponseBulkQuotationTax = 7,


        RequestDistributiveTaxRefund = 8,

        ResponseDistributiveTaxRefund = 9,


    }
}
