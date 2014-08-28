using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Net.Http;
using WebMatrix.WebData;

using AnarkRE.Models;
using AnarkRE.DAL;
using Casascius.Bitcoin;


namespace AnarkRE.Controllers
{
    [Authorize]
    public class EscrowController : BaseController
    {
        public ActionResult Index()
        {
            var bescrows = data.Escrows.Get(s => WebSecurity.CurrentUserId == s.BuyerId && s.BuyerDeleteDt == null);
            var sescrows = data.Escrows.Get(s => WebSecurity.CurrentUserId == s.SellerId && s.SellerDeleteDt == null);
            var escrows = bescrows.Union(sescrows)
                .Select(s => s.ToGridView(s.BuyerId == WebSecurity.CurrentUserId));
    
            ViewBag.Title = "Escrow transaction list";
            return View(escrows.OrderByDescending(s => s.CreatedDt).AsQueryable());
        }


        public ActionResult View(string id)
        {
            Guid guid;
            if (!Guid.TryParse(id, out guid))
                return RedirectToAction("notfound", "error");

            Escrow escrow = data.Escrows.GetByID(guid);
            Listing list = escrow != null ? escrow.Listing : null;
            

            if (list != null && escrow != null &&
                (WebSecurity.CurrentUserId == escrow.BuyerId || WebSecurity.CurrentUserId == escrow.SellerId))
            {
                Guid listid = list.ListingId;

                EscrowView model = new EscrowView()
                {
                    Id = listid.StringWithoutDashes(),
                    IsBuyer = WebSecurity.CurrentUserId == escrow.BuyerId,
                    EscrowId = escrow.EscrowId.StringWithoutDashes(),
                    Item = ListingToItem(list),
                    ClosedBy = escrow.ClosedBy != null
                        ? (EscrowClosedBy)escrow.ClosedBy
                        : new Nullable<EscrowClosedBy>(),
                    FeedBadge = WebSecurity.CurrentUserId == escrow.BuyerId ?
                    new FeedbackBadgeModel()
                    {
                        Sales = list.UserProfile.FeedbacksAbout.Count,
                        Username = list.UserProfile.UserName,
                        Score = list.UserProfile.SellerScore
                    }
                    :
                    new FeedbackBadgeModel()
                    {
                        Sales = escrow.UserBuyer.FeedbacksAbout.Count,
                        Username = escrow.UserBuyer.UserName,
                        Score = escrow.UserBuyer.BuyerScore
                    }
                };

                model.Item.Options = new PurchaseOptions()
                    {
                        ShippingName = escrow.ShippingName,
                        ShippingPrice = escrow.ShippingPrice,
                        VariationChoice = escrow.VariationName
                    };

                if (model.IsBuyer) // buyer
                {
                    if (escrow.EscrowAccept != null)
                    {
                        model.MyPubkey = escrow.BuyerPubkey;
                        model.ThierPubkey = escrow.EscrowAccept.SellerPubkey;
                        model.Eic = escrow.EicB;
                        model.PivEnc = escrow.EscrowAccept.PinvBuyer;
                        model.MyPrivkeyEnc = escrow.BuyerPrivkey;
                        model.BitcoinAddress = escrow.EscrowAccept.EscrowAddress;

                        if (escrow.SellerReleaseDt != null)
                            model.ReleaseCode = escrow.EicA;
                    }
                    else
                        return RedirectToAction("Index");
                }
                else // seller
                {
                    model.ThierPubkey = escrow.BuyerPubkey;
                    model.Eic = escrow.EicA;

                    if (escrow.EscrowAccept != null)
                    {
                        model.MyPubkey = escrow.EscrowAccept.SellerPubkey;
                        model.PivEnc = escrow.EscrowAccept.PinvSeller;
                        model.MyPrivkeyEnc = escrow.EscrowAccept.SellerPrivkey;
                        model.BitcoinAddress = escrow.EscrowAccept.EscrowAddress;

                        if (escrow.BuyerReleaseDt != null)
                            model.ReleaseCode = escrow.EicB;
                    }
                }

                model.Balance = 0;

                try
                {
                    if (!string.IsNullOrEmpty(model.BitcoinAddress))
                    {
                        model.Balance = decimal.Parse(new WebClient().DownloadString("https://blockchain.info/q/addressbalance/" + model.BitcoinAddress)) / 100000000M;
                        if (!escrow.EscrowAccept.LastAddressBalance.HasValue || escrow.EscrowAccept.LastAddressBalance.Value != model.Balance)
                        {
                            escrow.EscrowAccept.LastAddressBalance = model.Balance;
                            data.Save();
                        }
                    }
                }
                catch
                { }

                ViewBag.Title = "View escrow transaction";
                return View(model);

            }
            else
                return RedirectToAction("notfound", "error");
   

            
        }

