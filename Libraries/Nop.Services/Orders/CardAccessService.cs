using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Services.Events;
using System.Collections;
using Nop.Services.Helpers;
using Nop.Services.Stores;

using System.Web;
using System.Web.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;
using System.IO;
using Newtonsoft.Json;


namespace Nop.Services.Orders
{
    /// <summary>
    /// CardAccessService 
    /// </summary>
    public partial class CardAccessService : ICardAccessService
    {
        #region Fields

        private readonly IEventPublisher _eventPublisher;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IRepository<CardAccessRecord> _repository;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="BlackboardRepository">Meal plan context</param>
        /// <param name="eventPublisher"></param>
        /// <param name="storeMappingServices"></param>
        public CardAccessService(IRepository<CardAccessRecord> repository, IEventPublisher eventPublisher, IDateTimeHelper dateTimeHelper, IStoreMappingService storeMappingService)
        {
            this._repository = repository;
            this._eventPublisher = eventPublisher;
            this._dateTimeHelper = dateTimeHelper;
            this._storeMappingService = storeMappingService;
        }

        #endregion

        #region Methods

        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static long GetCurrentUnixTimestampMillis() { return (long)(DateTime.UtcNow - UnixEpoch).TotalMilliseconds; }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="seed"></param>
        /// <param name="issuerId"></param>
        /// <param name="acccountNumber"></param>
        /// <param name="planId"></param>
        /// <param name="applicationId"></param>
        /// <returns></returns>
        public CardAccessRecord GetBalance(string seed, string issuerId, string accountNumber, string planId, string applicationId)
        {
            String timeStamp = GetCurrentUnixTimestampMillis().ToString();
            String hashString = seed + issuerId + accountNumber + planId + applicationId + timeStamp;
            HashAlgorithm algorithm = SHA1.Create();
            var test = algorithm.ComputeHash(Encoding.UTF8.GetBytes(hashString));
            var hash = BitConverter.ToString(test).Replace("-", "").ToLower();
            string result = null;
            System.Net.ServicePointManager.Expect100Continue = false;

            //DEFAULTS FOR BALANCE
            string cmdValue = GetValueCommand(BlackboardCmdType.BalanceReturn);
            decimal amount = 0;


            using (WebClient client = new WebClient())
            {
                string mpm = "";// new Blackboard.Mealplan.Models.MealPlanModel();

                try
                {
                    byte[] response = client.UploadValues("https://test.campuscardcenter.com/cs/api/mealplanDrCr", new NameValueCollection()
                        {                   
                            { "issuerId", issuerId },
                            { "cardholderId", accountNumber },
                            { "applicationId", applicationId },
                            { "planId", planId },
                            { "valueCmd", cmdValue},
                            { "value", amount.ToString() },
                            { "timestamp", timeStamp },
                            { "hash", hash },

                        });

                    using (var responseReader = new StreamReader(new MemoryStream(response)))
                    {
                        mpm = responseReader.ReadToEnd();
                    }
                }
                catch (WebException we)
                {
                    mpm = new StreamReader(we.Response.GetResponseStream()).ReadToEnd();
                }


                CardAccessRecord board = new CardAccessRecord();
                JObject o = JObject.Parse(mpm);

                board.Status = o["success"].ToString();

                if (board.Status == "False") //request failed
                {
                    board.Error = o["error"].ToString();
                    return board;
                }
                else
                { //successful
                    board.CardHolderID = o["cardholderId"].ToString();
                    board.IssuerId = issuerId;
                    board.ApplicationID = applicationId;
                    board.PlanId = o["planId"].ToString();
                    board.Type = o["type"].ToString();
                    board.AccountId = o["accountId"].ToString();
                    board.Balance = o["balance"].ToString();
                    board.Hash = hashString;
                }
                return board;
            }
        }


