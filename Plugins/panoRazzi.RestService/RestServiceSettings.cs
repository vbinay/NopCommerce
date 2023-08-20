using Nop.Core.Configuration;

namespace panoRazzi.RestService
{
    public class RestServiceSettings : ISettings
    {
        public string ApiToken { get; set; }
    }
}