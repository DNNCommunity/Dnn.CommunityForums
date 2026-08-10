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

namespace DotNetNuke.Modules.ActiveForums.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Xml.Linq;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Modules.ActiveForums.Controllers;
    using DotNetNuke.Modules.ActiveForums.Controls;
    using DotNetNuke.Modules.ActiveForums.Entities;
    using DotNetNuke.Modules.ActiveForums.Extensions;
    using DotNetNuke.Modules.ActiveForums.Services.Cache;
    using DotNetNuke.Modules.ActiveForums.ViewModels;
    using DotNetNuke.Services.Log.EventLog;

    internal static class ImportExportHelper
    {
        internal static readonly IReadOnlyList<string> PortableEntityDependencyOrder =
        [
            "permissions",
            "settings",
            "userProfiles",
            "groups",
            "tags",
            "badges",
            "ranks",
            "filters",
            "forums",
            "properties",
            "categories",
            "contents",
            "topics",
            "attachments",
            "likes",
            "userMentions",
            "replies",
            "topicTags",
            "topicCategories",
            "topicRatings",
            "topicTracking",
            "subscriptions",
            "archivedUrls",
            "forumTopics",
            "forumTracking",
            "userBadges",
        ];

        internal static XContainer SerializeEntities<T>(string containerName, IEnumerable<T> items, Func<T, XElement> elementFactory)
        {
            var container = new XElement(containerName);
            foreach (var item in items)
            {
                try
                {
                    container.Add(elementFactory(item));
                }
                catch (Exception ex)
                {
                   Exceptions.LogException(ex);
                }
            }

            return container;
        }

        public static string ExportModule(int moduleId)
        {
            try
            {
                var moduleInfo = ModuleController.Instance.GetModule(moduleId, Null.NullInteger, false);
                var moduleSettings = SettingsBase.GetModuleSettings(moduleId);
                var groups = DotNetNuke.Modules.ActiveForums.Controllers.ForumGroupController.Instance.Get(moduleId)
                    .OrderBy(g => g.SortOrder)
                    .ThenBy(g => g.ForumGroupId)
                    .ToList();

                var tags = DotNetNuke.Modules.ActiveForums.Controllers.TagController.Instance.Get(moduleId)
                    .OrderBy(t => t.TagId)
                    .ToList();

                var categories = DotNetNuke.Modules.ActiveForums.Controllers.CategoryController.Instance.Get(moduleId)
                    .OrderBy(c => c.Priority)
                    .ThenBy(c => c.CategoryId)
                    .ToList();

                var badges = DotNetNuke.Modules.ActiveForums.Controllers.BadgeController.Instance.Get(moduleId)
                    .OrderBy(b => b.SortOrder)
                    .ThenBy(b => b.BadgeId)
                    .ToList();

                var filters = new DotNetNuke.Modules.ActiveForums.Controllers.FilterController().Get()
                    .OrderBy(b => b.FilterId)
                    .ToList();

                var ranks = DotNetNuke.Modules.ActiveForums.Controllers.RankController.Instance.Get(moduleId)
                    .OrderBy(b => b.RankId)
                    .ToList();

                var permissions = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.Instance.Get(moduleId)
                .OrderBy(p => p.PermissionsId)
                .ToList();

                var settings = DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.Instance.Get(moduleId)
                .OrderBy(s => s.SettingsKey)
                .ThenBy(s => s.SettingName)
                .ThenBy(s => s.SettingsId)
                .ToList();

                var forums = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Instance.GetForums(moduleId)
                    .OrderBy(f => f.ForumGroupId)
                    .ThenBy(f => f.SortOrder)
                    .ThenBy(f => f.ForumID)
                    .ToList();

                var forumIds = forums.Select(f => f.ForumID).ToHashSet();

                var properties = new DotNetNuke.Modules.ActiveForums.Controllers.PropertyController().Get()
                    .Where(p => p.PortalId == moduleInfo.PortalID
                        && p.ObjectType == 1
                        && forumIds.Contains(p.ObjectOwnerId))
                    .OrderBy(p => p.ObjectOwnerId)
                    .ThenBy(p => p.SortOrder)
                    .ThenBy(p => p.PropertyId)
                    .ToList();

                var forumTopics = DotNetNuke.Modules.ActiveForums.Controllers.ForumTopicController.Instance.Get()
                    .Where(ft => forumIds.Contains(ft.ForumId))
                    .OrderBy(ft => ft.ForumTopicId)
                    .ToList();

                var topicIds = forumTopics.Select(ft => ft.TopicId).Distinct().ToHashSet();

                var topics = DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Instance.Get()
                    .Where(t => topicIds.Contains(t.TopicId))
                    .OrderBy(t => t.TopicId)
                    .ToList();

                var replies = DotNetNuke.Modules.ActiveForums.Controllers.ReplyController.Instance.Get()
                    .Where(r => topicIds.Contains(r.TopicId))
                    .OrderBy(r => r.ReplyId)
                    .ToList();

                var contentIds = topics.Select(t => t.ContentId).Concat(replies.Select(r => r.ContentId)).Distinct().ToHashSet();

                var contents = DotNetNuke.Modules.ActiveForums.Controllers.ContentController.Instance.Get()
                    .Where(c => contentIds.Contains(c.ContentId))
                    .OrderBy(c => c.ContentId)
                    .ToList();

                var attachments = DotNetNuke.Modules.ActiveForums.Controllers.AttachmentController.Instance.Get()
                    .Where(a => contentIds.Contains(a.ContentId))
                    .OrderBy(a => a.AttachmentId)
                    .ToList();

                var likes = DotNetNuke.Modules.ActiveForums.Controllers.LikeController.Instance.Get()
                    .Where(l => contentIds.Contains(l.PostId))
                    .OrderBy(l => l.Id)
                    .ToList();

                var userMentions = DotNetNuke.Modules.ActiveForums.Controllers.UserMentionController.Instance.Get(moduleId)
                    .Where(um => contentIds.Contains(um.ContentId))
                    .OrderBy(um => um.UserMentionId)
                    .ToList();

                var topicTags = DotNetNuke.Modules.ActiveForums.Controllers.TopicTagController.Instance.Get()
                    .Where(tt => topicIds.Contains(tt.TopicId))
                    .OrderBy(tt => tt.TopicTagId)
                    .ToList();

                var topicCategories = DotNetNuke.Modules.ActiveForums.Controllers.TopicCategoryController.Instance.Get()
                    .Where(tc => topicIds.Contains(tc.TopicId))
                    .OrderBy(tc => tc.TopicCategoryId)
                    .ToList();

                var topicRatings = DotNetNuke.Modules.ActiveForums.Controllers.TopicRatingController.Instance.Get()
                    .Where(tr => topicIds.Contains(tr.TopicId))
                    .OrderBy(tr => tr.RatingId)
                    .ToList();

                var topicTracking = DotNetNuke.Modules.ActiveForums.Controllers.TopicTrackingController.Instance.Get()
                    .Where(tt => topicIds.Contains(tt.TopicId))
                    .OrderBy(tt => tt.TrackingId)
                    .ToList();

                var subscriptions = DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController.Instance.Get()
                    .Where(s => s.ModuleId == moduleId && (topicIds.Contains(s.TopicId) || forumIds.Contains(s.ForumId)))
                    .OrderBy(s => s.Id)
                    .ToList();

                var archivedUrls = DotNetNuke.Modules.ActiveForums.Controllers.ArchivedURLController.Instance.Get()
                    .Where(a => forumIds.Contains(a.ForumId) || topicIds.Contains(a.TopicId))
                    .OrderBy(a => a.Id)
                    .ToList();

                var forumTracking = DotNetNuke.Modules.ActiveForums.Controllers.ForumTrackingController.Instance.Get()
                    .Where(ft => ft.ModuleId == moduleId && forumIds.Contains(ft.ForumId))
                    .OrderBy(ft => ft.TrackingId)
                    .ToList();

                var userBadges = DotNetNuke.Modules.ActiveForums.Controllers.UserBadgeController.Instance.Get(moduleId)
                    .OrderBy(ub => ub.UserBadgeId)
                    .ToList();

                var exportedUserIds = new HashSet<int>(
                    contents.Select(c => c.AuthorId)
                        .Concat(attachments.Select(a => a.UserId))
                        .Concat(likes.Select(l => l.UserId))
                        .Concat(userMentions.Select(um => um.UserId))
                        .Concat(topicRatings.Select(tr => tr.UserId))
                        .Concat(topicTracking.Select(tt => tt.UserId))
                        .Concat(subscriptions.Select(s => s.UserId))
                        .Concat(forumTracking.Select(ft => ft.UserId))
                        .Concat(userBadges.Select(ub => ub.UserId))
                        .Where(id => id > 0));

                var userProfiles = DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.Instance.Get()
                    .Where(up => up.PortalId == moduleInfo.PortalID && exportedUserIds.Contains(up.UserId))
                    .OrderBy(up => up.ProfileId)
                    .ToList();

                var document = new XDocument(
                    new XElement(
                        "forumsExport",
                        new XAttribute("schemaVersion", "1.0"),
                        new XAttribute("defaultPermissionId", moduleSettings?.DefaultPermissionId ?? -1),
                        new XAttribute("defaultSettingsKey", moduleSettings?.DefaultSettingsKey ?? string.Empty),
                        SerializeEntities("groups", groups, g =>
                            new XElement(
                                "group",
                                new XAttribute("forumGroupId", g.ForumGroupId),
                                new XAttribute("groupName", g.GroupName ?? string.Empty),
                                new XAttribute("sortOrder", g.SortOrder),
                                new XAttribute("active", g.Active),
                                new XAttribute("hidden", g.Hidden),
                                new XAttribute("settingsKey", g.GroupSettingsKey ?? string.Empty),
                                new XAttribute("permissionsId", g.PermissionsId),
                                new XAttribute("prefixUrl", g.PrefixURL ?? string.Empty))),
                        SerializeEntities("tags", tags, t =>
                            new XElement(
                                "tag",
                                new XAttribute("tagId", t.TagId),
                                new XAttribute("tagName", t.TagName.EncodeInvalidXmlChars() ?? string.Empty),
                                new XAttribute("items", t.Items))),
                        SerializeEntities("badges", badges, b =>
                            new XElement(
                                "badge",
                                new XAttribute("badgeId", b.BadgeId),
                                new XAttribute("name", b.Name ?? string.Empty),
                                new XAttribute("description", b.Description.EncodeInvalidXmlChars() ?? string.Empty),
                                new XAttribute("imageMarkup", b.ImageMarkup ?? string.Empty),
                                new XAttribute("fileId", b.FileId),
                                new XAttribute("sortOrder", b.SortOrder),
                                new XAttribute("oneTimeAward", b.OneTimeAward),
                                new XAttribute("badgeMetric", (int)b.BadgeMetric),
                                new XAttribute("threshold", b.Threshold),
                                new XAttribute("intervalDays", b.IntervalDays),
                                new XAttribute("sendAwardNotification", b.SendAwardNotification),
                                new XAttribute("initialBackfillCompletedDate", ToPortableDate(b.InitialBackfillCompletedDate)),
                                new XAttribute("suppressAwardNotificationOnBackfill", b.SuppresssAwardNotificationOnBackfill))),
                        SerializeEntities("ranks", ranks, r =>
                            new XElement(
                                "rank",
                                new XAttribute("rankId", r.RankId),
                                new XAttribute("rankName", r.RankName ?? string.Empty),
                                new XAttribute("display", r.Display ?? string.Empty),
                                new XAttribute("minPosts", (int)r.MinPosts),
                                new XAttribute("maxPosts", (int)r.MaxPosts))),
                        SerializeEntities("filters", filters, f =>
                            new XElement(
                                "filter",
                                new XAttribute("filterId", f.FilterId),
                                new XAttribute("filterType", f.FilterType),
                                new XAttribute("find", f.Find ?? string.Empty),
                                new XAttribute("replace", f.Replace ?? string.Empty))),
                        SerializeEntities("permissions", permissions, p =>
                            new XElement(
                                "permission",
                                new XAttribute("permissionsId", p.PermissionsId),
                                new XAttribute("view", p.View ?? string.Empty),
                                new XAttribute("read", p.Read ?? string.Empty),
                                new XAttribute("create", p.Create ?? string.Empty),
                                new XAttribute("reply", p.Reply ?? string.Empty),
                                new XAttribute("edit", p.Edit ?? string.Empty),
                                new XAttribute("delete", p.Delete ?? string.Empty),
                                new XAttribute("lock", p.Lock ?? string.Empty),
                                new XAttribute("pin", p.Pin ?? string.Empty),
                                new XAttribute("attach", p.Attach ?? string.Empty),
                                new XAttribute("poll", p.Poll ?? string.Empty),
                                new XAttribute("trust", p.Trust ?? string.Empty),
                                new XAttribute("subscribe", p.Subscribe ?? string.Empty),
                                new XAttribute("announce", p.Announce ?? string.Empty),
                                new XAttribute("tag", p.Tag ?? string.Empty),
                                new XAttribute("categorize", p.Categorize ?? string.Empty),
                                new XAttribute("prioritize", p.Prioritize ?? string.Empty),
                                new XAttribute("moderate", p.Moderate ?? string.Empty),
                                new XAttribute("move", p.Move ?? string.Empty),
                                new XAttribute("split", p.Split ?? string.Empty),
                                new XAttribute("manageUsers", p.ManageUsers ?? string.Empty),
                                new XAttribute("mention", p.Mention ?? string.Empty))),
                        SerializeEntities("settings", settings, s =>
                            new XElement(
                                "setting",
                                new XAttribute("settingsId", s.SettingsId),
                                new XAttribute("settingsKey", s.SettingsKey ?? string.Empty),
                                new XAttribute("settingName", s.SettingName ?? string.Empty),
                                new XAttribute("settingValue", s.SettingValue ?? string.Empty))),
                        SerializeEntities("forums", forums, f =>
                            new XElement(
                                "forum",
                                new XAttribute("forumId", f.ForumID),
                                new XAttribute("forumGroupId", f.ForumGroupId),
                                new XAttribute("parentForumId", f.ParentForumId),
                                new XAttribute("forumName", f.ForumName.EncodeInvalidXmlChars() ?? string.Empty),
                                new XAttribute("forumDesc", f.ForumDesc.EncodeInvalidXmlChars() ?? string.Empty),
                                new XAttribute("sortOrder", f.SortOrder),
                                new XAttribute("active", f.Active),
                                new XAttribute("hidden", f.Hidden),
                                new XAttribute("totalTopics", f.TotalTopics),
                                new XAttribute("totalReplies", f.TotalReplies),
                                new XAttribute("forumSettingsKey", f.ForumSettingsKey ?? string.Empty),
                                new XAttribute("dateCreated", f.DateCreated.ToString("o", CultureInfo.InvariantCulture)),
                                new XAttribute("dateUpdated", f.DateUpdated.ToString("o", CultureInfo.InvariantCulture)),
                                new XAttribute("lastTopicId", f.LastTopicId),
                                new XAttribute("lastReplyId", f.LastReplyId),
                                new XAttribute("permissionsId", f.PermissionsId),
                                new XAttribute("prefixUrl", f.PrefixURL ?? string.Empty),
                                new XAttribute("socialGroupId", f.SocialGroupId),
                                new XAttribute("hasProperties", f.HasProperties))),
                        SerializeEntities("properties", properties, p =>
                            new XElement(
                                "property",
                                new XAttribute("propertyId", p.PropertyId),
                                new XAttribute("portalId", p.PortalId),
                                new XAttribute("objectType", p.ObjectType),
                                new XAttribute("objectOwnerId", p.ObjectOwnerId),
                                new XAttribute("name", p.Name.EncodeInvalidXmlChars() ?? string.Empty),
                                new XAttribute("dataType", p.DataType ?? string.Empty),
                                new XAttribute("defaultAccessControl", p.DefaultAccessControl),
                                new XAttribute("isHidden", p.IsHidden),
                                new XAttribute("isRequired", p.IsRequired),
                                new XAttribute("isReadOnly", p.IsReadOnly),
                                new XAttribute("validationExpression", p.ValidationExpression.EncodeInvalidXmlChars() ?? string.Empty),
                                new XAttribute("editTemplate", p.EditTemplate.EncodeInvalidXmlChars() ?? string.Empty),
                                new XAttribute("viewTemplate", p.ViewTemplate.EncodeInvalidXmlChars() ?? string.Empty),
                                new XAttribute("sortOrder", p.SortOrder),
                                new XAttribute("defaultValue", p.DefaultValue.EncodeInvalidXmlChars() ?? string.Empty))),
                        SerializeEntities("categories", categories, c =>
                            new XElement(
                                "category",
                                new XAttribute("categoryId", c.CategoryId),
                                new XAttribute("categoryName", c.CategoryName ?? string.Empty),
                                new XAttribute("clicks", c.Clicks),
                                new XAttribute("items", c.Items),
                                new XAttribute("priority", c.Priority),
                                new XAttribute("forumId", c.ForumId),
                                new XAttribute("forumGroupId", c.ForumGroupId))),
                        SerializeEntities("contents", contents, c =>
                            new XElement(
                                "content",
                                new XAttribute("contentId", c.ContentId),
                                new XAttribute("subject", c.Subject.EncodeInvalidXmlChars() ?? string.Empty),
                                new XAttribute("summary", c.Summary.EncodeInvalidXmlChars() ?? string.Empty),
                                new XAttribute("body", c.Body.EncodeInvalidXmlChars() ?? string.Empty),
                                new XAttribute("dateCreated", c.DateCreated.ToString("o", CultureInfo.InvariantCulture)),
                                new XAttribute("dateUpdated", c.DateUpdated.ToString("o", CultureInfo.InvariantCulture)),
                                new XAttribute("authorId", c.AuthorId),
                                new XAttribute("authorName", c.AuthorName ?? string.Empty),
                                new XAttribute("isDeleted", c.IsDeleted),
                                new XAttribute("ipAddress", c.IPAddress ?? string.Empty),
                                new XAttribute("contentItemId", c.ContentItemId))),
                        SerializeEntities("topics", topics, t =>
                            new XElement(
                                "topic",
                                new XAttribute("topicId", t.TopicId),
                                new XAttribute("contentId", t.ContentId),
                                new XAttribute("viewCount", t.ViewCount),
                                new XAttribute("replyCount", t.ReplyCount),
                                new XAttribute("isLocked", t.IsLocked),
                                new XAttribute("isPinned", t.IsPinned),
                                new XAttribute("topicIcon", t.TopicIcon ?? string.Empty),
                                new XAttribute("statusId", t.StatusId),
                                new XAttribute("isApproved", t.IsApproved),
                                new XAttribute("isRejected", t.IsRejected),
                                new XAttribute("isDeleted", t.IsDeleted),
                                new XAttribute("isAnnounce", t.IsAnnounce),
                                new XAttribute("isArchived", t.IsArchived),
                                new XAttribute("announceStart", ToPortableDate(t.AnnounceStart)),
                                new XAttribute("announceEnd", ToPortableDate(t.AnnounceEnd)),
                                new XAttribute("topicType", t.TopicType),
                                new XAttribute("priority", t.Priority),
                                new XAttribute("topicUrl", t.TopicUrl ?? string.Empty),
                                new XAttribute("prevTopic", t.PrevTopic),
                                new XAttribute("nextTopic", t.NextTopic),
                                new XAttribute("topicData", t.TopicData ?? string.Empty))),
                        SerializeEntities("attachments", attachments, a =>
                            new XElement(
                                "attachment",
                                new XAttribute("attachmentId", a.AttachmentId),
                                new XAttribute("contentId", a.ContentId),
                                new XAttribute("userId", a.UserId),
                                new XAttribute("fileName", a.FileName ?? string.Empty),
                                new XAttribute("fileId", a.FileId.HasValue ? a.FileId.Value.ToString(CultureInfo.InvariantCulture) : string.Empty),
                                new XAttribute("contentType", a.ContentType ?? string.Empty),
                                new XAttribute("dateAdded", ToPortableDate(a.DateAdded)),
                                new XAttribute("dateUpdated", ToPortableDate(a.DateUpdated)),
                                new XAttribute("fileSize", a.FileSize),
                                new XAttribute("displayInline", a.DisplayInline))),
                        SerializeEntities("likes", likes, l =>
                            new XElement(
                                "like",
                                new XAttribute("id", l.Id),
                                new XAttribute("postId", l.PostId),
                                new XAttribute("userId", l.UserId),
                                new XAttribute("checked", l.Checked),
                                new XAttribute("dateCreated", l.DateCreated.ToString("o", CultureInfo.InvariantCulture)))),
                        SerializeEntities("userMentions", userMentions, um =>
                            new XElement(
                                "userMention",
                                new XAttribute("userMentionId", um.UserMentionId),
                                new XAttribute("contentId", um.ContentId),
                                new XAttribute("userId", um.UserId),
                                new XAttribute("dateMentioned", um.DateMentioned.ToString("o", CultureInfo.InvariantCulture)))),
                        SerializeEntities("replies", replies, r =>
                            new XElement(
                                "reply",
                                new XAttribute("replyId", r.ReplyId),
                                new XAttribute("topicId", r.TopicId),
                                new XAttribute("replyToId", r.ReplyToId),
                                new XAttribute("contentId", r.ContentId),
                                new XAttribute("isApproved", r.IsApproved),
                                new XAttribute("isRejected", r.IsRejected),
                                new XAttribute("statusId", r.StatusId),
                                new XAttribute("isDeleted", r.IsDeleted))),
                        SerializeEntities("forumTopics", forumTopics, ft =>
                            new XElement(
                                "forumTopic",
                                new XAttribute("forumTopicId", ft.ForumTopicId),
                                new XAttribute("forumId", ft.ForumId),
                                new XAttribute("topicId", ft.TopicId),
                                new XAttribute("lastReplyId", ft.LastReplyId.HasValue ? ft.LastReplyId.Value.ToString(CultureInfo.InvariantCulture) : string.Empty))),
                        SerializeEntities("topicTags", topicTags, tt =>
                            new XElement(
                                "topicTag",
                                new XAttribute("topicTagId", tt.TopicTagId),
                                new XAttribute("topicId", tt.TopicId),
                                new XAttribute("tagId", tt.TagId))),
                        SerializeEntities("topicCategories", topicCategories, tc =>
                            new XElement(
                                "topicCategory",
                                new XAttribute("topicCategoryId", tc.TopicCategoryId),
                                new XAttribute("topicId", tc.TopicId),
                                new XAttribute("categoryId", tc.CategoryId))),
                        SerializeEntities("topicRatings", topicRatings, tr =>
                            new XElement(
                                "topicRating",
                                new XAttribute("ratingId", tr.RatingId),
                                new XAttribute("topicId", tr.TopicId),
                                new XAttribute("userId", tr.UserId),
                                new XAttribute("rating", tr.Rating),
                                new XAttribute("helpful", tr.Helpful),
                                new XAttribute("comments", tr.Comments.EncodeInvalidXmlChars() ?? string.Empty),
                                new XAttribute("ipAddress", tr.IPAddress ?? string.Empty),
                                new XAttribute("dateAdded", tr.DateAdded.ToString("o", CultureInfo.InvariantCulture)),
                                new XAttribute("dateUpdated", tr.DateUpdated.ToString("o", CultureInfo.InvariantCulture)))),
                        SerializeEntities("topicTracking", topicTracking, tt =>
                            new XElement(
                                "topicTracking",
                                new XAttribute("trackingId", tt.TrackingId),
                                new XAttribute("forumId", tt.ForumId),
                                new XAttribute("topicId", tt.TopicId),
                                new XAttribute("lastReplyId", tt.LastReplyId),
                                new XAttribute("userId", tt.UserId),
                                new XAttribute("dateAdded", tt.DateAdded.ToString("o", CultureInfo.InvariantCulture)))),
                        SerializeEntities("subscriptions", subscriptions, s =>
                            new XElement(
                                "subscription",
                                new XAttribute("id", s.Id),
                                new XAttribute("forumId", s.ForumId),
                                new XAttribute("topicId", s.TopicId),
                                new XAttribute("mode", s.Mode),
                                new XAttribute("userId", s.UserId))),
                        SerializeEntities("archivedUrls", archivedUrls, a =>
                            new XElement(
                                "archivedUrl",
                                new XAttribute("id", a.Id),
                                new XAttribute("url", a.Url ?? string.Empty),
                                new XAttribute("portalId", a.PortalId),
                                new XAttribute("forumId", a.ForumId),
                                new XAttribute("topicId", a.TopicId),
                                new XAttribute("forumGroupId", a.ForumGroupId),
                                new XAttribute("urlHash", ToPortableBytes(a.UrlHash)))),
                        SerializeEntities("userProfiles", userProfiles, up =>
                            new XElement(
                                "userProfile",
                                new XAttribute("profileId", up.ProfileId),
                                new XAttribute("userId", up.UserId),
                                new XAttribute("portalId", up.PortalId),
                                new XAttribute("topicCount", up.TopicCount),
                                new XAttribute("replyCount", up.ReplyCount),
                                new XAttribute("viewCount", up.ViewCount),
                                new XAttribute("answerCount", up.AnswerCount),
                                new XAttribute("rewardPoints", up.RewardPoints),
                                new XAttribute("userCaption", up.UserCaption.EncodeInvalidXmlChars() ?? string.Empty),
                                new XAttribute("avatarLastRefresh", ToPortableDate(up.AvatarLastRefresh)),
                                new XAttribute("avatarSourceLastModified", ToPortableDate(up.AvatarSourceLastModified)),
                                new XAttribute("avatarFileId", up.AvatarFileId.HasValue ? up.AvatarFileId.Value.ToString(CultureInfo.InvariantCulture) : string.Empty),
                                new XAttribute("dateCreated", up.DateCreated.ToString("o", CultureInfo.InvariantCulture)),
                                new XAttribute("dateUpdated", ToPortableDate(up.DateUpdated)),
                                new XAttribute("dateLastActivity", ToPortableDate(up.DateLastActivity)),
                                new XAttribute("dateLastPost", ToPortableDate(up.DateLastPost)),
                                new XAttribute("dateLastReply", ToPortableDate(up.DateLastReply)),
                                new XAttribute("signature", up.Signature.EncodeInvalidXmlChars() ?? string.Empty),
                                new XAttribute("signatureDisabled", up.SignatureDisabled),
                                new XAttribute("trustLevel", up.TrustLevel),
                                new XAttribute("adminWatch", up.AdminWatch),
                                new XAttribute("attachDisabled", up.AttachDisabled),
                                new XAttribute("avatarDisabled", up.AvatarDisabled),
                                new XAttribute("prefDefaultSort", up.PrefDefaultSort ?? string.Empty),
                                new XAttribute("prefDefaultShowReplies", up.PrefDefaultShowReplies),
                                new XAttribute("prefJumpLastPost", up.PrefJumpLastPost),
                                new XAttribute("prefTopicSubscribe", up.PrefTopicSubscribe),
                                new XAttribute("prefSubscriptionType", (int)up.PrefSubscriptionType),
                                new XAttribute("prefBlockAvatars", up.PrefBlockAvatars),
                                new XAttribute("prefBlockSignatures", up.PrefBlockSignatures),
                                new XAttribute("prefPageSize", up.PrefPageSize),
                                new XAttribute("likeNotificationsEnabled", up.LikeNotificationsEnabled),
                                new XAttribute("pinNotificationsEnabled", up.PinNotificationsEnabled),
                                new XAttribute("enableNotificationsForOwnContent", up.EnableNotificationsForOwnContent),
                                new XAttribute("badgeNotificationsEnabled", up.BadgeNotificationsEnabled),
                                new XAttribute("userMentionNotificationsEnabled", up.UserMentionNotificationsEnabled))),
                        SerializeEntities("forumTracking", forumTracking, ft =>
                            new XElement(
                                "forumTracking",
                                new XAttribute("trackingId", ft.TrackingId),
                                new XAttribute("userId", ft.UserId),
                                new XAttribute("forumId", ft.ForumId),
                                new XAttribute("lastAccessDateTime", ft.LastAccessDateTime.ToString("o", CultureInfo.InvariantCulture)),
                                new XAttribute("maxTopicRead", ft.MaxTopicRead),
                                new XAttribute("maxReplyRead", ft.MaxReplyRead))),
                        SerializeEntities("userBadges", userBadges, ub =>
                            new XElement(
                                "userBadge",
                                new XAttribute("userBadgeId", ub.UserBadgeId),
                                new XAttribute("badgeId", ub.BadgeId),
                                new XAttribute("userId", ub.UserId),
                                new XAttribute("dateAssigned", ub.DateAssigned.ToString("o", CultureInfo.InvariantCulture))))
                    ));

                return document.ToString(SaveOptions.DisableFormatting);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return string.Empty;
            }
        }

        public static void ImportModule(int moduleId, string content, string version, int userId)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return;
            }

            try
            {
                var document = XDocument.Parse(content);
                var root = document.Element("forumsExport");
                if (root == null)
                {
                    return;
                }

                var moduleInfo = ModuleController.Instance.GetModule(moduleId, Null.NullInteger, false);
                var portalId = moduleInfo?.PortalID ?? Null.NullInteger;

                var moduleSettings = SettingsBase.GetModuleSettings(moduleId);
                if (!moduleSettings.IsInstalled)
                {
                    // load initial Settings
                    new ForumsConfig().ForumsInit(portalId, moduleId, skipContent: true);
                }

                var groupMap = new Dictionary<int, int>();
                var forumMap = new Dictionary<int, int>();
                var contentMap = new Dictionary<int, int>();
                var topicMap = new Dictionary<int, int>();
                var replyMap = new Dictionary<int, int>();
                var tagMap = new Dictionary<int, int>();
                var categoryMap = new Dictionary<int, int>();
                var badgeMap = new Dictionary<int, int>();
                var permissionMap = new Dictionary<int, int>();
                var settingsKeyMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                var importedForums = new Dictionary<int, ForumInfo>();
                var pendingForumParents = new Dictionary<int, int>();
                var pendingReplyParents = new Dictionary<int, int>();

                var defaultSettingsKey = moduleSettings?.DefaultSettingsKey ?? string.Empty;
                var sourceDefaultSettingsKey = GetString(root, "defaultSettingsKey");
                if (sourceDefaultSettingsKey.StartsWith("M"))
                {
                    sourceDefaultSettingsKey = $"M{moduleId}";
                }

                if (!string.IsNullOrWhiteSpace(sourceDefaultSettingsKey) && !string.IsNullOrWhiteSpace(defaultSettingsKey))
                {
                    settingsKeyMap[sourceDefaultSettingsKey] = defaultSettingsKey;
                }

                foreach (var sourceSetting in GetElements(root, "settings", "setting"))
                {
                    var sourceSettingsKey = GetString(sourceSetting, "settingsKey");
                    var mappedSettingsKey = GetMappedSettingsKey(settingsKeyMap, sourceSettingsKey, sourceDefaultSettingsKey, defaultSettingsKey);
                    if (string.IsNullOrWhiteSpace(mappedSettingsKey))
                    {
                        continue;
                    }

                    if (mappedSettingsKey.StartsWith("M"))
                    {
                        mappedSettingsKey = $"M{moduleId}";
                    }

                    SettingsController.Instance.SaveSetting(
                        moduleId,
                        mappedSettingsKey,
                        GetString(sourceSetting, "settingName"),
                        GetString(sourceSetting, "settingValue"));
                }

                var sourcePermissions = GetElements(root, "permissions", "permission");
                foreach (var sourcePermission in sourcePermissions)
                {
                    var oldPermissionId = GetInt(sourcePermission, "permissionsId");
                    if (oldPermissionId <= 0)
                    {
                        continue;
                    }

                    var permission = CreatePermission(moduleId, sourcePermission);
                    permission = PermissionController.Instance.Insert(permission);
                    permissionMap[oldPermissionId] = permission.PermissionsId;
                }

                var sourceDefaultPermissionId = GetInt(root, "defaultPermissionId");
                var defaultPermissionId = -1;

                if (permissionMap.ContainsKey(sourceDefaultPermissionId))
                {
                    defaultPermissionId = permissionMap[sourceDefaultPermissionId];
                }
                else
                {
                    var defaultPermission = new DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo()
                    {
                        ModuleId = moduleId,
                    };
                    defaultPermission = PermissionController.Instance.Insert(defaultPermission);
                    permissionMap[sourceDefaultPermissionId] = defaultPermission.PermissionsId;
                    defaultPermissionId = defaultPermission.PermissionsId;
                }

                if (sourceDefaultPermissionId > 0 && permissionMap.TryGetValue(sourceDefaultPermissionId, out var mappedDefaultPermissionId))
                {
                    ModuleController.Instance.UpdateModuleSetting(moduleId, SettingKeys.DefaultPermissionId, mappedDefaultPermissionId.ToString(CultureInfo.InvariantCulture));
                    defaultPermissionId = mappedDefaultPermissionId;
                }

                foreach (var sourceUserProfile in GetElements(root, "userProfiles", "userProfile"))
                {
                    var resolvedUserId = ResolveUserIdOrDefault(portalId, GetInt(sourceUserProfile, "userId"));
                    if (!resolvedUserId.HasValue || resolvedUserId.Value <= 0)
                    {
                        continue;
                    }

                    var forumUser = ForumUserController.Instance.GetByUserId(portalId, moduleId, resolvedUserId.Value);
                    if (forumUser == null)
                    {
                        continue;
                    }

                    forumUser.TopicCount = GetInt(sourceUserProfile, "topicCount");
                    forumUser.ReplyCount = GetInt(sourceUserProfile, "replyCount");
                    forumUser.ViewCount = GetInt(sourceUserProfile, "viewCount");
                    forumUser.AnswerCount = GetInt(sourceUserProfile, "answerCount");
                    forumUser.RewardPoints = GetInt(sourceUserProfile, "rewardPoints");
                    forumUser.UserCaption = GetString(sourceUserProfile, "userCaption");
                    forumUser.AvatarLastRefresh = GetNullableDateTime(sourceUserProfile, "avatarLastRefresh");
                    forumUser.AvatarSourceLastModified = GetNullableDateTime(sourceUserProfile, "avatarSourceLastModified");
                    forumUser.AvatarFileId = GetNullableInt(sourceUserProfile, "avatarFileId");
                    forumUser.DateCreated = GetDateTime(sourceUserProfile, "dateCreated", forumUser.DateCreated);
                    forumUser.DateUpdated = GetNullableDateTime(sourceUserProfile, "dateUpdated");
                    forumUser.DateLastActivity = GetNullableDateTime(sourceUserProfile, "dateLastActivity");
                    forumUser.DateLastPost = GetNullableDateTime(sourceUserProfile, "dateLastPost");
                    forumUser.DateLastReply = GetNullableDateTime(sourceUserProfile, "dateLastReply");
                    forumUser.Signature = GetString(sourceUserProfile, "signature");
                    forumUser.SignatureDisabled = GetBool(sourceUserProfile, "signatureDisabled");
                    forumUser.TrustLevel = GetInt(sourceUserProfile, "trustLevel");
                    forumUser.AdminWatch = GetBool(sourceUserProfile, "adminWatch");
                    forumUser.AttachDisabled = GetBool(sourceUserProfile, "attachDisabled");
                    forumUser.AvatarDisabled = GetBool(sourceUserProfile, "avatarDisabled");
                    forumUser.PrefDefaultSort = GetString(sourceUserProfile, "prefDefaultSort");
                    forumUser.PrefDefaultShowReplies = GetBool(sourceUserProfile, "prefDefaultShowReplies");
                    forumUser.PrefJumpLastPost = GetBool(sourceUserProfile, "prefJumpLastPost");
                    forumUser.PrefTopicSubscribe = GetBool(sourceUserProfile, "prefTopicSubscribe");
                    forumUser.PrefSubscriptionType = (SubscriptionTypes)GetInt(sourceUserProfile, "prefSubscriptionType");
                    forumUser.PrefBlockAvatars = GetBool(sourceUserProfile, "prefBlockAvatars");
                    forumUser.PrefBlockSignatures = GetBool(sourceUserProfile, "prefBlockSignatures");
                    forumUser.PrefPageSize = GetInt(sourceUserProfile, "prefPageSize");
                    forumUser.LikeNotificationsEnabled = GetBool(sourceUserProfile, "likeNotificationsEnabled");
                    forumUser.PinNotificationsEnabled = GetBool(sourceUserProfile, "pinNotificationsEnabled");
                    forumUser.EnableNotificationsForOwnContent = GetBool(sourceUserProfile, "enableNotificationsForOwnContent");
                    forumUser.BadgeNotificationsEnabled = GetBool(sourceUserProfile, "badgeNotificationsEnabled");
                    forumUser.UserMentionNotificationsEnabled = GetBool(sourceUserProfile, "userMentionNotificationsEnabled");
                    ((IRepository<ForumUserInfo>)ForumUserController.Instance).Update(forumUser);
                }

                foreach (var sourceGroup in GetElements(root, "groups", "group"))
                {
                    var forumGroup = new ForumGroupInfo
                    {
                        ModuleId = moduleId,
                        GroupName = GetString(sourceGroup, "groupName"),
                        SortOrder = GetInt(sourceGroup, "sortOrder"),
                        Active = GetBool(sourceGroup, "active"),
                        Hidden = GetBool(sourceGroup, "hidden"),
                        GroupSettingsKey = GetMappedSettingsKey(settingsKeyMap, GetString(sourceGroup, "settingsKey"), sourceDefaultSettingsKey, defaultSettingsKey),
                        PermissionsId = GetMappedPermissionId(permissionMap, GetInt(sourceGroup, "permissionsId"), defaultPermissionId),
                        PrefixURL = GetString(sourceGroup, "prefixUrl"),
                    };

                    if (forumGroup.GroupSettingsKey.StartsWith("M"))
                    {
                        forumGroup.GroupSettingsKey = $"M{moduleId}";
                    }

                    ((IRepository<ForumGroupInfo>)ForumGroupController.Instance).Insert(forumGroup);
                    groupMap[GetInt(sourceGroup, "forumGroupId")] = forumGroup.ForumGroupId;
                }

                foreach (var sourceTag in GetElements(root, "tags", "tag"))
                {
                    var tag = new TagInfo
                    {
                        ModuleId = moduleId,
                        PortalId = portalId,
                        TagName = GetString(sourceTag, "tagName"),
                        Items = GetInt(sourceTag, "items"),
                    };

                    ((IRepository<TagInfo>)TagController.Instance).Insert(tag);
                    tagMap[GetInt(sourceTag, "tagId")] = tag.TagId;
                }

                foreach (var sourceBadge in GetElements(root, "badges", "badge"))
                {
                    var badge = new BadgeInfo
                    {
                        ModuleId = moduleId,
                        Name = GetString(sourceBadge, "name"),
                        Description = GetString(sourceBadge, "description"),
                        ImageMarkup = GetString(sourceBadge, "imageMarkup"),
                        FileId = GetInt(sourceBadge, "fileId"),
                        SortOrder = GetInt(sourceBadge, "sortOrder"),
                        OneTimeAward = GetBool(sourceBadge, "oneTimeAward"),
                        BadgeMetric = (Enums.BadgeMetric)GetInt(sourceBadge, "badgeMetric"),
                        Threshold = GetInt(sourceBadge, "threshold"),
                        IntervalDays = GetInt(sourceBadge, "intervalDays"),
                        SendAwardNotification = GetBool(sourceBadge, "sendAwardNotification"),
                        InitialBackfillCompletedDate = GetNullableDateTime(sourceBadge, "initialBackfillCompletedDate"),
                        SuppresssAwardNotificationOnBackfill = GetBool(sourceBadge, "suppressAwardNotificationOnBackfill"),
                    };

                    ((IRepository<BadgeInfo>)BadgeController.Instance).Insert(badge);
                    badgeMap[GetInt(sourceBadge, "badgeId")] = badge.BadgeId;
                }

                foreach (var sourceRank in GetElements(root, "ranks", "rank"))
                {
                    var rank = new RankInfo
                    {
                        ModuleId = moduleId,
                        RankName = GetString(sourceRank, "rankName"),
                        Display = GetString(sourceRank, "display"),
                        MinPosts = GetInt(sourceRank, "minPosts"),
                        MaxPosts = GetInt(sourceRank, "maxPosts"),
                    };

                    ((IRepository<RankInfo>)RankController.Instance).Insert(rank);
                }

                foreach (var sourceFilter in GetElements(root, "filters", "filter"))
                {
                    var filter = new FilterInfo
                    {
                        ModuleId = moduleId,
                        PortalId = portalId,
                        FilterType = GetString(sourceFilter, "filterType"),
                        Find = GetString(sourceFilter, "find"),
                        Replace = GetString(sourceFilter, "replace"),
                    };

                    new FilterController().Insert(filter);
                }

                foreach (var sourceForum in GetElements(root, "forums", "forum"))
                {
                    var oldForumId = GetInt(sourceForum, "forumId");
                    var oldParentForumId = GetInt(sourceForum, "parentForumId");
                    var forum = new ForumInfo
                    {
                        PortalId = portalId,
                        ModuleId = moduleId,
                        ForumGroupId = groupMap.TryGetValue(GetInt(sourceForum, "forumGroupId"), out var newForumGroupId) ? newForumGroupId : 0,
                        ParentForumId = 0,
                        ForumName = GetString(sourceForum, "forumName"),
                        ForumDesc = GetString(sourceForum, "forumDesc"),
                        SortOrder = GetInt(sourceForum, "sortOrder"),
                        Active = GetBool(sourceForum, "active"),
                        Hidden = GetBool(sourceForum, "hidden"),
                        TotalTopics = GetInt(sourceForum, "totalTopics"),
                        TotalReplies = GetInt(sourceForum, "totalReplies"),
                        ForumSettingsKey = GetMappedSettingsKey(settingsKeyMap, GetString(sourceForum, "forumSettingsKey"), sourceDefaultSettingsKey, defaultSettingsKey),
                        DateCreated = GetDateTime(sourceForum, "dateCreated", DateTime.UtcNow),
                        DateUpdated = GetDateTime(sourceForum, "dateUpdated", DateTime.UtcNow),
                        LastTopicId = 0,
                        LastReplyId = 0,
                        PermissionsId = GetMappedPermissionId(permissionMap, GetInt(sourceForum, "permissionsId"), defaultPermissionId),
                        PrefixURL = GetString(sourceForum, "prefixUrl"),
                        SocialGroupId = GetInt(sourceForum, "socialGroupId"),
                        HasProperties = GetBool(sourceForum, "hasProperties"),
                    };

                    if (forum.ForumSettingsKey.StartsWith("M"))
                    {
                        forum.ForumSettingsKey = $"M{moduleId}";
                    }
                    else if (forum.ForumSettingsKey.StartsWith("G"))
                    {
                        forum.ForumSettingsKey = $"G{forum.ForumGroupId}";
                    }

                    ((IRepository<ForumInfo>)ForumController.Instance).Insert(forum);
                    forumMap[oldForumId] = forum.ForumID;
                    importedForums[oldForumId] = forum;
                    pendingForumParents[oldForumId] = oldParentForumId;
                }

                var propertyController = new DotNetNuke.Modules.ActiveForums.Controllers.PropertyController();

                foreach (var sourceProperty in GetElements(root, "properties", "property"))
                {
                    var sourceObjectOwnerId = GetInt(sourceProperty, "objectOwnerId");
                    if (!forumMap.TryGetValue(sourceObjectOwnerId, out var newForumId))
                    {
                        continue;
                    }

                    var objectType = GetInt(sourceProperty, "objectType");
                    if (objectType <= 0)
                    {
                        objectType = 1;
                    }

                    var property = new PropertyInfo
                    {
                        PortalId = portalId,
                        ObjectType = objectType,
                        ObjectOwnerId = newForumId,
                        Name = GetString(sourceProperty, "name"),
                        DataType = GetString(sourceProperty, "dataType"),
                        DefaultAccessControl = GetInt(sourceProperty, "defaultAccessControl"),
                        IsHidden = GetBool(sourceProperty, "isHidden"),
                        IsRequired = GetBool(sourceProperty, "isRequired"),
                        IsReadOnly = GetBool(sourceProperty, "isReadOnly"),
                        ValidationExpression = GetString(sourceProperty, "validationExpression"),
                        EditTemplate = GetString(sourceProperty, "editTemplate"),
                        ViewTemplate = GetString(sourceProperty, "viewTemplate"),
                        SortOrder = GetInt(sourceProperty, "sortOrder"),
                        DefaultValue = GetString(sourceProperty, "defaultValue"),
                    };

                    propertyController.Insert(property);
                }

                foreach (var sourceCategory in GetElements(root, "categories", "category"))
                {
                    var oldCategoryId = GetInt(sourceCategory, "categoryId");
                    var category = new CategoryInfo
                    {
                        PortalId = portalId,
                        ModuleId = moduleId,
                        CategoryName = GetString(sourceCategory, "categoryName"),
                        Clicks = GetInt(sourceCategory, "clicks"),
                        Items = GetInt(sourceCategory, "items"),
                        Priority = GetInt(sourceCategory, "priority"),
                        ForumId = forumMap.TryGetValue(GetInt(sourceCategory, "forumId"), out var newForumId) ? newForumId : 0,
                        ForumGroupId = groupMap.TryGetValue(GetInt(sourceCategory, "forumGroupId"), out var newForumGroupId) ? newForumGroupId : 0,
                    };

                    ((IRepository<CategoryInfo>)CategoryController.Instance).Insert(category);
                    categoryMap[oldCategoryId] = category.CategoryId;
                }

                foreach (var pendingForum in pendingForumParents.Where(p => p.Value > 0))
                {
                    if (importedForums.TryGetValue(pendingForum.Key, out var forum) && forumMap.TryGetValue(pendingForum.Value, out var newParentForumId))
                    {
                        forum.ParentForumId = newParentForumId;
                        ((IRepository<ForumInfo>)ForumController.Instance).Update(forum);
                    }
                }

                foreach (var sourceContent in GetElements(root, "contents", "content"))
                {
                    var authorId = GetInt(sourceContent, "authorId");
                    var importedContent = new ContentInfo
                    {
                        Subject = GetString(sourceContent, "subject"),
                        Summary = GetString(sourceContent, "summary"),
                        Body = GetString(sourceContent, "body"),
                        DateCreated = GetDateTime(sourceContent, "dateCreated", DateTime.UtcNow),
                        DateUpdated = GetDateTime(sourceContent, "dateUpdated", DateTime.UtcNow),
                        AuthorId = ResolveAuthorId(portalId, authorId, userId),
                        AuthorName = GetString(sourceContent, "authorName"),
                        IsDeleted = GetBool(sourceContent, "isDeleted"),
                        IPAddress = GetString(sourceContent, "ipAddress"),
                        ContentItemId = GetInt(sourceContent, "contentItemId"),
                        ModuleId = moduleId,
                    };

                    ((IRepository<ContentInfo>)ContentController.Instance).Insert(importedContent);
                    contentMap[GetInt(sourceContent, "contentId")] = importedContent.ContentId;
                }

                foreach (var sourceTopic in GetElements(root, "topics", "topic"))
                {
                    if (!contentMap.TryGetValue(GetInt(sourceTopic, "contentId"), out var newContentId))
                    {
                        continue;
                    }

                    var topic = new TopicInfo
                    {
                        ContentId = newContentId,
                        ViewCount = GetInt(sourceTopic, "viewCount"),
                        ReplyCount = GetInt(sourceTopic, "replyCount"),
                        IsLocked = GetBool(sourceTopic, "isLocked"),
                        IsPinned = GetBool(sourceTopic, "isPinned"),
                        TopicIcon = GetString(sourceTopic, "topicIcon"),
                        StatusId = GetInt(sourceTopic, "statusId"),
                        IsApproved = GetBool(sourceTopic, "isApproved"),
                        IsRejected = GetBool(sourceTopic, "isRejected"),
                        IsDeleted = GetBool(sourceTopic, "isDeleted"),
                        IsAnnounce = GetBool(sourceTopic, "isAnnounce"),
                        IsArchived = GetBool(sourceTopic, "isArchived"),
                        AnnounceStart = GetNullableDateTime(sourceTopic, "announceStart"),
                        AnnounceEnd = GetNullableDateTime(sourceTopic, "announceEnd"),
                        TopicType = (TopicTypes)GetInt(sourceTopic, "topicType"),
                        Priority = GetInt(sourceTopic, "priority"),
                        TopicUrl = GetString(sourceTopic, "topicUrl"),
                        PrevTopic = GetInt(sourceTopic, "prevTopic"),
                        NextTopic = GetInt(sourceTopic, "nextTopic"),
                        TopicData = GetString(sourceTopic, "topicData"),
                    };

                    ((IRepository<TopicInfo>)TopicController.Instance).Insert(topic);
                    topicMap[GetInt(sourceTopic, "topicId")] = topic.TopicId;
                }

                foreach (var sourceAttachment in GetElements(root, "attachments", "attachment"))
                {
                    if (!contentMap.TryGetValue(GetInt(sourceAttachment, "contentId"), out var newContentId))
                    {
                        continue;
                    }

                    DotNetNuke.Modules.ActiveForums.Controllers.AttachmentController.Instance.Insert(new AttachmentInfo
                    {
                        ContentId = newContentId,
                        UserId = ResolveAuthorId(portalId, GetInt(sourceAttachment, "userId"), userId),
                        FileName = GetString(sourceAttachment, "fileName"),
                        FileId = GetNullableInt(sourceAttachment, "fileId"),
                        ContentType = GetString(sourceAttachment, "contentType"),
                        DateAdded = GetNullableDateTime(sourceAttachment, "dateAdded"),
                        DateUpdated = GetNullableDateTime(sourceAttachment, "dateUpdated"),
                        FileSize = GetLong(sourceAttachment, "fileSize"),
                        DisplayInline = GetBool(sourceAttachment, "displayInline"),
                    });
                }

                foreach (var sourceLike in GetElements(root, "likes", "like"))
                {
                    if (!contentMap.TryGetValue(GetInt(sourceLike, "postId"), out var newPostId))
                    {
                        continue;
                    }

                    var resolvedUserId = ResolveUserIdOrDefault(portalId, GetInt(sourceLike, "userId"));
                    if (!resolvedUserId.HasValue)
                    {
                        continue;
                    }

                    ((IRepository<LikeInfo>)LikeController.Instance).Insert(new LikeInfo
                    {
                        PostId = newPostId,
                        UserId = resolvedUserId.Value,
                        Checked = GetBool(sourceLike, "checked"),
                        DateCreated = GetDateTime(sourceLike, "dateCreated", DateTime.UtcNow),
                    });
                }

                foreach (var sourceUserMention in GetElements(root, "userMentions", "userMention"))
                {
                    if (!contentMap.TryGetValue(GetInt(sourceUserMention, "contentId"), out var newMentionContentId))
                    {
                        continue;
                    }

                    var resolvedUserId = ResolveUserIdOrDefault(portalId, GetInt(sourceUserMention, "userId"));
                    if (!resolvedUserId.HasValue)
                    {
                        continue;
                    }

                    ((IRepository<UserMentionInfo>)UserMentionController.Instance).Insert(new UserMentionInfo
                    {
                        ContentId = newMentionContentId,
                        UserId = resolvedUserId.Value,
                        PortalId = portalId,
                        ModuleId = moduleId,
                        DateMentioned = GetDateTime(sourceUserMention, "dateMentioned", DateTime.UtcNow),
                    });
                }

                foreach (var sourceReply in GetElements(root, "replies", "reply"))
                {
                    if (!topicMap.TryGetValue(GetInt(sourceReply, "topicId"), out var newTopicId))
                    {
                        continue;
                    }

                    if (!contentMap.TryGetValue(GetInt(sourceReply, "contentId"), out var newContentId))
                    {
                        continue;
                    }

                    var oldReplyId = GetInt(sourceReply, "replyId");
                    var oldReplyToId = GetInt(sourceReply, "replyToId");
                    var reply = new ReplyInfo
                    {
                        TopicId = newTopicId,
                        ReplyToId = 0,
                        ContentId = newContentId,
                        IsApproved = GetBool(sourceReply, "isApproved"),
                        IsRejected = GetBool(sourceReply, "isRejected"),
                        StatusId = GetInt(sourceReply, "statusId"),
                        IsDeleted = GetBool(sourceReply, "isDeleted"),
                    };

                    ((IRepository<ReplyInfo>)ReplyController.Instance).Insert(reply);
                    replyMap[oldReplyId] = reply.ReplyId;
                    if (oldReplyToId > 0)
                    {
                        pendingReplyParents[reply.ReplyId] = oldReplyToId;
                    }
                }

                foreach (var pendingReply in pendingReplyParents)
                {
                    var reply = ((IRepository<ReplyInfo>)ReplyController.Instance).GetById(pendingReply.Key);
                    if (reply == null)
                    {
                        continue;
                    }

                    if (replyMap.TryGetValue(pendingReply.Value, out var newReplyToId))
                    {
                        reply.ReplyToId = newReplyToId;
                        ((IRepository<ReplyInfo>)ReplyController.Instance).Update(reply);
                    }
                }

                foreach (var sourceForumTopic in GetElements(root, "forumTopics", "forumTopic"))
                {
                    if (!forumMap.TryGetValue(GetInt(sourceForumTopic, "forumId"), out var newForumId))
                    {
                        continue;
                    }

                    if (!topicMap.TryGetValue(GetInt(sourceForumTopic, "topicId"), out var newTopicId))
                    {
                        continue;
                    }

                    var forumTopic = new ForumTopicInfo
                    {
                        ForumId = newForumId,
                        TopicId = newTopicId,
                        LastReplyId = null,
                    };

                    var oldLastReplyId = GetNullableInt(sourceForumTopic, "lastReplyId");
                    if (oldLastReplyId.HasValue && replyMap.TryGetValue(oldLastReplyId.Value, out var newLastReplyId))
                    {
                        forumTopic.LastReplyId = newLastReplyId;
                    }

                    ((IRepository<ForumTopicInfo>)ForumTopicController.Instance).Insert(forumTopic);
                }

                foreach (var sourceTopicTag in GetElements(root, "topicTags", "topicTag"))
                {
                    if (!topicMap.TryGetValue(GetInt(sourceTopicTag, "topicId"), out var newTopicId))
                    {
                        continue;
                    }

                    if (!tagMap.TryGetValue(GetInt(sourceTopicTag, "tagId"), out var newTagId))
                    {
                        continue;
                    }

                    ((IRepository<TopicTagInfo>)TopicTagController.Instance).Insert(new TopicTagInfo
                    {
                        TopicId = newTopicId,
                        TagId = newTagId,
                    });
                }

                foreach (var sourceTopicCategory in GetElements(root, "topicCategories", "topicCategory"))
                {
                    if (!topicMap.TryGetValue(GetInt(sourceTopicCategory, "topicId"), out var newTopicId))
                    {
                        continue;
                    }

                    if (!categoryMap.TryGetValue(GetInt(sourceTopicCategory, "categoryId"), out var newCategoryId))
                    {
                        continue;
                    }

                    ((IRepository<TopicCategoryInfo>)TopicCategoryController.Instance).Insert(new TopicCategoryInfo
                    {
                        TopicId = newTopicId,
                        CategoryId = newCategoryId,
                    });
                }

                foreach (var sourceTopicRating in GetElements(root, "topicRatings", "topicRating"))
                {
                    if (!topicMap.TryGetValue(GetInt(sourceTopicRating, "topicId"), out var newTopicId))
                    {
                        continue;
                    }

                    var resolvedUserId = ResolveUserIdOrDefault(portalId, GetInt(sourceTopicRating, "userId"));
                    if (!resolvedUserId.HasValue)
                    {
                        continue;
                    }

                    ((IRepository<TopicRatingInfo>)TopicRatingController.Instance).Insert(new TopicRatingInfo
                    {
                        TopicId = newTopicId,
                        UserId = resolvedUserId.Value,
                        Rating = GetInt(sourceTopicRating, "rating"),
                        Helpful = GetBool(sourceTopicRating, "helpful"),
                        Comments = GetString(sourceTopicRating, "comments"),
                        IPAddress = GetString(sourceTopicRating, "ipAddress"),
                        DateAdded = GetDateTime(sourceTopicRating, "dateAdded", DateTime.UtcNow),
                        DateUpdated = GetDateTime(sourceTopicRating, "dateUpdated", DateTime.UtcNow),
                    });
                }

                foreach (var sourceTopicTracking in GetElements(root, "topicTracking", "topicTracking"))
                {
                    if (!topicMap.TryGetValue(GetInt(sourceTopicTracking, "topicId"), out var newTopicId))
                    {
                        continue;
                    }

                    if (!forumMap.TryGetValue(GetInt(sourceTopicTracking, "forumId"), out var newTrackingForumId))
                    {
                        continue;
                    }

                    var resolvedUserId = ResolveUserIdOrDefault(portalId, GetInt(sourceTopicTracking, "userId"));
                    if (!resolvedUserId.HasValue)
                    {
                        continue;
                    }

                    ((IRepository<TopicTrackingInfo>)TopicTrackingController.Instance).Insert(new TopicTrackingInfo
                    {
                        ForumId = newTrackingForumId,
                        TopicId = newTopicId,
                        LastReplyId = replyMap.TryGetValue(GetInt(sourceTopicTracking, "lastReplyId"), out var newLastReplyId) ? newLastReplyId : 0,
                        UserId = resolvedUserId.Value,
                        DateAdded = GetDateTime(sourceTopicTracking, "dateAdded", DateTime.UtcNow),
                    });
                }

                foreach (var sourceSubscription in GetElements(root, "subscriptions", "subscription"))
                {
                    var resolvedUserId = ResolveUserIdOrDefault(portalId, GetInt(sourceSubscription, "userId"));
                    if (!resolvedUserId.HasValue)
                    {
                        continue;
                    }

                    var oldForumId = GetInt(sourceSubscription, "forumId");
                    var oldTopicId = GetInt(sourceSubscription, "topicId");
                    var newSubscriptionForumId = 0;
                    var newSubscriptionTopicId = 0;
                    var hasForum = oldForumId <= 0 || forumMap.TryGetValue(oldForumId, out newSubscriptionForumId);
                    var hasTopic = oldTopicId <= 0 || topicMap.TryGetValue(oldTopicId, out newSubscriptionTopicId);
                    if (!hasForum || !hasTopic)
                    {
                        continue;
                    }

                    ((IRepository<SubscriptionInfo>)DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController.Instance).Insert(new SubscriptionInfo
                    {
                        PortalId = portalId,
                        ModuleId = moduleId,
                        ForumId = oldForumId > 0 ? newSubscriptionForumId : 0,
                        TopicId = oldTopicId > 0 ? newSubscriptionTopicId : 0,
                        Mode = GetInt(sourceSubscription, "mode"),
                        UserId = resolvedUserId.Value,
                    });
                }

                foreach (var sourceArchivedUrl in GetElements(root, "archivedUrls", "archivedUrl"))
                {
                    var oldForumId = GetInt(sourceArchivedUrl, "forumId");
                    var oldTopicId = GetInt(sourceArchivedUrl, "topicId");
                    var oldForumGroupId = GetInt(sourceArchivedUrl, "forumGroupId");
                    var newArchivedForumId = 0;
                    var newArchivedTopicId = 0;
                    var newArchivedForumGroupId = 0;
                    var hasForum = oldForumId <= 0 || forumMap.TryGetValue(oldForumId, out newArchivedForumId);
                    var hasTopic = oldTopicId <= 0 || topicMap.TryGetValue(oldTopicId, out newArchivedTopicId);
                    var hasForumGroup = oldForumGroupId <= 0 || groupMap.TryGetValue(oldForumGroupId, out newArchivedForumGroupId);
                    if (!hasForum || !hasTopic || !hasForumGroup)
                    {
                        continue;
                    }

                    ((IRepository<ArchivedURLInfo>)ArchivedURLController.Instance).Insert(new ArchivedURLInfo
                    {
                        Url = GetString(sourceArchivedUrl, "url"),
                        PortalId = portalId,
                        ForumId = oldForumId > 0 ? newArchivedForumId : 0,
                        TopicId = oldTopicId > 0 ? newArchivedTopicId : 0,
                        ForumGroupId = oldForumGroupId > 0 ? newArchivedForumGroupId : 0,
                        UrlHash = GetBytes(sourceArchivedUrl, "urlHash"),
                    });
                }

                foreach (var sourceForum in GetElements(root, "forums", "forum"))
                {
                    var oldForumId = GetInt(sourceForum, "forumId");
                    if (!forumMap.TryGetValue(oldForumId, out var newForumId))
                    {
                        continue;
                    }

                    var forum = ((IRepository<ForumInfo>)ForumController.Instance).GetById(newForumId, moduleId);
                    if (forum == null)
                    {
                        continue;
                    }

                    var oldLastTopicId = GetInt(sourceForum, "lastTopicId");
                    var oldLastReplyId = GetInt(sourceForum, "lastReplyId");

                    forum.LastTopicId = topicMap.TryGetValue(oldLastTopicId, out var newLastTopicId) ? newLastTopicId : 0;
                    forum.LastReplyId = replyMap.TryGetValue(oldLastReplyId, out var newLastReplyId) ? newLastReplyId : 0;
                    ((IRepository<ForumInfo>)ForumController.Instance).Update(forum);

                    _ = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.UpdateForumLastUpdates(newForumId);
                    _ = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.RecalculateTopicPointers(newForumId);
                }

                foreach (var sourceForumTracking in GetElements(root, "forumTracking", "forumTracking"))
                {
                    if (!forumMap.TryGetValue(GetInt(sourceForumTracking, "forumId"), out var newForumId))
                    {
                        continue;
                    }

                    var resolvedUserId = ResolveUserIdOrDefault(portalId, GetInt(sourceForumTracking, "userId"));
                    if (!resolvedUserId.HasValue)
                    {
                        continue;
                    }

                    ((IRepository<ForumTrackingInfo>)ForumTrackingController.Instance).Insert(new ForumTrackingInfo
                    {
                        ModuleId = moduleId,
                        UserId = resolvedUserId.Value,
                        ForumId = newForumId,
                        LastAccessDateTime = GetDateTime(sourceForumTracking, "lastAccessDateTime", DateTime.UtcNow),
                        MaxTopicRead = topicMap.TryGetValue(GetInt(sourceForumTracking, "maxTopicRead"), out var newMaxTopicRead) ? newMaxTopicRead : 0,
                        MaxReplyRead = replyMap.TryGetValue(GetInt(sourceForumTracking, "maxReplyRead"), out var newMaxReplyRead) ? newMaxReplyRead : 0,
                    });
                }

                foreach (var sourceUserBadge in GetElements(root, "userBadges", "userBadge"))
                {
                    if (!badgeMap.TryGetValue(GetInt(sourceUserBadge, "badgeId"), out var newBadgeId))
                    {
                        continue;
                    }

                    var resolvedUserId = ResolveUserIdOrDefault(portalId, GetInt(sourceUserBadge, "userId"));
                    if (!resolvedUserId.HasValue || resolvedUserId.Value <= 0)
                    {
                        continue;
                    }

                    ((IRepository<UserBadgeInfo>)UserBadgeController.Instance).Insert(new UserBadgeInfo
                    {
                        BadgeId = newBadgeId,
                        UserId = resolvedUserId.Value,
                        PortalId = portalId,
                        ModuleId = moduleId,
                        DateAssigned = GetDateTime(sourceUserBadge, "dateAssigned", DateTime.UtcNow),
                    });
                }

                // Clear out the cache
                DotNetNuke.Modules.ActiveForums.Services.Cache.CacheBase.ClearAllCache(moduleId);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }
        }

        private static PermissionInfo CreatePermission(int moduleId, XElement sourcePermission)
        {
            var permission = new PermissionInfo();
            PopulatePermission(permission, moduleId, sourcePermission);
            return permission;
        }

        private static int GetMappedPermissionId(IDictionary<int, int> permissionMap, int sourcePermissionId, int defaultPermissionId)
        {
            return permissionMap.TryGetValue(sourcePermissionId, out var mappedPermissionId) ? mappedPermissionId : defaultPermissionId;
        }

        private static string GetMappedSettingsKey(IDictionary<string, string> settingsKeyMap, string sourceSettingsKey, string sourceDefaultSettingsKey, string defaultSettingsKey)
        {
            if (string.IsNullOrWhiteSpace(sourceSettingsKey))
            {
                return defaultSettingsKey;
            }

            if (string.Equals(sourceSettingsKey, sourceDefaultSettingsKey, StringComparison.OrdinalIgnoreCase))
            {
                return defaultSettingsKey;
            }

            if (settingsKeyMap.TryGetValue(sourceSettingsKey, out var mappedSettingsKey))
            {
                return mappedSettingsKey;
            }

            settingsKeyMap[sourceSettingsKey] = sourceSettingsKey;
            return sourceSettingsKey;
        }

        private static void PopulatePermission(PermissionInfo permission, int moduleId, XElement sourcePermission)
        {
            permission.ModuleId = moduleId;
            permission.View = GetString(sourcePermission, "view");
            permission.Read = GetString(sourcePermission, "read");
            permission.Create = GetString(sourcePermission, "create");
            permission.Reply = GetString(sourcePermission, "reply");
            permission.Edit = GetString(sourcePermission, "edit");
            permission.Delete = GetString(sourcePermission, "delete");
            permission.Lock = GetString(sourcePermission, "lock");
            permission.Pin = GetString(sourcePermission, "pin");
            permission.Attach = GetString(sourcePermission, "attach");
            permission.Poll = GetString(sourcePermission, "poll");
            permission.Trust = GetString(sourcePermission, "trust");
            permission.Subscribe = GetString(sourcePermission, "subscribe");
            permission.Announce = GetString(sourcePermission, "announce");
            permission.Tag = GetString(sourcePermission, "tag");
            permission.Categorize = GetString(sourcePermission, "categorize");
            permission.Prioritize = GetString(sourcePermission, "prioritize");
            permission.Moderate = GetString(sourcePermission, "moderate");
            permission.Move = GetString(sourcePermission, "move");
            permission.Split = GetString(sourcePermission, "split");
            permission.ManageUsers = GetString(sourcePermission, "manageUsers");
            permission.Mention = GetString(sourcePermission, "mention");
        }

        private static int ResolveAuthorId(int portalId, int sourceAuthorId, int importingUserId)
        {
            if (sourceAuthorId < 1)
            {
                return importingUserId;
            }

            return DotNetNuke.Entities.Users.UserController.Instance.GetUserById(portalId, sourceAuthorId) != null ? sourceAuthorId : importingUserId;
        }

        private static int? ResolveUserIdOrDefault(int portalId, int sourceUserId)
        {
            if (sourceUserId < 1)
            {
                return sourceUserId;
            }

            return DotNetNuke.Entities.Users.UserController.Instance.GetUserById(portalId, sourceUserId) != null ? sourceUserId : (int?)null;
        }

        private static int GetInt(XElement element, string attributeName)
        {
            if (int.TryParse(element?.Attribute(attributeName)?.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
            {
                return value;
            }

            return 0;
        }

        private static int? GetNullableInt(XElement element, string attributeName)
        {
            var value = element?.Attribute(attributeName)?.Value;
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : (int?)null;
        }

        private static long GetLong(XElement element, string attributeName)
        {
            return long.TryParse(element?.Attribute(attributeName)?.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : 0L;
        }

        private static string GetString(XElement element, string attributeName)
        {
            return element?.Attribute(attributeName)?.Value ?? string.Empty;
        }

        private static bool GetBool(XElement element, string attributeName)
        {
            if (bool.TryParse(element?.Attribute(attributeName)?.Value, out var value))
            {
                return value;
            }

            return false;
        }

        private static DateTime GetDateTime(XElement element, string attributeName, DateTime defaultValue)
        {
            if (DateTime.TryParse(element?.Attribute(attributeName)?.Value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var value))
            {
                return value;
            }

            return defaultValue;
        }

        private static DateTime? GetNullableDateTime(XElement element, string attributeName)
        {
            var value = element?.Attribute(attributeName)?.Value;
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsed) ? parsed : (DateTime?)null;
        }

        private static string ToPortableDate(DateTime? value)
        {
            return value.HasValue ? value.Value.ToString("o", CultureInfo.InvariantCulture) : string.Empty;
        }

        private static byte[] GetBytes(XElement element, string attributeName)
        {
            var value = element?.Attribute(attributeName)?.Value;
            return string.IsNullOrWhiteSpace(value) ? null : Convert.FromBase64String(value);
        }

        private static string ToPortableBytes(byte[] value)
        {
            return value != null && value.Length > 0 ? Convert.ToBase64String(value) : string.Empty;
        }

        private static IEnumerable<XElement> GetElements(XElement root, string containerName, string elementName)
        {
            return root.Element(containerName)?.Elements(elementName) ?? Enumerable.Empty<XElement>();
        }
    }
}
