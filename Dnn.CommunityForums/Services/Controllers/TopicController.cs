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
namespace DotNetNuke.Modules.ActiveForums.Services.Controllers
{
    using System;
    using System.Data;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Reflection;
    using System.Text;
    using System.Web;
    using System.Web.Helpers;
    using System.Web.Http;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Modules.ActiveForums.Data;
    using DotNetNuke.Web.Api;
    using DotNetNuke.Web.UI.WebControls;

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

            public DotNetNuke.Modules.ActiveForums.Entities.TopicInfo Topic { get; set; }
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
            if (dto.TopicId > 0 && dto.ForumId > 0)
            {
                string userRoles = new DotNetNuke.Modules.ActiveForums.UserProfileController()
                    .Profiles_Get(this.ActiveModule.PortalID, this.ForumModuleId, this.UserInfo.UserID).Roles;
                int subscribed = new SubscriptionController().Subscription_Update(this.ActiveModule.PortalID,
                    this.ForumModuleId, dto.ForumId, dto.TopicId, 1, this.UserInfo.UserID, userRoles);
                return this.Request.CreateResponse(HttpStatusCode.OK, subscribed == 1);
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
            if (forumId > 0 && topicId > 0)
            {
                return this.Request.CreateResponse(HttpStatusCode.OK,
                    new DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController().Count(
                        this.ActiveModule.PortalID, this.ForumModuleId, forumId, topicId));
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
            if (forumId > 0)
            {
                return this.Request.CreateResponse(HttpStatusCode.OK,
                    $"{new DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController().Count(this.ActiveModule.PortalID, this.ForumModuleId, forumId, topicId)} {Utilities.GetSharedResource("[RESX:TOPICSUBSCRIBERCOUNT]", false)}");
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
        [ForumsAuthorize(SecureActions.ModPin)]
        [ForumsAuthorize(SecureActions.Pin)]
        public HttpResponseMessage Pin(TopicDto1 dto)
        {
            int topicId = dto.TopicId;
            if (topicId > 0)
            {
                DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(topicId);
                if (ti != null)
                {
                    ti.IsPinned = !ti.IsPinned;
                    DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Save(ti);
                    return this.Request.CreateResponse(HttpStatusCode.OK, value: ti.IsPinned);
                }

                return this.Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            return this.Request.CreateResponse(HttpStatusCode.NotFound);
        }

        /// <summary>
        /// Locks a Topic
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Topic/Lock</remarks>
        [HttpPost]
        [DnnAuthorize]
        [ForumsAuthorize(SecureActions.ModLock)]
        [ForumsAuthorize(SecureActions.Lock)]
        public HttpResponseMessage Lock(TopicDto1 dto)
        {
            int topicId = dto.TopicId;
            if (topicId > 0)
            {
                DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(topicId);
                if (ti != null)
                {
                    ti.IsLocked = !ti.IsLocked;
                    DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Save(ti);
                    return this.Request.CreateResponse(HttpStatusCode.OK, ti.IsLocked);
                }

                return this.Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            return this.Request.CreateResponse(HttpStatusCode.NotFound);
        }

        /// <summary>
        /// Moves a Topic
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Topic/Move</remarks>
        [HttpPost]
        [DnnAuthorize]
        [ForumsAuthorize(SecureActions.ModMove)]
        public HttpResponseMessage Move(TopicDto1 dto)
        {
            int topicId = dto.TopicId;
            int forumId = dto.ForumId;
            if (topicId > 0 && forumId > 0)
            {
                DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(topicId);
                if (ti != null)
                {
                    DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Move(topicId, forumId);
                    DotNetNuke.Modules.ActiveForums.DataCache.CacheClearPrefix(this.ForumModuleId, string.Format(CacheKeys.CacheModulePrefix, this.ForumModuleId));
                    return this.Request.CreateResponse(HttpStatusCode.OK, string.Empty);
                }

                return this.Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            return this.Request.CreateResponse(HttpStatusCode.NotFound);
        }
#pragma warning disable CS1570
        /// <summary>
        /// Loads a Topic
        /// <param name="forumId" type="int"></param>
        /// <param name="topicId" type="int"></param>
        /// <returns name="Topic" type="DotNetNuke.Modules.ActiveForums.Entities.TopicInfo"></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Topic/Load?ForumId=xxx&TopicId=xxx</remarks>
#pragma warning restore CS1570
        [HttpGet]
        [DnnAuthorize]
        [ForumsAuthorize(SecureActions.Read)]
        public HttpResponseMessage Load(int forumId, int topicId)
        {
            if (topicId > 0 && forumId > 0)
            {
                if (ServicesHelper.IsAuthorized(this.PortalSettings.PortalId, this.ForumModuleId, forumId, SecureActions.Read, this.UserInfo))
                {
                    DotNetNuke.Modules.ActiveForums.Entities.TopicInfo t = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(topicId);
                    if (t != null)
                    {
                        return this.Request.CreateResponse(HttpStatusCode.OK, new object[] { t });
                    }
                    else
                    {
                        this.Request.CreateResponse(HttpStatusCode.NotFound);
                    }
                }
                else
                {
                    this.Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
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
        [ForumsAuthorize(SecureActions.ModDelete)]
        public HttpResponseMessage Delete(int forumId, int topicId)
        {
            if (forumId > 0 && topicId > 0)
            {
                DotNetNuke.Modules.ActiveForums.Controllers.TopicController tc = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController();
                DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = tc.GetById(topicId);
                if (ti != null)
                {
                    tc.DeleteById(topicId);
                    return this.Request.CreateResponse(HttpStatusCode.OK, string.Empty);
                }

                return this.Request.CreateResponse(HttpStatusCode.NotFound);
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
        [ForumsAuthorize(SecureActions.ModEdit)]
        public HttpResponseMessage Rate(TopicDto1 dto, int rating)
        {
            if (dto.TopicId > 0 && rating >= 1 && rating <= 5)
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, new DotNetNuke.Modules.ActiveForums.Controllers.TopicRatingController().Rate(this.UserInfo.UserID, dto.TopicId, rating, HttpContext.Current.Request.UserHostAddress ?? string.Empty));
            }

            return this.Request.CreateResponse(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Updates an existing topic
        /// </summary>
        /// <param name="dto"></param>
        /// <returns name="Topic" type="DotNetNuke.Modules.ActiveForums.Entities.TopicInfo"></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Topic/Update</remarks>
        [HttpPost]
        [DnnAuthorize]
        [ForumsAuthorize(SecureActions.Edit)]
        [ForumsAuthorize(SecureActions.ModEdit)]
        public HttpResponseMessage Update(TopicDto2 dto)
        {
            int forumId = dto.ForumId;
            int topicId = dto.Topic.TopicId;

            if (topicId > 0 && forumId > 0)
            {
                DotNetNuke.Modules.ActiveForums.Entities.TopicInfo originalTopic = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(topicId);
                if (originalTopic != null)
                {
                    string subject = Utilities.XSSFilter(dto.Topic.Content.Subject, true);
                    originalTopic.Content.Subject = subject;
                    originalTopic.TopicUrl = DotNetNuke.Modules.ActiveForums.Controllers.UrlController.BuildTopicUrl(portalId: this.ActiveModule.PortalID, moduleId: this.ForumModuleId, topicId: topicId, subject: subject, forumInfo: originalTopic.Forum);

                    if (dto.Topic.IsLocked != originalTopic.IsLocked &&
                        (DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasAccess(originalTopic.Forum.Security.Lock, string.Join(";", DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIds(this.ActiveModule.PortalID, this.UserInfo.Roles))) ||
                            DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasAccess(originalTopic.Forum.Security.ModLock, string.Join(";", DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIds(this.ActiveModule.PortalID, this.UserInfo.Roles)))
                        )
                        )
                    {
                        originalTopic.IsLocked = dto.Topic.IsLocked;
                    }

                    if (dto.Topic.IsPinned != originalTopic.IsPinned &&
                        (DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasAccess(originalTopic.Forum.Security.Pin, string.Join(";", DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIds(this.ActiveModule.PortalID, this.UserInfo.Roles))) ||
                            DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasAccess(originalTopic.Forum.Security.ModPin, string.Join(";", DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIds(this.ActiveModule.PortalID, this.UserInfo.Roles)))
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

                    if (DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasAccess(originalTopic.Forum.Security.Tag, string.Join(";", DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIds(this.ActiveModule.PortalID, this.UserInfo.Roles))))
                    {
                        if (!string.IsNullOrEmpty(dto.Topic.Tags))
                        {
                            DataProvider.Instance().Tags_DeleteByTopicId(this.ActiveModule.PortalID, this.ForumModuleId, topicId);
                            string tagForm = dto.Topic.Tags;
                            string[] tags = tagForm.Split(',');
                            foreach (string tag in tags)
                            {
                                string sTag = Utilities.CleanString(this.ActiveModule.PortalID, tag.Trim(), false, EditorTypes.TEXTBOX, false, false, this.ForumModuleId, string.Empty, false);
                                DataProvider.Instance().Tags_Save(this.ActiveModule.PortalID, this.ForumModuleId, -1, sTag, 0, 1, 0, topicId, false, -1, -1);
                            }
                        }
                    }

                    if (DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasAccess(originalTopic.Forum.Security.Categorize, string.Join(";", DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIds(this.ActiveModule.PortalID, this.UserInfo.Roles))))
                    {
                        if (!string.IsNullOrEmpty(dto.Topic.SelectedCategoriesAsString))
                        {
                            string[] cats = dto.Topic.SelectedCategoriesAsString.Split(';');
                            DataProvider.Instance().Tags_DeleteTopicToCategory(this.ActiveModule.PortalID, this.ForumModuleId, -1, topicId);
                            foreach (string c in cats)
                            {
                                int cid = -1;
                                if (!string.IsNullOrEmpty(c) && SimulateIsNumeric.IsNumeric(c))
                                {
                                    cid = Convert.ToInt32(c);
                                    if (cid > 0)
                                    {
                                        DataProvider.Instance().Tags_AddTopicToCategory(this.ActiveModule.PortalID, this.ForumModuleId, cid, topicId);
                                    }
                                }
                            }
                        }
                    }

                    DotNetNuke.Modules.ActiveForums.Entities.TopicInfo updatedTopic = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(topicId);
                    return this.Request.CreateResponse(HttpStatusCode.OK, updatedTopic);
                }
                else
                {
                    return this.Request.CreateResponse(HttpStatusCode.NotFound, dto.Topic);
                }
            }

            return this.Request.CreateResponse(HttpStatusCode.BadRequest);
        }
    }
}
