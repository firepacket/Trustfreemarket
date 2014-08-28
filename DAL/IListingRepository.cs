using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using AnarkRE.Models;

namespace AnarkRE.DAL
{
    public interface IListingRepository : IDisposable
    {
        IEnumerable<Listing> GetListings();
        Dictionary<short, string> GetCatagories();
        Listing GetListingByID(Guid listid);
        void InsertListing(NewListingModel listing);
        void DeleteListing(Guid listid);
        void Save();
    }
}