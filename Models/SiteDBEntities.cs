using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace AnarkRE.Models
{
    public partial class SiteDBEntities : DbContext
    {
        public SiteDBEntities(bool debug) : base(debug ? "name=SiteDBEntitiesDebug" : "name=SiteDBEntities") { }
    }
}