using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AnarkRE.Models
{
    public partial class Escrow
    {
        public EscrowGridView ToGridView(bool isBuyer)
        {
            return new EscrowGridView()
            {
                IsBuyer = isBuyer,
                Id = this.ListingId.StringWithoutDashes(),
                EscrowId = this.EscrowId.StringWithoutDashes(),
                Catagory = this.Listing.Catagory.Name,
                PriceBtc = this.Listing.GetPriceBtc(),
                PriceUsd = this.Listing.GetPriceUsd(),
                Party = isBuyer ? "Buyer" : "Seller",
                State = (EscrowState)this.State,
                Username = isBuyer ? this.UserSeller.UserName : this.UserBuyer.UserName,
                Score = isBuyer ? this.UserSeller.SellerScore : this.UserBuyer.BuyerScore,
                Title = this.Listing.Title,
                ClosedBy = this.ClosedBy != null ? (EscrowClosedBy)this.ClosedBy : new Nullable<EscrowClosedBy>(),
                CreatedDt = this.CreatedDate,
                AddressBalance = this.EscrowAccept != null ? this.EscrowAccept.LastAddressBalance : null
            };
        }

        public Price GetTotal()
        {
            Price price = new Price(this.Listing.PriceCUR, this.Listing.PegCurrency);
            Price sprice = this.ShippingPrice.HasValue
                ? new Price(this.ShippingPrice.Value, this.Listing.PegCurrency)
                : null;
            Price total = sprice != null ? price.Add(sprice) : price;
            return total;
        }
    }
}