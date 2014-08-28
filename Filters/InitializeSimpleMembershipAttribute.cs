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
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class InitializeSimpleMembershipAttribute : ActionFilterAttribute
    {
        private static SimpleMembershipInitializer _initializer;
        private static object _initializerLock = new object();
        private static bool _isInitialized;

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Ensure ASP.NET Simple Membership is initialized only once per app start
            //LazyInitializer.EnsureInitialized(ref _initializer, ref _isInitialized, ref _initializerLock);
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

#if DEBUG
                    WebSecurity.InitializeDatabaseConnection("DefaultConnectionDebug", "UserProfile", "UserId", "UserName", autoCreateTables: true);
#else
                    WebSecurity.InitializeDatabaseConnection("DefaultConnection", "UserProfile", "UserId", "UserName", autoCreateTables: true);
#endif

                    var roles = (SimpleRoleProvider)Roles.Provider;
                    var membership = (SimpleMembershipProvider)Membership.Provider;

                    if (!roles.RoleExists("admin"))
                        roles.CreateRole("admin");
                    
                    if (membership.GetUser("default", false) == null)
                        membership.CreateUserAndAccount("default", "temp123!");

                    if (membership.GetUser("admin", false) == null)
                        membership.CreateUserAndAccount("admin", "admin");

                    if (!roles.GetRolesForUser("default").Contains("admin"))
                        roles.AddUsersToRoles(new[] { "default" }, new[] { "admin" });

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
