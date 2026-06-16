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
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlTypes;
    using System.Globalization;
    using System.Linq;
    using System.Web.UI.WebControls;
    using System.Xml.Linq;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Modules.ActiveForums.Controllers;
    using DotNetNuke.Modules.ActiveForums.Entities;
    using DotNetNuke.Modules.ActiveForums.Extensions;
    using DotNetNuke.Modules.ActiveForums.Helpers;
    using DotNetNuke.Modules.ActiveForums.Services.Cache;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.Search.Entities;

    #region Topics Controller
    public class TopicsController : DotNetNuke.Entities.Modules.ModuleSearchBase, DotNetNuke.Entities.Modules.IUpgradeable, DotNetNuke.Entities.Modules.IPortable
    {
        private static readonly DotNetNuke.Instrumentation.ILog Logger = LoggerSource.Instance.GetLogger(typeof(TopicsController));
        internal static readonly IReadOnlyList<string> PortableEntityDependencyOrder = new[]
        {
            "groups",
            "tags",
            "badges",
            "forums",
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
            "userProfiles",
            "forumTracking",
            "userBadges",
        };

        private readonly IPortalAliasService portalAliasService;
        private readonly IPortalSettings portalSettings;
        private readonly IPortalController portalController;

        public TopicsController()
            : this(
                  new DotNetNuke.Entities.Portals.PortalAliasController(),
                  ServiceLocator<IPortalController, PortalController>.Instance,
                  ServiceLocator<IPortalController, PortalController>.Instance.GetCurrentSettings())
        {
        }

        public TopicsController(IPortalAliasService portalAliasService, IPortalController portalController, IPortalSettings portalSettings)
        {
            this.portalAliasService = portalAliasService;
            this.portalController = portalController;
            this.portalSettings = portalSettings;
        }

        #region ModuleSearchBase

        public override IList<SearchDocument> GetModifiedSearchDocuments(ModuleInfo moduleInfo, DateTime beginDateUtc)
        {
            /* since this code runs without HttpContext, get https:// by looking at page settings */
            bool isHttps = DotNetNuke.Entities.Tabs.TabController.Instance.GetTab(moduleInfo.TabID, moduleInfo.PortalID).IsSecure;
            bool useFriendlyURLs = Utilities.UseFriendlyURLs(moduleInfo.ModuleID);
            string primaryPortalAlias = this.portalAliasService.GetPortalAliasesByPortalId(moduleInfo.PortalID).FirstOrDefault(x => x.IsPrimary).HttpAlias;

            Dictionary<int, string> authorizedRolesForForum = new Dictionary<int, string>();
            Dictionary<int, string> forumUrlPrefixes = new Dictionary<int, string>();

            List<string> roles = new List<string>();
            foreach (DotNetNuke.Security.Roles.RoleInfo r in DotNetNuke.Security.Roles.RoleController.Instance.GetRoles(portalId: moduleInfo.PortalID))
            {
                roles.Add(r.RoleName);
            }

            var portalSettings = new DotNetNuke.Modules.ActiveForums.Helpers.PortalSettingsHelper().GetPortalSettings(moduleInfo.PortalID);
            string roleIds = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetPortalRoleIds(moduleInfo.PortalID, roles.ToArray());
            string queryString = string.Empty;
            System.Text.StringBuilder qsb = new System.Text.StringBuilder();
            List<SearchDocument> searchDocuments = new List<SearchDocument>();
            IDataReader dr = null;
            try
            {
                dr = DataProvider.Instance().Search_DotNetNuke(moduleInfo.ModuleID, beginDateUtc);
                while (dr.Read())
                {
                    string subject = dr["Subject"].ToString();
                    string description = string.Empty;
                    string body = dr["Body"].ToString();
                    List<string> tags = dr["Tags"].ToString().Split(",".ToCharArray()).ToList();
                    DateTime dateupdated = Convert.ToDateTime(dr["DateUpdated"]);
                    int authorid = Convert.ToInt32(dr["AuthorId"]);
                    bool isDeleted = Convert.ToBoolean(dr["isDeleted"]);
                    bool isApproved = Convert.ToBoolean(dr["isApproved"]);
                    int contentid = Convert.ToInt32(dr["ContentId"]);
                    int forumid = Convert.ToInt32(dr["ForumId"]);
                    int forumGroupId = Convert.ToInt32(dr["ForumGroupId"]);
                    int topicid = Convert.ToInt32(dr["TopicId"]);
                    int replyId = Convert.ToInt32(dr["ReplyId"]);
                    string topicURL = dr["TopicUrl"].ToString();
                    string forumGroupUrlPrefix = dr["ForumGroupUrlPrefix"].ToString();
                    string forumUrlPrefix = dr["ForumUrlPrefix"].ToString();
                    int jumpid = (replyId > 0) ? replyId : topicid;
                    body = DotNetNuke.Common.Utilities.HtmlUtils.Clean(body, false);
                    if (!string.IsNullOrEmpty(body))
                    {
                        description = body.Length > 100 ? body.Substring(0, 100) + "..." : body;
                    }

                    DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forumInfo = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Instance.GetById(moduleInfo.ModuleID, forumid);

                    // NOTE: indexer is called from scheduler and has no httpcontext
                    // so any code that relies on HttpContext cannot be used...
                    string link = new ControlUtils().BuildUrl(portalId: moduleInfo.PortalID, tabId: moduleInfo.TabID, moduleId: moduleInfo.ModuleID, groupPrefix: forumGroupUrlPrefix, forumPrefix: forumUrlPrefix, forumGroupId: forumGroupId, forumID: forumid, topicId: topicid, topicURL: topicURL, tagId: -1, categoryId: -1, otherPrefix: string.Empty, pageId: 1, contentId: contentid, socialGroupId: forumInfo.SocialGroupId);
                    if (!string.IsNullOrEmpty(link) && !link.StartsWith("http"))
                    {
                        link = (isHttps ? "https://" : "http://") + primaryPortalAlias + link;
                    }

                    queryString = qsb.Clear().Append($"{ParamKeys.ForumId}={forumid}&{ParamKeys.TopicId}={topicid}&{ParamKeys.ViewType}={Views.Topic}&{ParamKeys.ContentJumpId}={jumpid}").ToString();
                    string permittedRolesCanView = string.Empty;
                    if (!authorizedRolesForForum.TryGetValue(forumid, out permittedRolesCanView))
                    {
                        var delimiter = ";";
                        var viewRolesAsDelimitedString = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsForRequestedAccess(moduleId: moduleInfo.ModuleID, permissionsId: forumid, requestedAccess: SecureActions.View).FromHashSetToDelimitedString(delimiter);
                        permittedRolesCanView = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetNamesForRoles(portalSettings: portalSettings, roles: viewRolesAsDelimitedString, delimiter: delimiter);
                        authorizedRolesForForum.Add(forumid, permittedRolesCanView);
                    }

                    var searchDoc = new SearchDocument
                    {
                        UniqueKey = $"{moduleInfo.ModuleID}-{contentid}",
                        ModuleId = moduleInfo.ModuleID,
                        AuthorUserId = authorid,
                        PortalId = moduleInfo.PortalID,
                        Title = subject,
                        Description = description,
                        Body = body,
                        Url = link,
                        QueryString = queryString,
                        ModifiedTimeUtc = dateupdated,
                        Tags = tags.Count > 0 ? tags : null,
                        NumericKeys = new Dictionary<string, int> { { "ForumId", forumid }, { "TopicId", topicid }, { "ReplyId", replyId }, { "ContentId", contentid }, { "AuthorUserId", authorid } },
                        TabId = moduleInfo.TabID,
                        Permissions = permittedRolesCanView,
                        IsActive = isApproved && !isDeleted,
                    };
                    searchDocuments.Add(searchDoc);
                }

                dr.Close();
                return searchDocuments;
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return null;
            }
            finally
            {
                if (dr != null)
                {
                    if (!dr.IsClosed)
                    {
                        dr.Close();
                    }
                }
            }
        }
        #endregion

        #region "IPortable"
        public string ExportModule(int moduleId)
        {
            try
            {
                var moduleInfo = ModuleController.Instance.GetModule(moduleId, Null.NullInteger, false);
                var groups = ((IRepository<ForumGroupInfo>)ForumGroupController.Instance)
                    .Get(moduleId)
                    .OrderBy(g => g.SortOrder)
                    .ThenBy(g => g.ForumGroupId)
                    .Select(g => new GroupPortable
                    {
                        ForumGroupId = g.ForumGroupId,
                        GroupName = g.GroupName,
                        SortOrder = g.SortOrder,
                        Active = g.Active,
                        Hidden = g.Hidden,
                        GroupSettingsKey = g.GroupSettingsKey,
                        PermissionsId = g.PermissionsId,
                        PrefixURL = g.PrefixURL,
                    })
                    .ToList();

                var tags = ((IRepository<TagInfo>)TagController.Instance)
                    .Get(moduleId)
                    .OrderBy(t => t.TagId)
                    .Select(t => new TagPortable
                    {
                        TagId = t.TagId,
                        PortalId = t.PortalId,
                        ModuleId = t.ModuleId,
                        TagName = t.TagName,
                        Items = t.Items,
                    })
                    .ToList();

                var badges = ((IRepository<BadgeInfo>)BadgeController.Instance)
                    .Get(moduleId)
                    .OrderBy(b => b.SortOrder)
                    .ThenBy(b => b.BadgeId)
                    .ToList();

                var forums = ForumController.Instance.GetForums(moduleId)
                    .OrderBy(f => f.ForumGroupId)
                    .ThenBy(f => f.SortOrder)
                    .ThenBy(f => f.ForumID)
                    .Select(f => new ForumPortable
                    {
                        ForumId = f.ForumID,
                        PortalId = f.PortalId,
                        ModuleId = f.ModuleId,
                        ForumGroupId = f.ForumGroupId,
                        ParentForumId = f.ParentForumId,
                        ForumName = f.ForumName,
                        ForumDesc = f.ForumDesc,
                        SortOrder = f.SortOrder,
                        Active = f.Active,
                        Hidden = f.Hidden,
                        TotalTopics = f.TotalTopics,
                        TotalReplies = f.TotalReplies,
                        ForumSettingsKey = f.ForumSettingsKey,
                        DateCreated = f.DateCreated,
                        DateUpdated = f.DateUpdated,
                        LastTopicId = f.LastTopicId,
                        LastReplyId = f.LastReplyId,
                        PermissionsId = f.PermissionsId,
                        PrefixURL = f.PrefixURL,
                        SocialGroupId = f.SocialGroupId,
                        HasProperties = f.HasProperties,
                    })
                    .ToList();

                var forumIds = forums.Select(f => f.ForumId).ToHashSet();
                var categories = ((IRepository<CategoryInfo>)CategoryController.Instance)
                    .Get(moduleId)
                    .OrderBy(c => c.Priority)
                    .ThenBy(c => c.CategoryId)
                    .ToList();

                var forumTopics = ((IRepository<ForumTopicInfo>)ForumTopicController.Instance)
                    .Get()
                    .Where(ft => forumIds.Contains(ft.ForumId))
                    .OrderBy(ft => ft.ForumTopicId)
                    .Select(ft => new ForumTopicPortable
                    {
                        ForumTopicId = ft.ForumTopicId,
                        ForumId = ft.ForumId,
                        TopicId = ft.TopicId,
                        LastReplyId = ft.LastReplyId,
                    })
                    .ToList();

                var topicIds = forumTopics.Select(ft => ft.TopicId).Distinct().ToList();
                var topics = topicIds
                    .Select(topicId => TopicController.Instance.GetById(moduleId, topicId))
                    .Where(t => t != null)
                    .OrderBy(t => t.TopicId)
                    .Select(t => new TopicPortable
                    {
                        TopicId = t.TopicId,
                        ContentId = t.ContentId,
                        ViewCount = t.ViewCount,
                        ReplyCount = t.ReplyCount,
                        IsLocked = t.IsLocked,
                        IsPinned = t.IsPinned,
                        TopicIcon = t.TopicIcon,
                        StatusId = t.StatusId,
                        IsApproved = t.IsApproved,
                        IsRejected = t.IsRejected,
                        IsDeleted = t.IsDeleted,
                        IsAnnounce = t.IsAnnounce,
                        IsArchived = t.IsArchived,
                        AnnounceStart = t.AnnounceStart,
                        AnnounceEnd = t.AnnounceEnd,
                        TopicType = (int)t.TopicType,
                        Priority = t.Priority,
                        TopicUrl = t.TopicUrl,
                        PrevTopic = t.PrevTopic,
                        NextTopic = t.NextTopic,
                        TopicData = t.TopicData,
                    })
                    .ToList();

                var replies = topicIds
                    .SelectMany(topicId => ReplyController.Instance.GetByTopicId(moduleId, topicId))
                    .OrderBy(r => r.ReplyId)
                    .Select(r => new ReplyPortable
                    {
                        ReplyId = r.ReplyId,
                        TopicId = r.TopicId,
                        ReplyToId = r.ReplyToId,
                        ContentId = r.ContentId,
                        IsApproved = r.IsApproved,
                        IsRejected = r.IsRejected,
                        StatusId = r.StatusId,
                        IsDeleted = r.IsDeleted,
                    })
                    .ToList();

                var contentIds = topics.Select(t => t.ContentId).Concat(replies.Select(r => r.ContentId)).Distinct().ToList();
                var contents = contentIds
                    .Select(contentId => ContentController.Instance.GetById(moduleId, contentId))
                    .Where(c => c != null)
                    .OrderBy(c => c.ContentId)
                    .Select(c => new ContentPortable
                    {
                        ContentId = c.ContentId,
                        Subject = c.Subject,
                        Summary = c.Summary,
                        Body = c.Body,
                        DateCreated = c.DateCreated,
                        DateUpdated = c.DateUpdated,
                        AuthorId = c.AuthorId,
                        AuthorName = c.AuthorName,
                        IsDeleted = c.IsDeleted,
                        IPAddress = c.IPAddress,
                        ContentItemId = c.ContentItemId,
                        ModuleId = c.ModuleId,
                    })
                    .ToList();

                var attachments = contentIds
                    .SelectMany(contentId => DotNetNuke.Modules.ActiveForums.Controllers.AttachmentController.Instance.GetByContentId(moduleId, contentId) ?? Enumerable.Empty<AttachmentInfo>())
                    .OrderBy(a => a.AttachmentId)
                    .ToList();

                var likes = ((IRepository<LikeInfo>)LikeController.Instance)
                    .Get()
                    .Where(l => contentIds.Contains(l.PostId))
                    .OrderBy(l => l.Id)
                    .ToList();

                var userMentions = ((IRepository<UserMentionInfo>)UserMentionController.Instance)
                    .Get(moduleId)
                    .Where(um => contentIds.Contains(um.ContentId))
                    .OrderBy(um => um.UserMentionId)
                    .ToList();

                var topicTags = topicIds
                    .SelectMany(topicId => TopicTagController.Instance.GetForTopic(topicId))
                    .OrderBy(tt => tt.TopicTagId)
                    .Select(tt => new TopicTagPortable
                    {
                        TopicTagId = tt.TopicTagId,
                        TopicId = tt.TopicId,
                        TagId = tt.TagId,
                    })
                    .ToList();

                var topicCategories = topicIds
                    .SelectMany(topicId => TopicCategoryController.Instance.GetForTopic(topicId))
                    .OrderBy(tc => tc.TopicCategoryId)
                    .ToList();

                var topicRatings = topicIds
                    .SelectMany(topicId => TopicRatingController.Instance.GetForTopic(topicId))
                    .OrderBy(tr => tr.RatingId)
                    .ToList();

                var topicTracking = ((IRepository<TopicTrackingInfo>)TopicTrackingController.Instance)
                    .Get()
                    .Where(tt => topicIds.Contains(tt.TopicId))
                    .OrderBy(tt => tt.TrackingId)
                    .ToList();

                var subscriptions = ((IRepository<SubscriptionInfo>)DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController.Instance)
                    .Get()
                    .Where(s => s.ModuleId == moduleId && (topicIds.Contains(s.TopicId) || forumIds.Contains(s.ForumId)))
                    .OrderBy(s => s.Id)
                    .ToList();

                var archivedUrls = ((IRepository<ArchivedURLInfo>)ArchivedURLController.Instance)
                    .Get()
                    .Where(a => forumIds.Contains(a.ForumId) || topicIds.Contains(a.TopicId))
                    .OrderBy(a => a.Id)
                    .ToList();

                var forumTracking = ((IRepository<ForumTrackingInfo>)ForumTrackingController.Instance)
                    .Get()
                    .Where(ft => ft.ModuleId == moduleId && forumIds.Contains(ft.ForumId))
                    .OrderBy(ft => ft.TrackingId)
                    .ToList();

                var userBadges = ((IRepository<UserBadgeInfo>)UserBadgeController.Instance)
                    .Get(moduleId)
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

                var userProfiles = ((IRepository<ForumUserInfo>)ForumUserController.Instance)
                    .Get()
                    .Where(up => up.PortalId == moduleInfo.PortalID && exportedUserIds.Contains(up.UserId))
                    .OrderBy(up => up.ProfileId)
                    .ToList();

                var document = new XDocument(
                    new XElement("forumsExport",
                        new XAttribute("schemaVersion", "1.0"),
                        TopicsController.SerializeEntities("groups", groups, g =>
                            new XElement("group",
                                new XAttribute("forumGroupId", g.ForumGroupId),
                                new XAttribute("groupName", g.GroupName ?? string.Empty),
                                new XAttribute("sortOrder", g.SortOrder),
                                new XAttribute("active", g.Active),
                                new XAttribute("hidden", g.Hidden),
                                new XAttribute("groupSettingsKey", g.GroupSettingsKey ?? string.Empty),
                                new XAttribute("permissionsId", g.PermissionsId),
                                new XAttribute("prefixUrl", g.PrefixURL ?? string.Empty))),
                        TopicsController.SerializeEntities("tags", tags, t =>
                            new XElement("tag",
                                new XAttribute("tagId", t.TagId),
                                new XAttribute("portalId", t.PortalId),
                                new XAttribute("moduleId", t.ModuleId),
                                new XAttribute("tagName", t.TagName ?? string.Empty),
                                new XAttribute("items", t.Items))),
                        TopicsController.SerializeEntities("badges", badges, b =>
                            new XElement("badge",
                                new XAttribute("badgeId", b.BadgeId),
                                new XAttribute("name", b.Name ?? string.Empty),
                                new XAttribute("description", b.Description ?? string.Empty),
                                new XAttribute("imageMarkup", b.ImageMarkup ?? string.Empty),
                                new XAttribute("fileId", b.FileId),
                                new XAttribute("sortOrder", b.SortOrder),
                                new XAttribute("oneTimeAward", b.OneTimeAward),
                                new XAttribute("badgeMetric", (int)b.BadgeMetric),
                                new XAttribute("threshold", b.Threshold),
                                new XAttribute("intervalDays", b.IntervalDays),
                                new XAttribute("sendAwardNotification", b.SendAwardNotification),
                                new XAttribute("initialBackfillCompletedDate", this.ToPortableDate(b.InitialBackfillCompletedDate)),
                                new XAttribute("suppressAwardNotificationOnBackfill", b.SuppresssAwardNotificationOnBackfill))),
                        TopicsController.SerializeEntities("forums", forums, f =>
                            new XElement("forum",
                                new XAttribute("forumId", f.ForumId),
                                new XAttribute("portalId", f.PortalId),
                                new XAttribute("moduleId", f.ModuleId),
                                new XAttribute("forumGroupId", f.ForumGroupId),
                                new XAttribute("parentForumId", f.ParentForumId),
                                new XAttribute("forumName", f.ForumName ?? string.Empty),
                                new XAttribute("forumDesc", f.ForumDesc ?? string.Empty),
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
                        TopicsController.SerializeEntities("categories", categories, c =>
                            new XElement("category",
                                new XAttribute("categoryId", c.CategoryId),
                                new XAttribute("portalId", c.PortalId),
                                new XAttribute("moduleId", c.ModuleId),
                                new XAttribute("categoryName", c.CategoryName ?? string.Empty),
                                new XAttribute("clicks", c.Clicks),
                                new XAttribute("items", c.Items),
                                new XAttribute("priority", c.Priority),
                                new XAttribute("forumId", c.ForumId),
                                new XAttribute("forumGroupId", c.ForumGroupId))),
                        TopicsController.SerializeEntities("contents", contents, c =>
                            new XElement("content",
                                new XAttribute("contentId", c.ContentId),
                                new XAttribute("subject", c.Subject ?? string.Empty),
                                new XAttribute("summary", c.Summary ?? string.Empty),
                                new XAttribute("body", c.Body ?? string.Empty),
                                new XAttribute("dateCreated", c.DateCreated.ToString("o", CultureInfo.InvariantCulture)),
                                new XAttribute("dateUpdated", c.DateUpdated.ToString("o", CultureInfo.InvariantCulture)),
                                new XAttribute("authorId", c.AuthorId),
                                new XAttribute("authorName", c.AuthorName ?? string.Empty),
                                new XAttribute("isDeleted", c.IsDeleted),
                                new XAttribute("ipAddress", c.IPAddress ?? string.Empty),
                                new XAttribute("contentItemId", c.ContentItemId),
                                new XAttribute("moduleId", c.ModuleId))),
                        TopicsController.SerializeEntities("topics", topics, t =>
                            new XElement("topic",
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
                                new XAttribute("announceStart", this.ToPortableDate(t.AnnounceStart)),
                                new XAttribute("announceEnd", this.ToPortableDate(t.AnnounceEnd)),
                                new XAttribute("topicType", t.TopicType),
                                new XAttribute("priority", t.Priority),
                                new XAttribute("topicUrl", t.TopicUrl ?? string.Empty),
                                new XAttribute("prevTopic", t.PrevTopic),
                                new XAttribute("nextTopic", t.NextTopic),
                                new XAttribute("topicData", t.TopicData ?? string.Empty))),
                        TopicsController.SerializeEntities("attachments", attachments, a =>
                            new XElement("attachment",
                                new XAttribute("attachmentId", a.AttachmentId),
                                new XAttribute("contentId", a.ContentId),
                                new XAttribute("userId", a.UserId),
                                new XAttribute("fileName", a.FileName ?? string.Empty),
                                new XAttribute("fileId", a.FileId.HasValue ? a.FileId.Value.ToString(CultureInfo.InvariantCulture) : string.Empty),
                                new XAttribute("contentType", a.ContentType ?? string.Empty),
                                new XAttribute("dateAdded", this.ToPortableDate(a.DateAdded)),
                                new XAttribute("dateUpdated", this.ToPortableDate(a.DateUpdated)),
                                new XAttribute("fileSize", a.FileSize),
                                new XAttribute("displayInline", a.DisplayInline))),
                        TopicsController.SerializeEntities("likes", likes, l =>
                            new XElement("like",
                                new XAttribute("id", l.Id),
                                new XAttribute("postId", l.PostId),
                                new XAttribute("userId", l.UserId),
                                new XAttribute("checked", l.Checked),
                                new XAttribute("dateCreated", l.DateCreated.ToString("o", CultureInfo.InvariantCulture)))),
                        TopicsController.SerializeEntities("userMentions", userMentions, um =>
                            new XElement("userMention",
                                new XAttribute("userMentionId", um.UserMentionId),
                                new XAttribute("contentId", um.ContentId),
                                new XAttribute("userId", um.UserId),
                                new XAttribute("portalId", um.PortalId),
                                new XAttribute("moduleId", um.ModuleId),
                                new XAttribute("dateMentioned", um.DateMentioned.ToString("o", CultureInfo.InvariantCulture)))),
                        TopicsController.SerializeEntities("replies", replies, r =>
                            new XElement("reply",
                                new XAttribute("replyId", r.ReplyId),
                                new XAttribute("topicId", r.TopicId),
                                new XAttribute("replyToId", r.ReplyToId),
                                new XAttribute("contentId", r.ContentId),
                                new XAttribute("isApproved", r.IsApproved),
                                new XAttribute("isRejected", r.IsRejected),
                                new XAttribute("statusId", r.StatusId),
                                new XAttribute("isDeleted", r.IsDeleted))),
                        TopicsController.SerializeEntities("forumTopics", forumTopics, ft =>
                            new XElement("forumTopic",
                                new XAttribute("forumTopicId", ft.ForumTopicId),
                                new XAttribute("forumId", ft.ForumId),
                                new XAttribute("topicId", ft.TopicId),
                                new XAttribute("lastReplyId", ft.LastReplyId.HasValue ? ft.LastReplyId.Value.ToString(CultureInfo.InvariantCulture) : string.Empty))),
                        TopicsController.SerializeEntities("topicTags", topicTags, tt =>
                            new XElement("topicTag",
                                new XAttribute("topicTagId", tt.TopicTagId),
                                new XAttribute("topicId", tt.TopicId),
                                new XAttribute("tagId", tt.TagId))),
                        TopicsController.SerializeEntities("topicCategories", topicCategories, tc =>
                            new XElement("topicCategory",
                                new XAttribute("topicCategoryId", tc.TopicCategoryId),
                                new XAttribute("topicId", tc.TopicId),
                                new XAttribute("categoryId", tc.CategoryId))),
                        TopicsController.SerializeEntities("topicRatings", topicRatings, tr =>
                            new XElement("topicRating",
                                new XAttribute("ratingId", tr.RatingId),
                                new XAttribute("topicId", tr.TopicId),
                                new XAttribute("userId", tr.UserId),
                                new XAttribute("rating", tr.Rating),
                                new XAttribute("helpful", tr.Helpful),
                                new XAttribute("comments", tr.Comments ?? string.Empty),
                                new XAttribute("ipAddress", tr.IPAddress ?? string.Empty),
                                new XAttribute("dateAdded", tr.DateAdded.ToString("o", CultureInfo.InvariantCulture)),
                                new XAttribute("dateUpdated", tr.DateUpdated.ToString("o", CultureInfo.InvariantCulture)))),
                        TopicsController.SerializeEntities("topicTracking", topicTracking, tt =>
                            new XElement("topicTracking",
                                new XAttribute("trackingId", tt.TrackingId),
                                new XAttribute("forumId", tt.ForumId),
                                new XAttribute("topicId", tt.TopicId),
                                new XAttribute("lastReplyId", tt.LastReplyId),
                                new XAttribute("userId", tt.UserId),
                                new XAttribute("dateAdded", tt.DateAdded.ToString("o", CultureInfo.InvariantCulture)))),
                        TopicsController.SerializeEntities("subscriptions", subscriptions, s =>
                            new XElement("subscription",
                                new XAttribute("id", s.Id),
                                new XAttribute("portalId", s.PortalId),
                                new XAttribute("moduleId", s.ModuleId),
                                new XAttribute("forumId", s.ForumId),
                                new XAttribute("topicId", s.TopicId),
                                new XAttribute("mode", s.Mode),
                                new XAttribute("userId", s.UserId))),
                        TopicsController.SerializeEntities("archivedUrls", archivedUrls, a =>
                            new XElement("archivedUrl",
                                new XAttribute("id", a.Id),
                                new XAttribute("url", a.Url ?? string.Empty),
                                new XAttribute("portalId", a.PortalId),
                                new XAttribute("forumId", a.ForumId),
                                new XAttribute("topicId", a.TopicId),
                                new XAttribute("forumGroupId", a.ForumGroupId),
                                new XAttribute("urlHash", this.ToPortableBytes(a.UrlHash)))),
                        TopicsController.SerializeEntities("userProfiles", userProfiles, up =>
                            new XElement("userProfile",
                                new XAttribute("profileId", up.ProfileId),
                                new XAttribute("userId", up.UserId),
                                new XAttribute("portalId", up.PortalId),
                                new XAttribute("topicCount", up.TopicCount),
                                new XAttribute("replyCount", up.ReplyCount),
                                new XAttribute("viewCount", up.ViewCount),
                                new XAttribute("answerCount", up.AnswerCount),
                                new XAttribute("rewardPoints", up.RewardPoints),
                                new XAttribute("userCaption", up.UserCaption ?? string.Empty),
                                new XAttribute("avatarLastRefresh", this.ToPortableDate(up.AvatarLastRefresh)),
                                new XAttribute("avatarSourceLastModified", this.ToPortableDate(up.AvatarSourceLastModified)),
                                new XAttribute("avatarFileId", up.AvatarFileId.HasValue ? up.AvatarFileId.Value.ToString(CultureInfo.InvariantCulture) : string.Empty),
                                new XAttribute("dateCreated", up.DateCreated.ToString("o", CultureInfo.InvariantCulture)),
                                new XAttribute("dateUpdated", this.ToPortableDate(up.DateUpdated)),
                                new XAttribute("dateLastActivity", this.ToPortableDate(up.DateLastActivity)),
                                new XAttribute("dateLastPost", this.ToPortableDate(up.DateLastPost)),
                                new XAttribute("dateLastReply", this.ToPortableDate(up.DateLastReply)),
                                new XAttribute("signature", up.Signature ?? string.Empty),
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
                        TopicsController.SerializeEntities("forumTracking", forumTracking, ft =>
                            new XElement("forumTracking",
                                new XAttribute("trackingId", ft.TrackingId),
                                new XAttribute("moduleId", ft.ModuleId),
                                new XAttribute("userId", ft.UserId),
                                new XAttribute("forumId", ft.ForumId),
                                new XAttribute("lastAccessDateTime", ft.LastAccessDateTime.ToString("o", CultureInfo.InvariantCulture)),
                                new XAttribute("maxTopicRead", ft.MaxTopicRead),
                                new XAttribute("maxReplyRead", ft.MaxReplyRead))),
                        TopicsController.SerializeEntities("userBadges", userBadges, ub =>
                            new XElement("userBadge",
                                new XAttribute("userBadgeId", ub.UserBadgeId),
                                new XAttribute("badgeId", ub.BadgeId),
                                new XAttribute("userId", ub.UserId),
                                new XAttribute("portalId", ub.PortalId),
                                new XAttribute("moduleId", ub.ModuleId),
                                new XAttribute("dateAssigned", ub.DateAssigned.ToString("o", CultureInfo.InvariantCulture))))
                    ));

                return document.ToString(SaveOptions.DisableFormatting);
            }
            catch (Exception ex)
            {
                this.LogError(ex.Message, ex);
                Exceptions.LogException(ex);
                return string.Empty;
            }
        }

        public void ImportModule(int moduleId, string content, string version, int userId)
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
                var defaultPermissionId = moduleSettings?.DefaultPermissionId ?? -1;
                var defaultSettingsKey = moduleSettings?.DefaultSettingsKey ?? string.Empty;

                var groupMap = new Dictionary<int, int>();
                var forumMap = new Dictionary<int, int>();
                var contentMap = new Dictionary<int, int>();
                var topicMap = new Dictionary<int, int>();
                var replyMap = new Dictionary<int, int>();
                var tagMap = new Dictionary<int, int>();
                var categoryMap = new Dictionary<int, int>();
                var badgeMap = new Dictionary<int, int>();
                var importedForums = new Dictionary<int, ForumInfo>();
                var pendingForumParents = new Dictionary<int, int>();
                var pendingReplyParents = new Dictionary<int, int>();

                foreach (var sourceGroup in TopicsController.GetElements(root, "groups", "group"))
                {
                    var forumGroup = new ForumGroupInfo
                    {
                        ModuleId = moduleId,
                        GroupName = this.GetString(sourceGroup, "groupName"),
                        SortOrder = this.GetInt(sourceGroup, "sortOrder"),
                        Active = this.GetBool(sourceGroup, "active"),
                        Hidden = this.GetBool(sourceGroup, "hidden"),
                        GroupSettingsKey = defaultSettingsKey,
                        PermissionsId = defaultPermissionId,
                        PrefixURL = this.GetString(sourceGroup, "prefixUrl"),
                    };

                    ((IRepository<ForumGroupInfo>)ForumGroupController.Instance).Insert(forumGroup);
                    groupMap[this.GetInt(sourceGroup, "forumGroupId")] = forumGroup.ForumGroupId;
                }

                foreach (var sourceTag in TopicsController.GetElements(root, "tags", "tag"))
                {
                    var tag = new TagInfo
                    {
                        ModuleId = moduleId,
                        PortalId = portalId,
                        TagName = this.GetString(sourceTag, "tagName"),
                        Items = this.GetInt(sourceTag, "items"),
                    };

                    ((IRepository<TagInfo>)TagController.Instance).Insert(tag);
                    tagMap[this.GetInt(sourceTag, "tagId")] = tag.TagId;
                }

                foreach (var sourceBadge in TopicsController.GetElements(root, "badges", "badge"))
                {
                    var badge = new BadgeInfo
                    {
                        ModuleId = moduleId,
                        Name = this.GetString(sourceBadge, "name"),
                        Description = this.GetString(sourceBadge, "description"),
                        ImageMarkup = this.GetString(sourceBadge, "imageMarkup"),
                        FileId = this.GetInt(sourceBadge, "fileId"),
                        SortOrder = this.GetInt(sourceBadge, "sortOrder"),
                        OneTimeAward = this.GetBool(sourceBadge, "oneTimeAward"),
                        BadgeMetric = (Enums.BadgeMetric)this.GetInt(sourceBadge, "badgeMetric"),
                        Threshold = this.GetInt(sourceBadge, "threshold"),
                        IntervalDays = this.GetInt(sourceBadge, "intervalDays"),
                        SendAwardNotification = this.GetBool(sourceBadge, "sendAwardNotification"),
                        InitialBackfillCompletedDate = this.GetNullableDateTime(sourceBadge, "initialBackfillCompletedDate"),
                        SuppresssAwardNotificationOnBackfill = this.GetBool(sourceBadge, "suppressAwardNotificationOnBackfill"),
                    };

                    ((IRepository<BadgeInfo>)BadgeController.Instance).Insert(badge);
                    badgeMap[this.GetInt(sourceBadge, "badgeId")] = badge.BadgeId;
                }

                foreach (var sourceForum in TopicsController.GetElements(root, "forums", "forum"))
                {
                    var oldForumId = this.GetInt(sourceForum, "forumId");
                    var oldParentForumId = this.GetInt(sourceForum, "parentForumId");
                    var forum = new ForumInfo
                    {
                        PortalId = portalId,
                        ModuleId = moduleId,
                        ForumGroupId = groupMap.TryGetValue(this.GetInt(sourceForum, "forumGroupId"), out var newForumGroupId) ? newForumGroupId : 0,
                        ParentForumId = 0,
                        ForumName = this.GetString(sourceForum, "forumName"),
                        ForumDesc = this.GetString(sourceForum, "forumDesc"),
                        SortOrder = this.GetInt(sourceForum, "sortOrder"),
                        Active = this.GetBool(sourceForum, "active"),
                        Hidden = this.GetBool(sourceForum, "hidden"),
                        TotalTopics = this.GetInt(sourceForum, "totalTopics"),
                        TotalReplies = this.GetInt(sourceForum, "totalReplies"),
                        ForumSettingsKey = defaultSettingsKey,
                        DateCreated = this.GetDateTime(sourceForum, "dateCreated", DateTime.UtcNow),
                        DateUpdated = this.GetDateTime(sourceForum, "dateUpdated", DateTime.UtcNow),
                        LastTopicId = 0,
                        LastReplyId = 0,
                        PermissionsId = defaultPermissionId,
                        PrefixURL = this.GetString(sourceForum, "prefixUrl"),
                        SocialGroupId = this.GetInt(sourceForum, "socialGroupId"),
                        HasProperties = this.GetBool(sourceForum, "hasProperties"),
                    };

                    ((IRepository<ForumInfo>)ForumController.Instance).Insert(forum);
                    forumMap[oldForumId] = forum.ForumID;
                    importedForums[oldForumId] = forum;
                    pendingForumParents[oldForumId] = oldParentForumId;
                }

                foreach (var sourceCategory in TopicsController.GetElements(root, "categories", "category"))
                {
                    var oldCategoryId = this.GetInt(sourceCategory, "categoryId");
                    var category = new CategoryInfo
                    {
                        PortalId = portalId,
                        ModuleId = moduleId,
                        CategoryName = this.GetString(sourceCategory, "categoryName"),
                        Clicks = this.GetInt(sourceCategory, "clicks"),
                        Items = this.GetInt(sourceCategory, "items"),
                        Priority = this.GetInt(sourceCategory, "priority"),
                        ForumId = forumMap.TryGetValue(this.GetInt(sourceCategory, "forumId"), out var newForumId) ? newForumId : 0,
                        ForumGroupId = groupMap.TryGetValue(this.GetInt(sourceCategory, "forumGroupId"), out var newForumGroupId) ? newForumGroupId : 0,
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

                foreach (var sourceContent in TopicsController.GetElements(root, "contents", "content"))
                {
                    var authorId = this.GetInt(sourceContent, "authorId");
                    var importedContent = new ContentInfo
                    {
                        Subject = this.GetString(sourceContent, "subject"),
                        Summary = this.GetString(sourceContent, "summary"),
                        Body = this.GetString(sourceContent, "body"),
                        DateCreated = this.GetDateTime(sourceContent, "dateCreated", DateTime.UtcNow),
                        DateUpdated = this.GetDateTime(sourceContent, "dateUpdated", DateTime.UtcNow),
                        AuthorId = this.ResolveAuthorId(portalId, authorId, userId),
                        AuthorName = this.GetString(sourceContent, "authorName"),
                        IsDeleted = this.GetBool(sourceContent, "isDeleted"),
                        IPAddress = this.GetString(sourceContent, "ipAddress"),
                        ContentItemId = this.GetInt(sourceContent, "contentItemId"),
                        ModuleId = moduleId,
                    };

                    ((IRepository<ContentInfo>)ContentController.Instance).Insert(importedContent);
                    contentMap[this.GetInt(sourceContent, "contentId")] = importedContent.ContentId;
                }

                foreach (var sourceTopic in TopicsController.GetElements(root, "topics", "topic"))
                {
                    if (!contentMap.TryGetValue(this.GetInt(sourceTopic, "contentId"), out var newContentId))
                    {
                        continue;
                    }

                    var topic = new TopicInfo
                    {
                        ContentId = newContentId,
                        ViewCount = this.GetInt(sourceTopic, "viewCount"),
                        ReplyCount = this.GetInt(sourceTopic, "replyCount"),
                        IsLocked = this.GetBool(sourceTopic, "isLocked"),
                        IsPinned = this.GetBool(sourceTopic, "isPinned"),
                        TopicIcon = this.GetString(sourceTopic, "topicIcon"),
                        StatusId = this.GetInt(sourceTopic, "statusId"),
                        IsApproved = this.GetBool(sourceTopic, "isApproved"),
                        IsRejected = this.GetBool(sourceTopic, "isRejected"),
                        IsDeleted = this.GetBool(sourceTopic, "isDeleted"),
                        IsAnnounce = this.GetBool(sourceTopic, "isAnnounce"),
                        IsArchived = this.GetBool(sourceTopic, "isArchived"),
                        AnnounceStart = this.GetNullableDateTime(sourceTopic, "announceStart"),
                        AnnounceEnd = this.GetNullableDateTime(sourceTopic, "announceEnd"),
                        TopicType = (TopicTypes)this.GetInt(sourceTopic, "topicType"),
                        Priority = this.GetInt(sourceTopic, "priority"),
                        TopicUrl = this.GetString(sourceTopic, "topicUrl"),
                        PrevTopic = this.GetInt(sourceTopic, "prevTopic"),
                        NextTopic = this.GetInt(sourceTopic, "nextTopic"),
                        TopicData = this.GetString(sourceTopic, "topicData"),
                    };

                    ((IRepository<TopicInfo>)TopicController.Instance).Insert(topic);
                    topicMap[this.GetInt(sourceTopic, "topicId")] = topic.TopicId;
                }

                foreach (var sourceAttachment in TopicsController.GetElements(root, "attachments", "attachment"))
                {
                    if (!contentMap.TryGetValue(this.GetInt(sourceAttachment, "contentId"), out var newContentId))
                    {
                        continue;
                    }

                    DotNetNuke.Modules.ActiveForums.Controllers.AttachmentController.Instance.Insert(new AttachmentInfo
                    {
                        ContentId = newContentId,
                        UserId = this.ResolveAuthorId(portalId, this.GetInt(sourceAttachment, "userId"), userId),
                        FileName = this.GetString(sourceAttachment, "fileName"),
                        FileId = this.GetNullableInt(sourceAttachment, "fileId"),
                        ContentType = this.GetString(sourceAttachment, "contentType"),
                        DateAdded = this.GetNullableDateTime(sourceAttachment, "dateAdded"),
                        DateUpdated = this.GetNullableDateTime(sourceAttachment, "dateUpdated"),
                        FileSize = this.GetLong(sourceAttachment, "fileSize"),
                        DisplayInline = this.GetBool(sourceAttachment, "displayInline"),
                    });
                }

                foreach (var sourceLike in TopicsController.GetElements(root, "likes", "like"))
                {
                    if (!contentMap.TryGetValue(this.GetInt(sourceLike, "postId"), out var newPostId))
                    {
                        continue;
                    }

                    var resolvedUserId = this.ResolveUserIdOrDefault(portalId, this.GetInt(sourceLike, "userId"));
                    if (!resolvedUserId.HasValue)
                    {
                        continue;
                    }

                    ((IRepository<LikeInfo>)LikeController.Instance).Insert(new LikeInfo
                    {
                        PostId = newPostId,
                        UserId = resolvedUserId.Value,
                        Checked = this.GetBool(sourceLike, "checked"),
                        DateCreated = this.GetDateTime(sourceLike, "dateCreated", DateTime.UtcNow),
                    });
                }

                foreach (var sourceUserMention in TopicsController.GetElements(root, "userMentions", "userMention"))
                {
                    if (!contentMap.TryGetValue(this.GetInt(sourceUserMention, "contentId"), out var newMentionContentId))
                    {
                        continue;
                    }

                    var resolvedUserId = this.ResolveUserIdOrDefault(portalId, this.GetInt(sourceUserMention, "userId"));
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
                        DateMentioned = this.GetDateTime(sourceUserMention, "dateMentioned", DateTime.UtcNow),
                    });
                }

                foreach (var sourceReply in TopicsController.GetElements(root, "replies", "reply"))
                {
                    if (!topicMap.TryGetValue(this.GetInt(sourceReply, "topicId"), out var newTopicId))
                    {
                        continue;
                    }

                    if (!contentMap.TryGetValue(this.GetInt(sourceReply, "contentId"), out var newContentId))
                    {
                        continue;
                    }

                    var oldReplyId = this.GetInt(sourceReply, "replyId");
                    var oldReplyToId = this.GetInt(sourceReply, "replyToId");
                    var reply = new ReplyInfo
                    {
                        TopicId = newTopicId,
                        ReplyToId = 0,
                        ContentId = newContentId,
                        IsApproved = this.GetBool(sourceReply, "isApproved"),
                        IsRejected = this.GetBool(sourceReply, "isRejected"),
                        StatusId = this.GetInt(sourceReply, "statusId"),
                        IsDeleted = this.GetBool(sourceReply, "isDeleted"),
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

                foreach (var sourceForumTopic in TopicsController.GetElements(root, "forumTopics", "forumTopic"))
                {
                    if (!forumMap.TryGetValue(this.GetInt(sourceForumTopic, "forumId"), out var newForumId))
                    {
                        continue;
                    }

                    if (!topicMap.TryGetValue(this.GetInt(sourceForumTopic, "topicId"), out var newTopicId))
                    {
                        continue;
                    }

                    var forumTopic = new ForumTopicInfo
                    {
                        ForumId = newForumId,
                        TopicId = newTopicId,
                        LastReplyId = null,
                    };

                    var oldLastReplyId = this.GetNullableInt(sourceForumTopic, "lastReplyId");
                    if (oldLastReplyId.HasValue && replyMap.TryGetValue(oldLastReplyId.Value, out var newLastReplyId))
                    {
                        forumTopic.LastReplyId = newLastReplyId;
                    }

                    ((IRepository<ForumTopicInfo>)ForumTopicController.Instance).Insert(forumTopic);
                }

                foreach (var sourceTopicTag in TopicsController.GetElements(root, "topicTags", "topicTag"))
                {
                    if (!topicMap.TryGetValue(this.GetInt(sourceTopicTag, "topicId"), out var newTopicId))
                    {
                        continue;
                    }

                    if (!tagMap.TryGetValue(this.GetInt(sourceTopicTag, "tagId"), out var newTagId))
                    {
                        continue;
                    }

                    ((IRepository<TopicTagInfo>)TopicTagController.Instance).Insert(new TopicTagInfo
                    {
                        TopicId = newTopicId,
                        TagId = newTagId,
                    });
                }

                foreach (var sourceTopicCategory in TopicsController.GetElements(root, "topicCategories", "topicCategory"))
                {
                    if (!topicMap.TryGetValue(this.GetInt(sourceTopicCategory, "topicId"), out var newTopicId))
                    {
                        continue;
                    }

                    if (!categoryMap.TryGetValue(this.GetInt(sourceTopicCategory, "categoryId"), out var newCategoryId))
                    {
                        continue;
                    }

                    ((IRepository<TopicCategoryInfo>)TopicCategoryController.Instance).Insert(new TopicCategoryInfo
                    {
                        TopicId = newTopicId,
                        CategoryId = newCategoryId,
                    });
                }

                foreach (var sourceTopicRating in TopicsController.GetElements(root, "topicRatings", "topicRating"))
                {
                    if (!topicMap.TryGetValue(this.GetInt(sourceTopicRating, "topicId"), out var newTopicId))
                    {
                        continue;
                    }

                    var resolvedUserId = this.ResolveUserIdOrDefault(portalId, this.GetInt(sourceTopicRating, "userId"));
                    if (!resolvedUserId.HasValue)
                    {
                        continue;
                    }

                    ((IRepository<TopicRatingInfo>)TopicRatingController.Instance).Insert(new TopicRatingInfo
                    {
                        TopicId = newTopicId,
                        UserId = resolvedUserId.Value,
                        Rating = this.GetInt(sourceTopicRating, "rating"),
                        Helpful = this.GetBool(sourceTopicRating, "helpful"),
                        Comments = this.GetString(sourceTopicRating, "comments"),
                        IPAddress = this.GetString(sourceTopicRating, "ipAddress"),
                        DateAdded = this.GetDateTime(sourceTopicRating, "dateAdded", DateTime.UtcNow),
                        DateUpdated = this.GetDateTime(sourceTopicRating, "dateUpdated", DateTime.UtcNow),
                    });
                }

                foreach (var sourceTopicTracking in TopicsController.GetElements(root, "topicTracking", "topicTracking"))
                {
                    if (!topicMap.TryGetValue(this.GetInt(sourceTopicTracking, "topicId"), out var newTopicId))
                    {
                        continue;
                    }

                    if (!forumMap.TryGetValue(this.GetInt(sourceTopicTracking, "forumId"), out var newTrackingForumId))
                    {
                        continue;
                    }

                    var resolvedUserId = this.ResolveUserIdOrDefault(portalId, this.GetInt(sourceTopicTracking, "userId"));
                    if (!resolvedUserId.HasValue)
                    {
                        continue;
                    }

                    ((IRepository<TopicTrackingInfo>)TopicTrackingController.Instance).Insert(new TopicTrackingInfo
                    {
                        ForumId = newTrackingForumId,
                        TopicId = newTopicId,
                        LastReplyId = replyMap.TryGetValue(this.GetInt(sourceTopicTracking, "lastReplyId"), out var newLastReplyId) ? newLastReplyId : 0,
                        UserId = resolvedUserId.Value,
                        DateAdded = this.GetDateTime(sourceTopicTracking, "dateAdded", DateTime.UtcNow),
                    });
                }

                foreach (var sourceSubscription in TopicsController.GetElements(root, "subscriptions", "subscription"))
                {
                    var resolvedUserId = this.ResolveUserIdOrDefault(portalId, this.GetInt(sourceSubscription, "userId"));
                    if (!resolvedUserId.HasValue)
                    {
                        continue;
                    }

                    var oldForumId = this.GetInt(sourceSubscription, "forumId");
                    var oldTopicId = this.GetInt(sourceSubscription, "topicId");
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
                        Mode = this.GetInt(sourceSubscription, "mode"),
                        UserId = resolvedUserId.Value,
                    });
                }

                foreach (var sourceArchivedUrl in TopicsController.GetElements(root, "archivedUrls", "archivedUrl"))
                {
                    var oldForumId = this.GetInt(sourceArchivedUrl, "forumId");
                    var oldTopicId = this.GetInt(sourceArchivedUrl, "topicId");
                    var oldForumGroupId = this.GetInt(sourceArchivedUrl, "forumGroupId");
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
                        Url = this.GetString(sourceArchivedUrl, "url"),
                        PortalId = portalId,
                        ForumId = oldForumId > 0 ? newArchivedForumId : 0,
                        TopicId = oldTopicId > 0 ? newArchivedTopicId : 0,
                        ForumGroupId = oldForumGroupId > 0 ? newArchivedForumGroupId : 0,
                        UrlHash = this.GetBytes(sourceArchivedUrl, "urlHash"),
                    });
                }

                foreach (var sourceForum in TopicsController.GetElements(root, "forums", "forum"))
                {
                    var oldForumId = this.GetInt(sourceForum, "forumId");
                    if (!forumMap.TryGetValue(oldForumId, out var newForumId))
                    {
                        continue;
                    }

                    var forum = ((IRepository<ForumInfo>)ForumController.Instance).GetById(newForumId, moduleId);
                    if (forum == null)
                    {
                        continue;
                    }

                    var oldLastTopicId = this.GetInt(sourceForum, "lastTopicId");
                    var oldLastReplyId = this.GetInt(sourceForum, "lastReplyId");

                    forum.LastTopicId = topicMap.TryGetValue(oldLastTopicId, out var newLastTopicId) ? newLastTopicId : 0;
                    forum.LastReplyId = replyMap.TryGetValue(oldLastReplyId, out var newLastReplyId) ? newLastReplyId : 0;
                    ((IRepository<ForumInfo>)ForumController.Instance).Update(forum);
                }

                foreach (var sourceUserProfile in TopicsController.GetElements(root, "userProfiles", "userProfile"))
                {
                    var resolvedUserId = this.ResolveUserIdOrDefault(portalId, this.GetInt(sourceUserProfile, "userId"));
                    if (!resolvedUserId.HasValue || resolvedUserId.Value <= 0)
                    {
                        continue;
                    }

                    var forumUser = ForumUserController.Instance.GetByUserId(portalId, moduleId, resolvedUserId.Value);
                    if (forumUser == null)
                    {
                        continue;
                    }

                    forumUser.TopicCount = this.GetInt(sourceUserProfile, "topicCount");
                    forumUser.ReplyCount = this.GetInt(sourceUserProfile, "replyCount");
                    forumUser.ViewCount = this.GetInt(sourceUserProfile, "viewCount");
                    forumUser.AnswerCount = this.GetInt(sourceUserProfile, "answerCount");
                    forumUser.RewardPoints = this.GetInt(sourceUserProfile, "rewardPoints");
                    forumUser.UserCaption = this.GetString(sourceUserProfile, "userCaption");
                    forumUser.AvatarLastRefresh = this.GetNullableDateTime(sourceUserProfile, "avatarLastRefresh");
                    forumUser.AvatarSourceLastModified = this.GetNullableDateTime(sourceUserProfile, "avatarSourceLastModified");
                    forumUser.AvatarFileId = this.GetNullableInt(sourceUserProfile, "avatarFileId");
                    forumUser.DateCreated = this.GetDateTime(sourceUserProfile, "dateCreated", forumUser.DateCreated);
                    forumUser.DateUpdated = this.GetNullableDateTime(sourceUserProfile, "dateUpdated");
                    forumUser.DateLastActivity = this.GetNullableDateTime(sourceUserProfile, "dateLastActivity");
                    forumUser.DateLastPost = this.GetNullableDateTime(sourceUserProfile, "dateLastPost");
                    forumUser.DateLastReply = this.GetNullableDateTime(sourceUserProfile, "dateLastReply");
                    forumUser.Signature = this.GetString(sourceUserProfile, "signature");
                    forumUser.SignatureDisabled = this.GetBool(sourceUserProfile, "signatureDisabled");
                    forumUser.TrustLevel = this.GetInt(sourceUserProfile, "trustLevel");
                    forumUser.AdminWatch = this.GetBool(sourceUserProfile, "adminWatch");
                    forumUser.AttachDisabled = this.GetBool(sourceUserProfile, "attachDisabled");
                    forumUser.AvatarDisabled = this.GetBool(sourceUserProfile, "avatarDisabled");
                    forumUser.PrefDefaultSort = this.GetString(sourceUserProfile, "prefDefaultSort");
                    forumUser.PrefDefaultShowReplies = this.GetBool(sourceUserProfile, "prefDefaultShowReplies");
                    forumUser.PrefJumpLastPost = this.GetBool(sourceUserProfile, "prefJumpLastPost");
                    forumUser.PrefTopicSubscribe = this.GetBool(sourceUserProfile, "prefTopicSubscribe");
                    forumUser.PrefSubscriptionType = (SubscriptionTypes)this.GetInt(sourceUserProfile, "prefSubscriptionType");
                    forumUser.PrefBlockAvatars = this.GetBool(sourceUserProfile, "prefBlockAvatars");
                    forumUser.PrefBlockSignatures = this.GetBool(sourceUserProfile, "prefBlockSignatures");
                    forumUser.PrefPageSize = this.GetInt(sourceUserProfile, "prefPageSize");
                    forumUser.LikeNotificationsEnabled = this.GetBool(sourceUserProfile, "likeNotificationsEnabled");
                    forumUser.PinNotificationsEnabled = this.GetBool(sourceUserProfile, "pinNotificationsEnabled");
                    forumUser.EnableNotificationsForOwnContent = this.GetBool(sourceUserProfile, "enableNotificationsForOwnContent");
                    forumUser.BadgeNotificationsEnabled = this.GetBool(sourceUserProfile, "badgeNotificationsEnabled");
                    forumUser.UserMentionNotificationsEnabled = this.GetBool(sourceUserProfile, "userMentionNotificationsEnabled");
                    ((IRepository<ForumUserInfo>)ForumUserController.Instance).Update(forumUser);
                }

                foreach (var sourceForumTracking in TopicsController.GetElements(root, "forumTracking", "forumTracking"))
                {
                    if (!forumMap.TryGetValue(this.GetInt(sourceForumTracking, "forumId"), out var newForumId))
                    {
                        continue;
                    }

                    var resolvedUserId = this.ResolveUserIdOrDefault(portalId, this.GetInt(sourceForumTracking, "userId"));
                    if (!resolvedUserId.HasValue)
                    {
                        continue;
                    }

                    ((IRepository<ForumTrackingInfo>)ForumTrackingController.Instance).Insert(new ForumTrackingInfo
                    {
                        ModuleId = moduleId,
                        UserId = resolvedUserId.Value,
                        ForumId = newForumId,
                        LastAccessDateTime = this.GetDateTime(sourceForumTracking, "lastAccessDateTime", DateTime.UtcNow),
                        MaxTopicRead = topicMap.TryGetValue(this.GetInt(sourceForumTracking, "maxTopicRead"), out var newMaxTopicRead) ? newMaxTopicRead : 0,
                        MaxReplyRead = replyMap.TryGetValue(this.GetInt(sourceForumTracking, "maxReplyRead"), out var newMaxReplyRead) ? newMaxReplyRead : 0,
                    });
                }

                foreach (var sourceUserBadge in TopicsController.GetElements(root, "userBadges", "userBadge"))
                {
                    if (!badgeMap.TryGetValue(this.GetInt(sourceUserBadge, "badgeId"), out var newBadgeId))
                    {
                        continue;
                    }

                    var resolvedUserId = this.ResolveUserIdOrDefault(portalId, this.GetInt(sourceUserBadge, "userId"));
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
                        DateAssigned = this.GetDateTime(sourceUserBadge, "dateAssigned", DateTime.UtcNow),
                    });
                }

                SettingsCache.ClearAll(moduleId);
            }
            catch (Exception ex)
            {
                this.LogError(ex.Message, ex);
                Exceptions.LogException(ex);
            }
        }

        internal static XContainer SerializeEntities<T>(string containerName, IEnumerable<T> items, Func<T, XElement> elementFactory)
        {
            var container = new XElement(containerName);
            foreach (var item in items)
            {
                container.Add(elementFactory(item));
            }

            return container;
        }

        private static IEnumerable<XElement> GetElements(XElement root, string containerName, string elementName)
        {
            return root.Element(containerName)?.Elements(elementName) ?? Enumerable.Empty<XElement>();
        }

        private int ResolveAuthorId(int portalId, int sourceAuthorId, int importingUserId)
        {
            if (sourceAuthorId < 1)
            {
                return importingUserId;
            }

            return UserController.Instance.GetUserById(portalId, sourceAuthorId) != null ? sourceAuthorId : importingUserId;
        }

        private int? ResolveUserIdOrDefault(int portalId, int sourceUserId)
        {
            if (sourceUserId < 1)
            {
                return sourceUserId;
            }

            return UserController.Instance.GetUserById(portalId, sourceUserId) != null ? sourceUserId : (int?)null;
        }

        private int GetInt(XElement element, string attributeName)
        {
            if (int.TryParse(element?.Attribute(attributeName)?.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
            {
                return value;
            }

            return 0;
        }

        private int? GetNullableInt(XElement element, string attributeName)
        {
            var value = element?.Attribute(attributeName)?.Value;
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : (int?)null;
        }

        private long GetLong(XElement element, string attributeName)
        {
            return long.TryParse(element?.Attribute(attributeName)?.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : 0L;
        }

        private string GetString(XElement element, string attributeName)
        {
            return element?.Attribute(attributeName)?.Value ?? string.Empty;
        }

        private bool GetBool(XElement element, string attributeName)
        {
            if (bool.TryParse(element?.Attribute(attributeName)?.Value, out var value))
            {
                return value;
            }

            return false;
        }

        private DateTime GetDateTime(XElement element, string attributeName, DateTime defaultValue)
        {
            if (DateTime.TryParse(element?.Attribute(attributeName)?.Value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var value))
            {
                return value;
            }

            return defaultValue;
        }

        private DateTime? GetNullableDateTime(XElement element, string attributeName)
        {
            var value = element?.Attribute(attributeName)?.Value;
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsed) ? parsed : (DateTime?)null;
        }

        private string ToPortableDate(DateTime? value)
        {
            return value.HasValue ? value.Value.ToString("o", CultureInfo.InvariantCulture) : string.Empty;
        }

        private byte[] GetBytes(XElement element, string attributeName)
        {
            var value = element?.Attribute(attributeName)?.Value;
            return string.IsNullOrWhiteSpace(value) ? null : Convert.FromBase64String(value);
        }

        private string ToPortableBytes(byte[] value)
        {
            return value != null && value.Length > 0 ? Convert.ToBase64String(value) : string.Empty;
        }

        private class GroupPortable
        {
            public int ForumGroupId { get; set; }

            public string GroupName { get; set; }

            public int SortOrder { get; set; }

            public bool Active { get; set; }

            public bool Hidden { get; set; }

            public string GroupSettingsKey { get; set; }

            public int PermissionsId { get; set; }

            public string PrefixURL { get; set; }
        }

        private class TagPortable
        {
            public int TagId { get; set; }

            public int PortalId { get; set; }

            public int ModuleId { get; set; }

            public string TagName { get; set; }

            public int Items { get; set; }
        }

        private class ForumPortable
        {
            public int ForumId { get; set; }

            public int PortalId { get; set; }

            public int ModuleId { get; set; }

            public int ForumGroupId { get; set; }

            public int ParentForumId { get; set; }

            public string ForumName { get; set; }

            public string ForumDesc { get; set; }

            public int SortOrder { get; set; }

            public bool Active { get; set; }

            public bool Hidden { get; set; }

            public int TotalTopics { get; set; }

            public int TotalReplies { get; set; }

            public string ForumSettingsKey { get; set; }

            public DateTime DateCreated { get; set; }

            public DateTime DateUpdated { get; set; }

            public int LastTopicId { get; set; }

            public int LastReplyId { get; set; }

            public int PermissionsId { get; set; }

            public string PrefixURL { get; set; }

            public int SocialGroupId { get; set; }

            public bool HasProperties { get; set; }
        }

        private class ContentPortable
        {
            public int ContentId { get; set; }

            public string Subject { get; set; }

            public string Summary { get; set; }

            public string Body { get; set; }

            public DateTime DateCreated { get; set; }

            public DateTime DateUpdated { get; set; }

            public int AuthorId { get; set; }

            public string AuthorName { get; set; }

            public bool IsDeleted { get; set; }

            public string IPAddress { get; set; }

            public int ContentItemId { get; set; }

            public int ModuleId { get; set; }
        }

        private class TopicPortable
        {
            public int TopicId { get; set; }

            public int ContentId { get; set; }

            public int ViewCount { get; set; }

            public int ReplyCount { get; set; }

            public bool IsLocked { get; set; }

            public bool IsPinned { get; set; }

            public string TopicIcon { get; set; }

            public int StatusId { get; set; }

            public bool IsApproved { get; set; }

            public bool IsRejected { get; set; }

            public bool IsDeleted { get; set; }

            public bool IsAnnounce { get; set; }

            public bool IsArchived { get; set; }

            public DateTime? AnnounceStart { get; set; }

            public DateTime? AnnounceEnd { get; set; }

            public int TopicType { get; set; }

            public int Priority { get; set; }

            public string TopicUrl { get; set; }

            public int PrevTopic { get; set; }

            public int NextTopic { get; set; }

            public string TopicData { get; set; }
        }

        private class ReplyPortable
        {
            public int ReplyId { get; set; }

            public int TopicId { get; set; }

            public int ReplyToId { get; set; }

            public int ContentId { get; set; }

            public bool IsApproved { get; set; }

            public bool IsRejected { get; set; }

            public int StatusId { get; set; }

            public bool IsDeleted { get; set; }
        }

        private class ForumTopicPortable
        {
            public int ForumTopicId { get; set; }

            public int ForumId { get; set; }

            public int TopicId { get; set; }

            public int? LastReplyId { get; set; }
        }

        private class TopicTagPortable
        {
            public int TopicTagId { get; set; }

            public int TopicId { get; set; }

            public int TagId { get; set; }
        }
        #endregion

        #region "IUpgradeable"
        public string UpgradeModule(string Version)
        {
            switch (Version)
            {
                case "07.00.07":
                    try
                    {
                        var fc = new ForumsConfig();
                        fc.ArchiveOrphanedAttachments_070007();
                    }
                    catch (Exception ex)
                    {
                        this.LogError(ex.Message, ex);
                        Exceptions.LogException(ex);
                        return "Failed";
                    }

                    break;
                case "07.00.11":
                    try
                    {
                        DotNetNuke.Modules.ActiveForums.Helpers.Upgrades.MoveSettings_070011();
                    }
                    catch (Exception ex)
                    {
                        this.LogError(ex.Message, ex);
                        Exceptions.LogException(ex);
                        return "Failed";
                    }

                    break;
                case "07.00.12":
                    try
                    {
                        ForumsConfig.FillMissingTopicUrls_070012();
                    }
                    catch (Exception ex)
                    {
                        this.LogError(ex.Message, ex);
                        Exceptions.LogException(ex);
                        return "Failed";
                    }

                    break;
                case "08.00.00":
                    try
                    {
                        DotNetNuke.Modules.ActiveForums.Helpers.Upgrades.Upgrade_Templates_080000();
                        var fc = new ForumsConfig();
                        fc.Install_Or_Upgrade_RenameThemeCssFiles_080000();
                        fc.Install_Or_Upgrade_RelocateDefaultThemeToLegacy_080000();
                        ForumsConfig.FillMissingTopicUrls_070012(); /* for anyone upgrading from 07.00.12-> 08.00.00 */
                    }
                    catch (Exception ex)
                    {
                        this.LogError(ex.Message, ex);
                        Exceptions.LogException(ex);
                        return "Failed";
                    }

                    break;
                case "08.01.00":
                    try
                    {
                        DotNetNuke.Modules.ActiveForums.Helpers.Upgrades.DeleteObsoleteModuleSettings_080100();
                        DotNetNuke.Modules.ActiveForums.Helpers.Upgrades.UpgradeSocialGroupForumConfigModuleSettings_080100();
                        ForumsConfig.Install_BanUser_NotificationType_080100();
                    }
                    catch (Exception ex)
                    {
                        this.LogError(ex.Message, ex);
                        Exceptions.LogException(ex);
                        return "Failed";
                    }

                    break;
                case "08.02.00":
                    try
                    {
                        ForumsConfig.Merge_Permissions_080200();
                        DotNetNuke.Modules.ActiveForums.Helpers.Upgrades.UpgradeSocialGroupForumConfigModuleSettings_080200();
                        DotNetNuke.Modules.ActiveForums.Helpers.Upgrades.Upgrade_EmailNotificationSubjectTokens_080200();
                        ForumsConfig.Upgrade_RelocateSqlFiles_080200();
                        ForumsConfig.Install_Upgrade_CreateForumDefaultSettingsAndSecurity_080200();
                        DotNetNuke.Modules.ActiveForums.Helpers.Upgrades.AddUrlPrefixLikes_080200();
                        ForumsConfig.Install_LikeNotificationType_080200();
                        ForumsConfig.Install_PinNotificationType_080200();
                    }
                    catch (Exception ex)
                    {
                        this.LogError(ex.Message, ex);
                        Exceptions.LogException(ex);
                        return "Failed";
                    }

                    break;
                case "09.00.00":
                    try
                    {
                        var log = new DotNetNuke.Services.Log.EventLog.LogInfo { LogTypeKey = DotNetNuke.Abstractions.Logging.EventLogType.ADMIN_ALERT.ToString() };
                        log.LogProperties.Add(new LogDetailInfo("Module", Globals.ModuleFriendlyName));
                        var message = $"Removing obsolete module settings for {Version}";
                        log.AddProperty("Message", message);
                        DotNetNuke.Services.Log.EventLog.LogController.Instance.AddLog(log);
                        DotNetNuke.Modules.ActiveForums.Helpers.Upgrades.DeleteObsoleteModuleSettings_090000();
                        log = new DotNetNuke.Services.Log.EventLog.LogInfo { LogTypeKey = DotNetNuke.Abstractions.Logging.EventLogType.ADMIN_ALERT.ToString() };
                        log.LogProperties.Add(new LogDetailInfo("Module", Globals.ModuleFriendlyName));
                        message = $"Upgrading permissions for {Version}";
                        log.AddProperty("Message", message);
                        DotNetNuke.Services.Log.EventLog.LogController.Instance.AddLog(log);
                        ForumsConfig.Upgrade_PermissionSets_090000();
                    }
                    catch (Exception ex)
                    {
                        this.LogError(ex.Message, ex);
                        Exceptions.LogException(ex);
                        return "Failed";
                    }

                    break;
                case "09.01.00":
                    try
                    {
                        DotNetNuke.Modules.ActiveForums.Helpers.Upgrades.DeleteObsoleteModuleSettings_090100();
                        DotNetNuke.Modules.ActiveForums.Helpers.Upgrades.AddAvatarModuleSettings_090100();
                    }
                    catch (Exception ex)
                    {
                        this.LogError(ex.Message, ex);
                        Exceptions.LogException(ex);
                        return "Failed";
                    }

                    break;
                case "09.02.00":
                    try
                    {
                        ForumsConfig.Install_BadgeNotificationType_090200();
                    }
                    catch (Exception ex)
                    {
                        this.LogError(ex.Message, ex);
                        Exceptions.LogException(ex);
                        return "Failed";
                    }

                    break;
                case "09.02.01":
                    try
                    {
                        DotNetNuke.Modules.ActiveForums.Helpers.Upgrades.UpgradeSocialGroupForumConfigModuleSettings_090201();
                        new ForumsConfig().Install_DefaultBadges_090201(upgrading: true);
                    }
                    catch (Exception ex)
                    {
                        this.LogError(ex.Message, ex);
                        Exceptions.LogException(ex);
                        return "Failed";
                    }

                    break;
                case "09.03.00":
                    try
                    {
                        ForumsConfig.Install_UserMentionNotificationType_090300();
                        DotNetNuke.Modules.ActiveForums.Helpers.Upgrades.UpgradeSocialGroupForumConfigModuleSettings_090300();
                    }
                    catch (Exception ex)
                    {
                        this.LogError(ex.Message, ex);
                        Exceptions.LogException(ex);
                        return "Failed";
                    }

                    break;
                case "09.05.00":
                    try
                    {
                        ForumsConfig.Reset_DNN_Search_Documents_090500();
                    }
                    catch (Exception ex)
                    {
                        this.LogError(ex.Message, ex);
                        Exceptions.LogException(ex);
                        return "Failed";
                    }

                    break;
                case "09.06.00":
                    try
                    {
                        DotNetNuke.Modules.ActiveForums.Helpers.Upgrades.DeleteObsoleteModuleSettings_090600();
                        ForumsConfig.Upgrade_EnsureVanityNames_090600();
                    }
                    catch (Exception ex)
                    {
                        this.LogError(ex.Message, ex);
                        Exceptions.LogException(ex);
                        return "Failed";
                    }

                    break;
                case "09.07.00":
                    try
                    {
                        var fc = new ForumsConfig();
                        fc.RemoveLegacyAvatarsFolder_090700();
                        fc.RelocateAttachments_090700();
                    }
                    catch (Exception ex)
                    {
                        this.LogError(ex.Message, ex);
                        Exceptions.LogException(ex);
                        return "Failed";
                    }

                    break;
                case "10.00.00":
                    try
                    {
                        DotNetNuke.Modules.ActiveForums.Helpers.Upgrades.DeleteObsoleteModuleSettings_100000();
                        DotNetNuke.Modules.ActiveForums.Helpers.Upgrades.Remove_TemplatesTable_100000();

                        /* ensure permissions are upgraded anyone upgrading from earlier than 09.00.00 to 10.00.00; 
                         * "GroupKey" on settings table was changed to "SettingsKey" in 09.02.00 so the 09.00.00 upgrade task failed for anyone upgrading from earlier than 09.00.00 to 09.02.00->09.08.00;
                         * ALSO handles additional "Mention" permission */
                        DotNetNuke.Modules.ActiveForums.Helpers.Upgrades.Upgrade_PermissionSets_100000();
                    }
                    catch (Exception ex)
                    {
                        this.LogError(ex.Message, ex);
                        Exceptions.LogException(ex);
                        return "Failed";
                    }

                    break;
                default:
                    break;
            }

            return Version;
        }

        private void LogError(string message, Exception ex)
        {
            if (ex != null)
            {
                Logger.Error(message, ex);
                if (ex.InnerException != null)
                {
                    Logger.Error(ex.InnerException.Message, ex.InnerException);
                }
            }
            else
            {
                Logger.Error(message);
            }
        }
        #endregion

    }

    #endregion
}
