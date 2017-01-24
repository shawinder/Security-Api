using System;
using System.Web.Http;

namespace Security.Service.Controllers
{
    [RoutePrefix("Application")]
    public class ApplicationController : ApiController
    {
        private Data.Repository.IApplication _application;
        public ApplicationController(Data.Repository.IApplication application)
        {
            _application = application;
        }
        public ApplicationController() : this(new Data.Repository.Applicaiton()) { }

        // POST api/Application/list
        [Route("List")]
        public IHttpActionResult List()
        {
            var apps = _application.List();

            if (apps.Count == 0)
            {
                return NotFound(); // Returns a NotFoundResult
            }

            return Ok(apps);
        }

        // POST api/Application/Add
        [Route("Add")]
        public IHttpActionResult Add(Data.Models.Application appModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            dynamic result = _application.Add(appModel);

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


        #region "Application Groups"

        // POST api/Applicaiton/ListGroups
        [Route("ListGroups")]
        public IHttpActionResult ListGroups(Guid ApplicationId)
        {
            var applicaitonGroups = _application.ListGroups(ApplicationId);
            //return Json(applicaitonGroups);
            return new ActionResults.JsonResult(Request, applicaitonGroups);
        }

        //POST api/Applicaiton/DefaultGroupTypes
        [Route("ListAccessTypes")]
        public IHttpActionResult ListAccessTypes(Guid ApplicationId)
        {
            var groupTypes = _application.AccessTypes(ApplicationId);
            return new ActionResults.JsonResult(Request, groupTypes);
        }

        //POST api/Applicaiton/ListGroupRules
        [Route("ListGroupRules")]
        public IHttpActionResult ListGroupRules(Guid ApplicationGroupId)
        {
            var groupRules = _application.GroupRules(ApplicationGroupId);
            return new ActionResults.JsonResult(Request, groupRules);
        }

        //POST api/Applicaiton/ListADMappings
        [Route("ListADGroups")]
        public IHttpActionResult ListADGroups(Guid ApplicationId)
        {
            var ADMappings = _application.ListADGroups(ApplicationId);
            return new ActionResults.JsonResult(Request, ADMappings);
        }

        //POST api/Applicaiton/ListADMappings
        [Route("ListADMappings")]
        public IHttpActionResult ListADMappings(Guid ApplicationId)
        {
            var ADMappings = _application.ListADMappings(ApplicationId);
            return new ActionResults.JsonResult(Request, ADMappings);
        }

        // POST api/Applicaiton/AddADMapping
        [Route("AddADMapping")]
        public IHttpActionResult AddADMapping(Data.ViewModels.ADMapping adMapping)
        {
            var groupTypes = _application.AddADMapping(adMapping);

            return Json(groupTypes);
        }

        // POST api/Applicaiton/UpdateADMapping
        [Route("UpdateADMapping")]
        public IHttpActionResult UpdateADMapping(Data.ViewModels.ADMapping adMapping)
        {
            var groupTypes = _application.UpdateADMapping(adMapping);

            return Json(groupTypes);
        }

        // POST api/Applicaiton/AddGroupRules
        [Route("AddGroupRules")]
        public IHttpActionResult AddGroupRules(Data.ViewModels.ApplicationGroup applicationGroup)
        {
            var groupTypes = _application.AddGroupRules(applicationGroup);

            return Json(groupTypes);
        }

        // POST api/Applicaiton/UpdateGroupRules
        [Route("UpdateGroupRules")]
        public IHttpActionResult UpdateGroupRules(Data.ViewModels.ApplicationGroup applicationGroup)
        {
            var groupTypes = _application.UpdateGroupRules(applicationGroup);

            return Json(groupTypes);
        }

        #endregion

        //Clean objects after use
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _application.Dispose();
            }

            base.Dispose(disposing);
        }

        // POST api/Application/Test
        //[Route("Test")]
        //public async Task<IHttpActionResult> GetTest()
        //{
        //    var result = _repo.SPGetTest();

        //    return Ok(result);
        //}
    }
}
