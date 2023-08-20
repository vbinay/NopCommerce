//------------------------------------------------------------------------------
// Contributor(s): oskar.kjellin 
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Services.Protocols;
using Nop.Plugin.Shipping.UPS.track;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Shipping.Tracking;
using System.Net;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
//---START: Codechages done by (na-sdxcorp\ADas)--------------
using System.Configuration;
using System.Xml;
using Nop.Plugin.Shipping.UPS.Domain;
using Nop.Core;
using System.IO;
//---END: Codechages done by (na-sdxcorp\ADas)--------------

namespace Nop.Plugin.Shipping.UPS
{
    public class UPSShipmentTracker : IShipmentTracker
    {
        private readonly ILogger _logger;
        private readonly ILocalizationService _localizationService;
        private readonly UPSSettings _upsSettings;

        public UPSShipmentTracker(ILogger logger, ILocalizationService localizationService, UPSSettings upsSettings)
        {
            this._logger = logger;
            this._localizationService = localizationService;
            this._upsSettings = upsSettings;
        }

        /// <summary>
        /// Gets if the current tracker can track the tracking number.
        /// </summary>
        /// <param name="trackingNumber">The tracking number to track.</param>
        /// <returns>True if the tracker can track, otherwise false.</returns>
        public virtual bool IsMatch(string trackingNumber)
        {
            if (string.IsNullOrWhiteSpace(trackingNumber))
                return false;

            //Not sure if this is true for every shipment, but it is true for all of our shipments
            return trackingNumber.ToUpperInvariant().StartsWith("1Z");
        }

        /// <summary>
        /// Gets a url for a page to show tracking info (third party tracking page).
        /// </summary>
        /// <param name="trackingNumber">The tracking number to track.</param>
        /// <returns>A url to a tracking page.</returns>
        public virtual string GetUrl(string trackingNumber)
        {
            string url = "http://wwwapps.ups.com/WebTracking/track?trackNums={0}&track.x=Track";
            url = string.Format(url, trackingNumber);
            return url;
        }


        public virtual IList<ShipmentStatusEvent> TrackingJSON(string trackingNumber)
        {
            string json =
                "{" +
      "\"Security\": " +
            "{" +
        "\"UsernameToken\":" +
            "{" +
          "\"Username\": \"ryan.markle\"," +
          "\"Password\": \"Eeknom52\"" +
        "}," +
       "\"UPSServiceAccessToken\": {" +
          "\"AccessLicenseNumber\": \"AD22ABB19ACE3AFF\"" +
        "}" +
      "}," +
      "\"TrackRequest\": {" +
        "\"Request\": {" +
          "\"RequestAction\": \"Track\"," +
          "\"RequestOption\": \"activity\"" +
        "}" +
        "\"InquiryNumber\": \"" + trackingNumber + "\"" +
        "}" +
        "}";

            string result = "";
            using (var client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                result = client.UploadString("https://onlinetools.ups.com/json/Track", "POST", json);
            }
            var stuff = JObject.Parse(result);

            var nes = new ShipmentStatusEvent();
            nes.EventName = "";
            var list = new List<ShipmentStatusEvent>();
            return list;

        }