        /// <summary>
        /// Gets list of accounts   
        /// </summary>
        /// <param name="seed"></param>
        /// <param name="issuerId"></param>
        /// <param name="acccountNumber"></param>
        /// <param name="planId"></param>
        /// <param name="applicationId"></param>
        /// <param name="?"></param>
        /// <returns></returns>
        public List<CardAccessRecord> GetAccounts(int customerId)
        {
            var query1 = (from cardAccessRecord in _repository.Table
                          where (cardAccessRecord.CustomerId == customerId)
                          select cardAccessRecord.CardHolderID).Distinct();

            var query2 = from cardAccessRecord in _repository.Table
                         orderby cardAccessRecord.CardHolderID
                         where (query1.Contains(cardAccessRecord.CardHolderID) && cardAccessRecord.CustomerId == customerId)
                         select cardAccessRecord;

            List<CardAccessRecord> previousRecords = query2.ToList();

            List<CardAccessRecord> updatedRecords = new List<CardAccessRecord>();
            foreach (CardAccessRecord previousRecord in previousRecords)
            {
                bool found = false;
                foreach (CardAccessRecord newRecord in updatedRecords)
                {
                    if (newRecord.CardHolderID == previousRecord.CardHolderID)
                    {
                        found = true;
                    }
                }
                if (!found)
                {
                    updatedRecords.Add(this.GetBalance("test", previousRecord.IssuerId, previousRecord.CardHolderID, previousRecord.PlanId, previousRecord.ApplicationID));
                }
            }
            return updatedRecords;
        }

        public CardAccessRecord SubmitMealPlanRequest(int customerId, string seed, string issuerId, string acccountNumber, string planId, string applicationId, decimal amount, BlackboardCmdType type)
        {
            String timeStamp = GetCurrentUnixTimestampMillis().ToString();

            String hashString = seed + issuerId + acccountNumber + planId + applicationId + timeStamp;

            HashAlgorithm algorithm = SHA1.Create();
            var test = algorithm.ComputeHash(Encoding.UTF8.GetBytes(hashString));

            var hash = BitConverter.ToString(test).Replace("-", "").ToLower();
            string result = null;
            System.Net.ServicePointManager.Expect100Continue = false;


            string cmdValue = GetValueCommand(type);



            using (WebClient client = new WebClient())
            {
                string mpm = "";// new Blackboard.Mealplan.Models.MealPlanModel();

                try
                {
                    byte[] response = client.UploadValues("https://test.campuscardcenter.com/cs/api/mealplanDrCr", new NameValueCollection()
                        {                   
                            { "issuerId", issuerId },
                            { "cardholderId", acccountNumber },
                            { "applicationId", applicationId },
                            { "planId", planId },
                            { "valueCmd", cmdValue},
                            { "value", amount.ToString() },
                            { "timestamp", timeStamp },
                            { "hash", hash },

                        });

                    using (var responseReader = new StreamReader(new MemoryStream(response)))
                    {
                        mpm = responseReader.ReadToEnd();
                    }
                }
                catch (WebException we)
                {
                    mpm = new StreamReader(we.Response.GetResponseStream()).ReadToEnd();
                }


                CardAccessRecord board = new CardAccessRecord();
                JObject o = JObject.Parse(mpm);
                board.Status = o["success"].ToString();

                if (board.Status == "False") //request failed
                {
                    board.Error = o["error"].ToString();
                    return board;
                }
                else
                { //successful
                    board.CustomerId = customerId;
                    board.CardHolderID = o["cardholderId"].ToString();
                    board.IssuerId = issuerId;
                    board.ApplicationID = applicationId;
                    board.PlanId = o["planId"].ToString();
                    board.Type = o["type"].ToString();
                    board.AccountId = o["accountId"].ToString();
                    board.Balance = o["balance"].ToString();
                    board.CreatedOnUTC = DateTime.UtcNow;
                    if (type != BlackboardCmdType.BalanceReturn)
                    {
                        board.AppliedAmount = o["appliedAmount"].ToString();
                    }
                    board.Hash = hashString;
                }

                _repository.Insert(board);


                //event notification
                _eventPublisher.EntityInserted(board);

                return board;
            }
        }


        private string GetValueCommand(BlackboardCmdType type)
        {
            string cmdValue = "";
            switch (type)
            {
                case BlackboardCmdType.Credit: cmdValue = "++"; break;
                case BlackboardCmdType.Debit: cmdValue = "--"; break;
                case BlackboardCmdType.CloseAccount: cmdValue = "C"; break;
                case BlackboardCmdType.BalanceReturn: cmdValue = "bal"; break;
                case BlackboardCmdType.CreateAccountAndSetBalance: cmdValue = "$$"; break;
            }
            return cmdValue;
        }
        #endregion
    }
}
