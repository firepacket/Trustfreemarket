using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AnarkRE.Models
{
    public class FeaturedItemView
    {
        public string ListId { get; set; }
        public string Title { get; set; }
        public Price Price { get; set; }
        public string Category { get; set; }
        public string SellerUser { get; set; }
        public decimal? SellerScore { get; set; }
        public DateTime ExpireDt { get; set; }
    }
}