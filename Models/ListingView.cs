using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AnarkRE.Models
{
    public class ListingView
    {
        public string ListId { get; set; }
        public DateTime Created { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsApproved { get; set; }
        public decimal PriceBtc { get; set; }
        public decimal PriceUsd { get; set; }
        public decimal PriceCur { get; set; }
        public string PegCurrency { get; set; }
        public string Category { get; set; }


        public int UserId { get; set; }

        public List<FeedbackView> Feedback { get; set; }
        public FeedbackBadgeModel FeedBadge { get; set; }

        //public string ActiveEscrowId { get; set; }
        //public int? ActiveBuyerId { get; set; }
        public string CurrentUserEscrowId { get; set; }

        public int PictureCount { get; set; }
        public BaseListingModel BaseListing { get; set; }

        public bool? HasContacted { get; set; }

        public List<ListingAddition> Additions { get; set; }
        public DateTime? FeaturedDate { get; set; }

        public int OpenTransctions { get; set; }
        public DateTime ExpireDt { get; set; }
    }

    public class FeedbackView
    {
        public string Username { get; set; }
        public string Message { get; set; }
        public decimal Score { get; set; }
        public DateTime Date { get; set;}
    }
}