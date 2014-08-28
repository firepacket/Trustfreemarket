using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using AnarkRE.Filters;
using AnarkRE.DAL;

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
    }
}
