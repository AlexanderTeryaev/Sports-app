using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

namespace CmsApp.Helpers
{
    public class AppAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase ctx)
        {
            string cookieName = FormsAuthentication.FormsCookieName;
            HttpCookie authCookie = ctx.Request.Cookies[cookieName];

            if (authCookie == null)
            {
                return false;
            }

            var authTicket = FormsAuthentication.Decrypt(authCookie.Value);

            string[] userData = authTicket.UserData.Split(new Char[] { '^' });
            if (userData == null || userData.Length < 2)
            {
                return false;
            }

            string[] roles = userData[0].Split(new Char[] { '|' });

            var userIdentity = new GenericIdentity(authTicket.Name);
            var userPrincipal = new GenericPrincipal(userIdentity, userData);
            ctx.User = userPrincipal;

            return true;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext ctx)
        {
            if (ctx.HttpContext.User.Identity.IsAuthenticated)
            {
                base.HandleUnauthorizedRequest(ctx);
            }
            else
            {
                ctx.Result = new RedirectResult(FormsAuthentication.LoginUrl);
            }
        }
    }
}