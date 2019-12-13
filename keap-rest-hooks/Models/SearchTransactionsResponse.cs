using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace keap_rest_hooks.Models
{
    public class SearchTransactionsResponse
    {
        public List<Transaction> transactions { get; set; }
}
}