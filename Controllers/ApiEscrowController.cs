using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Text;
using System.Threading;

using AnarkRE.Models;
using Casascius.Bitcoin;
using System.Security.Cryptography;
using AnarkRE.Filters;
using AnarkRE.DAL;
using WebMatrix.WebData;
using AnarkRE.Communication;

namespace AnarkRE.Content
{
    [Authorize]
    public class EscrowController : ApiController
    {
        private UnitOfWork data;

        public EscrowController()
        {
            this.data = new UnitOfWork();
        }

        [ValidateJsonAntiForgeryTokenAttribute]
        public HttpResponseMessage Put(string id, EscrowWebCreate form)
        {
            Guid listid;
            if (!string.IsNullOrEmpty(id) && Guid.TryParse(id, out listid))
            {
                Listing list = data.Listings.GetByID(listid);
                if (list != null)
                {
                    if (!form.PubKey.StartsWith("-----BEGIN PUBLIC KEY-----\nMI") ||
                        !form.PubKey.EndsWith("-----END PUBLIC KEY-----"))
                        ModelState.AddModelError("PubKey", "Invalid public key");
                    if (list.User.UserId == WebSecurity.CurrentUserId) // cant buy own item
                        ModelState.AddModelError("id", "Cannot buy own item");
                    if (list.ListingAdditions.Any(s => (ListingAdditionType)s.AdditionType == ListingAdditionType.Shipping)
                    && (!form.ShippingId.HasValue
                    || !list.ListingAdditions.Where(s => (ListingAdditionType)s.AdditionType == ListingAdditionType.Shipping)
                    .Any(s => s.LAId == form.ShippingId.Value))) // Bad shipping selection
                        ModelState.AddModelError("ShippingId", "Invalid shipping id");
                    if ((form.VariationId.HasValue && form.VariationId.Value > -1)
                    && !list.ListingAdditions.Where(s => (ListingAdditionType)s.AdditionType == ListingAdditionType.SingleSelect)
                    .Any(s => s.LAId == form.VariationId.Value)) // Bad variation
                        ModelState.AddModelError("VariationId", "Invalid variation id");

                    if (!ModelState.IsValid)
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
                    else
                    {
                        ListingAddition ship = form.ShippingId.HasValue
                            ? list.ListingAdditions.SingleOrDefault(s => s.LAId == form.ShippingId.Value)
                            : null;
                        ListingAddition vari = form.VariationId.HasValue && form.VariationId.Value > -1
                            ? list.ListingAdditions.SingleOrDefault(s => s.LAId == form.VariationId.Value)
                            : null;


                        EscrowCodeSet cs = new EscrowCodeSet();
                        Escrow escrow = new Escrow()
                        {
                            EscrowId = Guid.NewGuid(),
                            BuyerId = WebMatrix.WebData.WebSecurity.CurrentUserId,
                            SellerId = list.UserId,
                            BuyerPubkey = form.PubKey,
                            BuyerPrivkey = form.EncPrivKey,
                            EicA = cs.EscrowInvitationCodeA,
                            EicB = cs.EscrowInvitationCodeB,
                            State = (int)EscrowState.Created,
                            CreatedDate = DateTime.Now,
                            ListingId = list.ListingId,
                            ShippingName = ship != null ? ship.Name : null,
                            ShippingPrice = ship != null ? ship.Price : new Nullable<decimal>(),
                            VariationName = vari != null ? vari.Name : null
                        };

                        //pubkeymd5 = MD5.Create()
                        //    .ComputeHash(Encoding.UTF8.GetBytes(form.PubKey))
                        //    .Select(x => x.ToString("x2"))
                        //    .Aggregate((i, j) => i + j)
                        
                        try
                        {
                            data.Escrows.Insert(escrow);
                            data.Save();
                            User buyer = data.Users.GetByID(WebMatrix.WebData.WebSecurity.CurrentUserId);

                           
                                string emailBodyText =
                            "<p>User <b>" + buyer.UserName + "</b> wants to purchase your listing for: </p>" +
                            "<p><i>" + escrow.Listing.Title + "</p></i>" +
                            "<p>Click <a href=\"https://anark.it/escrow/\">here</a> to view your transactions</p><br/>" +
                            "<p>You may contact the buyer at <a href=\"mailto:" + buyer.Email + "\">" + buyer.Email + "</a></p>" +
                            "<p>For extra security, ask for the Session Code from the buyer if he has not already emailed it to you.</p>";
                                Globals.SendEmail(list.User.Email, "You have a purchase request!", emailBodyText);

                        }
                        catch (Exception err)
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Failed to save");
                        }

                        return Request.CreateResponse(HttpStatusCode.Created, list.User.Email);
                        
                    }
                }
                else
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Not found");
            }
            else
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Not found");
        }


