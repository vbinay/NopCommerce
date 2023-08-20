using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Mvc;
using System.Configuration;

namespace Nop.Core
{
    public class TroveUtilities
    {
        const PCode pCode = PCode.Credit;
        public static ResponseBucket<TContent> ProcessResponseMessage<TContent>(HttpResponseMessage response) where TContent : TroveReponseModel
        {
            //var responseContainer = response.RequestMessage.Properties["response-container"] as ResponseContainer;
            //var log = responseContainer.CreateHttpLog();
            //var jsonResponse = responseContainer.LogBucket.ResponseBodyContent;

            var jsonResponse = response.Content.ReadAsStringAsync().Result;

            //string requestUrl = responseContainer.LogBucket.RequestUrl;

            TContent responseObject = null;
            bool success = false;

            if (string.IsNullOrWhiteSpace(jsonResponse))
            {
                Debug.WriteLine("content is empty");
            }
            else
            {
                try
                {
                    responseObject = JsonConvert.DeserializeObject<TContent>(jsonResponse);
                    success = responseObject.Success;
                }
                catch (Exception)
                {
                    Debug.WriteLine("error during deserialization");
                }
            }

            if (success)
            {
                return ResponseBucketFactory.CreateForSuccessResponse("", "", responseObject, new TimeSpan());
            }
            else
            {
                return new ResponseBucket<TContent>
                {
                    Log = "",
                    Url = "",
                    Content = responseObject,
                    Duration = new TimeSpan(),
                    Error = new Error
                    {
                        Message = responseObject?.ResultMsg,
                        StatusCode = (int)response.StatusCode,
                        Details = responseObject?.ResultCode
                    }
                };
            }
        }
        public static int CreateFlag(bool flag) => flag ? 1 : 0;
        public static string CreatePcode(PCode pCode) => pCode.ToString().ToLower();
        public static string CreateDateString(DateTime date) => date.ToString("yyyyMMddHHmmss");
        public static long CreateSystemTraceAuditNumber()
        {
            return Math.Abs(GetUNIXDateTimeTicks());
        }

        public static long GetUNIXDateTimeTicks()
        {
            return (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }


        public static StringContent CreateJsonContent(object model, bool setDefaultValueHandlingIgnore = true, bool setNullValueHandlingIgnore = true)
        {
            CultureInfo _defaultCulture = CultureInfo.InvariantCulture.Clone() as CultureInfo;
            _defaultCulture.NumberFormat.NumberDecimalSeparator = ".";
            _defaultCulture.NumberFormat.NumberGroupSeparator = string.Empty;

            var settings = new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-ddTHH:mm:ssZ",
                Culture = _defaultCulture
            };

            if (setDefaultValueHandlingIgnore)
            {
                settings.DefaultValueHandling = DefaultValueHandling.Ignore;
            }

            if (setNullValueHandlingIgnore)
            {
                settings.NullValueHandling = NullValueHandling.Ignore;
            }

            string json = JsonConvert.SerializeObject(model, settings);

            StringContent content = new StringContent(json);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return content;
        }

        

