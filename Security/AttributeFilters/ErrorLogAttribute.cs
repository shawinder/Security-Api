using System;
using System.Web.Mvc;
using System.Net.Mail;
using System.Text;

namespace Security.Service.AttributeFilters
{
    public class ErrorLogAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.ExceptionHandled || filterContext.HttpContext.IsCustomErrorEnabled)
            {
                return;
            }

            // if the request is AJAX return JSON else view.
            if (IsAjax(filterContext))
            {
                //Because its a exception raised after ajax invocation, Lets return Json
                filterContext.Result = new JsonResult()
                {
                    Data = filterContext.Exception.Message,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };

                filterContext.ExceptionHandled = true;
                filterContext.HttpContext.Response.Clear();
            }
            else
            {
                //Normal Exception, So let it handle by its default ways.
                base.OnException(filterContext);

            }
            //Log error
            LogError(filterContext);

            //Send email to admins
            //SendEmail(filterContext);
        }

        private bool IsAjax(ExceptionContext filterContext)
        {
            return filterContext.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }

        private void LogError(ExceptionContext filterContext)
        {
            Data.ApplicationDbContext db = new Data.ApplicationDbContext();

            Data.Models.EventLog log = new Data.Models.EventLog()
            {
                EventSource = (string)filterContext.RouteData.Values["controller"] + " > " + (string)filterContext.RouteData.Values["action"],
                EventType = Data.Models.LogType.Error,
                EventDescription = "IP:" + filterContext.HttpContext.Request.UserHostAddress + "\r\n" + filterContext.Exception.Message + "\r\n" + filterContext.Exception.InnerException + "\r\n" + filterContext.Exception.StackTrace,
                DateCreated = DateTime.Now
            };
            db.EventLogs.Add(log);
            db.SaveChanges();
        }

        private void SendEmail(ExceptionContext filterContext)
        {
            MailMessage message = new MailMessage();
            message.From = new MailAddress("info@test.com");

            message.To.Add(new MailAddress("info@test.com"));
            message.CC.Add(new MailAddress("info@test.com"));
            message.Subject = "Error Log";

            string currentController = (string)filterContext.RouteData.Values["controller"];
            string currentActionName = (string)filterContext.RouteData.Values["action"];

            StringBuilder sb = new StringBuilder();
            sb.Append("Controller: " + currentController);
            sb.Append("\r\nAction: " + currentActionName);
            sb.Append("\r\n" + filterContext.Exception.Message);
            sb.Append("\r\n" + filterContext.Exception.InnerException);
            sb.Append("\r\n" + filterContext.Exception.StackTrace);

            message.Body = sb.ToString();

            SmtpClient client = new SmtpClient();
            try
            {
                //client.Send(message);
            }
            catch (Exception ex)
            {
                filterContext.Exception = ex;
                LogError(filterContext);
            }
        }
    }
}