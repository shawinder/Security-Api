using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using Microsoft.AspNet.Identity;
using System.Security.Claims;
using System.Xml.Linq;

namespace Security.Service.Controllers
{
    [RoutePrefix("User")]
    //[EnableCors(origins: "http://localhost:90,http://localhost:80", headers: "*", methods: "*")]
    public class UserController : ApiController
    {
        private Data.Repository.IUser _user = null;

        public UserController(Data.Repository.IUser user)
        {
            _user = user;
        }
        public UserController() : this(new Data.Repository.User()) { }

        [HttpGet]
        public IHttpActionResult Index()
        {
            return Ok();
        }

        [Route("Read")]
        public HttpResponseMessage Read()
        {
            XDocument xdoc = new XDocument(new XDeclaration("1.0", "utf-8", null));
            XElement xtoken = new XElement("Token");

            ClaimsPrincipal principal = Request.GetRequestContext().Principal as ClaimsPrincipal;

            foreach (var claim in principal.Claims)
            {
                xtoken.Add(new XAttribute(claim.Type, claim.Value));
            }

            xdoc.Add(xtoken);

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, "value");
            response.Content = new StringContent(xdoc.ToString(), System.Text.Encoding.UTF8, "application/json");

            //Response Caching
            //response.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue()
            //{
            //    MaxAge = TimeSpan.FromMinutes(20)
            //};

            return response;
        }

        [Route("ResetPassword")]
        public IHttpActionResult ChangePassword(Data.ViewModels.ResetPassword ob)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string status = _user.ChangePassword(ob);
            if (status == null)
            {
                return Ok("Incorrect Old Password.");
            }

            return Ok(new { sucess = true, message = status });
        }

        // POST /User/FindExtended
        [Route("FindExtended")]
        public IHttpActionResult FindExtendedUser(Data.ViewModels.ApplicationUser appUser)
        {
            Data.Models.ProfileDetails usr = _user.ApplicationUserExists(appUser);

            if (usr == null)
            {
                return Ok("Failure");
            }
            else
            {
                return Json(usr);
            }
        }

        // POST /User/Profile
        [Route("Profile")]
        public IHttpActionResult AddProfile(Data.ViewModels.User userModel)
        {
            if (String.IsNullOrEmpty(userModel.Password))
            {
                ModelState.Remove("userModel.Password");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            dynamic result = null;

            //Check If Application user exists
            Data.Models.ProfileDetails usr = _user.ApplicationUserExists(new Data.ViewModels.ApplicationUser { ApplicationId = userModel.ApplicationId, Username = userModel.UserName });
            if (usr != null)
            {
                result = _user.UpdateUser(userModel);
            }
            else
            {
                result = _user.RegisterUser(userModel, true);
            }

            string ResultType = result.Type;
            string _message = result.Message;

            switch (ResultType)
            {
                case "success":
                    return Ok(_message);
                case "error":
                    return InternalServerError(new Exception(_message));
                case "info":
                    return BadRequest(_message);
            }

            //If get this past then return modelstate
            return Ok(ModelState);
        }

        // POST /User/Add
        [Route("Add")]
        public IHttpActionResult AddUser(Data.ViewModels.User userModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            dynamic result = _user.RegisterUser(userModel);

            string ResultType = result.Type;
            string _message = result.Message;

            switch (ResultType)
            {
                case "success":
                    return Ok(_message);
                case "error":
                    return InternalServerError(new Exception(_message));
                case "info":
                    return BadRequest(_message);
            }

            //If get this past then return modelstate
            return Ok(ModelState);
        }
        
        // POST api/User/List
        [Route("List")]
        public IHttpActionResult ListUsers(string applicationId)
        {
            return Ok(_user.ListUsers(applicationId));
        }

        // POST api/User/Application/Upadate
        [Route("Application/Update")]
        public IHttpActionResult UpdateUserApplications(List<Security.Data.Models.ApplicationUser> userAppModel)
        {
            _user.UpdateUserApplications(userAppModel);
            return Ok();
        }

        // POST api/User/Application/list
        //[Authorize]
        [Route("Application/List")]
        public IHttpActionResult UserApplications()
        {
            return Ok(_user.UserApplications());
        }
        
        //Clean objects after use
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _user.Dispose();
            }

            base.Dispose(disposing);
        }

        //Error Handling
        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                //return InternalServerError();
                return Json(new { Error = new { Code = "500", Message = "Internal Server Error" } });
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("message", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    //return BadRequest();
                    return Json(new { Error = new { Code = HttpStatusCode.BadRequest, Message = "Bad Request" } });
                }

                return BadRequest(ModelState);
            }

            return null;
        }
    }
}
