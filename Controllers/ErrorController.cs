using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AnarkRE.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Error!";
            return View();
        }

        public ActionResult Notfound()
        {
            ViewBag.Title = "404 Not Found";
            return View();
        }
    }
}
