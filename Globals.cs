using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Mail;
using System.IO;
using System.Net.Security;
using System.Web.Hosting;
using System.Threading;
using AnarkRE.Models;


namespace AnarkRE
{
    public static class Globals
    {
      
        public const int ListingDays = 365;
        public const int MaxPictures = 15;
        public const int MaxUserListings = 10;
        public const decimal MinListingPriceUsd = 1M;
        public const int CancelIfNotShippedDays = 8;
        public const int AutoReleaseAfterShippedDays = 11;
        public const int BuyerExtendAutoreleaseDays = 5;
        public const int LoginMinutes = 30;
        public const int BTCRateUpdateMin = 9;
        public const int PWResetHrs = 2;

        public static string PicturePath = HostingEnvironment.MapPath("~/App_Data/pictures");

        public static Dictionary<ListingAdditionType, int> ListAddOpts = new Dictionary<ListingAdditionType, int>()
        {
            { ListingAdditionType.Shipping, 5 },
            { ListingAdditionType.Description, 5 },
            { ListingAdditionType.SingleSelect, 5 },
            { ListingAdditionType.MultiSelect, 5 }
        };

        public static void SendEmail(string to, string subject, string message)
        {
            Thread thread = new Thread(new ThreadStart(() =>
                {
                    try
                    {
                        var loginInfo = new NetworkCredential("user", "password");
                        var msg = new MailMessage();
                        var smtpClient = new SmtpClient("127.0.0.1", 25);

                        smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;

                        msg.From = new MailAddress("info@anark.it", "AnArk Trustfree Market");
                        msg.To.Add(new MailAddress(to));
                        msg.Subject = subject;
                        msg.Body = message;
                        msg.IsBodyHtml = true;
                        
                        smtpClient.EnableSsl = true;
                        smtpClient.UseDefaultCredentials = false;
                        smtpClient.Credentials = loginInfo;
                        smtpClient.Send(msg);
                    }
                    catch (Exception er)
                    {
                        WriteError("emailerror.log", er);
                    }
                }));
            thread.IsBackground = true;
            thread.Start();
        }

        public static void WriteError(string filename, Exception err)
        {
            try
            {
                string path = HostingEnvironment.MapPath("~/App_Data/" + filename);
                using (StreamWriter writer = new StreamWriter(new FileStream(path, FileMode.Append)))
                {
                    writer.WriteLine("");
                    writer.WriteLine("------------");
                    writer.WriteLine("Date: " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());
                    writer.WriteLine("Message: " + err.Message);
                    writer.WriteLine("Trace: " + err.StackTrace);
                    writer.WriteLine("Inner: " + err.InnerException);
                }
            }
            catch { }
        }

        public static void AppendFile(string filename, string text)
        {
            try
            {
                string path = HostingEnvironment.MapPath("~/App_Data/" + filename);
                using (StreamWriter writer = new StreamWriter(new FileStream(path, FileMode.Append)))
                {
                    
                    writer.WriteLine("");
                    writer.WriteLine("------------");
                    writer.WriteLine("Date: " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());
                    writer.WriteLine(text);
                }
            }
            catch { }
        }

        public static string ScoreColor(decimal? score)
        {
            string color = "#707070";
            if (!score.HasValue)
                return color;
            else if (score == 1000)
                return "#2e4579";

            if (score >= 5)
            {
                color = "#177e41";
            }
            else if (score >= 4)
            {
                color = "#5d7e17";
            }
            else if (score >= 3)
            {
                color = "#7e7a17";
            }
            else if (score >= 2)
            {
                color = "#7e5117";
            }
            else if (score < 2)
            {
                color = "#7e2017";
            }

            return color;
        }

        private static List<string> _Categories = new List<string>();
        public static List<string> Categories
        {
            get
            {
                if (_Categories.Count == 0)
                {
                    using (SiteDBEntities db = new SiteDBEntities())
                    {
                        _Categories = db.Catagories
                            .Select(s => s.Name).ToList();
                        _Categories.Sort();
                    }
                }
                return _Categories;
            }
        }
        
        public static Dictionary<string, int> GetCatCount()
        {
            Dictionary<string, int> count = new Dictionary<string, int>();
            using (SiteDBEntities db = new SiteDBEntities())
            {
                foreach (var c in db.Catagories)
                    count.Add(c.Name, c.Listings.Where(s => !s.IsDeleted
                && s.IsApproved && !s.IsPrivate
                && DateTime.Now <= s.ExpireDate).Count());
                return count;
            }
        }

        public static decimal? GetScore(int userid, bool buyer)
        {
            using (SiteDBEntities db = new SiteDBEntities())
            {
                User usr = db.Users.SingleOrDefault(s => s.UserId == userid);
                if (usr == null)
                    return null;
                return buyer ? usr.BuyerScore : usr.SellerScore;
            }
        }
    }
}