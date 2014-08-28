using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AnarkRE.Models
{
    public class EscrowGridView
    {
        public string Id { get; set; }
        public string EscrowId { get; set; }
        public string Catagory { get; set; }
        public decimal PriceBtc { get; set; }
        public decimal PriceUsd { get; set;}
        public string Party { get; set; }
        public EscrowState State { get; set; }
        public string Username { get; set; }
        public decimal? Score { get; set; }
        public string Title { get; set; }
        public EscrowClosedBy? ClosedBy { get; set; }
        public bool IsBuyer { get; set; }
        public DateTime CreatedDt { get; set; }
        public decimal? AddressBalance { get; set; }
    }
}