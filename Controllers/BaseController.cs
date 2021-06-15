using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AnarkRE.Security;
using AnarkRE.Models;
using AnarkRE.DAL;
using System.Net.Sockets;
using System.Net;
using WebMatrix.WebData;
using System.Web.Routing;
using System.IO;
using System.Threading.Tasks;
using System.Web.WebSockets;
using AnarkRE.Bitcoin;
using System.Text;
using System.Web.Security;
using System.Web.Caching;

namespace AnarkRE.Controllers
{
    [HandleError(ExceptionType = typeof(HttpAntiForgeryException), View = "~/Views/Error/Expired.cshtml")]
    public class BaseController : Controller
    {
        protected UnitOfWork data;

        public BaseController()
        {
            this.data = new UnitOfWork();
        }
        
        protected ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
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

                TempData["escrow"] = es > 0 ? Math.Round(es ?? 0.00000000M, 8) : 0.00M;
                

                decimal? rel = data.Escrows.context.Escrows.Where(s => 
                (((s.SellerId == WebSecurity.CurrentUserId && !s.SellerDeleteDt.HasValue) &&
                (s.ClosedBy == (int)EscrowClosedBy.ArbitForSeller || s.ClosedBy == (int)EscrowClosedBy.BuyerRelease
                || s.ClosedBy == (int)EscrowClosedBy.BuyerCancel || s.ClosedBy == (int)EscrowClosedBy.Buyer))

                || (s.BuyerId == WebSecurity.CurrentUserId && !s.BuyerDeleteDt.HasValue) &&
                (s.ClosedBy == (int)EscrowClosedBy.ArbitForBuyer || s.ClosedBy == (int)EscrowClosedBy.Autorelease
                || s.ClosedBy == (int)EscrowClosedBy.SellerCancel || s.ClosedBy == (int)EscrowClosedBy.SellerNoship
                || s.ClosedBy == (int)EscrowClosedBy.SellerReject || s.ClosedBy == (int)EscrowClosedBy.SellerRelease))
                    && s.AcceptId.HasValue && s.EscrowAccept.LastAddressBalance.HasValue).Select(x => x.EscrowAccept.LastAddressBalance).Sum<decimal?>(x => x);

                TempData["released"] = rel > 0 ? Math.Round(rel ?? 0.00000000M, 8) : 0.00M;

                if (Roles.IsUserInRole("arbiter"))
                {
                    decimal? ar = data.Escrows.context.Escrows.Where(s => s.State == (int)EscrowState.Arbitrating
                      && s.EscrowAccept.LastAddressBalance.HasValue).Select(x => x.EscrowAccept.LastAddressBalance).Sum<decimal?>(x => x);
                    TempData[ "arbiting" ] = ar > 0 ? Math.Round(rel ?? 0.00000000M, 8) : 0.00M; ;
                }
            }

        }
    }
}
