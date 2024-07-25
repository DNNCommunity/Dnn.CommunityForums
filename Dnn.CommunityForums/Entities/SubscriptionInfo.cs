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
namespace DotNetNuke.Modules.ActiveForums.Entities
{
    using System;
    using System.Globalization;
    using System.Web.Caching;

    using DotNetNuke.ComponentModel.DataAnnotations;
    using DotNetNuke.Modules.ActiveForums.API;

    [TableName("activeforums_Subscriptions")]
    [PrimaryKey("Id", AutoIncrement = true)]
    public class SubscriptionInfo
    {
        public SubscriptionInfo() { }

        public SubscriptionInfo(int id, int portalId, int moduleId, int forumId, int topicId, int mode, int userId, string forumGroupName, string forumName, string subject, DateTime lastPostDate, bool subscribed)
        {
            this.Id = id;
            this.PortalId = portalId;
            this.ModuleId = moduleId;
            this.ForumId = forumId;
            this.TopicId = topicId;
            this.Mode = mode;
            this.UserId = userId;
            this.ForumGroupName = forumGroupName;
            this.ForumName = forumName;
            this.Subject = subject;
            this.LastPostDate = lastPostDate;
            this.Subscribed = subscribed;
        }

        private DateTime? _lastPostDate;
        private string _subject;
        private string _forumName;
        private string _forumGroupName;
        private string _email;
        private DotNetNuke.Modules.ActiveForums.User _user;
        private DotNetNuke.Modules.ActiveForums.Entities.ForumInfo _forumInfo;
        private DotNetNuke.Modules.ActiveForums.Entities.TopicInfo _topicInfo;

        public int Id { get; set; }

        public int PortalId { get; set; }

        public int ModuleId { get; set; }

        public int ForumId { get; set; }

        public int TopicId { get; set; }

        public int Mode { get; set; }

        public int UserId { get; set; }

        [IgnoreColumn()]
        public bool TopicSubscriber { get => this.TopicId > 0; }

        [IgnoreColumn()]
        public bool ForumSubscriber { get => this.ForumId > 0 && this.TopicId == 0; }

        [IgnoreColumn()]
        public DotNetNuke.Modules.ActiveForums.User User => this._user ?? (this._user = new DotNetNuke.Modules.ActiveForums.UserController().GetUser(this.PortalId, this.ModuleId, this.UserId));

        [IgnoreColumn()]
        public DotNetNuke.Modules.ActiveForums.Entities.ForumInfo Forum => this._forumInfo ?? (this._forumInfo = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(this.ForumId, this.ModuleId));

        [IgnoreColumn()]
        public DotNetNuke.Modules.ActiveForums.Entities.TopicInfo Topic => this._topicInfo ?? (this._topicInfo = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(this.TopicId));

        [IgnoreColumn()]
        public string Email { get => this._email ?? (this._email = this.User?.Email); set => this._email = value; }

        [IgnoreColumn()]
        public TimeSpan TimeZoneOffSet { get => Utilities.GetTimeZoneOffsetForUser(this.PortalId, this.UserId); }

        [IgnoreColumn()]
        public CultureInfo UserCulture { get => Utilities.GetCultureInfoForUser(this.PortalId, this.UserId); }

        [IgnoreColumn()]
        public string ForumGroupName { get => this._forumGroupName ?? (this._forumGroupName = this.Forum.GroupName); set => this._forumGroupName = value; }

        [IgnoreColumn()]
        public string ForumName { get => this._forumName ?? (this._forumName = this.Forum.ForumName); set => this._forumName = value; }

        [IgnoreColumn()]
        public string Subject { get => this._subject ?? (this._subject = string.IsNullOrEmpty(this.Topic?.Content?.Subject) ? string.Empty : this.Topic?.Content?.Subject); set => this._subject = value; }

        [IgnoreColumn()]
        public DateTime LastPostDate { get => (DateTime)(this._lastPostDate ?? (this._lastPostDate = this.TopicId > 0 ? this.Topic?.Content?.DateUpdated : this.Forum?.LastPostDateTime)); set => this._lastPostDate = value; }

        [IgnoreColumn()]
        public bool Subscribed { get; set; }
    }
}
