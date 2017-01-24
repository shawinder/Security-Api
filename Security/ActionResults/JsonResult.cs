using System.Web.Mvc;
using Newtonsoft.Json;
using System.Web.Http;

using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Security.Service.ActionResults
{
    public class JsonResult : IHttpActionResult
    {
        private HttpRequestMessage _request { get; set; }
        private object _obj { get; set; }

        public JsonResult(HttpRequestMessage request, object obj)
        {
            _request = request;
            _obj = obj;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            var response = _request.CreateResponse(HttpStatusCode.Created);

            /* Fix for JSON Self-Referencing Loop */
            var result = JsonConvert.SerializeObject(_obj, Formatting.None,
            new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            response.Content = new StringContent(result, Encoding.UTF8, "application/json");
            return Task.FromResult(response);
        }
    }
}