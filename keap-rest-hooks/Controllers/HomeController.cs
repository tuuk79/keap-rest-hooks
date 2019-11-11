using keap_rest_hooks.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
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

        public ActionResult Authorize()
        {
            var callbackUrl = "https://localhost:44399/home/authenticate";
            var authorizeUrl = $"https://signin.infusionsoft.com/app/oauth/authorize?client_id={Constants.Constants.ClientId}&redirect_uri={callbackUrl}&response_type=code&state=doy";

            return Redirect($"{authorizeUrl}");
        }

        public async Task<ActionResult> Authenticate(string code, string state)
        {
            var authenticateUrl = $"https://api.infusionsoft.com/token";

            var payload = new Dictionary<string, string>()
            {
                { "client_id", Constants.Constants.ClientId },
                { "client_secret", Constants.Constants.ClientSecret },
                { "code", code },
                { "grant_type", "authorization_code" },
                { "redirect_uri", "https://localhost:44399/home/authenticate" },
                { "state", state }
            };

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            var content = new FormUrlEncodedContent(payload);
            var response = await client.PostAsync(authenticateUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var authenticationResponse = JsonConvert.DeserializeObject<AuthenticationResponse>(json);

                HttpCookie token = new HttpCookie("access_token", authenticationResponse.access_token);
                Response.Cookies.Add(token);
            }

            return await Task.Run(() => Redirect("about"));
        }

        public async Task<ActionResult> Events()
        {
            var url = $"{Constants.Constants.HooksUrl}/event_keys?access_token={Request.Cookies["access_token"].Value}";
            var client = new HttpClient();
            var response = await client.GetAsync(url);
            IEnumerable<string> events = null;

            if (response.IsSuccessStatusCode)
            {
                var results = await response.Content.ReadAsStringAsync();
                events = JsonConvert.DeserializeObject<IEnumerable<string>>(results);
            }

            return await Task.Run(() => View(events));
        }

        public async Task<ActionResult> Hooks()
        {
            var url = $"{Constants.Constants.HooksUrl}/event_keys?access_token={Request.Cookies["access_token"].Value}";
            var client = new HttpClient();
            var response = await client.GetAsync(url);
            IEnumerable<string> events = null;

            if (response.IsSuccessStatusCode)
            {
                var results = await response.Content.ReadAsStringAsync();
                events = JsonConvert.DeserializeObject<IEnumerable<string>>(results);
            }

            return await Task.Run(() => View(events));
        }

        public async Task<ActionResult> CreateHook()
        {
            var authenticateUrl = $"https://api.infusionsoft.com/hooks?access_token={Request.Cookies["access_token"].Value}";

            var payload = new Dictionary<string, string>()
            {
                { "eventKey", "contact.add" },
                { "hookUrl", "https://192.168.43.143:45455/home/verifyhook" }
            };

            var httpContent = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("accept", "application/json");

            var content = new FormUrlEncodedContent(payload);
            var response = await client.PostAsync(authenticateUrl, httpContent);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
            }

            return await Task.Run(() => View());
        }

        [HttpPost]
        public HttpResponseMessage VerifyHook()
        {
            // header and body json
            // if hook secret, then return hook secret for verification using delayed
            // if body json has object keys, then save whole body to file on serverfd

            var hookSecret = Request.Headers["X-Hook-Secret"];
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            responseMessage.Headers.Add("X-Hook-Secret", hookSecret);
            return responseMessage;
        }
    }
}