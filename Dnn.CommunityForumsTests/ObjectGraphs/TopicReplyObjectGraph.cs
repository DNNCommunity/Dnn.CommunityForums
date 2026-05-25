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

namespace DotNetNuke.Modules.ActiveForumsTests.ObjectGraphs
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Modules.ActiveForums.Entities;

    /// <summary>
    /// Provides pre-built <see cref="TopicInfo"/> and <see cref="ReplyInfo"/> object graphs
    /// for use in unit tests. Topics are assigned to forums from <see cref="ForumGroupObjectGraph"/>
    /// and authored by users from <see cref="ForumUserObjectGraph"/>.
    /// </summary>
    internal static class TopicReplyObjectGraph
    {
        // -----------------------------------------------------------------------
        // Content ID constants
        // -----------------------------------------------------------------------
        internal const int AnnouncementTopicContentId = 1;
        internal const int GeneralTopic1ContentId = 2;
        internal const int GeneralTopic2ContentId = 3;
        internal const int HelpTopicContentId = 4;
        internal const int MembersOnlyTopicContentId = 5;

        internal const int GeneralTopic1Reply1ContentId = 101;
        internal const int GeneralTopic1Reply2ContentId = 102;
        internal const int HelpTopicReply1ContentId = 103;

        // -----------------------------------------------------------------------
        // Topic ID constants
        // -----------------------------------------------------------------------
        internal const int AnnouncementTopicId = 1;
        internal const int GeneralTopic1Id = 2;
        internal const int GeneralTopic2Id = 3;
        internal const int HelpTopicId = 4;
        internal const int MembersOnlyTopicId = 5;

        // -----------------------------------------------------------------------
        // Reply ID constants
        // -----------------------------------------------------------------------
        internal const int GeneralTopic1Reply1Id = 1;
        internal const int GeneralTopic1Reply2Id = 2;
        internal const int HelpTopicReply1Id = 3;

        // -----------------------------------------------------------------------
        // Generalized builders
        // -----------------------------------------------------------------------

        /// <summary>
        /// Builds a <see cref="ContentInfo"/> authored by the given <see cref="ForumUserInfo"/>.
        /// </summary>
        internal static ContentInfo BuildContent(
            int contentId,
            int moduleId,
            ForumUserInfo author,
            string subject,
            string body,
            string summary = "",
            DateTime? dateCreated = null,
            DateTime? dateUpdated = null) =>
            new ContentInfo
            {
                ContentId = contentId,
                ModuleId = moduleId,
                AuthorId = author.UserId,
                AuthorName = author.DisplayName,
                Subject = subject,
                Summary = summary,
                Body = body,
                DateCreated = dateCreated ?? DateTime.UtcNow,
                DateUpdated = dateUpdated ?? DateTime.UtcNow,
                IsDeleted = false,
            };

        /// <summary>
        /// Builds a <see cref="TopicInfo"/> assigned to the given <see cref="ForumInfo"/>
        /// and authored by the given <see cref="ForumUserInfo"/>.
        /// </summary>
        internal static TopicInfo BuildTopic(
            int topicId,
            int moduleId,
            int portalId,
            ForumInfo forum,
            ForumUserInfo author,
            ContentInfo content,
            string topicUrl,
            bool isApproved = true,
            bool isLocked = false,
            bool isPinned = false,
            bool isDeleted = false,
            int replyCount = 0,
            int viewCount = 0,
            int lastReplyId = 0,
            int statusId = -1)
        {
            var topic = new TopicInfo
            {
                TopicId = topicId,
                ModuleId = moduleId,
                PortalId = portalId,
                ContentId = content.ContentId,
                Content = content,
                Forum = forum,
                ForumId = forum.ForumID,
                TopicUrl = topicUrl,
                IsApproved = isApproved,
                IsLocked = isLocked,
                IsPinned = isPinned,
                IsDeleted = isDeleted,
                ReplyCount = replyCount,
                ViewCount = viewCount,
                LastReplyId = lastReplyId,
                StatusId = statusId,
                Author = new AuthorInfo(author),
            };

            return topic;
        }

        /// <summary>
        /// Builds a <see cref="ReplyInfo"/> belonging to the given <see cref="TopicInfo"/>
        /// and authored by the given <see cref="ForumUserInfo"/>.
        /// </summary>
        internal static ReplyInfo BuildReply(
            int replyId,
            int moduleId,
            int portalId,
            TopicInfo topic,
            ForumUserInfo author,
            ContentInfo content,
            bool isApproved = true,
            bool isDeleted = false,
            int replyToId = 0) =>
            new ReplyInfo
            {
                ReplyId = replyId,
                TopicId = topic.TopicId,
                Topic = topic,
                ModuleId = moduleId,
                PortalId = portalId,
                ContentId = content.ContentId,
                Content = content,
                ReplyToId = replyToId,
                IsApproved = isApproved,
                IsDeleted = isDeleted,
                StatusId = 0,
                Author = new AuthorInfo(author),
            };

        // -----------------------------------------------------------------------
        // Full graph
        // -----------------------------------------------------------------------

        /// <summary>
        /// Builds the complete topic and reply graph.
        /// Forums are resolved from <paramref name="forumsGraph"/> by
        /// <see cref="ForumGroupObjectGraph"/> ID constants. Authors are resolved from
        /// <paramref name="forumUserGraph"/> by <see cref="ForumUserObjectGraph"/> profile ID constants.
        /// </summary>
        internal static List<TopicInfo> BuildFullGraph(
            int moduleId,
            int portalId,
            List<ForumInfo> forumsGraph,
            List<ForumUserInfo> forumUserGraph)
        {
            var announcementsForum = forumsGraph.Find(f => f.ForumID == ForumsObjectGraph.AnnouncementsForumId);
            var generalDiscussionForum = forumsGraph.Find(f => f.ForumID == ForumsObjectGraph.GeneralDiscussionForumId);
            var helpForum = forumsGraph.Find(f => f.ForumID == ForumsObjectGraph.HelpAndSupportForumId);
            var membersOnlyForum = forumsGraph.Find(f => f.ForumID == ForumsObjectGraph.MembersOnlyForumId);

            var admin = forumUserGraph.Find(u => u.ProfileId == ForumUserObjectGraph.AdminProfileId);
            var user10 = forumUserGraph.Find(u => u.ProfileId == ForumUserObjectGraph.RegisteredUser10ProfileId);
            var user12 = forumUserGraph.Find(u => u.ProfileId == ForumUserObjectGraph.RegisteredUser12ProfileId);
            var moderator = forumUserGraph.Find(u => u.ProfileId == ForumUserObjectGraph.ModeratorProfileId);

            // Announcements — one pinned topic by admin, no replies
            var announcementContent = BuildContent(AnnouncementTopicContentId, moduleId, admin, "Welcome to DNN Community Forums", "<p>Welcome to the community!</p>", "Community welcome announcement.");
            var announcementTopic = BuildTopic(AnnouncementTopicId, moduleId, portalId, announcementsForum, admin, announcementContent, "welcome-to-dnn-community-forums", isPinned: true, viewCount: 120);

            // General Discussion — two topics; topic 1 has two replies
            var generalTopic1Content = BuildContent(GeneralTopic1ContentId, moduleId, user10, "Getting Started Guide", "<p>Here is how to get started.</p>", "Beginner guide.");
            var generalTopic1Reply1Content = BuildContent(GeneralTopic1Reply1ContentId, moduleId, user12, "Re: Getting Started Guide", "<p>Thanks, very helpful!</p>");
            var generalTopic1Reply2Content = BuildContent(GeneralTopic1Reply2ContentId, moduleId, moderator, "Re: Getting Started Guide", "<p>Pinned for visibility.</p>");

            var generalTopic1 = BuildTopic(GeneralTopic1Id, moduleId, portalId, generalDiscussionForum, user10, generalTopic1Content, "getting-started-guide", replyCount: 2, viewCount: 55, lastReplyId: GeneralTopic1Reply2Id);
            var generalTopic1Reply1 = BuildReply(GeneralTopic1Reply1Id, moduleId, portalId, generalTopic1, user12, generalTopic1Reply1Content);
            var generalTopic1Reply2 = BuildReply(GeneralTopic1Reply2Id, moduleId, portalId, generalTopic1, moderator, generalTopic1Reply2Content, replyToId: GeneralTopic1Reply1Id);
            generalTopic1.LastReply = generalTopic1Reply2;
            generalTopic1.WithReplies(new List<ReplyInfo> { generalTopic1Reply1, generalTopic1Reply2 });

            var generalTopic2Content = BuildContent(GeneralTopic2ContentId, moduleId, user12, "Off-Topic Chat", "<p>Random discussion.</p>");
            var generalTopic2 = BuildTopic(GeneralTopic2Id, moduleId, portalId, generalDiscussionForum, user12, generalTopic2Content, "off-topic-chat", viewCount: 10);

            // Help & Support — one topic with one reply (resolved)
            var helpTopicContent = BuildContent(HelpTopicContentId, moduleId, user10, "How do I reset my password?", "<p>I cannot log in.</p>");
            var helpTopicReply1Content = BuildContent(HelpTopicReply1ContentId, moduleId, admin, "Re: How do I reset my password?", "<p>Use the forgot password link.</p>");

            var helpTopic = BuildTopic(HelpTopicId, moduleId, portalId, helpForum, user10, helpTopicContent, "how-do-i-reset-my-password", replyCount: 1, viewCount: 30, lastReplyId: HelpTopicReply1Id, statusId: 3 /* Resolved */);
            var helpTopicReply1 = BuildReply(HelpTopicReply1Id, moduleId, portalId, helpTopic, admin, helpTopicReply1Content);
            helpTopic.LastReply = helpTopicReply1;
            helpTopic.WithReplies(new List<ReplyInfo> { helpTopicReply1 });

            // Members Only — one topic by moderator, no replies
            var membersOnlyTopicContent = BuildContent(MembersOnlyTopicContentId, moduleId, moderator, "Members Announcements", "<p>For registered members only.</p>");
            var membersOnlyTopic = BuildTopic(MembersOnlyTopicId, moduleId, portalId, membersOnlyForum, moderator, membersOnlyTopicContent, "members-announcements", viewCount: 5);

            return new List<TopicInfo>
            {
                announcementTopic,
                generalTopic1,
                generalTopic2,
                helpTopic,
                membersOnlyTopic,
            };
        }
    }
}
