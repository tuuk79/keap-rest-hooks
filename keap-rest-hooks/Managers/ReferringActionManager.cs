using keap_rest_hooks.Models;
using System.Configuration;
using System.Linq;

namespace keap_rest_hooks.Managers
{
    public class ReferringActionManager
    {
        public string getReferringAction()
        {
            using (var db = new NinjacatorsEntities())
            {
                var clientId = ConfigurationManager.AppSettings["ClientId"];
                var referringAction = db.ReferringActions
                                .Where(x => x.client_id == clientId)
                                .OrderByDescending(x => x.id)
                                .FirstOrDefault();
                return referringAction.referring_action;
            }
        }

        public void saveReferringAction(string referringAction)
        {
            using(var db = new NinjacatorsEntities())
            {
                db.ReferringActions.Add(new ReferringAction()
                {
                    client_id = ConfigurationManager.AppSettings["ClientId"],
                    referring_action = referringAction
                });
                db.SaveChanges();
            }
        }
    }
}