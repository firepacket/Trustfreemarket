using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using System.Web.Security;

using AnarkRE.Models;
using AnarkRE.DAL;
using AnarkRE.Bitcoin;
using AnarkRE.Filters;
using System.IO;
using WebMatrix.WebData;
using System.Web.Routing;

namespace AnarkRE.Controllers
{
    
    public class ListingsController : BaseController
    {

        [Authorize]
        public ActionResult Index()
        {
            ViewBag.Title = "My Listings";
            return View(data.Listings.Get(s => s.UserId == WebSecurity.CurrentUserId
                && DateTime.Now <= s.ExpireDate && !s.IsDeleted)
                .OrderByDescending(s => s.CreateDate)
                .Select(s => s.ToGridItem(false)).AsQueryable());
        }

        
        public ActionResult Images(string id)
        {
            if (id.Contains("_"))
            {
                string[] split = id.Split(new char[] { '_' });
                Guid guid;
                int num;

                if (split.Length == 3 && split[1].Length == 1 && split[2].Length == 1
                    && Guid.TryParse(split[0], out guid)
                    && int.TryParse(split[1], out num)
                    && Directory.Exists(Path.Combine(Globals.PicturePath, 
                    guid.StringWithoutDashes())))
                {
                    string listingdir = Path.Combine(Globals.PicturePath, guid.StringWithoutDashes());
                    switch (split[2].ToLower())
                    {
                        case "l":
                            return base.File(Path.Combine(listingdir, num + "_l.png"), "image/png");
                        case "m":
                            return base.File(Path.Combine(listingdir, num + "_m.png"), "image/png");
                        case "s":
                            return base.File(Path.Combine(listingdir, num + "_s.png"), "image/png");
                    }
                }
            }
            return HttpNotFound();
        }

