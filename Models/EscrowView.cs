using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AnarkRE.Models
{
    public class EscrowView
    {
        public string Id { get; set; }
        public string EscrowId { get; set; }
        public bool IsBuyer { get; set; }
        public string MyPubkey { get; set; }
        public string ThierPubkey { get; set; }
        public string Eic { get; set; }
        public string PivEnc { get; set; }
        public string MyPrivkeyEnc { get; set; }
        public string BitcoinAddress { get; set; }
        public string ReleaseCode { get; set; }
        public decimal Balance { get; set; }
        public EscrowState State { get; set; }
        public EscrowClosedBy? ClosedBy { get; set; }
        public ItemCompactView Item { get; set; }
        public FeedbackBadgeModel FeedBadge { get; set; }
        
    }

   
}