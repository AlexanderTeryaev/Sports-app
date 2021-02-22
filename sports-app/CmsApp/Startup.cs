using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using CmsApp.Helpers;
using DataService.Services;
using AppModel;
using DataService;

namespace CmsApp
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureOAuth(app);

            HttpConfiguration config = new HttpConfiguration();
            WebApiConfig.Register(config);
            app.UseWebApi(config);
            app.Use(async (context, next) =>
            {
                if (context.Request.IsSecure)
                {
                    await next();
                }
                else if (context.Request.Host.Value.ToLower().Contains("loglig.com"))
                {
                    var withHttps = "https://" + context.Request.Host + context.Request.Path;
                    context.Response.Redirect(withHttps);
                }
                else
                {
                    await next();
                }
            });
        }

        public static void RunMedicalCertificateChecks()
        {
            using (var db = new DataEntities())
            {
                var playersRep = new PlayersRepo(db);
                playersRep.UpdateMedicalCertificatesToSection(6);  // tennis
                playersRep.UpdateMedicalCertificatesToSection(10); // gymnastics
                playersRep.UpdateMedicalCertificatesToSection(1); //basketball
                playersRep.UpdateMedicalCertificatesToSection(31); // rowing
                playersRep.UpdateMedicalCertificatesToSection(17); // bicycle
            }
        }

        public void ConfigureOAuth(IAppBuilder app)
        {
            var OAuthServerOptions = new OAuthAuthorizationServerOptions()
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/token"),
                //AuthorizeEndpointPath = new PathString("/api/Account/Login"),
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(1),
                Provider = new SimpleAuthorizationServerProvider()
            };

            // Token Generation
            app.UseOAuthAuthorizationServer(OAuthServerOptions);
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());
        }

    }
}