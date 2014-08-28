using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AnarkRE.Models
{
    public class ListingGridItem
    {
        public Guid ListId { get; set; }
        public DateTime CreateDt { get; set; }
        public DateTime ExpireDt { get; set; }
        public string Title { get; set; }
        public decimal PriceBtc { get; set; }
        public decimal PriceUsd { get; set; }
        public string Category { get; set; }
       
        public string Username { get; set; }
        public decimal? Score { get; set; }
        public bool IsBrowse { get; set; }
    }
}