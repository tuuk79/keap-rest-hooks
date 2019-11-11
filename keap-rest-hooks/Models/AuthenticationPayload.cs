using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace keap_rest_hooks.Models
{
    public class AuthenticationPayload
    {
        public string client_id { get; set; }
        public string client_secret { get; set; }
        public string code { get; set; }
        public string grant_type { get; set; }
        public string redirect_uri { get; set; }
        public string state { get; set; }
    }
}