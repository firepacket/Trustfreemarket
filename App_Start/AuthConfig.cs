using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.WebPages.OAuth;
using AnarkRE.Models;
using WebMatrix.WebData;

namespace AnarkRE
{
    public static class AuthConfig
    {
        public static void RegisterAuth()
        {
            // To let users of this site log in using their accounts from other sites such as Microsoft, Facebook, and Twitter,
            // you must update this site. For more information visit http://go.microsoft.com/fwlink/?LinkID=252166

            //OAuthWebSecurity.RegisterMicrosoftClient(
            //    clientId: "",
            //    clientSecret: "");

            //OAuthWebSecurity.RegisterTwitterClient(
            //    consumerKey: "",
            //    consumerSecret: "");

            //OAuthWebSecurity.RegisterFacebookClient(
            //    appId: "",
            //    appSecret: "");

            //OAuthWebSecurity.RegisterGoogleClient();
            if (!WebSecurity.Initialized)
            {
//#if DEBUG
//                WebSecurity.InitializeDatabaseConnection("DefaultConnectionDebug", "UserProfile", "UserId", "UserName", autoCreateTables: true);
//#else
                WebSecurity.InitializeDatabaseConnection("DefaultConnection", "Users", "UserId", "UserName", autoCreateTables: true);
//#endif
            }
        }
    }
}
