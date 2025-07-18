﻿// Copyright (c) by DNN Community
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

using DotNetNuke.Modules.ActiveForums.Enums;

namespace DotNetNuke.Modules.ActiveForums
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    public partial class af_modtopics_new : ForumBase
    {
        #region Private Members
        private bool bModDelete = false;
        private bool bModEdit = false;
        private bool bModBan = false;
        private bool bModerate = false;

        #endregion
        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.lblHeader.Text = Utilities.GetSharedResource("[RESX:PendingPosts]");
            this.cbMod.CallbackEvent += this.cbMod_Callback;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (this.Request.IsAuthenticated && (this.ForumUser.GetIsMod(this.ForumModuleId) || this.ForumUser.IsSuperUser || this.ForumUser.IsAdmin))
            {
                if (this.ForumId > 0)
                {
                    this.SetPermissions(this.ForumId);
                }

                if (!this.cbMod.IsCallback)
                {
                    this.BuildModList();
                }
            }
            else
            {
                this.Response.Redirect(this.NavigateUrl(this.TabId), false);
                this.Context.ApplicationInstance.CompleteRequest();
            }
        }

        private void cbMod_Callback(object sender, Modules.ActiveForums.Controls.CallBackEventArgs e)
        {
            SettingsInfo ms = SettingsBase.GetModuleSettings(this.ForumModuleId);
            DotNetNuke.Modules.ActiveForums.Entities.ForumInfo fi = null;
            if (e.Parameters.Length > 0)
            {
                if (this.ForumId < 1)
                {
                    this.SetPermissions(Convert.ToInt32(e.Parameters[1]));
                    fi = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(forumId: Convert.ToInt32(e.Parameters[1]), moduleId: this.ForumModuleId);
                }
                else
                {
                    fi = this.ForumInfo;
                }

                switch (e.Parameters[0].ToLowerInvariant())
                {
                    case "moddel":
                        {
                            if (this.bModDelete)
                            {
                                int delAction = ms.DeleteBehavior;
                                int tmpForumId = Convert.ToInt32(e.Parameters[1]);
                                int tmpTopicId = Convert.ToInt32(e.Parameters[2]);
                                int tmpReplyId = Convert.ToInt32(e.Parameters[3]);
                                if (tmpForumId > 0 & tmpTopicId > 0 && tmpReplyId == 0)
                                {
                                    DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(this.ForumModuleId).GetById(tmpTopicId);
                                    if (ti != null)
                                    {
                                        new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(this.ForumModuleId).DeleteById(tmpTopicId);
                                        if (fi.FeatureSettings.ModDeleteNotify && ti?.Author?.AuthorId > 0)
                                        {
                                            try
                                            {
                                                DotNetNuke.Modules.ActiveForums.Controllers.EmailController.SendEmail(
                                                    TemplateType.ModDelete,
                                                    this.TabId,
                                                    ti.Forum,
                                                    tmpTopicId,
                                                    -1,
                                                    ti.Author);
                                            }
                                            catch (Exception ex)
                                            {
                                                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                                            }
                                        }
                                    }
                                }
                                else if (tmpForumId > 0 & tmpTopicId > 0 & tmpReplyId > 0)
                                {
                                    DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo ri = new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController(this.ForumModuleId).GetById(tmpReplyId);
                                    if (ri != null)
                                    {
                                        new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController(this.ForumModuleId).Reply_Delete(this.PortalId, tmpForumId, tmpTopicId, tmpReplyId, delAction);
                                        if (fi.FeatureSettings.ModDeleteNotify && ri?.Author?.AuthorId > 0)
                                        {
                                            try
                                            {
                                                DotNetNuke.Modules.ActiveForums.Controllers.EmailController.SendEmail(
                                                    TemplateType.ModDelete,
                                                    this.TabId,
                                                    ri.Forum,
                                                    tmpTopicId,
                                                    tmpReplyId,
                                                    ri.Author);
                                            }
                                            catch (Exception ex)
                                            {
                                                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                                            }
                                        }
                                    }
                                }

                                DotNetNuke.Modules.ActiveForums.Controllers.ModerationController.RemoveModerationNotifications(this.ForumTabId, this.ForumModuleId, tmpForumId, tmpTopicId, tmpReplyId);
                            }

                            break;
                        }

                    case "modreject":
                        {
                            int tmpForumId = Convert.ToInt32(e.Parameters[1]);
                            int tmpTopicId = Convert.ToInt32(e.Parameters[2]);
                            int tmpReplyId = Convert.ToInt32(e.Parameters[3]);
                            int tmpAuthorId = Convert.ToInt32(e.Parameters[4]);
                            ModController mc = new ModController();
                            mc.Mod_Reject(this.PortalId, this.ForumModuleId, this.UserId, tmpForumId, tmpTopicId, tmpReplyId);
                            DotNetNuke.Modules.ActiveForums.Controllers.ModerationController.RemoveModerationNotifications(this.ForumTabId, this.ForumModuleId, tmpForumId, tmpTopicId, tmpReplyId);
                            if (fi.FeatureSettings.ModRejectNotify && tmpAuthorId > 0)
                            {
                                DotNetNuke.Entities.Users.UserInfo ui = DotNetNuke.Entities.Users.UserController.Instance.GetUser(this.PortalId, tmpAuthorId);
                                if (ui != null)
                                {
                                    DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo au = new DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo(this.ForumModuleId);
                                    au.AuthorId = tmpAuthorId;
                                    au.DisplayName = ui.DisplayName;
                                    au.Email = ui.Email;
                                    au.FirstName = ui.FirstName;
                                    au.LastName = ui.LastName;
                                    au.Username = ui.Username;
                                    DotNetNuke.Modules.ActiveForums.Controllers.EmailController.SendEmail(TemplateType.ModReject, this.TabId, this.ForumInfo, tmpTopicId, tmpReplyId, au);
                                }
                            }

                            break;
                        }

                    case "modappr":
                        {
                            int tmpForumId = Convert.ToInt32(e.Parameters[1]);
                            int tmpTopicId = Convert.ToInt32(e.Parameters[2]);
                            int tmpReplyId = Convert.ToInt32(e.Parameters[3]);
                            if (tmpForumId > 0 & tmpTopicId > 0 && tmpReplyId == 0)
                            {
                                DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(this.ForumModuleId).GetById(tmpTopicId);
                                if (ti != null)
                                {
                                    ti.IsApproved = true;
                                    DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Save(ti);
                                    DotNetNuke.Modules.ActiveForums.Controllers.TopicController.SaveToForum(this.ForumModuleId, tmpForumId, tmpTopicId, tmpReplyId);

                                    // TODO: Add Audit log for who approved topic
                                    if (fi.FeatureSettings.ModApproveNotify && ti.Author.AuthorId > 0)
                                    {
                                        DotNetNuke.Modules.ActiveForums.Controllers.EmailController.SendEmail(TemplateType.ModApprove, this.TabId, this.ForumInfo, tmpTopicId, tmpReplyId, ti.Author);
                                    }

                                    DotNetNuke.Modules.ActiveForums.Controllers.ModerationController.RemoveModerationNotifications(this.ForumTabId, this.ForumModuleId, tmpForumId, tmpTopicId, tmpReplyId);
                                    DotNetNuke.Modules.ActiveForums.Controllers.TopicController.QueueApprovedTopicAfterAction(portalId: this.PortalId, tabId: this.ForumTabId, moduleId: this.ForumModuleId, forumGroupId: fi.ForumGroupId, forumId: this.ForumId, topicId: tmpTopicId, replyId: -1, contentId: ti.ContentId, authorId: ti.Content.AuthorId, userId: this.ForumUser.UserId);
                                }
                            }
                            else if (tmpForumId > 0 & tmpTopicId > 0 & tmpReplyId > 0)
                            {
                                DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo ri = new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController(this.ForumModuleId).GetById(tmpReplyId);
                                if (ri != null)
                                {
                                    ri.IsApproved = true;
                                    new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController(this.ForumModuleId).Reply_Save(this.PortalId, this.ForumModuleId, ri);
                                    DotNetNuke.Modules.ActiveForums.Controllers.TopicController.SaveToForum(this.ForumModuleId, tmpForumId, tmpTopicId, tmpReplyId);

                                    // TODO: Add Audit log for who approved topic
                                    if (fi.FeatureSettings.ModApproveNotify && ri.Author.AuthorId > 0)
                                    {
                                        DotNetNuke.Modules.ActiveForums.Controllers.EmailController.SendEmail(TemplateType.ModApprove, this.TabId, ri.Forum, tmpTopicId, tmpReplyId, ri.Author);
                                    }

                                    DotNetNuke.Modules.ActiveForums.Controllers.ModerationController.RemoveModerationNotifications(this.ForumTabId, this.ForumModuleId, tmpForumId, tmpTopicId, tmpReplyId);
                                    DotNetNuke.Modules.ActiveForums.Controllers.ReplyController.QueueApprovedReplyAfterAction(portalId: this.PortalId, tabId: this.ForumTabId, moduleId: this.ForumModuleId, forumGroupId: fi.ForumGroupId, forumId: tmpForumId, topicId: tmpTopicId, replyId: tmpReplyId, contentId: ri.ContentId, authorId: ri.Content.AuthorId, userId: this.ForumUser.UserId);
                                }
                            }

                            break;
                        }
                }

                DataCache.CacheClearPrefix(this.ModuleId, string.Format(CacheKeys.ForumViewPrefix, this.ModuleId));
            }

            this.BuildModList();
            this.lblHeader.RenderControl(e.Output);
            this.litTopics.RenderControl(e.Output);
        }

        #endregion
        #region Private Members
        private void BuildModList()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            DataSet ds = DataProvider.Instance().Mod_Pending(this.PortalId, this.ModuleId, this.ForumId, this.UserId);
            DataTable dtContent = ds.Tables[0];
            DataTable dtAttach = ds.Tables[1];
            string tmpForum = string.Empty;
            sb.Append("<div id=\"afgrid\" style=\"position:relative;\"><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\">");
            if (dtContent.Rows.Count < 1)
            {
                this.lblHeader.Text = Utilities.GetSharedResource("[RESX:NoPendingPosts]");
            }
            else
            {
                foreach (DataRow dr in dtContent.Rows)
                {
                    string forumKey = dr["ForumId"].ToString() + dr["ForumName"].ToString();
                    if (forumKey != tmpForum)
                    {
                        if (this.ForumId == -1)
                        {
                            this.SetPermissions(Convert.ToInt32(dr["ForumId"]));
                        }

                        if (this.bModerate)
                        {
                            if (!(string.IsNullOrEmpty(tmpForum)))
                            {
                                sb.Append("</td></tr>");
                            }

                            int pendingCount = 0;
                            dtContent.DefaultView.RowFilter = "ForumId = " + Convert.ToInt32(dr["ForumId"]);
                            pendingCount = dtContent.DefaultView.ToTable().Rows.Count;
                            dtContent.DefaultView.RowFilter = "";
                            sb.Append("<tr><td class=\"afgrouprow\" style=\"padding-left:10px;\">" + dr["GroupName"].ToString() + " > " + dr["ForumName"].ToString() + " [RESX:Pending]: (" + pendingCount + ")</td><td class=\"afgrouprow\" align=\"right\" style=\"padding-right:5px;\">");
                            sb.Append(DotNetNuke.Modules.ActiveForums.Injector.InjectCollapsibleOpened(target: "section" + dr["ForumId"].ToString(), title: string.Empty));
                            sb.Append("</td></tr>");
                            sb.Append("<tr id=\"section" + dr["ForumId"].ToString() + "\"><td colspan=\"2\">");
                        }

                        tmpForum = forumKey;
                    }

                    if (this.bModerate)
                    {
                        sb.Append("<div class=\"afmodrow\">");
                        sb.Append("<table width=\"99%\">");
                        sb.Append("<tr><td style=\"white-space:nowrap;\">" + Utilities.GetUserFormattedDateTime(Convert.ToDateTime(dr["DateCreated"]), this.PortalId, this.UserId) + "</td>");
                        sb.Append("<td class=\"dnnFormItem dnnActions dnnClear dnnRight\">");
                        if (this.bModerate)
                        {
                            sb.Append("<span class=\"dnnPrimaryAction\" onclick=\"afmodApprove(" + dr["ForumId"].ToString() + "," + dr["TopicId"].ToString() + "," + dr["ReplyId"].ToString() + ");\">[RESX:Approve]</span>");
                        }

                        if (this.bModEdit)
                        {
                            sb.Append("<span class=\"dnnSecondaryAction\" onclick=\"javascript:if(confirm('[RESX:Confirm:Reject]')){afmodReject(" + dr["ForumId"].ToString() + "," + dr["TopicId"].ToString() + "," + dr["ReplyId"].ToString() + "," + dr["AuthorId"].ToString() + ");};\">[RESX:Reject]</span>");
                        }

                        if (this.bModDelete)
                        {
                            sb.Append("<span class=\"dnnTertiaryAction\" onclick=\"javascript:if(confirm('[RESX:Confirm:Delete]')){afmodDelete(" + dr["ForumId"].ToString() + "," + dr["TopicId"].ToString() + "," + dr["ReplyId"].ToString() + ");};\">[RESX:Delete]</span>");
                        }

                        if (this.bModEdit)
                        {
                            sb.Append("<span class=\"dnnTertiaryAction\" onclick=\"afmodEdit('" + this.TopicEditUrl(Convert.ToInt32(dr["ForumId"]), Convert.ToInt32(dr["TopicId"]), Convert.ToInt32(dr["ReplyId"])) + "');\">[RESX:Edit]</span>");
                        }

                        if (this.bModBan)
                        {
                            var banParams = new List<string> { $"{ParamKeys.ViewType}={Views.ModerateBan}", ParamKeys.ForumId + "=" + dr["ForumId"].ToString(), ParamKeys.TopicId + "=" + dr["TopicId"].ToString(), ParamKeys.ReplyId + "=" + Convert.ToInt32(dr["ReplyId"]), ParamKeys.AuthorId + "=" + Convert.ToInt32(dr["AuthorId"]), };
                            sb.Append("<a class=\"dnnSecondaryAction\" href=\"" + Utilities.NavigateURL(this.TabId, "", banParams.ToArray()) + "\" tooltip=\"[RESX:Tips:BanUser]\">[RESX:Ban]</a>");
                        }

                        sb.Append("</td></tr>");
                        sb.Append("<tr><td style=\"width:90px\" valign=\"top\">" + "<a href=\"" + Utilities.NavigateURL(this.PortalSettings.UserTabId, string.Empty, new[] { $"userId={dr["AuthorId"]}" }) + "\" class=\"af-profile-link\" rel=\"nofollow\" target=\"_blank\">" + dr["AuthorName"].ToString() + "</a></td>");
                        sb.Append("<td><div class=\"afrowsub\">[RESX:Subject]: " + dr["Subject"].ToString() + "</div><div class=\"afrowbod\">" + dr["Body"].ToString() + "</div>");

                        if (Convert.ToInt32(dr["ReplyId"]) > 0)
                        {
                            var @params = new List<string>() { $"{ParamKeys.ForumId}={dr["ForumId"]}", $"{ParamKeys.TopicId}={dr["TopicId"]}", $"{ParamKeys.ViewType}={Views.Topic}", };
                            var viewLink = Utilities.NavigateURL(this.TabId, string.Empty, @params.ToArray());
                            sb.Append("<div class=\"afrowbod\"><a href=\"" + viewLink + "\" class=\"dcf-link-text\" rel=\"nofollow\" target=\"_blank\">" + "[RESX:TopicReview]" + "</a></div>");
                        }

                        sb.Append(this.GetAttachments(Convert.ToInt32(dr["ContentId"]), this.PortalId, this.ModuleId, dtAttach) + "</td></tr>");
                        sb.Append("</table></div>");
                    }
                }
            }

            sb.Append("</table></div>");
            this.litTopics.Text = Utilities.LocalizeControl(sb.ToString());
        }

        private string TopicEditUrl(int forumId, int topicId, int replyId)
        {
            if (replyId == 0)
            {
                return this.NavigateUrl(this.TabId, string.Empty, new string[] { ParamKeys.ViewType + "=post", "action=te", ParamKeys.ForumId + "=" + forumId, ParamKeys.TopicId + "=" + topicId });
            }
            else
            {
                return this.NavigateUrl(this.TabId, string.Empty, new string[] { ParamKeys.ViewType + "=post", "action=re", ParamKeys.ForumId + "=" + forumId, ParamKeys.TopicId + "=" + topicId, "postid=" + replyId });
            }
        }

        private void SetPermissions(int fId)
        {
            DotNetNuke.Modules.ActiveForums.Entities.ForumInfo f = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(fId, this.ForumModuleId);

            bModerate = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(f?.Security?.ModerateRoleIds, ForumUser?.UserRoleIds);
            bModDelete = (bModerate && DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(f?.Security?.DeleteRoleIds, ForumUser?.UserRoleIds));
            bModEdit = (bModerate && DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(f?.Security?.EditRoleIds, ForumUser?.UserRoleIds));
            bModBan = (bModerate && DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(f?.Security?.BanRoleIds, ForumUser?.UserRoleIds));
        }

        private string GetAttachments(int contentId, int portalID, int moduleID, DataTable dtAttach)
        {
            string strHost = DotNetNuke.Common.Globals.AddHTTP(DotNetNuke.Common.Globals.GetDomainName(this.Request)) + "/";
            if (this.Request.IsSecureConnection)
            {
                strHost = strHost.Replace("http://", "https://");
            }

            // TODO: Add option for folder storage
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            string vpath = null;
            vpath = this.PortalSettings.HomeDirectory + "activeforums_Attach/";
            string fpath = null;
            fpath = Utilities.MapPath(this.PortalSettings.HomeDirectory + "activeforums_Attach/");
            dtAttach.DefaultView.RowFilter = "ContentId = " + contentId;
            foreach (DataRow dr in dtAttach.DefaultView.ToTable().Rows)
            {
                sb.Append("<br />");
                int attachId = Convert.ToInt32(dr["AttachId"]);
                string filename = dr["Filename"].ToString();
                string contentType = dr["ContentType"].ToString();
                if (dr.IsNull("FileData"))
                {
                    sb.Append($"<a href=\"/DesktopModules/ActiveForums/viewer.aspx?portalid={this.PortalId}&moduleid={this.ModuleId}&attachmentid={attachId}\" target=\"_blank\"><img src=\"/DesktopModules/ActiveForums/images/attach.gif\" border=\"0\" align=\"absmiddle\">Attachment: " + filename + "</a><br>");
                }
                else
                {
                    switch (contentType.ToLower())
                    {
                        case "image/jpeg":
                        case "image/pjpeg":
                        case "image/gif":
                        case "image/png":
                            sb.Append("<br /><span class=\"afimage\"><img src=\"" + strHost + "DesktopModules/ActiveForums/viewer.aspx?portalid=" + portalID + "&moduleid=" + moduleID + "&attachmentid=" + attachId + "\" border=0 align=center></span><br><br>");
                            break;
                        default:
                            sb.Append("<span class=\"afattachlink\"><a href=\"" + strHost + "DesktopModules/ActiveForums/viewer.aspx?portalid=" + portalID + "&moduleid=" + moduleID + "&attachmentid=" + attachId + "\" target=\"_blank\"><img src=\"" + strHost + "DesktopModules/ActiveForums/images/attach.gif\" border=\"0\" align=\"absmiddle\">Attachment: " + filename + "</a></span><br />");
                            break;
                    }
                }
            }

            sb.Append("<br />");

            return sb.ToString();
        }
        #endregion
    }
}
