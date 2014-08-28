using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AnarkRE.Controllers
{
    [Authorize]
    public class BalanceController : ApiController
    {
        public decimal Get(string id)
        {
            if (!string.IsNullOrEmpty(id) && id.Length <= 34)
            {
                try
                {
                    WebClient wc = new WebClient(); // bc.info is unreliable crap
                    return decimal.Parse(wc.DownloadString("https://blockchain.info/q/addressbalance/" + id)) / 10000000M;
                }
                catch
                {
                    return 0;
                }
            }
            else
                return 0;
        }
    }
}
