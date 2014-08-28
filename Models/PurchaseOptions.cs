using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AnarkRE.Models
{
    public class PurchaseOptions
    {
        public int? ShippingId { get; set; }
        public string ShippingName { get; set; }
        public decimal? ShippingPrice { get; set; }
        public string VariationChoice { get; set; }
        public int? VariationId { get; set; }
    }
}