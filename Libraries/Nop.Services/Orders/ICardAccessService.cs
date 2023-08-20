using System;
using System.Collections.Generic;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
namespace Nop.Services.Orders
{
    /// <summary>
    /// Card Access System Service
    /// </summary>
    public partial interface ICardAccessService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="seed"></param>
        /// <param name="issuerId"></param>
        /// <param name="accountNumber"></param>
        /// <param name="planId"></param>
        /// <param name="applicationId"></param>
        /// <param name="amount"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        CardAccessRecord SubmitMealPlanRequest(int customerId, string seed, string issuerId, string accountNumber, string planId, string applicationId, decimal amount, BlackboardCmdType type);


        /// <summary>
        /// Validate Account Number in card access system
        /// </summary>
        /// <param name="seed"></param>
        /// <param name="issuerId"></param>
        /// <param name="acccountNumber"></param>
        /// <param name="planId"></param>
        /// <param name="applicationId"></param>
        /// <returns></returns>
        CardAccessRecord GetBalance(string seed, string issuerId, string acccountNumber, string planId, string applicationId);


        /// <summary>
        /// Get all accounts
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        List<CardAccessRecord> GetAccounts(int customerId);

    }
}
