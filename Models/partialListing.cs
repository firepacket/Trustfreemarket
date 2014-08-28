using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.IO;
using AnarkRE.Bitcoin;

namespace AnarkRE.Models
{
    public partial class Listing
    {
        public int GetPictureCount()
        {
            try
            {
                string imageDir = Globals.PicturePath;
                string picDir = Path.Combine(imageDir, this.ListingId.StringWithoutDashes());
                return Directory.GetFiles(picDir, "*_l.png").Count();            }
            catch { return 0; }
        }

        public decimal GetPriceBtc()
        {
            //if (this.PegCurrency == "BTC")
            //    return this.PriceBTC;
            //else
            //    return BitcoinPriceData.ToBTC(this.PriceCUR, this.PegCurrency);
            return new Price(this.PriceCUR, this.PegCurrency).BTC;
        }

        public decimal GetPriceUsd()
        {
            //if (this.PegCurrency == "BTC")
            //    return BitcoinPriceData.ToFiat(this.PriceBTC, "USD");
            //else
            //{
            //    var btc = BitcoinPriceData.ToBTC(this.PriceCUR, this.PegCurrency);
            //    return BitcoinPriceData.ToFiat(btc, "USD");
            //}
            return new Price(this.PriceCUR, this.PegCurrency).USD;
        }

        public ListingGridItem ToGridItem(bool isBrowse)
        {
            return new ListingGridItem()
            {
                ListId = this.ListingId,
                CreateDt = this.CreateDate,
                Title = this.Title,
                PriceBtc = this.GetPriceBtc(),
                PriceUsd = this.GetPriceUsd(),
                Category = this.Catagory.Name,
                Username = this.UserProfile.UserName,
                Score = this.UserProfile.SellerScore,
                IsBrowse = isBrowse
            };
        }

        public bool IsAlive()
        {
            return !this.IsDeleted && this.IsApproved && DateTime.Now <= this.ExpireDate;
        }

        //public bool IsAlive
        //{
        //    get { return !this.IsDeleted && this.IsApproved && DateTime.Now <= this.ExpireDate; }
        //}

        public bool IsExpired()
        {
            return DateTime.Now > this.ExpireDate;
        }
    }
}