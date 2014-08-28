using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;

using WebMatrix.WebData;
using AnarkRE.Filters;
using AnarkRE.Models;
using System.Web.Helpers;

namespace AnarkRE.Controllers
{
    [Authorize]
    public class AccountController : BaseController
    {
        //
        // GET: /Account/Login

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.Title = "Login";
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (model.UserName.Contains("@") & model.UserName.Contains("."))
                {
                    Models.User usr = null;
                    try
                    {
                        usr = data.Users.Get(s => s.Email.ToLower() == model.UserName).SingleOrDefault();
                    }
                    catch { }
                    if (usr != null)
                        model.UserName = usr.UserName;
                }

                if (WebSecurity.Login(model.UserName, model.Password, persistCookie: model.RememberMe))
                {
                    //Response.Cookies.Add(new HttpCookie("Authorization", "Cookie " + 
                    //    Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(model.UserName))));
                    if (string.IsNullOrEmpty(returnUrl))
                        return RedirectToAction("index", "escrow");
                    else
                        return RedirectToLocal(returnUrl);
                }
            }

            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "The user name or password provided is incorrect.");
            return View(model);
        }

        //
        // POST: /Account/LogOff

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            WebSecurity.Logout();
            //if (Request.Cookies["Authorization"] != null)
            //{
            //    var c = new HttpCookie("Authorization");
            //    c.Expires = DateTime.Now.AddDays(-1);
            //    Response.Cookies.Add(c);
            //}
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/Register

        [AllowAnonymous]
        public ActionResult Register()
        {
            ViewBag.Title = "Register";
            return View();
        }

        //
        // POST: /Account/Register

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterModel model)
        {
#if DEBUG
            using (SiteDBEntities db = new SiteDBEntities(true))
#else
            using (SiteDBEntities db = new SiteDBEntities(false))
#endif
            {
                if (db.Users.Any(s => s.Email.ToLower() == model.Email.ToLower()))
                    ModelState.AddModelError("", "Email already exists");
            }
            
            if (ModelState.IsValid)
            {
                // Attempt to register the user
                try
                {
                    string token = WebSecurity.CreateUserAndAccount(model.UserName, model.Password, propertyValues: new
                        {
                            Email = model.Email
                        }, requireConfirmationToken: true);

                    
                    string confirmationLink = Url.Action("confirm", "account", new { code = token }, Request.Url.Scheme);
                    string emailBodyText =
    "<p>Thank you for signing up with us! "
    + "Please confirm your registration by clicking the following link:</p>"
    + "<p><a href=\"" + confirmationLink + "\">Click to confirm your registration</a></p>"
    + "<p>If the link does not work, copy and paste this url into your browser:<br/><strong> "
    + confirmationLink
    + "</strong></p>";
                    
                    Globals.SendEmail(model.Email, "Your account confirmation", emailBodyText);


                    ViewBag.Emailed = model.Email;
                    model = null;
                    return View(model);


                }
                catch (MembershipCreateUserException e)
                {
                    ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }


        [AllowAnonymous]
        public ActionResult Confirm(string code)
        {
            if (!string.IsNullOrEmpty(code))
                if (WebSecurity.ConfirmAccount(code))
                    return View(true);
                    //return Content("Your account has been confirmed. You may now login");

          
            //return Content("Confirmation code is invalid");
            return View(false);
        }


        //
        // GET: /Account/Manage

        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : "";
            ViewBag.HasLocalPassword = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.ReturnUrl = Url.Action("Manage");
            return View();
        }

        //
        // POST: /Account/Manage

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Manage(LocalPasswordModel model)
        {
            bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.HasLocalPassword = hasLocalAccount;
            ViewBag.ReturnUrl = Url.Action("Manage");
            if (hasLocalAccount)
            {
                if (ModelState.IsValid)
                {
                    // ChangePassword will throw an exception rather than return false in certain failure scenarios.
                    bool changePasswordSucceeded;
                    try
                    {
                        changePasswordSucceeded = WebSecurity.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword);
                    }
                    catch (Exception)
                    {
                        changePasswordSucceeded = false;
                    }

                    if (changePasswordSucceeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }
                    else
                    {
                        ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                    }
                }
            }
            else
            {
                // User does not have a local password so remove any validation errors caused by a missing
                // OldPassword field
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        WebSecurity.CreateAccount(User.Identity.Name, model.NewPassword);
                        return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
                    }
                    catch (Exception e)
                    {
                        ModelState.AddModelError("", e);
                        string[] strings = new string[] { "blah", "blah2" };
                        
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }



        #region Helpers
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
        }

        internal class ExternalLoginResult : ActionResult
        {
            public ExternalLoginResult(string provider, string returnUrl)
            {
                Provider = provider;
                ReturnUrl = returnUrl;
            }

            public string Provider { get; private set; }
            public string ReturnUrl { get; private set; }

            public override void ExecuteResult(ControllerContext context)
            {
                OAuthWebSecurity.RequestAuthentication(Provider, ReturnUrl);
            }
        }

        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion
    }
}
