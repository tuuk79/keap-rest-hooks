using keap_rest_hooks.Models;
using System.Configuration;
using System.Linq;

namespace keap_rest_hooks.Managers
{
    public class EventKeyManager
    {
        public string getEventKey()
        {
            using (var db = new NinjacatorsEntities())
            {
                var clientId = ConfigurationManager.AppSettings["ClientId"];
                var eventKey = db.EventKeys
                                .Where(x => x.client_id == clientId)
                                .OrderByDescending(x => x.id)
                                .FirstOrDefault();
                return eventKey.event_key;
            }
        }

        public void saveEventKey(string eventKey)
        {
            using(var db = new NinjacatorsEntities())
            {
                db.EventKeys.Add(new EventKey()
                {
                    client_id = ConfigurationManager.AppSettings["ClientId"],
                    event_key = eventKey
                });
                db.SaveChanges();
            }
        }
    }
}