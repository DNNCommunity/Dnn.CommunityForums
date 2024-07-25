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
namespace DotNetNuke.Modules.ActiveForums
{
    using System;
    using System.Reflection;

    [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo.")]
    public class ReplyInfo : DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo { }

    [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.ReplyController()")]
    public class ReplyController
    {
        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.ReplyController()")]
        public void Reply_Delete(int PortalId, int ForumId, int TopicId, int ReplyId, int DelBehavior) => new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController().Reply_Delete(PortalId, ForumId, TopicId, ReplyId, DelBehavior);

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.ReplyController()")]
        public int Reply_QuickCreate(int PortalId, int ModuleId, int ForumId, int TopicId, int ReplyToId, string Subject, string Body, int UserId, string DisplayName, bool IsApproved, string IPAddress) => new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController().Reply_QuickCreate(PortalId, ModuleId, ForumId, TopicId, ReplyToId, Subject, Body, UserId, DisplayName, IsApproved, IPAddress);

        [Obsolete("Deprecated in Community Forums. Scheduled removal in 09.00.00. Use ReplyController.Reply_Save(int PortalId, int ModuleId, ReplyInfo ri)")]
        public int Reply_Save(int PortalId, ReplyInfo ri) => new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController().Reply_Save(PortalId, -1, ri);

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.ReplyController()")]
        public int Reply_Save(int PortalId, int ModuleId, DotNetNuke.Modules.ActiveForums.ReplyInfo ri) => new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController().Reply_Save(PortalId, ModuleId, ri);

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.ReplyController()")]
        public DotNetNuke.Modules.ActiveForums.ReplyInfo Reply_Get(int PortalId, int ModuleId, int TopicId, int ReplyId) => (DotNetNuke.Modules.ActiveForums.ReplyInfo)new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController().GetById(ReplyId);

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.ReplyController()")]
        public DotNetNuke.Modules.ActiveForums.ReplyInfo ApproveReply(int PortalId, int TabId, int ModuleId, int ForumId, int TopicId, int ReplyId) => (DotNetNuke.Modules.ActiveForums.ReplyInfo)new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController().ApproveReply(PortalId, TabId, ModuleId, ForumId, TopicId, ReplyId);

        [Obsolete("Deprecated in Community Forums. Moved to Utilities and changed to internal in 10.00.00.")]
        public void UpdateModuleLastContentModifiedOnDate(int ModuleId) => Utilities.UpdateModuleLastContentModifiedOnDate(ModuleId);
    }
}
