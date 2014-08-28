using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AnarkRE.Controllers
{
    public class InfoController : BaseController
    {

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }

        public ActionResult Faq()
        {
            ViewBag.Title = "FAQ";
            return View();
        }

        public ActionResult Terms()
        {
            ViewBag.Title = "Terms of Service";
            return View();
        }

        public ActionResult Abuse()
        {
            return View();
        }

        public ActionResult Privacy()
        {
            ViewBag.Title = "Privacy Policy";
            return View();
        }

        public ActionResult Demo()
        {
            return View();
        }
    }
}
