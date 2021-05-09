using AnarkRE.Filters;
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
        public ActionResult Expired()
        {
            ViewBag.Tite = "Page expired";
            
            return View();
        }
        public ActionResult Banned()
        {
            ViewBag.Tite = "Banned";
            return View();
        }

        public ActionResult Notfound()
        {
            ViewBag.Title = "404 Not Found";
            Response.StatusCode = 404;
            Response.TrySkipIisCustomErrors = true;
            return View();
        }
        public ActionResult Notallowed()
        {
            ViewBag.Title = "Action not allowed";
            Response.StatusCode = 405;
            Response.TrySkipIisCustomErrors = true;
            return View();
        }

        public ActionResult Unauthorized()
        {
            Response.StatusCode = 401;
            Response.TrySkipIisCustomErrors = true;
            ViewBag.Title = "Unauthorized";
            return View();
        }

        public ActionResult Aggressive()
        {
            Response.StatusCode = 429;
            Response.TrySkipIisCustomErrors = true;
            ViewBag.Title = "Aggressive";
            return View();
        }

        public ActionResult BadRequest()
        {
            Response.StatusCode = 400;
            Response.TrySkipIisCustomErrors = true;
            ViewBag.Tite = "Bad Request";
            return View();
        }

        public ActionResult Forbidden()
        {
            Response.StatusCode = 403;
            Response.TrySkipIisCustomErrors = true;
            ViewBag.Title = "Forbidden";
            return View();
        }

    }
}
