using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace keap_rest_hooks.Models
{
    public class Contact
    {
        public string id { get; set; }
        public string email { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string company_name { get; set; }
        public string job_title { get; set; }
    }
}