        private ItemCompactView ListingToItem(Listing list)
        {
            return new ItemCompactView()
                    {
                        Category = list.Catagory.Name,
                        ListId = list.ListingId.StringWithoutDashes(),
                        PeggedTo = list.PegCurrency,
                        PriceBTC = list.GetPriceBtc(),
                        PriceCUR = list.PriceCUR,
                        PriceUSD = list.GetPriceUsd(),
                        Title = list.Title,
                        SellerUser = list.UserProfile.UserName,
                        SellerScore = (list.UserProfile.FeedbacksAbout.Count > 0 ?
                            list.UserProfile.FeedbacksAbout.Select(s => (decimal)s.Stars).Average() :
                            new Nullable<decimal>()),
                       
                    };
        }

        


        
        public ActionResult Create(string id, int? Variation, int? ShippingSelection)
        {
            Guid listid;
            if (!Guid.TryParse(id, out listid))
                return RedirectToAction("notfound", "error");

            Listing list = data.Listings.GetByID(listid);

            if (list != null)
            {
                if (list.Escrows.Where(s => s.ClosedDt == null)
                    .Any(s => s.BuyerId == WebSecurity.CurrentUserId))
                    return RedirectToAction("Index"); // user already has active escrow

                if (list.UserProfile.UserId == WebSecurity.CurrentUserId) // cant buy own item
                    return RedirectToAction("view", "listings", new { id = id });

                if (list.ListingAdditions.Any(s => (ListingAdditionType)s.AdditionType == ListingAdditionType.Shipping)
                    && (!ShippingSelection.HasValue
                    || !list.ListingAdditions.Where(s => (ListingAdditionType)s.AdditionType == ListingAdditionType.Shipping)
                       .Any(s => s.LAId == ShippingSelection.Value))) // Bad shipping selection
                    return RedirectToAction("view", "listings", new { id = id });

                if ((Variation.HasValue && Variation.Value > -1)
                    && !list.ListingAdditions.Where(s => (ListingAdditionType)s.AdditionType == ListingAdditionType.SingleSelect)
                        .Any(s => s.LAId == Variation.Value)) // Bad variation
                    return RedirectToAction("view", "listings", new { id = id });

                ListingAddition ship = ShippingSelection.HasValue
                    ? list.ListingAdditions.SingleOrDefault(s => s.LAId == ShippingSelection.Value)
                    : null;
                ListingAddition vari = Variation.HasValue && Variation.Value > -1
                    ? list.ListingAdditions.SingleOrDefault(s => s.LAId == Variation.Value)
                    : null;

                EscrowCreateView view = new EscrowCreateView()
                {
                    Id = listid.StringWithoutDashes(),
                    Item = ListingToItem(list),
                    FeedBadge = new FeedbackBadgeModel()
                    {
                        Sales = list.UserProfile.FeedbacksAbout.Count,
                        Username = list.UserProfile.UserName,
                        Score = list.UserProfile.SellerScore
                    }
                };

                view.Item.Options = new PurchaseOptions()
                    {
                        ShippingId = ShippingSelection,
                        ShippingName = ship != null ? ship.Name : null,
                        ShippingPrice = ship != null ? ship.Price : null,
                        VariationId = Variation,
                        VariationChoice = vari != null ? vari.Name : null
                    };

                ViewBag.Title = "Request buy: " + list.Title;
                return View(view);
            }


            return  RedirectToAction("notfound", "error");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Release(string id)
        {
            Guid guid;
            if (!Guid.TryParse(id, out guid))
                return RedirectToAction("notfound", "error");

            Escrow escrow = data.Escrows.GetByID(guid);
            if (escrow != null && 
                (escrow.BuyerId == WebSecurity.CurrentUserId || escrow.SellerId == WebSecurity.CurrentUserId))
            {

                escrow.ClosedDt = DateTime.Now;
                escrow.State = (int)EscrowState.Closed;
                //if (escrow.EscrowId == escrow.Listing.ActiveEscrowId) // remove active from listing once purchaced - disabled
                //{
                //    escrow.Listing.ActiveEscrowId = null;
                //    data.Listings.Update(escrow.Listing);
                //}

                if (WebSecurity.CurrentUserId == escrow.SellerId)
                {
                    if (escrow.EscrowAccept == null)
                    {
                        escrow.ClosedBy = (int)EscrowClosedBy.SellerReject;
                        escrow.SellerDeleteDt = DateTime.Now;
                    }
                    else
                    {
                        escrow.ClosedBy = (int)EscrowClosedBy.Seller;
                        escrow.SellerReleaseDt = DateTime.Now;

                        string emailBodyText =
                            "<p>The seller <b>" + escrow.UserSeller.UserName + "</b> has released the escrow funds to you for the listing:</p>" +
                            "<p><i>" + escrow.Listing.Title + "</p></i>" +
                            "<p>To view the transaction and claim your funds <a href=\""
                            + Url.Action("view", "escrow", new { id = escrow.EscrowId.StringWithoutDashes() }, Request.Url.Scheme)
                            + "\">click here</a></p><br/>" +
                            "<p>We recommend you save this transaction page by using File -> Save as a backup before deleting the transaction.</p>" +
                            "<p>You will need to enter your escrow transaction password to calculate the bitcoin private key. If you have lost this password then you will be unable to spend your coins and must return them to the other party. If this happens to you, please <a href=\"mailto:support@anark.it\">email support</a></p>";
                            
                        Globals.SendEmail(escrow.UserBuyer.Email, "Claim your escrow funds!", emailBodyText);
                           
                    }
                }
                else if (WebSecurity.CurrentUserId == escrow.BuyerId)
                {
                    if (escrow.EscrowAccept == null)
                    {
                        escrow.ClosedBy = (int)EscrowClosedBy.BuyerCancel;
                        escrow.BuyerDeleteDt = DateTime.Now;
                    }
                    else
                    {
                        escrow.ClosedBy = (int)EscrowClosedBy.Buyer;
                        escrow.BuyerReleaseDt = DateTime.Now;

                        string emailBodyText =
                            "<p>The buyer <b>" + escrow.UserBuyer.UserName + "</b> has released the escrow funds to you for the listing:</p>" +
                            "<p><i>" + escrow.Listing.Title + "</p></i>" +
                            "<p>To view the transaction and claim your funds <a href=\""
                            + Url.Action("view", "escrow", new { id = escrow.EscrowId.StringWithoutDashes() }, Request.Url.Scheme)
                            + "\">click here</a></p><br/>" +
                            "<p>We recommend you save this transaction page by using File -> Save as a backup before deleting the transaction.</p>" +
                            "<p>You will need to enter your escrow transaction password to calculate the bitcoin private key. If you have lost this password then you will be unable to spend your coins and must return them to the other party. If this happens to you, please <a href=\"mailto:support@anark.it\">email support</a></p>";

                        Globals.SendEmail(escrow.UserSeller.Email, "Claim your escrow funds!", emailBodyText);
                    }
                }
                data.Save();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(string id)
        {
            Guid guid;
            if (!Guid.TryParse(id, out guid))
                return RedirectToAction("notfound", "error");
            Escrow es = data.Escrows.GetByID(guid);
            if ((EscrowState)es.State == EscrowState.Closed)
            {
                EscrowClosedBy closedby = (EscrowClosedBy)es.ClosedBy.Value;
                if (closedby == EscrowClosedBy.BuyerCancel || closedby == EscrowClosedBy.SellerReject)
                {
                    if (WebSecurity.CurrentUserId == es.SellerId)
                    {
                        es.SellerDeleteDt = DateTime.Now;
                        data.Save();
                    }
                    else if (WebSecurity.CurrentUserId == es.BuyerId)
                    {
                        es.BuyerDeleteDt = DateTime.Now;
                        data.Save();
                    }
                    
                }
                else
                    return View(new FeedbackModel() { EscrowId = guid.StringWithoutDashes() });
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Finalize(FeedbackModel model)
        {
            Guid guid;
            if (!Guid.TryParse(model.EscrowId, out guid))
                return RedirectToAction("notfound", "error");
            Escrow es = data.Escrows.GetByID(guid);
            if ((EscrowState)es.State == EscrowState.Closed)
            {
                EscrowClosedBy closedby = (EscrowClosedBy)es.ClosedBy.Value;
                if (closedby != EscrowClosedBy.BuyerCancel || closedby != EscrowClosedBy.SellerReject)
                {
                    if (WebSecurity.CurrentUserId == es.SellerId)
                    {
                        es.SellerDeleteDt = DateTime.Now;
                        Feedback fb = new Feedback()
                        {
                            AboutSeller = false,
                            AboutUserId = es.BuyerId,
                            CreatedDate = DateTime.Now,
                            FromUserId = WebSecurity.CurrentUserId,
                            ListingId = es.ListingId,
                            Message = model.Message,
                            Stars = model.Score,
                        };
                        data.Feedbacks.Insert(fb);
                        if (es.UserBuyer.FeedbacksAbout.Count > 0)
                        {
                            es.UserBuyer.TotalScore = es.UserBuyer.FeedbacksAbout
                                .Select(s => (decimal)s.Stars).Average();
                            if (es.UserBuyer.FeedbacksAbout.Where(s => !s.AboutSeller).Count() > 0)
                            {
                                es.UserBuyer.BuyerScore = es.UserBuyer.FeedbacksAbout.Where(s => !s.AboutSeller)
                                    .Select(s => (decimal)s.Stars).Average();
                            }
                        }
                        data.Save();
                    }
                    else if (WebSecurity.CurrentUserId == es.BuyerId)
                    {
                        es.BuyerDeleteDt = DateTime.Now;
                        Feedback fb = new Feedback()
                        {
                            AboutSeller = true,
                            AboutUserId = es.SellerId,
                            CreatedDate = DateTime.Now,
                            FromUserId = WebSecurity.CurrentUserId,
                            ListingId = es.ListingId,
                            Message = model.Message,
                            Stars = model.Score
                        };
                        data.Feedbacks.Insert(fb);

                        if (es.UserSeller.FeedbacksAbout.Count > 0)
                        {
                            es.UserSeller.TotalScore = es.UserSeller.FeedbacksAbout
                                .Select(s => (decimal)s.Stars).Average();
                            if (es.UserSeller.FeedbacksAbout.Where(s => s.AboutSeller).Count() > 0)
                            {
                            es.UserSeller.SellerScore = es.UserSeller.FeedbacksAbout.Where(s => s.AboutSeller)
                                .Select(s => (decimal)s.Stars).Average();
                            }
                        }
                        data.Save();
                        
                    }
                }
            }
            return RedirectToAction("Index");
        }

    }
}
