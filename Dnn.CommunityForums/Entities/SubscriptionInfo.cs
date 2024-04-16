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
using DotNetNuke.ComponentModel.DataAnnotations;
using DotNetNuke.Modules.ActiveForums.API;
using System;
using System.Globalization;
using System.Web.Caching;
namespace DotNetNuke.Modules.ActiveForums.Entities
{
    [TableName("activeforums_Subscriptions")]
    [PrimaryKey("Id", AutoIncrement = true)]
    public class SubscriptionInfo
    {
        public SubscriptionInfo() { }
        public SubscriptionInfo(int id, int portalId, int moduleId, int forumId, int topicId, int mode, int userId, string forumGroupName, string forumName, string subject, DateTime lastPostDate, bool subscribed)
        {
            Id = id;
            PortalId = portalId;
            ModuleId = moduleId;
            ForumId = forumId;
            TopicId = topicId;
            Mode = mode;
            UserId = userId;
            ForumGroupName = forumGroupName;
            ForumName = forumName;
            Subject = subject;
            LastPostDate = lastPostDate;
            Subscribed = subscribed;
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
        public bool TopicSubscriber { get => TopicId > 0; }
        [IgnoreColumn()]
        public bool ForumSubscriber { get => (ForumId > 0 && TopicId == 0); }
        [IgnoreColumn()]
        public DotNetNuke.Modules.ActiveForums.User User => _user ?? (_user = new DotNetNuke.Modules.ActiveForums.UserController().GetUser(PortalId, ModuleId, UserId));
        [IgnoreColumn()]
        public DotNetNuke.Modules.ActiveForums.Entities.ForumInfo Forum => _forumInfo ?? (_forumInfo = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(ForumId, ModuleId)); 
        [IgnoreColumn()]
        public DotNetNuke.Modules.ActiveForums.Entities.TopicInfo Topic => _topicInfo ?? (_topicInfo = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(TopicId)); 
        [IgnoreColumn()] 
        public string Email { get => _email ?? (_email = User?.Email); set => _email = value; }
        [IgnoreColumn()]
        public TimeSpan TimeZoneOffSet { get => Utilities.GetTimeZoneOffsetForUser(PortalId, UserId); }
        [IgnoreColumn()]
        public CultureInfo UserCulture { get => Utilities.GetCultureInfoForUser(PortalId, UserId); }
        [IgnoreColumn()]
        public string ForumGroupName { get => _forumGroupName ?? (_forumGroupName = Forum.GroupName); set => _forumGroupName = value; }
        [IgnoreColumn()]
        public string ForumName { get => _forumName ?? (_forumName = Forum.ForumName); set => _forumName = value; }
        [IgnoreColumn()]
        public string Subject { get => _subject ?? (_subject = string.IsNullOrEmpty(Topic?.Content?.Subject) ? string.Empty : Topic?.Content?.Subject); set => _subject = value; } 
        [IgnoreColumn()]
        public DateTime LastPostDate { get => (DateTime)(_lastPostDate ?? (_lastPostDate = (TopicId > 0 ? Topic?.Content?.DateUpdated : Forum?.LastPostDateTime))); set => _lastPostDate = value; }
        [IgnoreColumn()]
        public bool Subscribed { get; set; }
    }
}
