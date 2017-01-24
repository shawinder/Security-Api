using System;
using System.Web;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Security.Claims;
using System.Linq;


namespace Security.Service.AttributeFilters
{
    //[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class AuthorizeAttribute : System.Web.Http.AuthorizeAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            ClaimsPrincipal principal = actionContext.RequestContext.Principal as ClaimsPrincipal;

            var validity = principal.Claims.SingleOrDefault(o => o.Type == "validity");

            Int32 currentTime = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            if (validity == null || Convert.ToInt32(validity.Value) < currentTime)
            {
                HandleUnauthorizedRequest(actionContext);
            }
        }

        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            if (actionContext.RequestContext.Principal.Identity.IsAuthenticated)
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Forbidden, "Token expired.");
            }
            else
            {
                base.HandleUnauthorizedRequest(actionContext);
            }
        }
    }
}