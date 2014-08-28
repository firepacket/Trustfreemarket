using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http.Filters;
using System.Net.Http;

public class ValidateModelAttribute : ActionFilterAttribute
{
    public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
    {
        var modelState = actionExecutedContext.ActionContext.ModelState;
        if (!modelState.IsValid)
            actionExecutedContext.Request.CreateResponse(HttpStatusCode.BadRequest, modelState);
        
        base.OnActionExecuted(actionExecutedContext);
    }
}