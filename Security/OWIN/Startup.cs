using Microsoft.Owin;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

using Microsoft.Owin.Security.OAuth;

using System.Threading.Tasks;

[assembly: OwinStartup(typeof(Security.Service.OWIN.Startup))]
namespace Security.Service.OWIN
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            HttpConfiguration config = new HttpConfiguration();

            //oAuth
            ConfigureOAuth(app);


            WebApiConfig.Register(config);

            //Enable OWIN CORS
            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);


            app.UseWebApi(config);
        }

        public void ConfigureOAuth(IAppBuilder app)
        {
            Int32 validityTimestamp = (Int32)(DateTime.UtcNow.AddMinutes(1).Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            OAuthAuthorizationServerOptions OAuthServerOptions = new OAuthAuthorizationServerOptions()
            {
                TokenEndpointPath = new PathString("/token"),
                Provider = new OAuthProvider(),

                //Must match with validity property under OAuthProvider.cs
                //AccessTokenExpireTimeSpan = TimeSpan.FromDays(1),
                AccessTokenExpireTimeSpan = TimeSpan.FromSeconds(validityTimestamp),

                AllowInsecureHttp = true,

            };

            // Token Generation
            app.UseOAuthAuthorizationServer(OAuthServerOptions);
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());

            //#ssekhon - Used for debugging
            //app.Run(Invoke);

        }

        // Invoked once per request.
        public Task Invoke(IOwinContext context)
        {
            context.Response.ContentType = "text/plain";
            return context.Response.WriteAsync("Hello World");
        }
    }
}