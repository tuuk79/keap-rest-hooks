using keap_rest_hooks.Models;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace keap_rest_hooks.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Authorize()
        {
            var callbackUrl = "https://localhost:44399/home/authenticate";
            var authorizeUrl = $"https://signin.infusionsoft.com/app/oauth/authorize?client_id={Constants.Constants.ClientId}&redirect_uri={callbackUrl}&response_type=code";

            return Redirect($"{authorizeUrl}");
        }

        public async Task Authenticate(string code)
        {
            var authenticateUrl = $"https://api.infusionsoft.com/crm/rest/v1/token";
            //var authenticateUrl = $"https://accounts.infusionsoft.com/app/oauth/userToken";
            //var authenticateUrl = $"https://accounts.infusionsoft.com/app/oauth/token";

            var payload = new AuthenticationPayload()
            {
                client_id = Constants.Constants.ClientId,
                client_secret = Constants.Constants.ClientSecret,
                code = code,
                grant_type = "authorization_code",
                redirect_uri = "https://localhost:44399/home/authenticate"
            };

            var json = JsonConvert.SerializeObject(payload);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/x-www-form-urlencoded");

            var client = new HttpClient();
            var response = await client.PostAsync(authenticateUrl, httpContent);

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var thing = JsonConvert.DeserializeObject<AuthenticationResponse>(jsonString);
            }

            //return View();
        }
    }
}