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

namespace DotNetNuke.Modules.ActiveForums.Entities
{
using System;

using DotNetNuke.ComponentModel.DataAnnotations;
using DotNetNuke.Services.Log.EventLog;

[TableName("activeforums_Forums_Tracking")]
[PrimaryKey("TrackingId", AutoIncrement = true)]

internal class ForumTrackingInfo
    {
        private DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forum;
        private DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo forumUser;

        public int TrackingId { get; set; }

        public int ModuleId { get; set; }

        public int UserId { get; set; }

        public int ForumId { get; set; }

        [ColumnName("LastAccessDate")]
        public DateTime LastAccessDateTime { get; set; } = DateTime.UtcNow;

        public int MaxTopicRead { get; set; } = 0;

        public int MaxReplyRead { get; set; } = 0;

        [IgnoreColumn()]
        public DotNetNuke.Modules.ActiveForums.Entities.ForumInfo Forum
        {
            get => this.forum ?? (this.forum = this.LoadForum());
            set => this.forum = value;
        }

        internal DotNetNuke.Modules.ActiveForums.Entities.ForumInfo LoadForum()
        {
            this.forum = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(this.ForumId, this.ModuleId);
            if (this.forum == null)
            {
                var log = new DotNetNuke.Services.Log.EventLog.LogInfo { LogTypeKey = DotNetNuke.Abstractions.Logging.EventLogType.ADMIN_ALERT.ToString() };
                log.LogProperties.Add(new LogDetailInfo("Module", Globals.ModuleFriendlyName));
                string message = string.Format(Utilities.GetSharedResource("[RESX:ForumMissingForForumTrackingId]"), this.ForumId, this.TrackingId);
                log.AddProperty("Message", message);
                DotNetNuke.Services.Log.EventLog.LogController.Instance.AddLog(log);

                var ex = new NullReferenceException(string.Format(Utilities.GetSharedResource("[RESX:ForumMissingForForumTrackingId]"), this.ForumId, this.TrackingId));
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                throw ex;
            }

            return this.forum;
        }

        [IgnoreColumn()]
        public DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo ForumUser
        {
            get => this.forumUser ?? (this.forumUser = this.GetForumUser());
            set => this.forumUser = value;
        }

        internal DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo GetForumUser()
        {
            this.forumUser = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.Forum.ModuleId).GetByUserId(this.Forum.PortalId, this.UserId);
            if (this.forumUser == null)
            {
                this.forumUser = new DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo(this.ModuleId)
                {
                    UserId = this.UserId,
                };
                this.forumUser.UserInfo.DisplayName = this.UserId > 0 ? Utilities.GetSharedResource("[RESX:DeletedUser]") : Utilities.GetSharedResource("[RESX:Anonymous]");
            }

            return this.forumUser;
        }
    }
}
