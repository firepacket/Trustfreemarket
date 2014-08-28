using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;


using System.Web;
using System.Web.Mvc;

using AnarkRE.Models;
using AnarkRE.DAL;

namespace AnarkRE.Controllers
{
    public class NotifyController : BaseController
    {

        public ActionResult Index(int id, string addr, decimal amount)
        {
            if (Request.IsLocal)
            {
            
                EscrowAccept escrowaccept = data.EscrowAccepts.GetByID(id);
                Escrow escrow = escrowaccept.Escrow.FirstOrDefault();
                if (escrow != null && escrowaccept.EscrowAddress == addr && escrow != null)
                {
                    escrowaccept.LastAddressBalance = escrowaccept.LastAddressBalance.HasValue
                        ? escrowaccept.LastAddressBalance.Value + amount
                        : amount;
                    data.Save();

                    Price total = escrow.GetTotal();
                    decimal balance = (total.BTC - amount);

                    string direction = balance >= 0 ? "<p>Looks like the payment was a little short. Try contacting the buyer and asking them to send the difference. If you cannot come to a resolution, contact support</p>"
                        : "<p>The buyer's balance has been paid, now it's time for you to fill the order!</p>";
                    
                    string emailBodyText =
                                "<p>The user <a href=\"mailto:" + escrow.UserBuyer.Email + "\">" + escrow.UserBuyer.UserName + "</a> has sent " + amount.FormatNumber() + " BTC to the escrow address for item:</p>" +
                                "<p><i>" + escrow.Listing.Title + "</p></i><br/>" +
                                "<p>Total: " + total.BTC.FormatNumber() + " BTC</p>" +
                                "<p>Paid: " + amount.FormatNumber() + " BTC</p>" +
                                "<p><b>Balance: " + (total.BTC - amount).FormatNumber() + " BTC</b></p><br/>" +
                                direction +
                                "<p>Click <a href=\""
                                + Url.Action("view", "escrow", new { id = escrow.EscrowId.StringWithoutDashes() }, Request.Url.Scheme)
                                + "\">here</a> to view this transaction</p><br/>";

                    Globals.SendEmail(escrow.UserSeller.Email, "You've received " + amount.FormatNumber() + " BTC!", emailBodyText);
                    return Content("Dispatched email to " + escrow.UserBuyer.Email);
                }
                else
                    return Content("Bitcoin address does not match!");
            }
            else
                return Content("Unauthorized");
        }

        //public ActionResult Test()
        //{
        //    EscrowAccept ea = data.EscrowAccepts.GetByID(4);
        //    AnarkRE.Communication.PipeClient pc = new Communication.PipeClient();
        //    pc.SendData(ea.AcceptId + "," + ea.EscrowAddress);
        //    return Content("ok");
        //}


    }
}
