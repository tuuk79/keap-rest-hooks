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
    public class KeapController : Controller
    {

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ViewLog()
        {
            string logDirectory = ConfigurationManager.AppSettings["LogDirectory"];
            ViewBag.Files = Directory.GetFiles(logDirectory);

            if (Request.Form["ddlDailyLog"] == null)
            {
                return View();
            }

            var filePath = Request.Form["ddlDailyLog"];
            var content = System.IO.File.ReadAllLines(filePath);
            ViewBag.LogText = content;

            return View();
        }

        public ActionResult Authorize()
        {
            var urlManager = new UrlManager();
            var authorizeUrl = urlManager.getAuthorizationUrl();

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

                var accessTokenManager = new AccessTokenManager();
                accessTokenManager.saveAccessToken(authenticationResponse.access_token);

                var referringActionManager = new ReferringActionManager();
                var referringAction = referringActionManager.getReferringAction();

                return await Task.Run(() => Redirect(referringAction));
            }

            return await Task.Run(() => RedirectToAction("Index"));
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

                var eventKeyManager = new EventKeyManager();
                eventKey = eventKeyManager.getEventKey();

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
                var referringActionManager = new ReferringActionManager();
                referringActionManager.saveReferringAction("CreateHook");

                var eventKeyManager = new EventKeyManager();
                eventKeyManager.saveEventKey(eventKey);

                return await Task.Run(() => RedirectToAction("Authorize"));
            }
        }

        public async Task<ActionResult> VerifyHookDelayed()
        {
            var clientId = ConfigurationManager.AppSettings["ClientId"];

            var accessTokenManager = new AccessTokenManager();
            var accessToken = accessTokenManager.getAccessToken();

            var baseApiUrl = ConfigurationManager.AppSettings["BaseApiUrl"];
            var verifyHookDelayedUrl = $"{baseApiUrl}/hooks/{TempData["RestHookKey"].ToString()}/delayedVerify?access_token={accessToken}";

            var payload = new Dictionary<string, string>()
            {
                { "key", TempData["RestHookKey"].ToString() }
            };

            var httpContent = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            var hookSecretManager = new HookSecretManager();
            var hookSecret = hookSecretManager.getHookSecret();

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("X-Hook-Secret", hookSecret);

            var content = new FormUrlEncodedContent(payload);
            var response = await client.PostAsync(verifyHookDelayedUrl, httpContent);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var createHookResponse = JsonConvert.DeserializeObject<CreateHookResponse>(json);
            }

            return await Task.Run(() => View("Index"));
        }

        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> VerifyHook([FromBody] EventSubscriptionPayload eventSubscriptionPayload)
        {
            var xHookSecretHeader = ConfigurationManager.AppSettings["XHookSecretHeader"];
            var xHookSecret = Request.Headers[xHookSecretHeader];

            if (xHookSecret == null)
            {
                var clientId = ConfigurationManager.AppSettings["ClientId"];

                var accessTokenManager = new AccessTokenManager();
                var accessToken = accessTokenManager.getAccessToken();

                if (accessToken == null)
                {
                    var referringActionManager = new ReferringActionManager();
                    referringActionManager.saveReferringAction("VerifyHook");

                    return await Task.Run(() => RedirectToAction("Authorize"));
                }

                if (eventSubscriptionPayload.event_key == "order.edit")
                {
                    var orderId = eventSubscriptionPayload.object_keys.ToList().FirstOrDefault().id;
                    var baseApiUrl = ConfigurationManager.AppSettings["BaseApiUrl"];
                    var url = $"{baseApiUrl}/orders/{orderId}?access_token={accessToken}";

                    var client = new HttpClient();
                    var response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        var results = await response.Content.ReadAsStringAsync();
                        var order = JsonConvert.DeserializeObject<Order>(results);

                        var logFileManager = new LogFileManager();
                        logFileManager.writeToLogFile(eventSubscriptionPayload.event_key, order);
                    }
                }
                else if (eventSubscriptionPayload.event_key == "order.delete")
                {
                    var sinceDate = DateTime.Now.AddMinutes(-2).ToString("yyyy-MM-ddTHH\\:mm\\:ss.000Z");

                    var orderId = eventSubscriptionPayload.object_keys.ToList().FirstOrDefault().id;
                    var baseApiUrl = ConfigurationManager.AppSettings["BaseApiUrl"];
                    var url = $"{baseApiUrl}/transactions?since={sinceDate}&access_token={accessToken}";

                    //2019-12-13T00:20:26.000Z

                    var client = new HttpClient();
                    var response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        // filter by order id for sku info

                        var results = await response.Content.ReadAsStringAsync();
                        var transactions = JsonConvert.DeserializeObject<SearchTransactionsResponse>(results);

                        var logFileManager = new LogFileManager();
                        //logFileManager.writeToLogFile(eventSubscriptionPayload.event_key, order);
                    }

                }
                else if (eventSubscriptionPayload.event_key == "invoice.payment.add")
                {
                    var transactionId = eventSubscriptionPayload.object_keys.ToList().FirstOrDefault().id;
                    var baseApiUrl = ConfigurationManager.AppSettings["BaseApiUrl"];
                    var url = $"{baseApiUrl}/transactions/{transactionId}?access_token={accessToken}";

                    var client = new HttpClient();
                    var response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        var results = await response.Content.ReadAsStringAsync();
                        var transaction = JsonConvert.DeserializeObject<Transaction>(results);

                        var logFileManager = new LogFileManager();
                        logFileManager.writeToLogFile(eventSubscriptionPayload.event_key, transaction.orders.FirstOrDefault());
                    }
                }

                return await Task.Run(() => View("Index"));
            }
            else
            {
                Response.AddHeader(xHookSecretHeader, xHookSecret);

                var hookSecretManager = new HookSecretManager();
                hookSecretManager.saveHookSecret(xHookSecret);

                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }


        }


    }
}