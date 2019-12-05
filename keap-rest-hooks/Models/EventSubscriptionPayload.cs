using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace keap_rest_hooks.Models
{
    public class EventSubscriptionPayload
    {
        public string event_key { get; set; }
        public string object_type { get; set; }
        public string api_url { get; set; }
        public List<ObjectKey> object_keys { get; set; }
    }
}