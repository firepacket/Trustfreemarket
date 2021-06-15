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
using System.Web.Mvc;
using System.Web.SessionState;
using AnarkRE.Security;
using System.Text;
using System.Net.Http.Headers;

namespace AnarkRE.Filters
{
    // Anti forgery token, in view:
    //
    // $token = $('input[name=""__RequestVerificationToken""]').val();
    // ...$.ajax...
    // headers: { __RequestVerificationToken: $token },

    //[SessionState(SessionStateBehavior.Required)]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class ValidateJsonAntiForgeryTokenAttribute : System.Web.Http.Filters.FilterAttribute, System.Web.Http.Filters.IAuthorizationFilter
    {
        volatile bool newaft = false;
        private static object lockObj = new object();
        private static string _formtoke;
        private static string _ncookie;
        public static string formtoke { get { lock (lockObj) { return _formtoke; } } }
        public static string ncookie { get { lock (lockObj) { return _ncookie; } } }


        public ValidateJsonAntiForgeryTokenAttribute()
        { }
        public ValidateJsonAntiForgeryTokenAttribute(bool newaft)
        { this.newaft = newaft; }


        public Task<HttpResponseMessage> ExecuteAuthorizationFilterAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
        {
            StringBuilder sb = new StringBuilder();
            int allowedticks = 700000;
            var headers = actionContext.Request.Headers;
            string auth = headers.GetValues("Authorization").FirstOrDefault();
            sb.Append("Authorization: " + auth);
            try
            {

                DocumentSigner ds = new DocumentSigner();
                string[] parts = auth.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    string doc = ds.Decrypt(parts[ 0 ]);
                    sb.Append(" doc: " + doc);
                    sb.Append(" Allowed time: " + (long.Parse(doc) + allowedticks) + " Sent time:" + DateTime.Now);
                    if (ds.Validate(doc, parts[ 1 ]) && long.Parse(doc) + allowedticks < DateTime.Now.Ticks)
                    {

                        var cookie = headers.GetCookies().Select((c) => c[ AntiForgeryConfig.CookieName ]).FirstOrDefault();
                        //Globals.AppendFile("headers.log", headers.ToString());
                        string token = headers.GetValues("RequestVerificationToken").FirstOrDefault();
                        string ncookie;
                        string formtoken;


                        sb.Append("Cookie: " + (cookie != null ? cookie.Value : null) + "  Token: " + token);
                        AntiForgery.Validate(cookie != null ? cookie.Value : null, token);

                        if (newaft)
                        {

                            lock (lockObj)
                            {
                                AntiForgery.GetTokens(cookie.Value, out ncookie, out formtoken);
                                _ncookie = ncookie;
                                _formtoke = formtoken;

                            }
                        }

                    }
                }


            }
            catch (Exception err)
            {
                Globals.WriteError("APIautherror.log", err);
                actionContext.Response = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Forbidden,
                    RequestMessage = actionContext.ControllerContext.Request
                };
                return FromResult(actionContext.Response);
            }
            return continuation();
        }

        //public Task<HttpResponseMessage> ExecuteAuthorizationFilterAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
        //{
        //    try
        //    {
        //        var headers = actionContext.Request.Headers;
        //        var cookie = headers.GetCookies().Select((c) => c[AntiForgeryConfig.CookieName]).FirstOrDefault();
        //        Globals.AppendFile("headers.log", headers.ToString());
        //        var token = headers.GetValues("__RequestVerificationToken").FirstOrDefault();
        //        AntiForgery.Validate(cookie != null ? cookie.Value : null, token);
        //    }
        //    catch (Exception err)
        //    {
        //        //Globals.WriteError("autherror.log", err);
        //        actionContext.Response = new HttpResponseMessage
        //        {
        //            StatusCode = HttpStatusCode.Forbidden,
        //            RequestMessage = actionContext.ControllerContext.Request
        //        };
        //        return FromResult(actionContext.Response);
        //    }
        //    return continuation();
        //}

        private Task<HttpResponseMessage> FromResult(HttpResponseMessage result)
        {
            var source = new TaskCompletionSource<HttpResponseMessage>();
            source.SetResult(result);
            return source.Task;
        }
    }
}