using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AnarkRE.Filters
{
    using System;
    using System.Web.Mvc;
    public class RequireProductionHttpsAttribute : RequireHttpsAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            if (filterContext.HttpContext != null && filterContext.HttpContext.Request.IsLocal)
            {
                return;
            }

            base.OnAuthorization(filterContext);
        }
    }
}