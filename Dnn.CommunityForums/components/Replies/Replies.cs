// Copyright (c) by DNN Community
//
// DNN Community licenses this file to you under the MIT license.
//
// See the LICENSE file in the project root for more information.
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

namespace DotNetNuke.Modules.ActiveForums
{
    using System;

    [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo.")]
    public class ReplyInfo : DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo { }

    [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.ReplyController()")]
    public class ReplyController
    {
        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.ReplyController()")]
        public void Reply_Delete(int portalId, int forumId, int topicId, int replyId, int delBehavior) => new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController(-1).Reply_Delete(portalId, forumId, topicId, replyId, delBehavior);

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.ReplyController()")]
        public int Reply_QuickCreate(int portalId, int moduleId, int forumId, int topicId, int replyToId, string subject, string body, int userId, string displayName, bool isApproved, string iPAddress) => new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController(moduleId).Reply_QuickCreate(portalId, moduleId, forumId, topicId, replyToId, subject, body, userId, displayName, isApproved, iPAddress);

        [Obsolete("Deprecated in Community Forums. Scheduled removal in 09.00.00. Use ReplyController.Reply_Save(int PortalId, int ModuleId, ReplyInfo ri)")]
        public int Reply_Save(int portalId, ReplyInfo ri) => new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController(-1).Reply_Save(portalId, -1, ri);

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.ReplyController()")]
        public int Reply_Save(int portalId, int moduleId, DotNetNuke.Modules.ActiveForums.ReplyInfo ri) => new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController(moduleId).Reply_Save(portalId, moduleId, ri);

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.ReplyController()")]
        public DotNetNuke.Modules.ActiveForums.ReplyInfo Reply_Get(int portalId, int moduleId, int topicId, int replyId) => (DotNetNuke.Modules.ActiveForums.ReplyInfo)new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController(moduleId).GetById(replyId);

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.ReplyController()")]
        public DotNetNuke.Modules.ActiveForums.ReplyInfo ApproveReply(int portalId, int tabId, int moduleId, int forumId, int topicId, int replyId) => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Moved to Utilities and changed to internal in 10.00.00.")]
        public void UpdateModuleLastContentModifiedOnDate(int moduleId) => Utilities.UpdateModuleLastContentModifiedOnDate(moduleId);
    }
}
