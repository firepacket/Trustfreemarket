using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AnarkRE.Models
{
    public class EscrowCreateView
    {
        public string Id { get; set; }
        public ItemCompactView Item { get; set; }
        
        public FeedbackBadgeModel FeedBadge { get; set; }
    }
}