        [ValidateJsonAntiForgeryTokenAttribute]
        public HttpResponseMessage Post(Guid id, EscrowWebAccept model)
        {

            Escrow escrow = data.Escrows.GetByID(id);
            Listing list = escrow != null ? escrow.Listing : null;

            if (list != null && escrow != null && WebSecurity.CurrentUserId == escrow.SellerId)
            {
                if (escrow.EscrowAccept != null)
                    return Request.CreateResponse(HttpStatusCode.BadRequest);

                if (!model.PubKey.StartsWith("-----BEGIN PUBLIC KEY-----\nMI") ||
            !model.PubKey.EndsWith("-----END PUBLIC KEY-----"))
                    ModelState.AddModelError("PubKey", "Invalid public key");
                byte[] hex = Util.Base58CheckToByteArray(model.BitcoinAddr);
                if (hex == null || hex.Length != 21)
                    ModelState.AddModelError("BitcoinAddr", "Address is not valid.");


                if (!ModelState.IsValid)
                    Request.CreateResponse(HttpStatusCode.BadRequest, ModelState);

                EscrowAccept accept = new EscrowAccept()
                {
                    AutoReleaseDt = DateTime.Now.AddDays(14),
                    CreatedDate = DateTime.Now,
                    EscrowAddress =model.BitcoinAddr,
                    PinvBuyer = model.PinvEncBuyer,
                    PinvSeller = model.PinvEncSeller,
                    SellerPubkey = model.PubKey,
                    SellerPrivkey = model.EncPrivKey
                };

                escrow.State = (int)EscrowState.Accepted;

                try
                {
                    data.EscrowAccepts.Insert(accept);
                    escrow.EscrowAccept = accept;
                    //list.CurrentEscrow = escrow;
                    data.Save();

                    Thread t = new Thread(new ThreadStart(delegate()
                        {
                            try
                            {
                                AnarkRE.Communication.PipeClient pc = new Communication.PipeClient();
                                pc.SendData(accept.AcceptId + "," + accept.EscrowAddress);
                            }
                            catch (Exception err)
                            {
                                Globals.WriteError("pipeerror.log", err);
                            }
                        }));
                    t.IsBackground = true;
                    t.Start();

                    try
                    {

                        string emailBodyText =
                            "<p>The seller has accepted your request to buy the listing: </p>" +
                            "<p><i>" + escrow.Listing.Title + "</p></i>" +
                            "<p>Click <a href=\"https://anark.it/escrow/view/" + escrow.EscrowId.StringWithoutDashes() + "\">here</a> to view this transaction</p><br/>" +
                            "<p>Upon entering your transaction password, the verified escrow bitcoin address will be shown. Send your payment to this bitcoin address and notify the seller so that he may begin delivery." +
                            "<p>After you have received your order, be sure to log back in and release the funds to the seller.</p>";
                            
                        Globals.SendEmail(escrow.UserBuyer.Email, "Purchase Request Accepted!", emailBodyText);
                        
                    }
                    catch { }


                    return Request.CreateResponse(HttpStatusCode.OK, new { });
                }
                catch (Exception err)
                {
                    Globals.WriteError("accepterror.log", err);
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Save failed");
                }
            }
            

            return Request.CreateResponse(HttpStatusCode.NotFound);

          
        }

        protected override void Dispose(bool disposing)
        {
            data.Dispose();
            base.Dispose(disposing);
        }
    }
}
