using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using AnarkRE.Models;
using AnarkRE.Bitcoin;

namespace AnarkRE.DAL
{
    public class ListingRepository : IListingRepository, IDisposable
    {
        public SiteDBEntities db;

        public ListingRepository(SiteDBEntities db)
        {
            this.db = db;
        }



        #region IListingRepository Members

        public Dictionary<short, string> GetCatagories()
        {
            return db.Catagories.ToDictionary(s => s.CatId, x => x.Name);
        }

        public IEnumerable<Listing> GetListings()
        {
            return db.Listings.ToList();
        }

        public Listing GetListingByID(Guid listid)
        {
            return db.Listings.Find(listid);
        }

        public void InsertListing(NewListingModel listing)
        {
            decimal btcprice = listing.PegCurrency == "BTC" ? listing.Price : BitcoinPriceData.ToBTC(listing.Price, listing.PegCurrency);
            
            db.Listings.Add(new Listing()
                {
                    ListingId = Guid.NewGuid(),
                    CatId = (short)listing.Category.Value,
                    CreateDate = DateTime.Now,
                    Description = listing.Description,
                    ExpireDate = DateTime.Now.AddDays(30),
                    IsApproved = false,
                    PegCurrency = listing.PegCurrency,
                    PriceBTC = btcprice,
                    PriceUSD = BitcoinPriceData.ToFiat(btcprice, "USD"),
                    Title = listing.Title,
                    UserId = listing.UserId
                });
        }

        public void DeleteListing(Guid listid)
        {
            db.Listings.Remove(db.Listings.Find(listid));
        }

        public void Save()
        {
            db.SaveChanges();
        }

        #endregion

        #region IDisposable Members

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    db.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}