using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace keap_rest_hooks.Models
{
    public class Product
    {
        public string id { get; set; }
        public string name { get; set; }
        public string sku { get; set; }
        public string description { get; set; }
        public string shippable { get; set; }
        public string taxable { get; set; }
    }
}