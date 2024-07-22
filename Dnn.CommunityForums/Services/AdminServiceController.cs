//
// Community Forums
// Copyright (c) 2013-2024
// by DNN Community
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
//
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Security;
using DotNetNuke.UI.UserControls;
using DotNetNuke.Web.Api;


namespace DotNetNuke.Modules.ActiveForums
{
    [ValidateAntiForgeryToken]
    [SupportedModules(Globals.ModuleName)]
    [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
    public class AdminServiceController : DnnApiController
    {
        // DTO for ToggleUrlHandler
        public class ToggleUrlHandlerDTO
        {
            public int ModuleId { get; set; }
        }

        public HttpResponseMessage ToggleURLHandler(ToggleUrlHandlerDTO dto)
        {
            if (Utilities.IsRewriteLoaded())
            {
                ConfigUtils.UninstallRewriter(DotNetNuke.Modules.ActiveForums.Utilities.MapPath("~/web.config"));
                return Request.CreateResponse(HttpStatusCode.OK, "disabled");
            }

            ConfigUtils.InstallRewriter(DotNetNuke.Modules.ActiveForums.Utilities.MapPath("~/web.config"));
            return Request.CreateResponse(HttpStatusCode.OK, "enabled");
        }

        //DTO for RunMaintenance
        public class RunMaintenanceDTO
        {
            public int ModuleId { get; set; }
            public int ForumId { get; set; }
            public int OlderThan { get; set; }
            public int ByUserId { get; set; }
            public int LastActive { get; set; }
            public bool WithNoReplies { get; set; }
            public bool DryRun { get; set; }
        }

        public HttpResponseMessage RunMaintenance(RunMaintenanceDTO dto)
        {
            var moduleSettings = new SettingsInfo { MainSettings = DotNetNuke.Entities.Modules.ModuleController.Instance.GetModule(moduleId: dto.ModuleId, DotNetNuke.Common.Utilities.Null.NullInteger, true).ModuleSettings };
            var rows = DataProvider.Instance().Forum_Maintenance(dto.ForumId, dto.OlderThan, dto.LastActive, dto.ByUserId, dto.WithNoReplies, dto.DryRun, moduleSettings.DeleteBehavior);
            if (dto.DryRun)
                return Request.CreateResponse(HttpStatusCode.OK, new { Result = string.Format(Utilities.GetSharedResource("[RESX:Maint:DryRunResults]", true), rows.ToString()) });

            return Request.CreateResponse(HttpStatusCode.OK, new { Result = Utilities.GetSharedResource("[RESX:ProcessComplete]", true) });
        }

        public string GetSecurityGrid(int groupId, int forumId)  // Needs DTO
        {
            var sb = new StringBuilder();

            return sb.ToString();
        }

        public class UserProfileDTO
        {
            public int UserId { get; set; }
            public int? TrustLevel { get; set; }
            public string UserCaption { get; set; }
            public string Signature { get; set; }
            public int? RewardPoints { get; set; }
        }

        [HttpGet]
        public HttpResponseMessage GetUserProfile(int userId)
        {
            var up = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController().GetByUserId(ActiveModule.PortalID, userId);
            if(up == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var result = new
                             {
                                 up.UserId,
                                 up.TrustLevel,
                                 up.UserCaption,
                                 up.Signature,
                                 up.RewardPoints
                             };

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [HttpPost]
        public HttpResponseMessage UpdateUserProfile(UserProfileDTO dto)
        {
            var up = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController().GetByUserId(ActiveModule.PortalID, dto.UserId);

            if (up == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            if (dto.TrustLevel.HasValue)
                up.TrustLevel = dto.TrustLevel.Value;

            up.UserCaption = dto.UserCaption;
            up.Signature = dto.Signature;

            if (dto.RewardPoints.HasValue)
                up.RewardPoints = dto.RewardPoints.Value;

            new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController().Update(up);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        //DTO for ToggleSecurity
        public class ToggleSecurityDTO
        {
            public int ModuleId { get; set; }
            public string Action { get; set; }
            public int PermissionsId { get; set; }
            public string SecurityId { get; set; }
            public int SecurityType { get; set; }
            public string SecurityAccessRequested { get; set; }
            public string ReturnId { get; set; }
        }

        [HttpPost]
        public HttpResponseMessage ToggleSecurity(ToggleSecurityDTO dto)
        {
            switch (dto.Action)
            {
                case "delete":
                    {
                        DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.RemoveObjectFromAll(dto.ModuleId, dto.SecurityId, dto.SecurityType, dto.PermissionsId);
                        return Request.CreateResponse(HttpStatusCode.OK);
                    }
                case "addobject":
                    {
                        if (dto.SecurityType == 1)
                        {
                            var ui = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController().GetByUserId(ActiveModule.PortalID, Convert.ToInt32(dto.SecurityId));
                            dto.SecurityId = ui != null ? ui.UserId.ToString() : string.Empty;
                        }
                        else
                        {
                            if (dto.SecurityId.Contains(":"))
                                dto.SecurityType = 2;
                        }
                        if (!(string.IsNullOrEmpty(dto.SecurityId)))
                        {
                            DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddObjectToPermissions(dto.ModuleId, dto.PermissionsId, "View", dto.SecurityId, dto.SecurityType);
                        }

                        return Request.CreateResponse(HttpStatusCode.OK);
                    }
                default:
                    {
                        if (dto.Action == "remove")
                        {
                            DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.RemoveObjectFromPermissions(dto.ModuleId,dto.PermissionsId, dto.SecurityAccessRequested, dto.SecurityId, dto.SecurityType);
                        }
                        else
                        {
                            DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddObjectToPermissions(dto.ModuleId, dto.PermissionsId, dto.SecurityAccessRequested, dto.SecurityId, dto.SecurityType);
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, dto.Action + "|" + dto.ReturnId);
                    }
            }
        }

    }
}
