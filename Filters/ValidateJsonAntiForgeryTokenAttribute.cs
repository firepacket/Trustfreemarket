using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Filters;
using System.Threading.Tasks;
using System.Net.Http;
using System.Web.Helpers;
using System.Net;
using System.Web.Http.Controllers;
using System.Threading;

namespace AnarkRE.Filters
{
    // Anti forgery token, in view:
    //
    // $token = $('input[name=""__RequestVerificationToken""]').val();
    // ...$.ajax...
    // headers: { __RequestVerificationToken: $token },


    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class ValidateJsonAntiForgeryTokenAttribute : FilterAttribute, IAuthorizationFilter
    {
        public Task<HttpResponseMessage> ExecuteAuthorizationFilterAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
        {
            try
            {
                var headers = actionContext.Request.Headers;
                var cookie = headers.GetCookies().Select((c) => c[AntiForgeryConfig.CookieName]).FirstOrDefault();
                //Globals.AppendFile("headers.log", headers.ToString());
                var token = headers.GetValues("RequestVerificationToken").FirstOrDefault();
                AntiForgery.Validate(cookie != null ? cookie.Value : null, token);
            }
            catch (Exception err)
            {
                Globals.WriteError("autherror.log", err);
                actionContext.Response = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Forbidden,
                    RequestMessage = actionContext.ControllerContext.Request
                };
                return FromResult(actionContext.Response);
            }
            return continuation();
        }

        

        private Task<HttpResponseMessage> FromResult(HttpResponseMessage result)
        {
            var source = new TaskCompletionSource<HttpResponseMessage>();
            source.SetResult(result);
            return source.Task;
        }
    }
}