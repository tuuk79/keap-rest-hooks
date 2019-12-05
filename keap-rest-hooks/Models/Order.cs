using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace keap_rest_hooks.Models
{
    public class Order
    {
        public string id { get; set; }
        public string title { get; set; }
        public string status { get; set; }
        public string recurring { get; set; }
        public string total { get; set; }
        public Contact contact { get; set; }
        public string notes { get; set; }
        public string terms { get; set; }
        public string creation_date { get; set; }
        public string modification_date { get; set; }
        public string lead_affiliate_id { get; set; }
        public string sales_affiliate_id { get; set; }
        public string total_paid { get; set; }
        public string total_due { get; set; }
    }


//  "shipping_information": {
//    "id": 27336,
//    "first_name": "Duane",
//    "middle_name": "",
//    "last_name": "Gray",
//    "company": "",
//    "phone": "(301) 482-0305",
//    "street1": "26200 Purdum Rd",
//    "street2": "",
//    "city": "Damascus",
//    "state": "MD",
//    "zip": "20872",
//    "country": "United States"
//  },
//  "refund_total": 0,
//  "allow_payment": null,
//  "allow_paypal": null,
//  "order_items": [
//    {
//      "id": 98260,
//      "name": "Momentum Zone Pro",
//      "description": " ",
//      "type": "Product",
//      "notes": null,
//      "quantity": 1,
//      "cost": 0,
//      "price": 197,
//      "discount": null,
//      "product": {
//        "id": 4598,
//        "name": "Momentum Zone Pro",
//        "sku": "",
//        "description": "",
//        "shippable": false,
//        "taxable": false
//      }
//    },
//    {
//      "id": 98262,
//      "name": "Dynamic Risk Reward Calculator Tool",
//      "description": "Bundle item",
//      "type": "Product",
//      "notes": "Appended through API.",
//      "quantity": 1,
//      "cost": 0,
//      "price": 0,
//      "discount": null,
//      "product": {
//        "id": 4612,
//        "name": "Dynamic Risk Reward Calculator Tool",
//        "sku": "122888",
//        "description": "",
//        "shippable": false,
//        "taxable": false
//      }
//    },
//    {
//      "id": 98264,
//      "name": "Momentum Continuation Zones ",
//      "description": "Bundle item",
//      "type": "Product",
//      "notes": "Appended through API.",
//      "quantity": 1,
//      "cost": 0,
//      "price": 0,
//      "discount": null,
//      "product": {
//        "id": 4608,
//        "name": "Momentum Continuation Zones ",
//        "sku": "122886",
//        "description": "",
//        "shippable": false,
//        "taxable": false
//      }
//    },
//    {
//      "id": 98266,
//      "name": "Momentum Scalping Zones",
//      "description": "Bundle item",
//      "type": "Product",
//      "notes": "Appended through API.",
//      "quantity": 1,
//      "cost": 0,
//      "price": 0,
//      "discount": null,
//      "product": {
//        "id": 4610,
//        "name": "Momentum Scalping Zones",
//        "sku": "122885",
//        "description": "",
//        "shippable": false,
//        "taxable": false
//      }
//    },
//    {
//      "id": 98268,
//      "name": "Momentum Reversal Zones",
//      "description": "Bundle item",
//      "type": "Product",
//      "notes": "Appended through API.",
//      "quantity": 1,
//      "cost": 0,
//      "price": 0,
//      "discount": null,
//      "product": {
//        "id": 4606,
//        "name": "Momentum Reversal Zones",
//        "sku": "122887",
//        "description": "",
//        "shippable": false,
//        "taxable": false
//      }
//    }
//  ]
//}
}