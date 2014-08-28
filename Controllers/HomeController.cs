using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AnarkRE.Models;

namespace AnarkRE.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            var features = data.Listings.Get(s => s.FeaturedDate.HasValue 
                && s.IsApproved && !s.IsDeleted && !s.IsPrivate && s.ExpireDate >= DateTime.Now)
                .OrderByDescending(s => s.FeaturedDate)
                .Select(s => new FeaturedItemView()
                {
                    Category = s.Catagory.Name,
                    ExpireDt = s.ExpireDate,
                    ListId = s.ListingId.StringWithoutDashes(),
                    Price = new Price(s.PriceCUR, s.PegCurrency),
                    SellerScore = s.UserProfile.SellerScore,
                    SellerUser = s.UserProfile.UserName,
                    Title = s.Title
                }).ToList();

            return View(features);
        }




    }
}