        public static TroveWalletsTokenResponseModel GetWalletsToken(string realId)
        {
            const string hash = "7d84adb360717834a2682454f71b63f3abff33ea";
            string troveApiPaymentBaseUrl = String.Empty;
            string troveApiVersion = "1.0";
            string consumerId = String.Empty;

            if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings["TroveBaseUrl"]))
            {
                troveApiPaymentBaseUrl = ConfigurationManager.AppSettings["TroveBaseUrl"] + "jcard/api/wallets/" + realId + "/token";
            }
            if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings["TroveConsumerId"]))
            {
                consumerId = ConfigurationManager.AppSettings["TroveConsumerId"];
            }

            TroveDetailCustomerModel cust = new TroveDetailCustomerModel();
            List<Card> cards = new List<Card>();
            List<TroveWalletAccountModel> accounts = new List<TroveWalletAccountModel>();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(troveApiPaymentBaseUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                client.DefaultRequestHeaders.Add("version", troveApiVersion);
                client.DefaultRequestHeaders.Add("consumer-id", consumerId);
                client.DefaultRequestHeaders.Add("nonce", Guid.NewGuid().ToString());
                client.DefaultRequestHeaders.Add("hash", hash);
                using (var tokenResponse = client.GetAsync(troveApiPaymentBaseUrl).Result)
                {
                    return TroveUtilities.ProcessResponseMessage<TroveWalletsTokenResponseModel>(tokenResponse).Content;
                }
            }
        }

        public static PostPaymentToWalletResponse PostPaymentToWallet(PostPaymentToWalletRequest request)
        {
            const string hash = "7d84adb360717834a2682454f71b63f3abff33ea";
            string troveApiPaymentBaseUrl = String.Empty;
            string troveApiVersion = "1.0";
            string consumerId = String.Empty;

            if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings["TroveBaseUrl"]))
            {
                troveApiPaymentBaseUrl = ConfigurationManager.AppSettings["TroveBaseUrl"] + "jcard/api/wallets/payments";
            }
            if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings["TroveConsumerId"]))
            {
                consumerId = ConfigurationManager.AppSettings["TroveConsumerId"];
            }

            TroveDetailCustomerModel cust = new TroveDetailCustomerModel();
            List<Card> cards = new List<Card>();
            List<TroveWalletAccountModel> accounts = new List<TroveWalletAccountModel>();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(troveApiPaymentBaseUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                client.DefaultRequestHeaders.Add("version", troveApiVersion);
                client.DefaultRequestHeaders.Add("consumer-id", consumerId);
                client.DefaultRequestHeaders.Add("nonce", Guid.NewGuid().ToString());
                client.DefaultRequestHeaders.Add("hash", hash);
                using (var content = TroveUtilities.CreateJsonContent(request, setDefaultValueHandlingIgnore: false))
                {
                    using (var response = client.PostAsync(troveApiPaymentBaseUrl, content).Result)
                    {
                        return TroveUtilities.ProcessResponseMessage<PostPaymentToWalletResponse>(response).Content;
                    }
                }
            }
        }

        public static PostPaymentToWalletResponse VoidCCPaymentToWallet(PostVoidCCPaymentRequest request)
        {
            const string hash = "7d84adb360717834a2682454f71b63f3abff33ea";
            string troveApiPaymentBaseUrl = String.Empty;
            string troveApiVersion = "1.0";
            string consumerId = String.Empty;

            if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings["TroveBaseUrl"]))
            {
                troveApiPaymentBaseUrl = ConfigurationManager.AppSettings["TroveBaseUrl"] + "jcard/api/wallets/payments";
            }
            if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings["TroveConsumerId"]))
            {
                consumerId = ConfigurationManager.AppSettings["TroveConsumerId"];
            }

            TroveDetailCustomerModel cust = new TroveDetailCustomerModel();
            List<Card> cards = new List<Card>();
            List<TroveWalletAccountModel> accounts = new List<TroveWalletAccountModel>();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(troveApiPaymentBaseUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                client.DefaultRequestHeaders.Add("version", troveApiVersion);
                client.DefaultRequestHeaders.Add("consumer-id", consumerId);
                client.DefaultRequestHeaders.Add("nonce", Guid.NewGuid().ToString());
                client.DefaultRequestHeaders.Add("hash", hash);
                using (var content = CreateJsonContent(request, setDefaultValueHandlingIgnore: false))
                {
                    using (var response = client.PostAsync(troveApiPaymentBaseUrl, content).Result)
                    {
                        return TroveUtilities.ProcessResponseMessage<PostPaymentToWalletResponse>(response).Content;
                    }
                }
            }
        }
    }

    public enum PCode
    {
        Campus = 1,
        Credit = 2,
        Pickup = 3
    }

    public class PostVoidCCPaymentRequest
    {
        [JsonProperty("mti")]
        public string mti => "void";

        [JsonProperty("stan")]
        public long stan { get; set; }

        [JsonProperty("rrn")]
        public string rrn { get; set; }

        [JsonProperty("tid")]
        public string tid { get; set; }

        [JsonProperty("mid")]
        public string mid { get; set; }

    }

    public class PostPaymentToWalletResponse : TroveReponseModel
    {
        [JsonProperty("approvalCode")]
        public string approvalCode { get; set; }

        [JsonProperty("id")]
        public int id { get; set; }

        [JsonProperty("rrn")]
        public string rrn { get; set; }

        [JsonProperty("amount")]
        public double amount { get; set; }

        [JsonProperty("balance")]
        public double balance { get; set; }

        [JsonProperty("errors")]
        public string Errors { get; set; }


    }
    public class PostPaymentToWalletRequest
    {
        [JsonProperty("amount")]
        public float amount { get; set; }

        [JsonProperty("date")]
        public string date { get; set; }

        /// <summary>
        /// should be always 'app' - requirement from Trove
        /// </summary>
        [JsonProperty("entryMode")]
        public string entryMode => "app";

        [JsonProperty("mid")]
        public string mid { get; set; }

        /// <summary>
        /// should be always 'sale' - requirement from Trove
        /// </summary>
        [JsonProperty("mti")]
        public string mti { get; set; }

        /// <summary>
        /// should be 'campus' if payment from Student card
        /// should be 'credit' if payment from Bite Pay card
        /// </summary>
        [JsonProperty("pcode")]
        public string pcode { get; set; }

        [JsonProperty("stan")]
        public long stan { get; set; }

        [JsonProperty("tender")]
        public string tender { get; set; }

        [JsonProperty("tid")]
        public string tid { get; set; }

        /// <summary>
        /// (required) Active token for patron
        /// <br/>
        /// 1 if should include loyalty funds
        /// </summary>
        [JsonProperty("token")]
        public string token { get; set; }

        [JsonProperty("redeem")]
        public int redeem { get; set; }

        [JsonProperty("rrn")]
        public string rrn { get; set; }

        /// <summary>
        /// 0 if should not include loyalty funds
        /// <br/>
        /// 1 if should include loyalty funds
        /// </summary>
        [JsonProperty("loyalty")]
        public int loyalty { get; set; }

        /// <summary>
        /// 0 if paying as dining dollars
        /// <br/>
        /// 1 if paying as meal plan
        /// <br/>
        /// nill if paying via credit card
        /// </summary>
        [JsonProperty("mealPlan")]
        public int? mealPlan { get; set; }

        /// <summary>
        /// Id of the byo card that will be used, required for byo credit card transactions
        /// </summary>
        [JsonProperty("cardId")]
        public string cardId { get; set; }

        [JsonProperty("subscription")]
        public string subscription { get; set; }


    }

    public class payment
    {
        [JsonProperty("amount")]
        public float amount { get; set; }


        [JsonProperty("mid")]
        public string mid { get; set; }

        /// <summary>
        /// should be 'campus' if payment from Student card
        /// should be 'credit' if payment from Bite Pay card
        /// </summary>
        [JsonProperty("pcode")]
        public string pcode { get; set; }

        [JsonProperty("tender")]
        public string tender { get; set; }

        [JsonProperty("tid")]
        public string tid { get; set; }

        /// <summary>
        /// 0 if paying as dining dollars
        /// <br/>
        /// 1 if paying as meal plan
        /// <br/>
        /// nill if paying via credit card
        /// </summary>
        [JsonProperty("mealPlan")]
        public int? mealPlan { get; set; }

        [JsonProperty("redeem")]
        public int redeem { get; set; }

        /// <summary>
        /// 0 if should not include loyalty funds
        /// <br/>
        /// 1 if should include loyalty funds
        /// </summary>
        [JsonProperty("loyalty")]
        public int loyalty { get; set; }

        /// <summary>
        /// Id of the byo card that will be used, required for byo credit card transactions
        /// </summary>
        [JsonProperty("cardId")]
        public string cardId { get; set; }
        [JsonProperty("token")]
        public string token { get; set; }
    }

    public class ResponseBucketFactory
    {
        public static ResponseBucket<T> CreateForSuccessResponse<T>(string requestUrl, string log, T content, TimeSpan duration)
        {
            return new ResponseBucket<T>
            {
                Log = log,
                Url = requestUrl,
                Content = content,
                Duration = duration
            };
        }
    }
    public class LogBucket
    {
        public string Method { get; set; }
        public string RequestHeaders { get; set; }
        public string RequestBodyContentType { get; set; }
        public string RequestBodyContent { get; set; }

        public string RequestUrl { get; set; }
        public HttpStatusCode StatusCode { get; set; }

        public string ResponseHeaders { get; set; }
        public string ResponseBodyContentType { get; set; }
        public string ResponseBodyContent { get; set; }
    }

    public class ResponseContainer
    {
        public DateTime Started { get; }
        public DateTime Finished { get; }
        public TimeSpan Duration { get; }

        public LogBucket LogBucket { get; }


        public ResponseContainer(DateTime started, DateTime finished, LogBucket logBucket)
        {
            Started = started;
            Finished = finished;

            Duration = finished - started;
            if (Duration < TimeSpan.Zero)
            {
                Duration = TimeSpan.Zero;
            }

            LogBucket = logBucket;
        }

        public string CreateHttpLog()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"duration: {Duration}");

            sb.AppendLine($"{LogBucket.Method} {LogBucket.RequestUrl}");

            bool appendLine = false;

            if (!string.IsNullOrWhiteSpace(LogBucket.RequestHeaders))
            {
                sb.AppendLine();
                sb.AppendLine("headers:");
                sb.AppendLine($"{LogBucket.RequestHeaders}");

                appendLine = true;
            }

            if (appendLine)
            {
                sb.AppendLine();
            }

            appendLine = false;

            if (!string.IsNullOrWhiteSpace(LogBucket.RequestBodyContent))
            {
                sb.AppendLine($"body:{LogBucket.RequestBodyContentType}");
                sb.AppendLine($"{LogBucket.RequestBodyContent}");

                appendLine = true;
            }

            if (appendLine)
            {
                sb.AppendLine();
            }

            sb.AppendLine();
            sb.AppendLine("response:");
            sb.AppendLine(LogBucket.StatusCode.ToString());

            if (!string.IsNullOrWhiteSpace(LogBucket.ResponseHeaders))
            {
                sb.AppendLine();
                sb.AppendLine("headers:");
                sb.AppendLine($"{LogBucket.ResponseHeaders}");
            }

            if (!string.IsNullOrWhiteSpace(LogBucket.ResponseBodyContentType))
            {
                sb.AppendLine();
                sb.AppendLine("body content type:");
                sb.AppendLine($"{LogBucket.ResponseBodyContentType}");
            }

            if (!string.IsNullOrWhiteSpace(LogBucket.ResponseBodyContent))
            {
                sb.AppendLine($"body:");
                sb.AppendLine($"{LogBucket.ResponseBodyContent}");
            }

            return sb.ToString().Trim();
        }
    }
    public class TroveReponseModel
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("resultMsg")]
        public string ResultMsg { get; set; }

        [JsonProperty("resultCode")]
        public string ResultCode { get; set; }

        [JsonProperty("tender")]
        public string TenderId { get; set; }
    }

    public class TroveErrorReponseModel : TroveReponseModel
    {
        public string Errors { get; set; }
    }

    public class TroveCreateCustomerResponseModel : TroveReponseModel
    {
        public string Pin { get; set; }
        public string Wallet { get; set; }
    }

    public class TroveCustomerResponseModel : TroveReponseModel
    {
        public TroveCreateCustomerModel Customer { get; set; }
    }

    public class TroveDetailCustomerResponseModel : TroveReponseModel
    {
        public TroveDetailCustomerModel Customer { get; set; }
    }

    public class UIResponseVieModel  
    {
        public string TroveUserId { get; set; }

        public string TroveUserPassword { get; set; }

        public bool UserConsent { get; set; }

        public string RealId { get; set; }

        public TroveDetailCustomerModel Customer { get; set; }

        //public List<Card> Cards { get; set; }

        public int SelectedCardId { get; set; }
        public int SelectAccountId { get; set; }

        public IList<SelectListItem> AvailableCards { get; set; }
        public IList<SelectListItem> AvailableAccounts { get; set; }

        //public List<TroveWalletAccountModel> Accounts { get; set; }
    }

    [DebuggerDisplay("{RealId} campus:{Campus}")]
    public class TroveDetailCustomerModel
    {
        [JsonProperty("lastName")]
        public string LastName { get; set; }
        [JsonProperty("endDate")]
        public string EndDate { get; set; }
        [JsonProperty("issuer")]
        public int Issuer { get; set; }
        [JsonProperty("bitepay")]
        public bool Bitepay { get; set; }
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("realId")]
        public string RealId { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("institutionId")]
        public string InstitutionId { get; set; }
        [JsonProperty("campus")]
        public bool Campus { get; set; }
        [JsonProperty("active")]
        public bool Active { get; set; }
        [JsonProperty("firstName")]
        public string FirstName { get; set; }
        [JsonProperty("phone")]
        public string Phone { get; set; }
        [JsonProperty("startDate")]
        public string StartDate { get; set; }
    }
    public class TroveWalletsResponseModel : TroveReponseModel
    {
        public List<TroveWalletAccountModel> Wallet { get; set; }
    }

    public class TroveWalletsDetailsResponseModel : TroveReponseModel // details = balance + tender
    {
        public List<TroveWalletAccountModel> Accounts { get; set; }
    }

    public class TransactionLogModel
    {
        [JsonProperty("date")]
        public DateTime Date { get; set; }
        [JsonProperty("amount")]
        public decimal Amount { get; set; }
        [JsonProperty("wallet1")]
        public string Wallet1 { get; set; }
        [JsonProperty("mid")]
        public string Mid { get; set; }
        [JsonProperty("typeDescription")]
        public string TypeDescription { get; set; }
        [JsonProperty("tid")]
        public string Tid { get; set; }
        [JsonProperty("rrn")]
        public string Rrn { get; set; }
        [JsonProperty("ssData")]
        public string ssData { get; set; }
        [JsonProperty("itc")]
        public string Itc { get; set; }
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("irc")]
        public string Irc { get; set; }
        [JsonProperty("additionalData")]
        public TransactionLogAdditionalDataModel AdditionalData { get; set; }
        [JsonProperty("customer1")]
        public string Customer1 { get; set; }
        [JsonProperty("currencyCode")]
        public string CurrencyCode { get; set; }
        [JsonProperty("reversalCount")]
        public int ReversalCount { get; set; }

    }

    public class TransactionRequest
    {
        [JsonProperty("amount")]
        public decimal Amount { get; set; }
        [JsonProperty("date")]
        public string Date { get; set; }
        [JsonProperty("entryMode")]
        public string EntryMode { get; set; }
        [JsonProperty("mid")]
        public string Mid { get; set; }
        [JsonProperty("mti")]
        public string Mti { get; set; }
        [JsonProperty("pcode")]
        public string Pcode { get; set; }
        [JsonProperty("softVersion")]
        public string SoftVersion { get; set; }
        [JsonProperty("stan")]
        public string Stan { get; set; }
        [JsonProperty("tender")]
        public string Tender { get; set; }
        [JsonProperty("tid")]
        public string Tid { get; set; }
        [JsonProperty("token")]
        public string Token { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class TransactionLogAdditionalDataModel
    {
        [JsonProperty("jsonRequest")]
        public TransactionRequest JsonRequest { get; set; }
        [JsonProperty("typeDescription")]
        public string TypeDescription { get; set; }
    }


    [DebuggerDisplay("{Name} \t{Tender} \t{Type} \t{Balance}")]
    public class TroveWalletAccountModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("tender")]
        public string Tender { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("balance")]
        public decimal Balance { get; set; }
    }
    public class TransactionHistoryModel
    {
        [JsonProperty("accountCode")]
        public string AccountCode { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("journalCode")]
        public string JournalCode { get; set; }
        [JsonProperty("tranlogs")]
        public List<TransactionLogModel> TransactionLogs { get; set; }
    }
    public class GetWalletTendersResponse : TroveReponseModel
    {
        public List<TroveWalletAccountModel> Accounts { get; set; }
        public List<Card> Cards { get; set; }
    }

    public class TroveWalletsTokenResponseModel : TroveReponseModel
    {
        public string Token { get; set; }
    }

    public class TroveWalletsTransactionHistoryModel : TroveReponseModel
    {
        [JsonProperty("transactions")]
        public TransactionHistoryModel TransactionHistory { get; set; }
    }

    public class AddCreditToWalletResponse : TroveReponseModel
    {
        [JsonProperty("amountWithFees")]
        public double amountWithFees { get; set; }

        [JsonProperty("fees")]
        public object[] fees { get; set; }

        [JsonProperty("errors")]
        public string Errors { get; set; }
    }

    
    [DebuggerDisplay("{CardId} \t{Last4} \t{Brand,nq} \t{Name} \t{cardProduct} \t{balance}")]
    public class Card
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("cardId")]
        public int CardId { get; set; }

        [JsonProperty("brand")]
        public string Brand { get; set; }

        [JsonProperty("last4")]
        public string Last4 { get; set; }

        [JsonProperty("cardProduct")]
        public string CardProduct { get; set; }

        [JsonProperty("balance")]
        public decimal Balance { get; set; }
    }

    public class PostPaymentToWalletResponseBase : TroveReponseModel
    {
        [JsonProperty("approvalCode")]
        public string approvalCode { get; set; }

        [JsonProperty("id")]
        public int id { get; set; }

        [JsonProperty("rrn")]
        public string rrn { get; set; }

        [JsonProperty("amount")]
        public double amount { get; set; }

        [JsonProperty("balance")]
        public double balance { get; set; }

        [JsonProperty("errors")]
        public string Errors { get; set; }


    }

    public class BatchPostPaymentToWalletResponse : TroveReponseModel
    {
        public BatchPostPaymentToWalletResponse()
        {
            this.results = new List<PostPaymentToWalletResponseBase>();
        }
        public List<PostPaymentToWalletResponseBase> results { get; set; }
    }

    public class GetCardsResponse : TroveReponseModel
    {
        public List<Card> Cards { get; set; }
    }

    public class ResponseBucket<T>
    {
        public T Content { get; set; }
        public string Log { get; set; }
        public string Url { get; set; }
        public Error Error { get; set; }
        public TimeSpan Duration { get; set; }
    }

    public class Error
    {
        public int? StatusCode { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }
    }

    public class TroveCreateCustomerModel
    {
        [JsonProperty("realId")]
        public string RealId { get; set; }// Required
        [JsonProperty("active")]
        public bool Active { get; set; }
        [JsonProperty("startDate")]
        public string StartDate { get; set; }
        [JsonProperty("endDate")]
        public string EndDate { get; set; }
        [JsonProperty("firstName")]
        public string FirstName { get; set; } // Required
        [JsonProperty("lastName")]
        public string LastName { get; set; } // Required
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("address1")]
        public string Address1 { get; set; }
        [JsonProperty("city")]
        public string City { get; set; }
        [JsonProperty("state")]
        public string State { get; set; }
        [JsonProperty("zip")]
        public string Zip { get; set; }
        [JsonProperty("country")]
        public string Country { get; set; }
        [JsonProperty("birthDate")]
        public string BirthDate { get; set; }
        [JsonProperty("phone")]
        public string Phone { get; set; }

        /// <summary>
        /// Mon, Aug 3, 2020 at 2:51 PM
        /// Shawn Edwards <shawn@foundrypayments.com>
        /// paymentCredential is optional parameter is no value just omit it from the request.
        /// </summary>
        [JsonProperty("paymentCredential")]
        public string PaymentCredential { get; set; }

        [JsonProperty("institutionId")]
        public string InstitutionId { get; set; }  // Requiered
    }
}