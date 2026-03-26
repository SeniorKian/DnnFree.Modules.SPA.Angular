using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
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

        [HttpGet]
        [DnnAuthorize] // 👈 فقط کاربرهای لاگین شده
        public HttpResponseMessage GetCurrentUser()
        {
            var user = new
            {
                UserId = UserInfo.UserID,
                Username = UserInfo.Username,
                DisplayName = UserInfo.DisplayName,
                Email = UserInfo.Email,
                FirstName = UserInfo.FirstName,
                LastName = UserInfo.LastName,
                IsSuperUser = UserInfo.IsSuperUser, // آیا ادمین کل سایته؟

                // نقش‌های کاربر
                Roles = UserInfo.Roles,
                // دسترسی‌های ماژول
                IsAdmin = UserInfo.IsInRole(PortalSettings.AdministratorRoleName),

                // اطلاعات پورتال
                PortalId = PortalSettings.PortalId,
                ModuleId = GetModuleId()
            };

            return Request.CreateResponse(HttpStatusCode.OK, user);
        }

        [HttpGet]
        [DnnAuthorize]
        public HttpResponseMessage CheckPermissions()
        {
            var moduleId = GetModuleId();

            var permissions = new
            {
                CanEdit = ModulePermissionController.HasModuleAccess(
                    SecurityAccessLevel.Edit,
                    "EDIT",
                    ActiveModule
                ),

                CanView = ModulePermissionController.HasModuleAccess(
                    SecurityAccessLevel.View,
                    "VIEW",
                    ActiveModule
                ),

                IsModuleAdmin = UserInfo.IsInRole(PortalSettings.AdministratorRoleName),

                IsSuperUser = UserInfo.IsSuperUser
            };

            return Request.CreateResponse(HttpStatusCode.OK, permissions);
        }
    }
}
