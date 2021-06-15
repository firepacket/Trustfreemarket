using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
//using System.Web.Http.Filters;
using System.Web.Mvc;
using System.Web.Routing;

namespace AnarkRE.Filters
{
    [AttributeUsageAttribute(AttributeTargets.Class | AttributeTargets.Method, Inherited = true,
       AllowMultiple = true)]
    public class NoCacheAfterLogout : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.HttpContext.Response.Cookies.AllKeys.Contains(System.Web.Helpers.AntiForgeryConfig.CookieName))
            {
                filterContext.HttpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                filterContext.HttpContext.Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
                filterContext.HttpContext.Response.Cache.SetNoStore();

            }
            base.OnActionExecuting(filterContext);
        }
        
    }
}