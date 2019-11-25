using keap_rest_hooks.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace keap_rest_hooks.Controllers
{
    public class HomeController : Controller
    {

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Authorize()
        {
            var clientId = ConfigurationManager.AppSettings["ClientId"];
            var redirectUri = ConfigurationManager.AppSettings["RedirectUri"];
            var baseAuthorizeUrl = ConfigurationManager.AppSettings["BaseAuthorizeUrl"];
            var responseType = ConfigurationManager.AppSettings["ResponseType"];
            var state = ConfigurationManager.AppSettings["State"];

            var authorizeUrl = $"{baseAuthorizeUrl}?client_id={clientId}&redirect_uri={redirectUri}&response_type={responseType}&state={state}";

            return Redirect($"{authorizeUrl}");
        }

        public async Task<ActionResult> Authenticate(string code, string state)
        {
            var baseTokenUrl = ConfigurationManager.AppSettings["BaseTokenUrl"];
            var authenticateUrl = $"{baseTokenUrl}";
            var clientId = ConfigurationManager.AppSettings["ClientId"];

            var payload = new Dictionary<string, string>()
            {
                { "client_id", clientId },
                { "client_secret", ConfigurationManager.AppSettings["ClientSecret"] },
                { "code", code },
                { "grant_type", ConfigurationManager.AppSettings["GrantType"] },
                { "redirect_uri", ConfigurationManager.AppSettings["RedirectUri"] },
                { "state", state }
            };
            var content = new FormUrlEncodedContent(payload);

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("accept", "application/json");

            var response = await client.PostAsync(authenticateUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var authenticationResponse = JsonConvert.DeserializeObject<AuthenticationResponse>(json);

                var tokenCookie = new HttpCookie("access_token", authenticationResponse.access_token);
                Response.Cookies.Add(tokenCookie);
            }

            // get referring action
            ReferringAction referringAction = null;
            using (var db = new NinjacatorsEntities())
            {
                referringAction = db.ReferringActions.Where(x => x.client_id == clientId).OrderByDescending(x => x.id).FirstOrDefault();
            }

            return await Task.Run(() => Redirect(referringAction.referring_action));
        }

        public async Task<ActionResult> Events()
        {
            // authenticate if necessary
            if (Request.Cookies["access_token"] == null)
            {
                using (var db = new NinjacatorsEntities())
                {
                    var referringActions = db.Set<ReferringAction>();
                    referringActions.Add(new ReferringAction()
                    {
                        client_id = ConfigurationManager.AppSettings["ClientId"],
                        referring_action = "events"
                    });
                    db.SaveChanges();
                }
                return await Task.Run(() => RedirectToAction("Authorize"));
            }

            var baseApiUrl = ConfigurationManager.AppSettings["BaseApiUrl"];
            var url = $"{baseApiUrl}/hooks/event_keys?access_token={Request.Cookies["access_token"].Value}";

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

        public async Task<ActionResult> CreateHook(string eventKey)
        {
            // authenticate if necessary
            if (Request.Cookies["access_token"] == null)
            {
                // authenticate

            }

            var baseApiUrl = ConfigurationManager.AppSettings["BaseApiUrl"];
            var hookUrl = ConfigurationManager.AppSettings["HookUrl"];

            var createHookUrl = $"{baseApiUrl}/hooks?access_token={Request.Cookies["access_token"].Value}";

            var payload = new Dictionary<string, string>()
            {
                { "eventKey", eventKey },
                { "hookUrl", hookUrl }
            };

            var httpContent = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("accept", "application/json");

            var content = new FormUrlEncodedContent(payload);
            var response = await client.PostAsync(createHookUrl, httpContent);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var createHookResponse = JsonConvert.DeserializeObject<CreateHookResponse>(json);

                TempData["RestHookKey"] = createHookResponse.key;
            }

            return await Task.Run(() => Redirect("index"));
        }

        public async Task<ActionResult> VerifyHookDelayed()
        {
            var baseApiUrl = ConfigurationManager.AppSettings["BaseApiUrl"];
            var accessToken = Request.Cookies["access_token"].Value;
            var verifyHookDelayedUrl = $"{baseApiUrl}/hooks/{TempData["RestHookKey"].ToString()}/delayedVerify?access_token={accessToken}";

            var payload = new Dictionary<string, string>()
            {
                { "key", TempData["RestHookKey"].ToString() }
            };

            var httpContent = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            var clientId = ConfigurationManager.AppSettings["ClientId"];

            using (var db = new NinjacatorsEntities())
            {
                var xHookSecret = db.HookSecrets.Where(x => x.client_id == clientId).OrderByDescending(x => x.id).FirstOrDefault();

                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("accept", "application/json");
                client.DefaultRequestHeaders.Add("x-hook-secret", xHookSecret.x_hook_secret);

                var content = new FormUrlEncodedContent(payload);
                var response = await client.PostAsync(verifyHookDelayedUrl, httpContent);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var createHookResponse = JsonConvert.DeserializeObject<CreateHookResponse>(json);
                }
            }

            return await Task.Run(() => View("index"));
        }

        [System.Web.Mvc.HttpPost]
        public HttpResponseMessage VerifyHook([FromBody] string content)
        {
            // header and body json
            // if hook secret, then return hook secret for verification using delayed
            // if body json has object keys, then save whole body to file on server
            var xHookSecretHeader = ConfigurationManager.AppSettings["XHookSecretHeader"];

            var xHookSecret = Request.Headers[xHookSecretHeader];

            if (xHookSecret == null)
            {
                // event triggered
                // need to capture body
                var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);

                return responseMessage;
            }
            else
            {
                // regular verification of hook right during creation
                var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                responseMessage.Headers.Add(xHookSecretHeader, xHookSecret);

                // store hook secret
                using (var context = new NinjacatorsEntities())
                {
                    var hookSecrets = context.Set<HookSecret>();
                    hookSecrets.Add(new HookSecret()
                    {
                        client_id = ConfigurationManager.AppSettings["ClientId"],
                        x_hook_secret = xHookSecret
                    });
                    context.SaveChanges();
                }

                return responseMessage;
            }

            
        }
    }
}