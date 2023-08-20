using System;
using System.Web;

namespace Nop.Web.Framework.Tridion.HostNameMapping
{
    /// <summary>
    /// Custom Exceptions impmented in Tridion Integration pieces SODMYWAY-2956
    /// </summary>
    public class HostNameNotFoundException : HttpException
    {
        private readonly string message;
        public override string Message
        {
            get
            {
                return message;
            }
        }
        public override int ErrorCode
        {
            get
            {
                return 400; //Bad Request - The server cannot or will not process the request due to something that is perceived to be a client error.
            }
        }
        public HostNameNotFoundException(string hostName)
        {
            message = "Host name '" + hostName + "' was not found. Please correct the Host Name Mapping configuration.";
        }
    }

    public class AppSettingsNotFoundException : Exception
    {
        private readonly string message;
        public override string Message
        {
            get
            {
                return message;
            }
        }

        public AppSettingsNotFoundException(string hostName)
        {
            message = "Application settings for host name '" + hostName + "' were not found or could not be loaded.";
        }
    }
}