        /// <summary>
        /// Gets all events for a tracking number.
        /// </summary>
        /// <param name="trackingNumber">The tracking number to track</param>
        /// <returns>List of Shipment Events.</returns>
        public virtual IList<ShipmentStatusEvent> GetShipmentEvents(string trackingNumber)
        {
            if (string.IsNullOrEmpty(trackingNumber))
                return new List<ShipmentStatusEvent>();

            var result = new List<ShipmentStatusEvent>();
            try
            {
                //---START: Codechages done by (na-sdxcorp\ADas)-------commented out below code-------

                ////use try-catch to ensure exception won't be thrown is web service is not available
                //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls; // comparable to modern browsers
                //ServicePointManager.ServerCertificateValidationCallback += delegate { return true; };
                //var track = new TrackService();               
                //var tr = new TrackRequest();
                //var upss = new UPSSecurity();
                //var upssSvcAccessToken = new UPSSecurityServiceAccessToken();
                //upssSvcAccessToken.AccessLicenseNumber = _upsSettings.AccessKey;
                //upss.ServiceAccessToken = upssSvcAccessToken;
                //var upssUsrNameToken = new UPSSecurityUsernameToken();
                //upssUsrNameToken.Username = _upsSettings.Username;
                //upssUsrNameToken.Password = _upsSettings.Password;
                //upss.UsernameToken = upssUsrNameToken;
                //track.UPSSecurityValue = upss;
                //var request = new RequestType();
                //string[] requestOption = { "15" };
                //request.RequestOption = requestOption;
                //tr.Request = request;
                //tr.InquiryNumber = trackingNumber;


                //var trackResponse = track.ProcessTrack(tr);
                //result.AddRange(trackResponse.Shipment.SelectMany(c => c.Package[0].Activity.Select(ToStatusEvent)).ToList());

                //---END: Codechages done by (na-sdxcorp\ADas)-------commented out below code-------

                //==============================================================================================================

                //---START: Codechages done by (na-sdxcorp\ADas)--------------

                string trackingUrl = ConfigurationManager.AppSettings["ups_TrackingUrl"];
                string accessKey = ConfigurationManager.AppSettings["ups_AccessLicenseNumber"];
                string username = ConfigurationManager.AppSettings["ups_UserId"];
                string password = ConfigurationManager.AppSettings["ups_Password"];

                XmlDocument doc = null;
                string statusCode = string.Empty;
                //Create TrackingRequest
                string trackingRequestString = CreateTrackingRequest(accessKey, username, password, trackingNumber,
                        _upsSettings.CustomerClassification);

                //Execute TrackRequest and get TrackResponse
                string trackingResponseXml = DoRequest(trackingUrl, trackingRequestString);

                doc = new XmlDocument();
                doc.LoadXml(trackingResponseXml);

                statusCode = doc.SelectSingleNode("TrackResponse/Response/ResponseStatusCode").InnerText;

                if (statusCode == "1")
                {
                    XmlNodeList idNodes = doc.SelectNodes("TrackResponse/Shipment/Package/Activity");

                    foreach (XmlNode node in idNodes)
                    {
                        string activityXmlString = node.OuterXml;
                        var docActivityXml = new XmlDocument();
                        docActivityXml.LoadXml(activityXmlString);

                        string activityStatusType = docActivityXml.SelectSingleNode("Activity/Status/StatusType/Code").InnerText;
                        string activityStatusCode = docActivityXml.SelectSingleNode("Activity/Status/StatusCode/Code").InnerText;
                        string activityStatusDate = docActivityXml.SelectSingleNode("Activity/Date").InnerText;
                        string activityStatusTime = docActivityXml.SelectSingleNode("Activity/Time").InnerText;
                        string activityStatusActivityLocationAddressCountryCode = docActivityXml.SelectSingleNode("Activity/ActivityLocation/Address/CountryCode") != null ? docActivityXml.SelectSingleNode("Activity/ActivityLocation/Address/CountryCode").InnerText : string.Empty;
                        string activityStatusActivityLocationAddressCity = docActivityXml.SelectSingleNode("Activity/ActivityLocation/Address/City") != null ? docActivityXml.SelectSingleNode("Activity/ActivityLocation/Address/City").InnerText : string.Empty;

                        result.Add(GetStatusEvent(activityStatusType, activityStatusCode, activityStatusDate, activityStatusTime, activityStatusActivityLocationAddressCountryCode, activityStatusActivityLocationAddressCity));
                    }
                }

                //---END: Codechages done by (na-sdxcorp\ADas)--------------

            }
            //---START: Codechages done by (na-sdxcorp\ADas)-------commented out below code-------
            //catch (SoapException ex)
            //{
            //    var sb = new StringBuilder();
            //    sb.AppendFormat("SoapException Message= {0}.", ex.Message);
            //    sb.AppendFormat("SoapException Category:Code:Message= {0}.", ex.Detail.LastChild.InnerText);
            //    //sb.AppendFormat("SoapException XML String for all= {0}.", ex.Detail.LastChild.OuterXml);
            //    _logger.Error(string.Format("Error while getting UPS shipment tracking info - {0}", trackingNumber), new Exception(sb.ToString()));
            //}
            //---END: Codechages done by (na-sdxcorp\ADas)-------commented out below code-------
            catch (Exception exc)
            {
                //_logger.Error(string.Format("Error while getting UPS shipment tracking info - {0}", trackingNumber), exc);
            }
            return result;
        }

        //---START: Codechages done by (na-sdxcorp\ADas)--------------

        private string CreateTrackingRequest(string accessKey, string username, string password,
            string trackingNumber, UPSCustomerClassification customerClassification)
        {
            var sb = new StringBuilder();

            sb.Append("<?xml version='1.0'?>");
            sb.Append("<AccessRequest xml:lang='en-US'>");
            sb.AppendFormat("<AccessLicenseNumber>{0}</AccessLicenseNumber>", accessKey);
            sb.AppendFormat("<UserId>{0}</UserId>", username);
            sb.AppendFormat("<Password>{0}</Password>", password);
            sb.Append("</AccessRequest>");
            sb.Append("<?xml version='1.0'?>");
            sb.Append("<TrackRequest xml:lang='en-US'>");
            sb.Append("<Request>");
            sb.Append("<TransactionReference>");
            sb.Append("<CustomerContext>Bare Bones Rate Request</CustomerContext>");
            sb.Append("</TransactionReference>");
            sb.Append("<RequestAction>Track</RequestAction>");
            sb.Append("<RequestOption>activity</RequestOption>");
            sb.Append("</Request>");
            sb.AppendFormat("<TrackingNumber>{0}</TrackingNumber>", trackingNumber);

            sb.Append("</TrackRequest>");
            return sb.ToString();
        }


