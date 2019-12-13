using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace keap_rest_hooks.Models
{
    public class Order
    {
        public string id { get; set; }
        public Contact contact { get; set; }
        public List<OrderItem> order_items { get; set; }
    }
}