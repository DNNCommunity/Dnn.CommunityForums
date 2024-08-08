// Copyright (c) 2013-2024 by DNN Community
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

    [TableName("activeforums_Topics_Tracking")]
    [PrimaryKey("TrackingId", AutoIncrement = true)]

    internal class TopicTrackingInfo
    {
        private DotNetNuke.Modules.ActiveForums.Entities.TopicInfo topic;
        private DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo forumUser;

        public int TrackingId { get; set; }

        public int ForumId { get; set; }

        public int TopicId { get; set; }

        public int LastReplyId { get; set; } = 0;

        public int UserId { get; set; }
        
        public DateTime DateAdded { get; set; } = DateTime.UtcNow;

        public DateTime DateAdded { get; set; } = DateTime.UtcNow;

        [IgnoreColumn()]
        public DotNetNuke.Modules.ActiveForums.Entities.TopicInfo Topic
        {
            get => this.topic ?? (this.topic = this.LoadTopic());
            set => this.topic = value;
        }

        internal DotNetNuke.Modules.ActiveForums.Entities.TopicInfo LoadTopic()
        {
            this.topic = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(this.TopicId);
            if (this.topic == null)
            {
                var log = new DotNetNuke.Services.Log.EventLog.LogInfo { LogTypeKey = DotNetNuke.Abstractions.Logging.EventLogType.ADMIN_ALERT.ToString() };
                log.LogProperties.Add(new LogDetailInfo("Module", Globals.ModuleFriendlyName));
                string message = string.Format(Utilities.GetSharedResource("[RESX:TopicMissingForTopicTrackingId]"), this.TopicId, this.TrackingId);
                log.AddProperty("Message", message);
                DotNetNuke.Services.Log.EventLog.LogController.Instance.AddLog(log);

                var ex = new NullReferenceException(string.Format(Utilities.GetSharedResource("[RESX:TopicMissingForTopicTrackingId]"), this.TopicId, this.TrackingId));
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                throw ex;
            }

            return this.topic;
        }

        [IgnoreColumn()]
        public DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo ForumUser
        {
            get => this.forumUser ?? (this.forumUser = this.GetForumUser());
            set => this.forumUser = value;
        }

        internal DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo GetForumUser()
        {
            this.forumUser = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController().GetByUserId(this.Topic.PortalId, this.UserId);
            if (this.forumUser == null)
            {
                this.forumUser = new DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo
                {
                    UserId = this.UserId,
                };
                this.forumUser.UserInfo.DisplayName = this.UserId > 0 ? Utilities.GetSharedResource("[RESX:DeletedUser]") : Utilities.GetSharedResource("[RESX:Anonymous]");
            }

            return this.forumUser;
        }
    }
}
