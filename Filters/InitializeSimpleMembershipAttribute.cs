using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Threading;
using System.Web.Mvc;
using WebMatrix.WebData;
using AnarkRE.Models;
using System.Web.Security;

namespace AnarkRE.Filters
{
    /// <summary>
    /// Use this *once* to create the tables required for ASP Simplemembership within your standard database.
    /// Also creates "admin:admin123" user account along with the ASP admin Role.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class InitializeSimpleMembershipAttribute : ActionFilterAttribute
    {
        private static SimpleMembershipInitializer _initializer;
        private static object _initializerLock = new object();
        private static bool _isInitialized;

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Ensure ASP.NET Simple Membership is initialized only once per app start
            LazyInitializer.EnsureInitialized(ref _initializer, ref _isInitialized, ref _initializerLock);
        }

        private class SimpleMembershipInitializer
        {
            public SimpleMembershipInitializer()
            {
                Database.SetInitializer<UsersContext>(null);

                try
                {
                    using (var context = new UsersContext())
                    {
                        if (!context.Database.Exists())
                        {
                            // Create the SimpleMembership database without Entity Framework migration schema
                            ((IObjectContextAdapter)context).ObjectContext.CreateDatabase();
                        }
                    }

//#if DEBUG
//                    WebSecurity.InitializeDatabaseConnection("DefaultConnectionDebug", "Users", "UserId", "UserName", autoCreateTables: true);
//#else
                    if (!WebSecurity.Initialized)
                    {
                        WebSecurity.InitializeDatabaseConnection("DefaultConnection", "Users", "UserId", "UserName", autoCreateTables: true);
                    }
//#endif

                    var roles = (SimpleRoleProvider)Roles.Provider;
                    var membership = (SimpleMembershipProvider)Membership.Provider;

                    if (!roles.RoleExists("admin"))
                        roles.CreateRole("admin");
                    if (!roles.RoleExists("arbiter"))
                        roles.CreateRole("arbiter");

                    if (membership.GetUser("admin", false) == null)
                        WebSecurity.CreateUserAndAccount("admin", "admin", propertyValues: new
                        {
                            Email = "admin@trustfree.market"
                        }, requireConfirmationToken: false);

                    if (!roles.GetRolesForUser("admin").Contains("admin"))
                        roles.AddUsersToRoles(new[] { "admin" }, new[] { "admin" });
                    
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("The ASP.NET Simple Membership database could not be initialized. For more information, please see http://go.microsoft.com/fwlink/?LinkId=256588", ex);
                }
            }
        }
    }
}
