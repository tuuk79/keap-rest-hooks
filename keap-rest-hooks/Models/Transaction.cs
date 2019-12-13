using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace keap_rest_hooks.Models
{
    public class Transaction
    {
        public int id { get; set; }
        public double amount { get; set; }
        public string currency { get; set; }
        public string gateway { get; set; }
        public string type { get; set; }
        public string status { get; set; }
        public List<Order> orders { get; set; }
    }
}