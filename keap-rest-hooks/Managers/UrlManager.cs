using keap_rest_hooks.Models;
using System.Configuration;
using System.Linq;

namespace keap_rest_hooks.Managers
{
    public class UrlManager
    {
        public UrlManager()
        {

        }
        
        public string getAuthorizationUrl()
        {
            var clientId = ConfigurationManager.AppSettings["ClientId"];
            var redirectUri = ConfigurationManager.AppSettings["RedirectUri"];
            var baseAuthorizeUrl = ConfigurationManager.AppSettings["BaseAuthorizeUrl"];
            var responseType = ConfigurationManager.AppSettings["ResponseType"];
            var state = ConfigurationManager.AppSettings["State"];

            var authorizeUrl = $"{baseAuthorizeUrl}?client_id={clientId}&redirect_uri={redirectUri}&response_type={responseType}&state={state}";

            return authorizeUrl;
        }
    }
}