using keap_rest_hooks.Models;
using System.Configuration;
using System.Linq;

namespace keap_rest_hooks.Managers
{
    public class HookSecretManager
    {
        public string getHookSecret()
        {
            using (var db = new NinjacatorsEntities())
            {
                var clientId = ConfigurationManager.AppSettings["ClientId"];
                var hookSecret = db.HookSecrets
                                .Where(x => x.client_id == clientId)
                                .OrderByDescending(x => x.id)
                                .FirstOrDefault();
                return hookSecret.x_hook_secret;
            }
        }

        public void saveHookSecret(string hookSecret)
        {
            using(var db = new NinjacatorsEntities())
            {
                db.HookSecrets.Add(new HookSecret()
                {
                    client_id = ConfigurationManager.AppSettings["ClientId"],
                    x_hook_secret = hookSecret
                });
                db.SaveChanges();
            }
        }
    }
}