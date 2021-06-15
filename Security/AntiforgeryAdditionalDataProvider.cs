using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.SessionState;
using WebMatrix.WebData;

namespace AnarkRE.Security
{
    // This file salts our authentication token so it cannot be reused after logout.
    public class AntiforgeryAdditionalDataProvider : IAntiForgeryAdditionalDataProvider
    {
        public string GetAdditionalData(HttpContextBase context)
        {
            if (context.Session == null)
                return string.Empty;

            DocumentSigner ds = new DocumentSigner();
            string salt = "TFM" + MyCrypto.GenerateRandomString(6);
            string sig = ds.Sign(salt);

            context.Session.Add(salt, sig);
            return salt;
        }
        
        public bool ValidateAdditionalData(HttpContextBase context, string additionalData)
        {
          
        
            if (context.Session == null)
                return true;

            DocumentSigner ds = new DocumentSigner();
            string sig = context.Session[additionalData] as string;
            context.Session.Remove(additionalData);
            bool res = ds.Validate(additionalData, sig);
            
            return res;
 
        }


    }
}