using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AnarkRE.Models
{
    public class ItemCompactView
    {
        public string ListId { get; set; }
        public string Title { get; set; }
        public decimal PriceBTC { get; set; }
        public decimal PriceUSD { get; set; }
        public decimal PriceCUR { get; set; }
        public string PeggedTo { get; set; }
        public string Category { get; set; }
        public string SellerUser { get; set; }
        public decimal? SellerScore { get; set; }
        public PurchaseOptions Options { get; set; }
    }
}