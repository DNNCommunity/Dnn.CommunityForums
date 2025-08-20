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
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Web;
    using System.Web.Http;

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
                    int subscribed = new SubscriptionController().Subscription_Update(this.ActiveModule.PortalID, this.ForumModuleId, dto.ForumId, dto.TopicId, 1, new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ForumModuleId).GetByUserId(this.ActiveModule.PortalID, this.UserInfo.UserID));
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
                    DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(this.ForumModuleId).GetById(topicId);
                    if (ti != null)
                    {
                        var forumUser = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ForumModuleId).GetByUserId(this.ActiveModule.PortalID, this.UserInfo.UserID);
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
                                new DotNetNuke.Modules.ActiveForums.Controllers.ProcessQueueController().Add(ProcessType.TopicPinned,
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
                        new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(this.ForumModuleId).GetById(topicId);
                    if (ti != null)
                    {
                        var forumUser = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ForumModuleId).GetByUserId(this.ActiveModule.PortalID, this.UserInfo.UserID);
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
                    DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(this.ForumModuleId).GetById(topicId);
                    if (ti != null)
                    {
                        var forumUser = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ForumModuleId).GetByUserId(this.ActiveModule.PortalID, this.UserInfo.UserID);
                        if (this.UserInfo.IsAdmin ||
                            this.UserInfo.IsSuperUser ||
                            Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(ti.Forum.Security.ModerateRoleIds, forumUser.UserRoleIds) ||
                            (Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(ti.Forum.Security.MoveRoleIds, forumUser.UserRoleIds) && this.UserInfo.UserID == ti.Content.AuthorId)
                            )
                        {
                            DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Move(moduleId: this.ForumModuleId, userId: this.UserInfo.UserID, topicId: topicId, newForumId: forumId);
                            DotNetNuke.Modules.ActiveForums.DataCache.CacheClearPrefix(this.ForumModuleId, string.Format(CacheKeys.CacheModulePrefix, this.ForumModuleId));
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
                        DotNetNuke.Modules.ActiveForums.Entities.TopicInfo t = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(this.ForumModuleId).GetById(topicId);
                        if (t != null)
                        {
                            var forumUser = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ForumModuleId).GetByUserId(this.ActiveModule.PortalID, this.UserInfo.UserID);
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
                    DotNetNuke.Modules.ActiveForums.Controllers.TopicController tc = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(this.ForumModuleId);
                    DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = tc.GetById(topicId);
                    if (ti != null)
                    {
                        var forumUser = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ForumModuleId).GetByUserId(this.ActiveModule.PortalID, this.UserInfo.UserID);
                        if (this.UserInfo.IsAdmin ||
                            this.UserInfo.IsSuperUser ||
                            Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(ti.Forum.Security.ModerateRoleIds, forumUser.UserRoleIds) ||
                            (Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(ti.Forum.Security.DeleteRoleIds, forumUser.UserRoleIds) && this.UserInfo.UserID == ti.Content.AuthorId)
                            )
                        {
                            tc.DeleteById(topicId, SettingsBase.GetModuleSettings(ti.ModuleId).DeleteBehavior);
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
                    return this.Request.CreateResponse(HttpStatusCode.OK, new DotNetNuke.Modules.ActiveForums.Controllers.TopicRatingController().Rate(this.UserInfo.UserID, dto.TopicId, rating, HttpContext.Current.Request.UserHostAddress ?? string.Empty));
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
                    DotNetNuke.Modules.ActiveForums.Entities.TopicInfo originalTopic = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(this.ForumModuleId).GetById(topicId);
                    if (originalTopic != null)
                    {
                        var forumUser = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ForumModuleId).GetByUserId(this.ActiveModule.PortalID, this.UserInfo.UserID);
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
                                StringBuilder tData = new StringBuilder();
                                tData.Append("<topicdata>");
                                tData.Append("<properties>");
                                foreach (var p in originalTopic.Forum.Properties)
                                {
                                    tData.Append("<property id=\"" + p.PropertyId.ToString() + "\">");
                                    tData.Append("<name><![CDATA[");
                                    tData.Append(p.Name);
                                    tData.Append("]]></name>");
                                    if (!string.IsNullOrEmpty(dto.Topic.TopicProperties?.Where(pl => pl.PropertyId == p.PropertyId).FirstOrDefault().Value))
                                    {
                                        tData.Append("<value><![CDATA[");
                                        tData.Append(Utilities.XSSFilter(dto.Topic.TopicProperties.Where(pl => pl.PropertyId == p.PropertyId).FirstOrDefault().Value));
                                        tData.Append("]]></value>");
                                    }
                                    else
                                    {
                                        tData.Append("<value></value>");
                                    }

                                    tData.Append("</property>");
                                }

                                tData.Append("</properties>");
                                tData.Append("</topicdata>");
                                originalTopic.TopicData = tData.ToString();
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
                                    new DotNetNuke.Modules.ActiveForums.Controllers.TopicTagController().DeleteForTopic(topicId);
                                    string tagForm = dto.Topic.Tags;
                                    string[] tags = tagForm.Split(',');
                                    foreach (string tag in tags)
                                    {
                                        string sTag = Utilities.CleanString(this.ActiveModule.PortalID, tag.Trim(), false, EditorTypes.TEXTBOX, false, false, this.ForumModuleId, string.Empty, false);
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
                                    new DotNetNuke.Modules.ActiveForums.Controllers.TopicCategoryController().DeleteForTopic(topicId);
                                    foreach (string c in cats)
                                    {
                                        int cid = -1;
                                        if (!string.IsNullOrEmpty(c) && Utilities.IsNumeric(c))
                                        {
                                            cid = Convert.ToInt32(c);
                                            if (cid > 0)
                                            {
                                                new DotNetNuke.Modules.ActiveForums.Controllers.TopicCategoryController().AddCategoryToTopic(cid, topicId);
                                            }
                                        }
                                    }
                                }
                            }

                            DotNetNuke.Modules.ActiveForums.Entities.TopicInfo updatedTopic = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(this.ForumModuleId).GetById(topicId);
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
                    var topicController = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(this.ForumModuleId);
                    var topic = topicController.GetById(dto.TopicId);
                    if (topic != null)
                    {
                        if (topic.IsDeleted == false)
                        {
                            return this.Request.CreateResponse(HttpStatusCode.BadRequest);
                        }

                        topicController.Restore(this.ActiveModule.PortalID,
                            dto.ForumId,
                            dto.TopicId);
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
