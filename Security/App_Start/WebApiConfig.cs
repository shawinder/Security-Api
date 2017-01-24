using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

using System.Net.Http.Formatting;
using Newtonsoft.Json.Serialization;

namespace Security.Service
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // #ssekhon - Enable Attribute Routing
            config.MapHttpAttributeRoutes();

            //Enable WebAPI CORS
            config.EnableCors();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "{controller}/{id}",
                defaults: new { controller = "Home", action = "Index", id = RouteParameter.Optional }
            );

            var jsonFormatter = config.Formatters.OfType<JsonMediaTypeFormatter>().First();
            jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            //#ssekhon - Fix Chrome JSON return problem
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new System.Net.Http.Headers.MediaTypeHeaderValue("text/html"));

            //#ssekhon - Enable Global Authorize Filter
            //config.Filters.Add(new AuthorizeAttribute());
            config.Filters.Add(new AttributeFilters.AuthorizeAttribute());
        }
    }
}
