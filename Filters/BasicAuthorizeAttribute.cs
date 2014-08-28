using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Web.Http;
using System.Web.Http.Controllers;
using WebMatrix.WebData;
using System.Net.Http;

namespace AnarkRE.Filters
{
    [AttributeUsageAttribute(AttributeTargets.Class | AttributeTargets.Method, Inherited = true,
       AllowMultiple = true)]
    public class BasicAuthorizeAttribute : AuthorizeAttribute
    {

        private enum AuthType { basic, cookie };

        private string DecodeFrom64(string encodedData)
        {

            byte[] encodedDataAsBytes
                = System.Convert.FromBase64String(encodedData);
            string returnValue =
               System.Text.Encoding.ASCII.GetString(encodedDataAsBytes);

            return returnValue;
        }

        private bool GetUserNameAndPassword(HttpActionContext actionContext, out string username, out string password, out AuthType authType)
        {
            authType = AuthType.basic;
            bool gotIt = false;
            username = string.Empty;
            password = string.Empty;
            IEnumerable<string> headerVals;
            if (actionContext.Request.Headers.TryGetValues("Authorization", out headerVals))
            {
                try
                {
                    string authHeader = headerVals.FirstOrDefault();
                    char[] delims = { ' ' };
                    string[] authHeaderTokens = authHeader.Split(new char[] { ' ' });
                    if (authHeaderTokens[0].Contains("Basic"))
                    {
                        string decodedStr = DecodeFrom64(authHeaderTokens[1]);
                        string[] unpw = decodedStr.Split(new char[] { ':' });
                        username = unpw[0];
                        password = unpw[1];
                    }
                    else
                    {
                        if (authHeaderTokens.Length > 1)
                            username = DecodeFrom64(authHeaderTokens[1]);
                        authType = AuthType.cookie;
                    }

                    gotIt = true;
                }
                catch { gotIt = false; }
            }

            return gotIt;
        }

        private bool Authenticate(HttpActionContext actionContext, out string username)
        {
            bool isAuthenticated = false;
            username = string.Empty;
            string password;
            AuthType authenticationType;

            if (GetUserNameAndPassword(actionContext, out username, out password, out authenticationType))
            {

                if (authenticationType == AuthType.basic)
                {
                    if (WebSecurity.Login(username, password, true))
                    {
                        isAuthenticated = true;
                    }
                    else
                    {
                        WebSecurity.Logout();
                    }
                }
                else //authType == cookie
                {
                    if (WebSecurity.IsAuthenticated)
                        isAuthenticated = true;

                    username = WebSecurity.CurrentUserName;
                }
            }
            else
            {
                actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
            }

            return isAuthenticated;
        }

        private bool isAuthorized(string username)
        {
            bool authorized = false;

            var roles = (SimpleRoleProvider)System.Web.Security.Roles.Provider;
            authorized = roles.IsUserInRole(username, this.Roles);
            return authorized;
        }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            string username;

            if (Authenticate(actionContext, out username))
            {
                if (!isAuthorized(username))
                    actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden);
            }
            else
            {
                actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
            }
        }
    }
}