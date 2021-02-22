using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using WebApi.Models;
using Resources;
using AppModel;
using System.Web;
using DataService;

namespace WebApi.Providers
{
    public class ApplicationOAuthProvider : OAuthAuthorizationServerProvider
    {
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            string authenticationType = context.OwinContext.Request.Headers.Get("AuthenticationType");
            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });

            if (string.IsNullOrEmpty(authenticationType))
            {
                context.SetError("invalid_grant", "Missing Authentication Type");
                return;
            }
            User user = null;
            using (DataEntities db = new DataEntities())
            {
                switch (authenticationType.ToLower())
                {
                    case "facebook":
                        user = FacebookLogin(db, context);
                        break;
                    case "usernamepassword":
                        user = UsernamePasswordLogin(db, context);
                        break;
                    case "personalid":
                        user = WorkerLogin(db, context);
                        break;
                    default:
                        context.SetError("invalid_grant", Messages.InvalidLogin);
                        break;
                }

                if (user == null)
                {
                    //context.SetError("invalid_grant", "The user name or password is incorrect.");
                    return;
                }

                if (!user.IsActive)
                {
                    context.SetError("invalid_grant", Messages.UserNotActive);
                    return;
                }

                var isWaterpoloReferee = user.UsersJobs.Select(r => r.Job).Any(r => r.JobsRole.RoleName == JobRole.Referee
                    && r.Section.Alias == GamesAlias.WaterPolo);

                context.OwinContext.Response.Headers.Add("IsWaterpoloReferee", new[] { isWaterpoloReferee.ToString() });

                var identity = new ClaimsIdentity(context.Options.AuthenticationType);
                identity.AddClaim(new Claim(ClaimTypes.Name, user.UserId.ToString()));
                identity.AddClaim(new Claim(ClaimTypes.Role, user.UsersType.TypeRole));
                context.Validated(identity);
            }
        }

        private User FacebookLogin(DataEntities db, OAuthGrantResourceOwnerCredentialsContext context)
        {
            string fbid = context.OwinContext.Request.Headers.Get("id");

            if (string.IsNullOrEmpty(fbid))
            {
                context.SetError("Facebook_grant", Messages.FacebookIdNotFound);
                return null;
            }

            User usr = db.Users.FirstOrDefault(u => u.FbId == fbid);

            if (usr == null)
            {
                context.SetError("Facebook_grant", Messages.FacebookUserNotFound);
            }

            return usr;
        }

        private User UsernamePasswordLogin(DataEntities db, OAuthGrantResourceOwnerCredentialsContext context)
        {
            if (string.IsNullOrEmpty(context.Password) || string.IsNullOrEmpty(context.UserName))
            {
                context.SetError("UsernamePassword_grant", Messages.UsernameOrPassMissing);
                return null;
            }
            string encryptedPass = Protector.Encrypt(context.Password);

            User user = db.Users.FirstOrDefault(u => u.UserName == context.UserName && u.Password == encryptedPass)
                ?? db.Users.FirstOrDefault(u => u.Email == context.UserName && u.Password == encryptedPass);

            if (user == null)
            {
                context.SetError("UsernamePassword_grant", Messages.InvalidLogin);
            }
            bool Tennis_validity = user.TenicardValidity == null ? false : user.TenicardValidity > DateTime.Now ? true : false;
            if (Tennis_validity == false && user.UsersType.TypeRole == "players")
            {
                //check tennis
                var unionsQuery = user.TeamsPlayers.Any(t => t.Club.Union.Section.Name == "Tennis");
                if (unionsQuery)
                {
                    context.SetError("Worker_grant", Messages.InvalidLogin);
                    user.IsActive = false;
                }
            }
            return user;
        }

        private User WorkerLogin(DataEntities db, OAuthGrantResourceOwnerCredentialsContext context)
        {
            if (string.IsNullOrEmpty(context.Password) || string.IsNullOrEmpty(context.UserName))
            {
                context.SetError("Worker_grant", Messages.PersonalIdMissing);
                return null;
            }

            string encryptedPass = Protector.Encrypt(context.Password);

            User user = db.Users.Include("UsersJobs").FirstOrDefault(u => u.IdentNum == context.UserName && u.Password == encryptedPass);

            if (user == null)
            {
                context.SetError("Worker_grant", Messages.InvalidLogin);
            }
            bool Tennis_validity = user?.TenicardValidity == null ? false : user.TenicardValidity > DateTime.Now ? true : false;
            bool isUserHasUnionManagerRole = user?.UsersJobs.Any(uj => uj.Job.JobsRole.RoleName == JobRole.UnionManager) ?? false;
            if (isUserHasUnionManagerRole)
            {
                return user;
            }
            if (isUserHasUnionManagerRole || (Tennis_validity == false && user?.UsersType?.TypeRole == "players"))
            {
                //check tennis
                var unionsQuery = user.TeamsPlayers.Any(t=>t.Club?.Union?.Section?.Alias == GamesAlias.Tennis);
                if (unionsQuery)
                {
                    context.SetError("Worker_grant", Messages.InvalidLogin);
                    user.IsActive = false;
                }
            }
            return user;
        }

        //private User WorkerLogin(DataEntities db, OAuthGrantResourceOwnerCredentialsContext context)
        //{
        //    string pid = context.OwinContext.Request.Headers.Get("id");
        //    if (string.IsNullOrEmpty(pid))
        //    {
        //        context.SetError("Worker_grant", Messages.PersonalIdMissing);
        //        return null;
        //    }
        //    User user = db.Users.FirstOrDefault(u => u.IdentNum == pid);
        //    if (user == null)
        //    {
        //        context.SetError("Worker_grant", Messages.WorkeNotFound);
        //    }
        //    return user;
        //}

        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
            {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }

            return Task.FromResult<object>(null);
        }
    }
}