        private string DoRequest(string url, string requestString)
        {
            //UPS requires TLS 1.2 since May 2016
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            byte[] bytes = Encoding.ASCII.GetBytes(requestString);
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = WebRequestMethods.Http.Post;
            request.ContentType = MimeTypes.ApplicationXWwwFormUrlencoded;
            request.ContentLength = bytes.Length;
            using (var requestStream = request.GetRequestStream())
                requestStream.Write(bytes, 0, bytes.Length);
            using (var response = request.GetResponse())
            {
                string responseXml;
                using (var reader = new StreamReader(response.GetResponseStream()))
                    responseXml = reader.ReadToEnd();

                return responseXml;
            }
        }

        private ShipmentStatusEvent GetStatusEvent(string activityStatusType, string activityStatusCode, string activityStatusDate, string activityStatusTime, string activityStatusActivityLocationAddressCountryCode, string activityStatusActivityLocationAddressCity)
        {
            var ev = new ShipmentStatusEvent();
            switch (activityStatusType)
            {
                case "I":
                    if (activityStatusCode == "DP")
                    {
                        ev.EventName = _localizationService.GetResource("Plugins.Shipping.UPS.Tracker.Departed");
                    }
                    else if (activityStatusCode == "EP")
                    {
                        ev.EventName = _localizationService.GetResource("Plugins.Shipping.UPS.Tracker.ExportScanned");
                    }
                    else if (activityStatusCode == "OR")
                    {
                        ev.EventName = _localizationService.GetResource("Plugins.Shipping.UPS.Tracker.OriginScanned");
                    }
                    else
                    {
                        ev.EventName = _localizationService.GetResource("Plugins.Shipping.UPS.Tracker.Arrived");
                    }
                    break;
                case "X":
                    ev.EventName = _localizationService.GetResource("Plugins.Shipping.UPS.Tracker.NotDelivered");
                    break;
                case "M":
                    ev.EventName = _localizationService.GetResource("Plugins.Shipping.UPS.Tracker.Booked");
                    break;
                case "D":
                    ev.EventName = _localizationService.GetResource("Plugins.Shipping.UPS.Tracker.Delivered");
                    break;
            }
            string dateString = string.Concat(activityStatusDate, " ", activityStatusTime);
            ev.Date = DateTime.ParseExact(dateString, "yyyyMMdd HHmmss", CultureInfo.InvariantCulture);
            ev.CountryCode = activityStatusActivityLocationAddressCountryCode;
            ev.Location = activityStatusActivityLocationAddressCity;
            return ev;
        }

        //---END: Codechages done by (na-sdxcorp\ADas)--------------


        private ShipmentStatusEvent ToStatusEvent(ActivityType activity)
        {
            var ev = new ShipmentStatusEvent();
            switch (activity.Status.Type)
            {
                case "I":
                    if (activity.Status.Code == "DP")
                    {
                        ev.EventName = _localizationService.GetResource("Plugins.Shipping.UPS.Tracker.Departed");
                    }
                    else if (activity.Status.Code == "EP")
                    {
                        ev.EventName = _localizationService.GetResource("Plugins.Shipping.UPS.Tracker.ExportScanned");
                    }
                    else if (activity.Status.Code == "OR")
                    {
                        ev.EventName = _localizationService.GetResource("Plugins.Shipping.UPS.Tracker.OriginScanned");
                    }
                    else
                    {
                        ev.EventName = _localizationService.GetResource("Plugins.Shipping.UPS.Tracker.Arrived");
                    }
                    break;
                case "X":
                    ev.EventName = _localizationService.GetResource("Plugins.Shipping.UPS.Tracker.NotDelivered");
                    break;
                case "M":
                    ev.EventName = _localizationService.GetResource("Plugins.Shipping.UPS.Tracker.Booked");
                    break;
                case "D":
                    ev.EventName = _localizationService.GetResource("Plugins.Shipping.UPS.Tracker.Delivered");
                    break;
            }
            string dateString = string.Concat(activity.Date, " ", activity.Time);
            ev.Date = DateTime.ParseExact(dateString, "yyyyMMdd HHmmss", CultureInfo.InvariantCulture);
            ev.CountryCode = activity.ActivityLocation.Address.CountryCode;
            ev.Location = activity.ActivityLocation.Address.City;
            return ev;
        }
    }

}