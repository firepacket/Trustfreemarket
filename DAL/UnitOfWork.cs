using System;
using AnarkRE.Models;

namespace AnarkRE.DAL
{
    public class UnitOfWork : IDisposable
    {
#if DEBUG
        private SiteDBEntities context = new SiteDBEntities(true);
#else
        private SiteDBEntities context = new SiteDBEntities(false);
#endif
        private GenericRepository<Listing> listingRepository;
        private GenericRepository<Escrow> escrowRepository;
        private GenericRepository<EscrowAccept> escrowacceptRepository;
        private GenericRepository<Catagory> catRepository;
        private GenericRepository<User> userRepository;
        private GenericRepository<Feedback> feedRepository;
        private GenericRepository<ContactMsg> contactsRepository;
        private GenericRepository<ListingAddition> ladditionsRepository;

        public UnitOfWork()
        {
            
        }

        public GenericRepository<Listing> Listings
        {
            get
            {

                if (this.listingRepository == null)
                {
                    this.listingRepository = new GenericRepository<Listing>(context);
                }
                return listingRepository;
            }
        }

        public GenericRepository<Escrow> Escrows
        {
            get
            {

                if (this.escrowRepository == null)
                {
                    this.escrowRepository = new GenericRepository<Escrow>(context);
                }
                return escrowRepository;
            }
        }

        public GenericRepository<EscrowAccept> EscrowAccepts
        {
            get
            {

                if (this.escrowacceptRepository == null)
                {
                    this.escrowacceptRepository = new GenericRepository<EscrowAccept>(context);
                }
                return escrowacceptRepository;
            }
        }

        public GenericRepository<Catagory> Catagories
        {
            get
            {

                if (this.catRepository == null)
                {
                    this.catRepository = new GenericRepository<Catagory>(context);
                }
                return catRepository;
            }
        }

        public GenericRepository<User> Users
        {
            get
            {

                if (this.userRepository == null)
                {
                    this.userRepository = new GenericRepository<User>(context);
                }
                return userRepository;
            }
        }

        public GenericRepository<Feedback> Feedbacks
        {
            get
            {

                if (this.feedRepository == null)
                {
                    this.feedRepository = new GenericRepository<Feedback>(context);
                }
                return feedRepository;
            }
        }

        public GenericRepository<ContactMsg> ContactMsgs
        {
            get
            {

                if (this.contactsRepository == null)
                {
                    this.contactsRepository = new GenericRepository<ContactMsg>(context);
                }
                return contactsRepository;
            }
        }

        public GenericRepository<ListingAddition> ListingAdditions
        {
            get
            {

                if (this.ladditionsRepository == null)
                {
                    this.ladditionsRepository = new GenericRepository<ListingAddition>(context);
                }
                return ladditionsRepository;
            }
        }

        public void Save()
        {
            context.SaveChanges();
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}