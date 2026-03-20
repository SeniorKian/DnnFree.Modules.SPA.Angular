using DotNetNuke.Security;
using DotNetNuke.Web.Api;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace DnnFree.Modules.SPA.Angular
{
    [SupportedModules("DnnFree.Modules.SPA.Angular")]
    [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
    public class MController : DnnApiController
    {
        /// <summary>
        /// Gets the ModuleID from the request header or ActiveModule
        /// </summary>
        private int GetModuleId()
        {
            try
            {
                // Try to get from request header first
                if (Request?.Headers != null && Request.Headers.Contains("ModuleId"))
                {
                    var moduleIdHeader = Request.Headers.GetValues("ModuleId").FirstOrDefault();
                    if (int.TryParse(moduleIdHeader, out int moduleId) && moduleId > 0)
                    {
                        return moduleId;
                    }
                }

                // Fallback to ActiveModule
                if (ActiveModule != null && ActiveModule.ModuleID > 0)
                {
                    return ActiveModule.ModuleID;
                }
            }
            catch
            {
                // ignored, will fall through and return -1
            }

            return -1;
        }

        /*[HttpPost]
        [DnnAuthorize]
        [ValidateAntiForgeryToken] */

        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage PublicAccess()
        {
            var moduleId = GetModuleId();
            string response = "PublicAccess : " + moduleId;
            return Request.CreateResponse(response);
        }

        [HttpGet]
        [DnnAuthorize]
        public HttpResponseMessage UserAccess()
        {
            var moduleId = GetModuleId();
            string response = "UserAccess : " + moduleId + "\nUser :" + UserInfo.DisplayName;
            return Request.CreateResponse(response);
        }
    }
}
