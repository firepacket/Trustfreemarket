using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using AnarkRE.Filters;
using AnarkRE.DAL;
using WebMatrix.WebData;

namespace AnarkRE.Controllers
{
    //[RequireProductionHttps]
    public class BaseController : Controller
    {
        protected UnitOfWork data;

        public BaseController()
        {
            this.data = new UnitOfWork();
        }

        protected override void Dispose(bool disposing)
        {
            data.Dispose();
            base.Dispose(disposing);
        }


        protected override void OnActionExecuting(ActionExecutingContext context)
        {

            if (WebSecurity.IsAuthenticated && WebSecurity.HasUserId) // Top Bar Content
            {
                decimal? es = data.Escrows.context.Escrows.Where(s => (s.BuyerId == WebSecurity.CurrentUserId || s.SellerId == WebSecurity.CurrentUserId)
                && s.AcceptId.HasValue && s.EscrowAccept.LastAddressBalance.HasValue)
                    .Select(w => w.EscrowAccept.LastAddressBalance)
                    .Sum<decimal?>(d => d);

                TempData[ "escrow" ] = es > 0 ? Math.Round(es ?? 0.00000000M, 8) : 0.00M;
                TempData[ "buyerscore" ] = data.Users.GetByID(WebSecurity.CurrentUserId).BuyerScore;
                TempData[ "sellerscore" ] = data.Users.GetByID(WebSecurity.CurrentUserId).SellerScore;

                decimal? rel = data.Escrows.context.Escrows.Where(s =>
                (((s.SellerId == WebSecurity.CurrentUserId && !s.SellerDeleteDt.HasValue) &&
                (s.ClosedBy == (int)EscrowClosedBy.ArbitForSeller || s.ClosedBy == (int)EscrowClosedBy.BuyerRelease
                || s.ClosedBy == (int)EscrowClosedBy.BuyerCancel || s.ClosedBy == (int)EscrowClosedBy.Buyer))

                || (s.BuyerId == WebSecurity.CurrentUserId && !s.BuyerDeleteDt.HasValue) &&
                (s.ClosedBy == (int)EscrowClosedBy.ArbitForBuyer || s.ClosedBy == (int)EscrowClosedBy.Autorelease
                || s.ClosedBy == (int)EscrowClosedBy.SellerCancel || s.ClosedBy == (int)EscrowClosedBy.SellerNoship
                || s.ClosedBy == (int)EscrowClosedBy.SellerReject || s.ClosedBy == (int)EscrowClosedBy.SellerRelease))
                    && s.AcceptId.HasValue && s.EscrowAccept.LastAddressBalance.HasValue).Select(x => x.EscrowAccept.LastAddressBalance).Sum<decimal?>(x => x);

                TempData[ "released" ] = rel > 0 ? Math.Round(rel ?? 0.00000000M, 8) : 0.00M;
            }

        }
    }
}
