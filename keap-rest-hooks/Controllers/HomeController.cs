using keap_rest_hooks.Managers;
using keap_rest_hooks.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
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
                ReferringAction referringAction = null;

                var accessTokenManager = new AccessTokenManager();
                accessTokenManager.saveAccessToken(authenticationResponse.access_token);

                using (var db = new NinjacatorsEntities())
                {
                    referringAction = db.ReferringActions.Where(x => x.client_id == clientId).OrderByDescending(x => x.id).FirstOrDefault();
                }

                return await Task.Run(() => Redirect(referringAction.referring_action));
            }

            return await Task.Run(() => Redirect("index"));
        }

        public async Task<ActionResult> Events()
        {
            var accessTokenManager = new AccessTokenManager();
            string accessToken = accessTokenManager.getAccessToken();

            if (accessToken == null)
            {
                using (var db = new NinjacatorsEntities())
                {
                    db.ReferringActions.Add(new ReferringAction()
                    {
                        client_id = ConfigurationManager.AppSettings["ClientId"],
                        referring_action = "events"
                    });
                    db.SaveChanges();
                }
                return await Task.Run(() => RedirectToAction("Authorize"));
            }

            var baseApiUrl = ConfigurationManager.AppSettings["BaseApiUrl"];
            var url = $"{baseApiUrl}/hooks/event_keys?access_token={accessToken}";

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
            if (eventKey == null)
            {
                var manager = new AccessTokenManager();
                var accessToken = manager.getAccessToken();

                var clientId = ConfigurationManager.AppSettings["ClientId"];
                var baseApiUrl = ConfigurationManager.AppSettings["BaseApiUrl"];
                var hookUrl = ConfigurationManager.AppSettings["HookUrl"];

                var createHookUrl = $"{baseApiUrl}/hooks?access_token={accessToken}";

                using (var db = new NinjacatorsEntities())
                {
                    eventKey = db.EventKeys.Where(x => x.client_id == clientId).OrderByDescending(x => x.id).FirstOrDefault().event_key;
                }

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

                return await Task.Run(() => RedirectToAction("VerifyHookDelayed"));
            }
            else
            {
                using (var db = new NinjacatorsEntities())
                {
                    db.ReferringActions.Add(new ReferringAction()
                    {
                        client_id = ConfigurationManager.AppSettings["ClientId"],
                        referring_action = "createhook"
                    });

                    db.EventKeys.Add(new EventKey()
                    {
                        client_id = ConfigurationManager.AppSettings["ClientId"],
                        event_key = eventKey
                    });

                    db.SaveChanges();
                }

                return await Task.Run(() => RedirectToAction("Authorize"));
            }
        }

        public async Task<ActionResult> VerifyHookDelayed()
        {
            string accessToken = null;
            var clientId = ConfigurationManager.AppSettings["ClientId"];

            using (var db = new NinjacatorsEntities())
            {
                var latestAccessTokenRecord = db.AccessTokens
                                .Where(x => x.client_id == clientId)
                                .OrderByDescending(x => x.id)
                                .FirstOrDefault();
                accessToken = latestAccessTokenRecord.access_token;
            }

            var baseApiUrl = ConfigurationManager.AppSettings["BaseApiUrl"];
            var verifyHookDelayedUrl = $"{baseApiUrl}/hooks/{TempData["RestHookKey"].ToString()}/delayedVerify?access_token={accessToken}";

            var payload = new Dictionary<string, string>()
            {
                { "key", TempData["RestHookKey"].ToString() }
            };

            var httpContent = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

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
        public async Task<ActionResult> VerifyHook([FromBody] EventSubscriptionPayload eventSubscriptionPayload)
        {
            var xHookSecretHeader = ConfigurationManager.AppSettings["XHookSecretHeader"];
            var xHookSecret = Request.Headers[xHookSecretHeader];

            if (xHookSecret == null)
            {
                string accessToken = null;
                using (var db = new NinjacatorsEntities())
                {
                    var clientId = ConfigurationManager.AppSettings["ClientId"];
                    var latestAccessTokenRecord = db.AccessTokens
                                    .Where(x => x.client_id == clientId)
                                    .OrderByDescending(x => x.id)
                                    .FirstOrDefault();
                    accessToken = latestAccessTokenRecord.access_token;
                }

                if (accessToken == null)
                {
                    using (var db = new NinjacatorsEntities())
                    {
                        db.ReferringActions.Add(new ReferringAction()
                        {
                            client_id = ConfigurationManager.AppSettings["ClientId"],
                            referring_action = "verifyhook"
                        });
                        db.SaveChanges();
                    }
                    return await Task.Run(() => RedirectToAction("Authorize"));
                }

                Order order = null;

                // handle order.edit
                if (eventSubscriptionPayload.event_key == "order.edit")
                {
                    var orderId = eventSubscriptionPayload.object_keys[0].id;

                    // look up order using order id
                    var baseApiUrl = ConfigurationManager.AppSettings["BaseApiUrl"];
                    var url = $"{baseApiUrl}/orders/{orderId}?access_token={accessToken}";

                    var client = new HttpClient();
                    var response = await client.GetAsync(url);



                    if (response.IsSuccessStatusCode)
                    {
                        var results = await response.Content.ReadAsStringAsync();
                        order = JsonConvert.DeserializeObject<Order>(results);
                    }
                }

                string path = @"c:\log";

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                path = Path.Combine(path, $"dailylog_{DateTime.Now.ToString("yyyyMMdd")}.csv");

                if (System.IO.File.Exists(path))
                {
                    using (StreamWriter sw = System.IO.File.AppendText(path))
                    {
                        sw.WriteLine(order.contact.email);
                        sw.WriteLine();
                    }
                }
                else
                {
                    using (StreamWriter sw = System.IO.File.CreateText(path))
                    {
                        sw.WriteLine("Hello");
                        sw.WriteLine("And");
                        sw.WriteLine("Welcome");
                        sw.WriteLine();
                    }
                }

                return await Task.Run(() => View("index"));
            }
            else
            {
                //var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                //responseMessage.Headers.Add(xHookSecretHeader, xHookSecret);

                Response.AddHeader(xHookSecretHeader, xHookSecret);

                // store hook secret
                using (var db = new NinjacatorsEntities())
                {
                    db.HookSecrets.Add(new HookSecret()
                    {
                        client_id = ConfigurationManager.AppSettings["ClientId"],
                        x_hook_secret = xHookSecret
                    });
                    db.SaveChanges();
                }

                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }


        }


    }
}