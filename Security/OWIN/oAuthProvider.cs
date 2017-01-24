using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;

using Microsoft.AspNet.Identity.EntityFramework;
using System.Security.Claims;
using System.Web.Script.Serialization;
using Newtonsoft.Json;

namespace Security.Service.OWIN
{
    public class OAuthProvider : OAuthAuthorizationServerProvider
    {
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            string _applicationId = String.Empty;
            bool _isActiveDirectoryUser = false;
            string _domain = null;
            string _usrApplications = String.Empty;
            dynamic _usrGroups = String.Empty;
            dynamic result = null;

            dynamic user = null;

            //To allow CORS (Cross Origin Resource Sharing)
            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });
            //context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "http://localhost:85"});

            using (Data.Repository.IUser _user = new Data.Repository.User())
            {
                //#ssekhon - Get Custom Application Id from request parameters
                foreach (var ob in context.Request.ReadFormAsync().Result.Skip(1).Take(5))
                {
                    switch (ob.Key.ToLower())
                    {
                        case "appid":
                            _applicationId = ob.Value.FirstOrDefault().ToString().ToLower();
                            break;
                        case "ad":
                            _isActiveDirectoryUser = Convert.ToBoolean(ob.Value.FirstOrDefault());
                            break;
                        case "domain":
                            _domain = ob.Value.FirstOrDefault().ToString().ToLower();
                            break;
                    }
                }

                if (_isActiveDirectoryUser)
                {
                    result = _user.FindADUser(context.UserName, context.Password, _applicationId, _domain);
                }
                else
                {
                    result = _user.FindExtendedUser(context.UserName, context.Password, _applicationId);
                }

                string ResultType = result.Type;
                string _message = result.Message;
                user = result.user;

                switch (ResultType)
                {
                    case "success":
                        break;
                    case "error":
                        context.SetError("invalid_grant", new Exception(_message).ToString());
                        return;
                    case "info":
                        context.SetError(_message, "Information description");
                        return;
                }

                if (user == null)
                {
                    context.SetError("Wrong User/Pass", "Invalid credentials supplied.");
                    return;
                }

                //Check if active directory User - using existing Extended User fields for AD User e.g "IPAddress" to overcome null exception for normal user.
                if (user.IPAddress == "ADUser")
                {
                    _usrApplications = user.Applications;
                    _usrGroups = user.Groups;
                    _usrGroups = JsonConvert.SerializeObject(_usrGroups, Formatting.None,
                       new JsonSerializerSettings
                       {
                           ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                       });
                }
                else
                {
                    //Get comma separated user applications
                    _usrApplications = _user.CSVUserAppList(user.Id);

                    using (Data.Repository.IApplication _application = new Data.Repository.Applicaiton())
                    {
                        _usrGroups = _user.AccessRules(new Guid(_applicationId), user.Id);
                        _usrGroups = JsonConvert.SerializeObject(_usrGroups, Formatting.None,
                        new JsonSerializerSettings
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        });
                    }
                }
            }

            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            Int32 validityTimestamp = (Int32)(DateTime.UtcNow.AddDays(1).Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            ClaimsIdentity identity = new ClaimsIdentity(context.Options.AuthenticationType);

            //Must match with AccessTokenExpireTimeSpan under OWIN/startup.cs and is checked inside CustomAuthorize.cs
            identity.AddClaim(new Claim("validity", validityTimestamp.ToString()));

            //#ssekhon - Token Data
            //identity.AddClaim(new Claim("Username", context.UserName));
            //identity.AddClaim(new Claim("Applications", _usrApplications));
            //identity.AddClaim(new Claim("DateCreated", DateTime.Now.ToString("yyyyMMddHHmmssffff")));
            ////Must match with AccessTokenExpireTimeSpan under startup.cs
            //identity.AddClaim(new Claim("Validity", TimeSpan.FromDays(1).ToString()));

            //#ssekhon - Token Data
            AuthenticationProperties properties = new AuthenticationProperties(
                new Dictionary<string, string>
                {
                    {
                        "prop_UserId", user.Id
                    },
                    { 
                        "prop_Username", context.UserName
                    },
                    { 
                        "prop_Applications", _usrApplications
                    },
                    { 
                        "prop_DateCreated", unixTimestamp.ToString()
                    },
                    { 
                        "prop_Validity", validityTimestamp.ToString() //Must match with AccessTokenExpireTimeSpan under OWIN/startup.cs
                    },
                    { 
                        "prop_Roles", _usrGroups
                    }
                }
            );

            //#ssekhon - Generates the Token in the background
            //context.Validated(identity);
            var ticket = new AuthenticationTicket(identity, properties); //To make properties work, make sure that you override the <TokenEndpoint> method.
            context.Validated(ticket);
        }

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