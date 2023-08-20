using System;
using Nop.Web.Framework.Mvc;

namespace Nop.Admin.Models.Orders
{
    /// <summary>
    /// Donation Model
    /// </summary>
    public class DonationModel : BaseNopEntityModel /// NU-33
    {
        public int PurchasedWithOrderItemId { get; set; }
        /// <summary>
        /// Gets or sets the recipient name
        /// </summary>
        public string DonorFirstName { get; set; }

        /// <summary>
        /// Gets or sets the recipient name
        /// </summary>
        public string DonorLastName { get; set; }


        /// <summary>
        /// Gets or sets the company name
        /// </summary>
        public string DonorCompany { get; set; }


        /// <summary>
        /// Gets or sets a recipient address
        /// </summary>
        public string DonorAddress { get; set; }
        public string DonorAddress2 { get; set; }

        public string DonorCity { get; set; }
        public string DonorState { get; set; }
        public string DonorZip { get; set; }
        public string DonorPhone { get; set; }
        public string DonorCountry { get; set; }


        public bool IsOnBehalf { get; set; }


        public string OnBehalfOfFullName { get; set; }


        public string NotificationEmail { get; set; }


        public bool IncludeGiftAmount { get; set; }

        public string Comments { get; set; }

        public string Amount { get; set; }

        public bool IsProcessed { get; set; }

        public string CreatedOnLocal { get; set; }

        public string ProductName { get; set; }

    }
}