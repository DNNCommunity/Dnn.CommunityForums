//
// Community Forums
// Copyright (c) 2013-2021
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
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security;
using DotNetNuke.Security.Roles;
using DotNetNuke.Web.Api;

namespace DotNetNuke.Modules.ActiveForums.Services.Controllers
{
    [DnnAuthorize]
    public class ReplyController : ControllerBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DotNetNuke.Modules.ActiveForums.Services.Controllers.ReplyController));
        
        [HttpGet]
        [DnnAuthorize]
        [ForumsAuthorize(SecureActions.View)]
        public HttpResponseMessage Get(int ForumId, int ReplyId)
        {
            if (!ControllerHelper.IsAuthorized(PortalSettings.PortalId, ActiveModule.ModuleID, ForumId, SecureActions.View, UserInfo))
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            return Request.CreateResponse(HttpStatusCode.OK, new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController().Get(PortalController.GetCurrentPortalSettings().PortalId, ActiveModule.ModuleID, ReplyId));
        }            
        [HttpPost]
        [DnnAuthorize]
        [ValidateAntiForgeryToken]
        [ForumsAuthorize(SecureActions.ModApprove)]
        public HttpResponseMessage Approve(int ForumId, Entities.ReplyInfo reply)
        {
            new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController().Approve(reply);
            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}