        [Authorize]
        public ActionResult Create()
        {
            ViewBag.Title = "Create new listing";
            var cats = data.Catagories.Get().ToDictionary(s => s.CatId, s => s.Name);
            SelectList list = new SelectList(cats, "Key", "Value");
            return View(new NewListingModel() { Categories = list });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [InitializeSimpleMembership]
        public ActionResult Create(NewListingModel form)
        {
            Price price = new Price(form.Price, form.PegCurrency);
            List<string> perrs;
            if (!price.CheckValid(out perrs))
                foreach (string er in perrs)
                    ModelState.AddModelError("Price", er);


            if (!form.CheckImageValidAndPrep())
                ModelState.AddModelError("Picture", "Please add a picture");

            if (!ModelState.IsValid)
            {
                var cats = data.Catagories.Get().ToDictionary(s => s.CatId, s => s.Name);
                SelectList list = new SelectList(cats, "Key", "Value");
                return View(new NewListingModel() { Categories = list });
            }
            else
            {
                form.UserId = WebSecurity.CurrentUserId;
                //decimal btcprice = form.PegCurrency == "BTC" ? form.Price : 
                //    BitcoinPriceData.ToBTC(form.Price, form.PegCurrency);

                

                var list = new Listing()
                {
                    ListingId = Guid.NewGuid(),
                    CatId = (short)form.Category.Value,
                    CreateDate = DateTime.Now,
                    Description = form.Description,
                    ExpireDate = DateTime.Now.AddDays(Globals.ListingDays),
                    IsApproved = true,
                    PegCurrency = form.PegCurrency,
                    PriceBTC = price.BTC,
                    PriceUSD = price.USD,
                    Title = form.Title,
                    UserId = form.UserId,
                    PriceCUR = price.Value,
                    IsDeleted = false,
                    IsPrivate = false

                };

                data.Listings.Insert(list);
                data.Save();
                form.ProcessPicture(list.ListingId);


                return RedirectToAction("view", new { id = list.ListingId.ToString().Replace("-", "") });
            }

            
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddPic(ListingView listpic)
        {
            string[] formats = new string[] { ".jpg", ".png", ".gif", ".jpeg" }; 

    

            Guid id;
            if (Guid.TryParse(listpic.ListId, out id)
                && listpic.BaseListing.Picture != null
                && formats.Any(item => listpic.BaseListing.Picture.FileName.EndsWith(item, StringComparison.OrdinalIgnoreCase)))
            {
                Listing list = data.Listings.GetByID(id);
                if (list != null && list.IsAlive() && list.UserId == WebSecurity.CurrentUserId)
                {
                    if (listpic.BaseListing.CheckImageValidAndPrep())
                        listpic.BaseListing.ProcessPicture(id);
                }
            }
            return RedirectToAction("View", new { id = id.StringWithoutDashes() });
        }


        public ActionResult View(string id)
        {
            Guid guid;
            if (!Guid.TryParse(id, out guid))
                return RedirectToAction("notfound", "error");

            Listing list = data.Listings.GetByID(guid);
            if (list == null || !list.IsAlive())
                return RedirectToAction("notfound", "error");

            var feeds = list.UserProfile.FeedbacksAbout.OrderByDescending(s => s.CreatedDate)
            .Select(s => new FeedbackView()
            {
                Date = s.CreatedDate,
                Message = s.Message,
                Score = s.Stars,
                Username = s.UserFrom.UserName
            }).Take(5).ToList();

            //feeds.Add(new FeedbackView() { Date = DateTime.Now, Username = "testuser", Score = 4, Message = "Slow delivery" });
            //feeds.Add(new FeedbackView() { Date = DateTime.Now, Username = "anotheruser", Score = 5, Message = "I thought about it for a while, couldn't decide. but then I did. thanks!" });
            //feeds.Add(new FeedbackView() { Date = DateTime.Now, Username = "anotheruser", Score = 3, Message = "I thought about it for a while, couldn't decide. but then I did. thanks! I thought about it for a while, couldn't decide. but then I did. thanks! I thought about it for a while, couldn't decide. but then I did. thanks! I thought about it for a while, couldn't decide. but then I did. thanks! sadsad" });

            ListingView model = new ListingView()
            {
                Category = list.Catagory.Name,
                Created = list.CreateDate,
                Description = list.Description,
                Feedback = feeds,
                //FeedbackScore = list.UserProfile.SellerScore,
                ListId = list.ListingId.StringWithoutDashes(),
                PegCurrency = list.PegCurrency,
                //Sales = list.UserProfile.FeedbacksAbout.Count,
                Title = list.Title,
                //Username = list.UserProfile.UserName,
                IsApproved = list.IsApproved,
                UserId = list.UserId,
                PriceCur = list.PriceCUR,
                //ActiveBuyerId = list.ActiveEscrowId.HasValue ? list.CurrentEscrow.BuyerId : new Nullable<int>(),
                //ActiveEscrowId = list.ActiveEscrowId.HasValue ? list.ActiveEscrowId.Value.StringWithoutDashes() : "",
                PictureCount = 1,
                BaseListing = null,
                CurrentUserEscrowId = list.Escrows.Where(s => s.BuyerId == WebSecurity.CurrentUserId && s.ClosedDt == null).Select(s => s.EscrowId.StringWithoutDashes()).FirstOrDefault(),
                Additions = list.ListingAdditions.ToList(),
                FeedBadge = new FeedbackBadgeModel()
                {
                    Sales = list.UserProfile.FeedbacksAbout.Count,
                    Username = list.UserProfile.UserName,
                    Score = list.UserProfile.SellerScore
                },
                FeaturedDate = list.FeaturedDate,
                OpenTransctions = list.Escrows.Where(s => s.ClosedDt == null).Count(),
                ExpireDt = list.ExpireDate
            };
            model.PictureCount = list.GetPictureCount();
            model.PriceBtc = list.GetPriceBtc();
            model.PriceUsd = list.GetPriceUsd();

            if (User.Identity.IsAuthenticated)
                model.HasContacted = data.ContactMsgs.Get(s => s.ListingId == list.ListingId && s.UserId == WebSecurity.CurrentUserId).FirstOrDefault() != null;



            ViewBag.Title = "Viewing: " + model.Title;
            return View(model);
        }

        
        public ActionResult Browse()
        {
            ViewBag.Title = "Browse listings";
            return View(data.Listings.Get(s =>
                !s.IsDeleted
                && s.IsApproved && !s.IsPrivate
                && DateTime.Now <= s.ExpireDate)
                .OrderByDescending(s => s.CreateDate)
                .Select(s => s.ToGridItem(true)).AsQueryable());
        }

        public ActionResult Search(string q)
        {
            object htmlAttributes = new { grid_filter = "Title__2__" + q };
            RouteValueDictionary result = new RouteValueDictionary();
            foreach (System.ComponentModel.PropertyDescriptor property in System.ComponentModel.TypeDescriptor.GetProperties(htmlAttributes))
            {
                result.Add(property.Name.Replace('_', '-'), property.GetValue(htmlAttributes));
            }
            return RedirectToAction("browse", result);
        }

        public ActionResult Category(string q)
        {
            object htmlAttributes = new { grid_filter = "Category__1__" + q };
            RouteValueDictionary result = new RouteValueDictionary();
            foreach (System.ComponentModel.PropertyDescriptor property in System.ComponentModel.TypeDescriptor.GetProperties(htmlAttributes))
            {
                result.Add(property.Name.Replace('_', '-'), property.GetValue(htmlAttributes));
            }
            return RedirectToAction("browse", result);
        }

  


        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(Guid id)
        {
            Listing list = data.Listings.GetByID(id);
            if (list != null && !list.IsDeleted && (WebSecurity.CurrentUserId == list.UserId || User.IsInRole("admin")))
            {
                if (list.Escrows.Count == 0 || list.Escrows.All(s => s.ClosedDt != null)) // ensure no active escrows
                {
                    if (list.Feedbacks.Count == 0)
                    {
                        var escs = list.Escrows.ToList();
                        foreach (var e in escs)
                        {
                            list.Escrows.Remove(e);
                            if (e.EscrowAccept != null)
                                data.EscrowAccepts.Delete(e.EscrowAccept);
                            data.Escrows.Delete(e);
                        }

                        var adds = list.ListingAdditions.ToList();
                        foreach (var a in adds)
                        {
                            list.ListingAdditions.Remove(a);
                            data.ListingAdditions.Delete(a);
                        }

                        if (list.ContactMsgs.Count() > 0)
                            foreach (ContactMsg m in list.ContactMsgs.ToList())
                                data.ContactMsgs.Delete(m);

                        data.Listings.Delete(list);

                        try
                        {
                            Directory.Delete(Path.Combine(Globals.PicturePath, list.ListingId.StringWithoutDashes()), true);
                        }
                        catch { }
                    }
                    else
                    {
                        list.IsDeleted = true;
                    }
                    data.Save();
                }
                return RedirectToAction("Index");
            }
            else
                throw new HttpException(401, "Unauthorized");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Message(Guid id, string msg)
        {
            if (msg.Length > 12 && msg.Length <= 600)
            {
                Listing list = data.Listings.GetByID(id);
                if (list != null && list.IsAlive() && WebSecurity.CurrentUserId != list.UserId)
                {
                    ContactMsg cm = data.ContactMsgs.Get(s => s.ListingId == id && s.UserId == WebSecurity.CurrentUserId).FirstOrDefault();
                    if (cm == null)
                    {
                        cm = new ContactMsg() { ListingId = id, UserId = WebSecurity.CurrentUserId };

                        try
                        {
                            string emailBodyText =
                        "<p>User  has sent you the following message about the item: </p>" +
                        "<p><i>" + list.Title + "</p></i><br/>" +
                        "<p><b>Message:</b></p>" +
                        "<p><pre>" + System.Web.HttpUtility.HtmlEncode(msg) + "</pre></p>" +
                        "<p><b><a href='mailto:" + data.Users.GetByID(WebSecurity.CurrentUserId).Email + "'>Send Reply</a></b></p>";
                            Globals.SendEmail(list.UserProfile.Email, "Contacted by user: " + WebSecurity.CurrentUserName, emailBodyText);
                            data.ContactMsgs.Insert(cm);
                            data.Save();
                        }
                        catch { }

                        
                    }
                }
            }
            return RedirectToAction("view", new { id = id.StringWithoutDashes() });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Addition(Guid id, string name, decimal? price, ListingAdditionType latype)
        {
            if (name.Length <= 50 && name.Length > 1 && price.HasValue && price.Value >= 0 
                && latype != ListingAdditionType.Description)
            {
                
                Listing list = data.Listings.GetByID(id);
                if (list != null && !list.IsDeleted && !list.IsExpired() && WebSecurity.CurrentUserId == list.UserId)
                    if (list.ListingAdditions.Count(s => (ListingAdditionType)s.AdditionType == latype) < Globals.ListAddOpts[latype])
                    {
                        ListingAddition la = new ListingAddition()
                        {
                            AdditionType = (short)latype,
                            CreatedDate = DateTime.Now,
                            ListingId = list.ListingId,
                            Name = name
                        };
                        if (latype == ListingAdditionType.Shipping) // only allow price on shipping for now
                            la.Price = price;
                        data.ListingAdditions.Insert(la);
                        data.Save();
                    }
                
            }
            return RedirectToAction("view", new { id = id.StringWithoutDashes() });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Append(Guid id, string descadd)
        {
            if (descadd.Length <= 500 && descadd.Length > 3)
            {
                Listing list = data.Listings.GetByID(id);
                if (list != null && !list.IsDeleted && !list.IsExpired() && WebSecurity.CurrentUserId == list.UserId)
                    if (list.ListingAdditions.Count(s => (ListingAdditionType)s.AdditionType == ListingAdditionType.Description) < Globals.ListAddOpts[ListingAdditionType.Description])
                    {
                        ListingAddition la = new ListingAddition()
                        {
                            AdditionType = (short)ListingAdditionType.Description,
                            CreatedDate = DateTime.Now,
                            ListingId = list.ListingId,
                            Description = descadd
                        };
                        data.ListingAdditions.Insert(la);
                        data.Save();
                    }

            }
            return RedirectToAction("view", new { id = id.StringWithoutDashes() });
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Feature(Guid id, bool featured)
        {
            Listing list = data.Listings.GetByID(id);
            if (list != null && !list.IsDeleted && !list.IsExpired())
            {
                if (featured)
                    list.FeaturedDate = DateTime.Now;
                else
                    list.FeaturedDate = null;

                data.Save();
            }
            return RedirectToAction("view", new { id = id.StringWithoutDashes() });
        }

        protected override void Dispose(bool disposing)
        {
            data.Dispose();
            base.Dispose(disposing);
        }
    }
}
