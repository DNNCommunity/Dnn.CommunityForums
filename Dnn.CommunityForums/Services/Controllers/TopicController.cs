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

namespace DotNetNuke.Modules.ActiveForums.Services.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Reflection;
    using System.Text;
    using System.Web;
    using System.Web.Http;

    using DotNetNuke.Modules.ActiveForums.Enums;
    using DotNetNuke.Modules.ActiveForums.Extensions;
    using DotNetNuke.Modules.ActiveForums.Services.Cache;
    using DotNetNuke.Modules.ActiveForums.Services.ProcessQueue;
    using DotNetNuke.Web.Api;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public class TopicController : ControllerBase<TopicController>
    {
        public struct TopicDto1
        {
            public int ForumId { get; set; }

            public int TopicId { get; set; }
        }

        public struct TopicDto2
        {
            public int ForumId { get; set; }

            public DotNetNuke.Modules.ActiveForums.ViewModels.Topic Topic { get; set; }
        }

        private sealed class TopicListItem
        {
            public int TopicId { get; set; }

            public string Subject { get; set; }
        }

        /// <summary>
        /// Represents a request to split replies from an existing topic.
        /// </summary>
        public class CreateSplitDto
        {
            /// <summary>
            /// Gets or sets the source forum ID.
            /// </summary>
            public int ForumId { get; set; }

            /// <summary>
            /// Gets or sets the source topic ID.
            /// </summary>
            public int OldTopicId { get; set; }

            /// <summary>
            /// Gets or sets the destination topic ID.
            /// </summary>
            public int NewTopicId { get; set; }

            /// <summary>
            /// Gets or sets the destination forum ID.
            /// </summary>
            public int NewForumId { get; set; }

            /// <summary>
            /// Gets or sets the subject for a newly created topic.
            /// </summary>
            public string Subject { get; set; }

            /// <summary>
            /// Gets or sets the reply IDs being split.
            /// </summary>
            public string Replies { get; set; }
        }

        /// <summary>
        /// Subscribes to a Topic
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Topic/Subscribe</remarks>
        [HttpPost]
        [DnnAuthorize]
        [ForumsAuthorize(SecureActions.Subscribe)]
        public HttpResponseMessage Subscribe(TopicDto1 dto)
        {
            try
            {
                if (dto.TopicId > 0 && dto.ForumId > 0)
                {
                    int subscribed = new SubscriptionController().Subscription_Update(this.ActiveModule.PortalID, this.ForumModuleId, dto.ForumId, dto.TopicId, 1, DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.Instance.GetByUserId(this.ActiveModule.PortalID, this.ForumModuleId, this.UserInfo.UserID));
                    return this.Request.CreateResponse(HttpStatusCode.OK, subscribed == 1);
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }

            return this.Request.CreateResponse(HttpStatusCode.BadRequest);
        }

#pragma warning disable CS1570
        /// <summary>
        /// Gets Subscriber count for a Topic
        /// </summary>
        /// <param name="forumId" type="int"></param>
        /// <param name="topicId" type="int"></param>
        /// <returns></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Topic/SubscriberCount?ForumId=xxx&TopicId=xxx</remarks>
#pragma warning restore CS1570
        [HttpGet]
        [DnnAuthorize]
        [ForumsAuthorize(SecureActions.View)]
        public HttpResponseMessage SubscriberCount(int forumId, int topicId)
        {
            try
            {
                if (forumId > 0 && topicId > 0)
                {
                    return this.Request.CreateResponse(HttpStatusCode.OK,
                        new DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController().Count(
                            this.ActiveModule.PortalID, this.ForumModuleId, forumId, topicId));
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }

            return this.Request.CreateResponse(HttpStatusCode.BadRequest);
        }

#pragma warning disable CS1570
        /// <summary>
        /// Gets Subscriber count string for a Topic
        /// </summary>
        /// <param name="forumId" type="int"></param>
        /// <param name="topicId" type="int"></param>
        /// <returns></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Forum/SubscriberCountString?ForumId=xxx&TopicId=xxx</remarks>
#pragma warning restore CS1570
        [HttpGet]
        [DnnAuthorize]
        [ForumsAuthorize(SecureActions.View)]
        public HttpResponseMessage SubscriberCountString(int forumId, int topicId)
        {
            try
            {
                if (forumId > 0)
                {
                    return this.Request.CreateResponse(HttpStatusCode.OK,
                        $"{new DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController().Count(this.ActiveModule.PortalID, this.ForumModuleId, forumId, topicId)} {Utilities.GetSharedResource("[RESX:TOPICSUBSCRIBERCOUNT]", false)}");
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }

            return this.Request.CreateResponse(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Pins a Topic
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Topic/Pin</remarks>
        [HttpPost]
        [DnnAuthorize]
        [ForumsAuthorize(SecureActions.Pin)]
        public HttpResponseMessage Pin(TopicDto1 dto)
        {
            try
            {
                int topicId = dto.TopicId;
                if (topicId > 0)
                {
                    DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Instance.GetById(this.ForumModuleId, topicId);
                    if (ti != null)
                    {
                        var forumUser = DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.Instance.GetByUserId(this.ActiveModule.PortalID, this.ForumModuleId, this.UserInfo.UserID);
                        if (this.UserInfo.IsAdmin ||
                            this.UserInfo.IsSuperUser ||
                            Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(ti.Forum.Security.ModerateRoleIds, forumUser.UserRoleIds) ||
                            (Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(ti.Forum.Security.PinRoleIds, forumUser.UserRoleIds) && this.UserInfo.UserID == ti.Content.AuthorId)
                            )
                        {
                            ti.IsPinned = !ti.IsPinned;
                            DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Save(ti);
                            if (ti.IsPinned)
                            {
                                DotNetNuke.Modules.ActiveForums.Controllers.ProcessQueueController.Instance.Add(
                                    ProcessType.TopicPinned,
                                    portalId: ti.PortalId,
                                    tabId: ti.Forum.GetTabId(),
                                    moduleId: ti.ModuleId,
                                    forumGroupId: ti.Forum.ForumGroupId,
                                    forumId: ti.ForumId,
                                    topicId: topicId,
                                    replyId: -1,
                                    contentId: ti.ContentId,
                                    authorId: ti.Content.AuthorId,
                                    userId: this.UserInfo.UserID,
                                    badgeId: DotNetNuke.Common.Utilities.Null.NullInteger,
                                    dateCreated: DateTime.UtcNow,
                                    requestUrl: this.Request.RequestUri.ToString());
                            }

                            return this.Request.CreateResponse(HttpStatusCode.OK, value: ti.IsPinned);
                        }

                        return this.Request.CreateResponse(HttpStatusCode.Unauthorized);
                    }

                    return this.Request.CreateResponse(HttpStatusCode.BadRequest);
                }

                return this.Request.CreateResponse(HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }

            return this.Request.CreateResponse(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Locks a Topic
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Topic/Lock</remarks>
        [HttpPost]
        [DnnAuthorize]
        [ForumsAuthorize(SecureActions.Lock)]
        public HttpResponseMessage Lock(TopicDto1 dto)
        {
            try
            {
                int topicId = dto.TopicId;
                if (topicId > 0)
                {
                    DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti =
                        DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Instance.GetById(this.ForumModuleId, topicId);
                    if (ti != null)
                    {
                        var forumUser = DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.Instance.GetByUserId(this.ActiveModule.PortalID, this.ForumModuleId, this.UserInfo.UserID);
                        if (this.UserInfo.IsAdmin ||
                            this.UserInfo.IsSuperUser ||
                            Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(ti.Forum.Security.ModerateRoleIds, forumUser.UserRoleIds) ||
                            (Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(ti.Forum.Security.LockRoleIds, forumUser.UserRoleIds) && this.UserInfo.UserID == ti.Content.AuthorId)
                            )
                        {
                            ti.IsLocked = !ti.IsLocked;
                            DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Save(ti);
                            return this.Request.CreateResponse(HttpStatusCode.OK, ti.IsLocked);
                        }

                        return this.Request.CreateResponse(HttpStatusCode.Unauthorized);
                    }

                    return this.Request.CreateResponse(HttpStatusCode.BadRequest);
                }

                return this.Request.CreateResponse(HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }

            return this.Request.CreateResponse(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Moves a Topic
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Topic/Move</remarks>
        [HttpPost]
        [DnnAuthorize]
        [ForumsAuthorize(SecureActions.Move)]
        public HttpResponseMessage Move(TopicDto1 dto)
        {
            try
            {
                int topicId = dto.TopicId;
                int forumId = dto.ForumId;
                if (topicId > 0 && forumId > 0)
                {
                    DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Instance.GetById(this.ForumModuleId, topicId);
                    if (ti != null)
                    {
                        var forumUser = DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.Instance.GetByUserId(this.ActiveModule.PortalID, this.ForumModuleId, this.UserInfo.UserID);
                        if (this.UserInfo.IsAdmin ||
                            this.UserInfo.IsSuperUser ||
                            Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(ti.Forum.Security.ModerateRoleIds, forumUser.UserRoleIds) ||
                            (Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(ti.Forum.Security.MoveRoleIds, forumUser.UserRoleIds) && this.UserInfo.UserID == ti.Content.AuthorId)
                            )
                        {
                            DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Move(moduleId: this.ForumModuleId, userId: this.UserInfo.UserID, topicId: topicId, newForumId: forumId);

                            DotNetNuke.Modules.ActiveForums.Services.Cache.CacheBase.CacheClearPrefix(string.Format(CacheKeys.CacheModulePrefix, this.ForumModuleId));
                            return this.Request.CreateResponse(HttpStatusCode.OK, string.Empty);
                        }

                        return this.Request.CreateResponse(HttpStatusCode.Unauthorized);
                    }

                    return this.Request.CreateResponse(HttpStatusCode.BadRequest);
                }

                return this.Request.CreateResponse(HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }

            return this.Request.CreateResponse(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Gets a topic list for a forum.
        /// </summary>
        /// <param name="forumId">Forum ID.</param>
        /// <returns>Topic ID / subject pairs serialized as JSON.</returns>
        [HttpGet]
        [DnnAuthorize]
        [ForumsAuthorize(SecureActions.View)]
        public HttpResponseMessage GetTopicList(int forumId)
        {
            using var ctx = DotNetNuke.Data.DataContext.Instance();
            var topics = ctx.ExecuteQuery<TopicListItem>(
                System.Data.CommandType.Text,
                $@"SELECT TOP 20
                        t.TopicId,
                        t.Subject
                   FROM {{databaseOwner}}[{{objectQualifier}}vw_communityforums_TopicsView] t
                   WHERE t.ForumId = @0
                   ORDER BY t.IsPinned DESC, t.Priority DESC, COALESCE(t.LastReplyDate, t.DateCreated) DESC",
                forumId).ToList();
            if (topics.Count > 0)
            {
                Dictionary<string, string> rows = new Dictionary<string, string>();
                foreach (var topic in topics)
                {
                    rows.Add(topic.TopicId.ToString(), topic.Subject);
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, rows);
            }

            return this.Request.CreateResponse(HttpStatusCode.NotFound);
        }

        /// <summary>
        /// Splits replies from one topic into another topic.
        /// </summary>
        /// <param name="dto">Split request.</param>
        /// <returns>The target topic ID.</returns>
        [HttpPost]
        [DnnAuthorize]
        [ForumsAuthorize(SecureActions.Split)]
        public HttpResponseMessage CreateSplit(CreateSplitDto dto)
        {
            if (dto.NewTopicId == dto.OldTopicId)
            {
                return this.Request.CreateResponse(HttpStatusCode.OK);
            }

            var portalSettings = this.PortalSettings;
            var userInfo = portalSettings.UserInfo;
            var forumUser = DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.Instance.GetByUserId(this.ActiveModule.PortalID, this.ActiveModule.ModuleID, userInfo.UserID);

            var oldForum = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Forums_Get(portalSettings.PortalId, this.ActiveModule.ModuleID, 0, true, dto.OldTopicId);
            var newForum = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Instance.GetById(this.ActiveModule.ModuleID, dto.NewForumId);
            if (oldForum != null && newForum != null && oldForum.ForumID == dto.ForumId)
            {
                var ti = DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Instance.GetById(this.ActiveModule.ModuleID, dto.OldTopicId);
                if (ti != null)
                {
                    bool hasCreatePerm;

                    if (oldForum == newForum)
                    {
                        hasCreatePerm = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(oldForum.Security.CreateRoleIds, forumUser.UserRoleIds);
                    }
                    else
                    {
                        hasCreatePerm = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(oldForum.Security.CreateRoleIds, forumUser.UserRoleIds) && DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(newForum.Security.CreateRoleIds, forumUser.UserRoleIds);
                    }

                    var canSplit = (ti.Content.AuthorId == userInfo.UserID && DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(oldForum.Security.SplitRoleIds, forumUser.UserRoleIds)) || userInfo.IsAdmin || userInfo.IsSuperUser ||
                                   (DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(oldForum.Security.ModerateRoleIds, forumUser.UserRoleIds) && DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(oldForum.Security.SplitRoleIds, forumUser.UserRoleIds));

                    if (hasCreatePerm && canSplit)
                    {
                        int topicId;

                        if (dto.NewTopicId < 1)
                        {
                            var subject = Utilities.CleanString(portalSettings.PortalId, dto.Subject, false, EditorType.TEXTBOX, false, false, this.ActiveModule.ModuleID, string.Empty, false);
                            var replies = dto.Replies.Split('|');
                            var firstReply = DotNetNuke.Modules.ActiveForums.Controllers.ReplyController.Instance.GetById(this.ActiveModule.ModuleID, Convert.ToInt32(replies[0]));
                            var firstContent = DotNetNuke.Modules.ActiveForums.Controllers.ContentController.Instance.GetById(this.ActiveModule.ModuleID, firstReply.ContentId);
                            topicId = DotNetNuke.Modules.ActiveForums.Controllers.TopicController.QuickCreate(portalSettings.PortalId, this.ActiveModule.ModuleID, dto.NewForumId, subject, string.Empty, firstContent.AuthorId, firstContent.AuthorName, true, this.Request.GetIPAddress());
                            DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Replies_Split(this.ActiveModule.ModuleID, oldForum.ForumID, newForum.ForumID, dto.OldTopicId, topicId, dto.Replies, true);
                        }
                        else
                        {
                            topicId = dto.NewTopicId;
                            DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Replies_Split(this.ActiveModule.ModuleID, oldForum.ForumID, newForum.ForumID, dto.OldTopicId, topicId, dto.Replies, false);
                        }

                        return this.Request.CreateResponse(HttpStatusCode.OK, topicId);
                    }

                    return this.Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
            }

            return this.Request.CreateResponse(HttpStatusCode.BadRequest);
        }

#pragma warning disable CS1570
        /// <summary>
        /// Loads a Topic
        /// <param name="forumId" type="int"></param>
        /// <param name="topicId" type="int"></param>
        /// <returns name="Topic" type="DotNetNuke.Modules.ActiveForums.ViewModels.Topic"></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Topic/Load?ForumId=xxx&TopicId=xxx</remarks>
#pragma warning restore CS1570
        [HttpGet]
        [DnnAuthorize]
        [ForumsAuthorize(SecureActions.Read)]
        public HttpResponseMessage Load(int forumId, int topicId)
        {
            try
            {
                if (topicId > 0 && forumId > 0)
                {
                    if (ServicesHelper.IsAuthorized(this.PortalSettings, this.ForumModuleId, forumId, SecureActions.Read, this.UserInfo))
                    {
                        DotNetNuke.Modules.ActiveForums.Entities.TopicInfo t = DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Instance.GetById(this.ForumModuleId, topicId);
                        if (t != null)
                        {
                            var forumUser = DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.Instance.GetByUserId(this.ActiveModule.PortalID, this.ForumModuleId, this.UserInfo.UserID);
                            if (this.UserInfo.IsAdmin ||
                                this.UserInfo.IsSuperUser ||
                                Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(t.Forum.Security.ModerateRoleIds, forumUser.UserRoleIds) ||
                                (Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(t.Forum.Security.ReadRoleIds, forumUser.UserRoleIds) && this.UserInfo.UserID == t.Content.AuthorId)
                                )
                            {
                                var topic = new DotNetNuke.Modules.ActiveForums.ViewModels.Topic(t);
                                return this.Request.CreateResponse(HttpStatusCode.OK, topic);
                            }
                            this.Request.CreateResponse(HttpStatusCode.Unauthorized);
                        }
                        this.Request.CreateResponse(HttpStatusCode.NotFound);
                    }
                    this.Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }

            return this.Request.CreateResponse(HttpStatusCode.BadRequest);
        }

#pragma warning disable CS1570
        /// <summary>
        /// Deletes a Topic
        /// </summary>
        /// <param name="forumId" type="int"></param>
        /// <param name="topicId" type="int"></param>
        /// <returns></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Topic/Delete?forumId=xxx&topicId=yyy</remarks>
#pragma warning restore CS1570
        [HttpDelete]
        [DnnAuthorize]
        [ForumsAuthorize(SecureActions.Delete)]
        public HttpResponseMessage Delete(int forumId, int topicId)
        {
            try
            {
                if (forumId > 0 && topicId > 0)
                {
                    DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Instance.GetById(this.ForumModuleId, topicId);
                    if (ti != null)
                    {
                        var forumUser = DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.Instance.GetByUserId(this.ActiveModule.PortalID, this.ForumModuleId, this.UserInfo.UserID);
                        if (this.UserInfo.IsAdmin ||
                            this.UserInfo.IsSuperUser ||
                            Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(ti.Forum.Security.ModerateRoleIds, forumUser.UserRoleIds) ||
                            (Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(ti.Forum.Security.DeleteRoleIds, forumUser.UserRoleIds) && this.UserInfo.UserID == ti.Content.AuthorId)
                            )
                        {
                            DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Instance.DeleteById(this.ForumModuleId, topicId, SettingsBase.GetModuleSettings(ti.ModuleId).DeleteBehavior);
                            return this.Request.CreateResponse(HttpStatusCode.OK, string.Empty);
                        }

                        return this.Request.CreateResponse(HttpStatusCode.Unauthorized);
                    }

                    return this.Request.CreateResponse(HttpStatusCode.NotFound);
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }

            return this.Request.CreateResponse(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Rates a topic
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="rating" type="int"></param>
        /// <returns></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Topic/Rate</remarks>
        [HttpPost]
        [DnnAuthorize]
        [ForumsAuthorize(SecureActions.Edit)]
        public HttpResponseMessage Rate(TopicDto1 dto, int rating)
        {
            try
            {
                if (dto.TopicId > 0 && rating >= 1 && rating <= 5)
                {
                    return this.Request.CreateResponse(HttpStatusCode.OK, DotNetNuke.Modules.ActiveForums.Controllers.TopicRatingController.Instance.Rate(this.UserInfo.UserID, dto.TopicId, rating, HttpContext.Current.Request.UserHostAddress ?? string.Empty));
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }

            return this.Request.CreateResponse(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Updates an existing topic
        /// </summary>
        /// <param name="dto"></param>
        /// <returns name="Topic" type="DotNetNuke.Modules.ActiveForums.ViewModels.Topic"></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Topic/Update</remarks>
        [HttpPost]
        [DnnAuthorize]
        [ForumsAuthorize(SecureActions.Edit)]
        public HttpResponseMessage Update(TopicDto2 dto)
        {
            try
            {
                int forumId = dto.ForumId;
                int topicId = dto.Topic.TopicId;

                if (topicId > 0 && forumId > 0)
                {
                    DotNetNuke.Modules.ActiveForums.Entities.TopicInfo originalTopic = DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Instance.GetById(this.ForumModuleId, topicId);
                    if (originalTopic != null)
                    {
                        var forumUser = DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.Instance.GetByUserId(this.ActiveModule.PortalID, this.ForumModuleId, this.UserInfo.UserID);
                        if (this.UserInfo.IsAdmin ||
                            this.UserInfo.IsSuperUser ||
                            Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(originalTopic.Forum.Security.ModerateRoleIds, forumUser.UserRoleIds) ||
                            (Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(originalTopic.Forum.Security.EditRoleIds, forumUser.UserRoleIds) && this.UserInfo.UserID == originalTopic.Content.AuthorId)
                            )
                        {
                            string subject = Utilities.XSSFilter(dto.Topic.Subject, true);
                            originalTopic.Content.Subject = subject;
                            originalTopic.TopicUrl = DotNetNuke.Modules.ActiveForums.Controllers.UrlController.BuildTopicUrlSegment(portalId: this.ActiveModule.PortalID, moduleId: this.ForumModuleId, topicId: topicId, subject: subject, forumInfo: originalTopic.Forum);

                            if (dto.Topic.IsLocked != originalTopic.IsLocked &&
                                (this.UserInfo.IsAdmin ||
                                this.UserInfo.IsSuperUser ||
                                Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(originalTopic.Forum.Security.ModerateRoleIds, forumUser.UserRoleIds) ||
                                (Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(originalTopic.Forum.Security.LockRoleIds, forumUser.UserRoleIds) && this.UserInfo.UserID == originalTopic.Content.AuthorId)
                                )
                                )
                            {
                                originalTopic.IsLocked = dto.Topic.IsLocked;
                            }

                            if (dto.Topic.IsPinned != originalTopic.IsPinned &&
                                (this.UserInfo.IsAdmin ||
                                this.UserInfo.IsSuperUser ||
                                Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(originalTopic.Forum.Security.ModerateRoleIds, forumUser.UserRoleIds) ||
                                (Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(originalTopic.Forum.Security.PinRoleIds, forumUser.UserRoleIds) && this.UserInfo.UserID == originalTopic.Content.AuthorId)
                                )
                                )
                            {
                                originalTopic.IsLocked = dto.Topic.IsLocked;
                            }

                            originalTopic.Priority = dto.Topic.Priority;
                            originalTopic.StatusId = dto.Topic.StatusId;

                            if (originalTopic.Forum.Properties != null && originalTopic.Forum.Properties.Count > 0)
                            {
                                originalTopic.TopicProperties = dto.Topic.TopicProperties.ToList();
                            }

                            DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Save(originalTopic);
                            Utilities.UpdateModuleLastContentModifiedOnDate(this.ForumModuleId);

                            if (this.UserInfo.IsAdmin ||
                                this.UserInfo.IsSuperUser ||
                                Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(originalTopic.Forum.Security.ModerateRoleIds, forumUser.UserRoleIds) ||
                                (Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(originalTopic.Forum.Security.TagRoleIds, forumUser.UserRoleIds) && this.UserInfo.UserID == originalTopic.Content.AuthorId)
                                )
                            {
                                if (!string.IsNullOrEmpty(dto.Topic.Tags))
                                {
                                    DotNetNuke.Modules.ActiveForums.Controllers.TopicTagController.Instance.DeleteForTopic(topicId);
                                    string tagForm = dto.Topic.Tags;
                                    string[] tags = tagForm.Split(',');
                                    foreach (string tag in tags)
                                    {
                                        string sTag = Utilities.CleanString(this.ActiveModule.PortalID, tag.Trim(), false, EditorType.TEXTBOX, false, false, this.ForumModuleId, string.Empty, false);
                                        DataProvider.Instance().Tags_Save(this.ActiveModule.PortalID, this.ForumModuleId, -1, sTag, 0, topicId);
                                    }
                                }
                            }

                            if (this.UserInfo.IsAdmin ||
                                this.UserInfo.IsSuperUser ||
                                Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(originalTopic.Forum.Security.ModerateRoleIds, forumUser.UserRoleIds) ||
                                (Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(originalTopic.Forum.Security.EditRoleIds, forumUser.UserRoleIds) && this.UserInfo.UserID == originalTopic.Content.AuthorId)
                                )
                            {
                                if (!string.IsNullOrEmpty(dto.Topic.SelectedCategoriesAsString))
                                {
                                    string[] cats = dto.Topic.SelectedCategoriesAsString.Split(';');
                                    DotNetNuke.Modules.ActiveForums.Controllers.TopicCategoryController.Instance.DeleteForTopic(topicId);
                                    foreach (string c in cats)
                                    {
                                        int cid = -1;
                                        if (!string.IsNullOrEmpty(c) && Utilities.IsNumeric(c))
                                        {
                                            cid = Convert.ToInt32(c);
                                            if (cid > 0)
                                            {
                                                 DotNetNuke.Modules.ActiveForums.Controllers.TopicCategoryController.Instance.AddCategoryToTopic(cid, topicId);
                                            }
                                        }
                                    }
                                }
                            }

                            DotNetNuke.Modules.ActiveForums.Entities.TopicInfo updatedTopic = DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Instance.GetById(this.ForumModuleId, topicId);
                            return this.Request.CreateResponse(HttpStatusCode.OK, new DotNetNuke.Modules.ActiveForums.ViewModels.Topic(updatedTopic));
                        }

                        return this.Request.CreateResponse(HttpStatusCode.Unauthorized);
                    }

                    return this.Request.CreateResponse(HttpStatusCode.NotFound, dto.Topic);
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }

            return this.Request.CreateResponse(HttpStatusCode.BadRequest);
        }
#pragma warning disable CS1570
        /// <summary>
        /// Reatores a Topic
        /// </summary>
        /// <param name="forumId" type="int"></param>
        /// <param name="topicId" type="int"></param>
        /// <returns></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Topic/Restore?forumId=xxx&topicId=zzz</remarks>
#pragma warning restore CS1570
        [HttpPost]
        [DnnAuthorize]
        [ForumsAuthorize(SecureActions.Moderate)]
        public HttpResponseMessage Restore(TopicDto1 dto)
        {
            try
            {
                if (dto.ForumId > 0 && dto.TopicId > 0)
                {
                    var topic = DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Instance.GetById(this.ForumModuleId, dto.TopicId);
                    if (topic != null)
                    {
                        if (topic.IsDeleted == false)
                        {
                            return this.Request.CreateResponse(HttpStatusCode.BadRequest);
                        }

                        DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Instance.Restore(this.ActiveModule.PortalID, this.ForumModuleId, dto.ForumId, dto.TopicId);
                        return this.Request.CreateResponse(HttpStatusCode.OK, string.Empty);
                    }
                }

                return this.Request.CreateResponse(HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }

            return this.Request.CreateResponse(HttpStatusCode.BadRequest);
        }
    }
}
