﻿using System.Collections.Generic;
using System.Linq;
using Nop.Core.Domain.Payments;

namespace Nop.Services.Payments
{
    /// <summary>
    /// Process payment result
    /// </summary>
    public partial class ProcessPaymentResult
    {
        private PaymentStatus _newPaymentStatus = PaymentStatus.Pending;

        /// <summary>
        /// Ctor
        /// </summary>
        public ProcessPaymentResult() 
        {
            this.Errors = new List<string>();
        }

        /// <summary>
        /// Gets a value indicating whether request has been completed successfully
        /// </summary>
        public bool Success
        {
            get { return (!this.Errors.Any()); }
        }

        /// <summary>
        /// Add error
        /// </summary>
        /// <param name="error">Error</param>
        public void AddError(string error)
        {
            this.Errors.Add(error);
        }

        /// <summary>
        /// Errors
        /// </summary>
        public IList<string> Errors { get; set; }


        /// <summary>
        /// Gets or sets an AVS result
        /// </summary>
        public string AvsResult { get; set; }

        /// <summary>
        /// Gets or sets the authorization transaction identifier
        /// </summary>
        public string AuthorizationTransactionId { get; set; }

        /// <summary>
        /// Gets or sets the authorization transaction code
        /// </summary>
        public string AuthorizationTransactionCode { get; set; }

        /// <summary>
        /// Gets or sets the authorization transaction result
        /// </summary>
        public string AuthorizationTransactionResult { get; set; }

        /// <summary>
        /// Gets or sets the capture transaction identifier
        /// </summary>
        public string CaptureTransactionId { get; set; }

        /// <summary>
        /// Gets or sets the capture transaction result
        /// </summary>
        public string CaptureTransactionResult { get; set; }

        /// <summary>
        /// Gets or sets the subscription transaction identifier
        /// </summary>
        public string SubscriptionTransactionId { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether storing of credit card number, CVV2 is allowed
        /// </summary>
        public bool AllowStoringCreditCardNumber { get; set; }

        /// <summary>
        /// Gets or sets a payment status after processing
        /// </summary>
        public PaymentStatus NewPaymentStatus
        {
            get
            {
                return _newPaymentStatus;
            }
            set
            {
                _newPaymentStatus = value;
            }
        }

        //Trove specific Data
        public string TroveResultCode { get; set; }
        public string TroveResultMsg { get; set; }
        public bool TroveSuccess { get; set; }
        public string TroveTenderId { get; set; }
        public double TroveAmount { get; set; }
        public string TroveApprovalCode { get; set; }
        public double TroveBalance { get; set; }
        public int TroveId { get; set; }
        public string TroveRrn { get; set; }
    }
}
