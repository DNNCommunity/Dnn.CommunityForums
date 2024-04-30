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
using System;
using System.Data;

namespace DotNetNuke.Modules.ActiveForums
{
    public partial class af_modtopics_new : ForumBase
    {
        #region Private Members
        private bool bModDelete = false;
        private bool bModEdit = false;
        private bool bModApprove = false;
        private bool bModMove = false;
        private bool bCanMod = false;

        #endregion
        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            lblHeader.Text = Utilities.GetSharedResource("[RESX:PendingPosts]");
            cbMod.CallbackEvent += cbMod_Callback;

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (Request.IsAuthenticated && ForumUser.Profile.IsMod)
            {
                if (ForumId > 0)
                {
                    SetPermissions(ForumId);
                }


                if (!cbMod.IsCallback)
                {
                    BuildModList();
                }

            }
            else
            {
                Response.Redirect(NavigateUrl(TabId));
            }

        }
        private void cbMod_Callback(object sender, Modules.ActiveForums.Controls.CallBackEventArgs e)
        {
            SettingsInfo ms = SettingsBase.GetModuleSettings(ForumModuleId);
            DotNetNuke.Modules.ActiveForums.Entities.ForumInfo fi = null;
            if (e.Parameters.Length > 0)
            {
                if (ForumId < 1)
                {
                    SetPermissions(Convert.ToInt32(e.Parameters[1]));
                    fi = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(forumId: Convert.ToInt32(e.Parameters[1]), moduleId: ForumModuleId);
                }
                else
                {
                    fi = ForumInfo;
                }
                switch (e.Parameters[0].ToLowerInvariant())
                {
                    case "moddel":
                        {
                            if (bModDelete)
                            {
                                int delAction = ms.DeleteBehavior;
                                int tmpForumId = -1;
                                int tmpTopicId = -1;
                                int tmpReplyId = -1;
                                tmpForumId = Convert.ToInt32(e.Parameters[1]);
                                tmpTopicId = Convert.ToInt32(e.Parameters[2]);
                                tmpReplyId = Convert.ToInt32(e.Parameters[3]);
                                Author auth = null;
                                if (fi.ModDeleteTemplateId > 0)
                                {
                                    try
                                    {
                                        DotNetNuke.Modules.ActiveForums.Controllers.EmailController.SendEmailToModerators(fi.ModDeleteTemplateId, PortalId, tmpForumId, tmpTopicId, tmpReplyId, ForumModuleId, ForumTabId, string.Empty);
                                    }
                                    catch (Exception ex)
                                    {
                                        DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                                    }

                                }
                                if (tmpForumId > 0 & tmpTopicId > 0 && tmpReplyId == 0)
                                {
                                    DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(tmpTopicId);
                                    if (ti != null)
                                    {
                                        auth = ti.Author;
                                    }
                                    new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().DeleteById(tmpTopicId);

                                }
                                else if (tmpForumId > 0 & tmpTopicId > 0 & tmpReplyId > 0)
                                {
                                    DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo ri = DotNetNuke.Modules.ActiveForums.Controllers.ReplyController.GetReply(tmpReplyId);
                                    if (ri != null)
                                    {
                                        auth = ri.Author;
                                    }
                                    new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController().Reply_Delete(PortalId, tmpForumId, tmpTopicId, tmpReplyId, delAction);
                                }

                            }
                            break;
                        }
                    case "modreject":
                        {
                            int tmpForumId = 0;
                            int tmpTopicId = 0;
                            int tmpReplyId = 0;
                            int tmpAuthorId = 0;
                            tmpForumId = Convert.ToInt32(e.Parameters[1]);
                            tmpTopicId = Convert.ToInt32(e.Parameters[2]);
                            tmpReplyId = Convert.ToInt32(e.Parameters[3]);
                            tmpAuthorId = Convert.ToInt32(e.Parameters[4]);
                            ModController mc = new ModController();
                            mc.Mod_Reject(PortalId, ForumModuleId, UserId, tmpForumId, tmpTopicId, tmpReplyId);
                            if (fi.ModRejectTemplateId > 0 & tmpAuthorId > 0)
                            {
                                DotNetNuke.Entities.Users.UserInfo ui = DotNetNuke.Entities.Users.UserController.Instance.GetUser(PortalId, tmpAuthorId);
                                if (ui != null)
                                {
                                    Author au = new Author();
                                    au.AuthorId = tmpAuthorId;
                                    au.DisplayName = ui.DisplayName;
                                    au.Email = ui.Email;
                                    au.FirstName = ui.FirstName;
                                    au.LastName = ui.LastName;
                                    au.Username = ui.Username;
                                    DotNetNuke.Modules.ActiveForums.Controllers.EmailController.SendEmail(fi.ModRejectTemplateId, PortalId, ForumModuleId, TabId, tmpForumId, tmpTopicId, tmpReplyId, string.Empty, au);
                                }

                            }

                            break;
                        }
                    case "modappr":
                        {
                            int tmpForumId = -1;
                            int tmpTopicId = -1;
                            int tmpReplyId = -1;
                            tmpForumId = Convert.ToInt32(e.Parameters[1]);
                            tmpTopicId = Convert.ToInt32(e.Parameters[2]);
                            tmpReplyId = Convert.ToInt32(e.Parameters[3]);
                            if (tmpForumId > 0 & tmpTopicId > 0 && tmpReplyId == 0)
                            {
                                DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(tmpTopicId);
                                if (ti != null)
                                { 
                                    ti.IsApproved = true;
                                    DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Save(ti);
                                    DotNetNuke.Modules.ActiveForums.Controllers.TopicController.SaveToForum(ForumModuleId, tmpForumId, tmpTopicId, tmpReplyId);
                                    //TODO: Add Audit log for who approved topic
                                    if (fi.ModApproveTemplateId > 0 & ti.Author.AuthorId > 0)
                                    {
                                        DotNetNuke.Modules.ActiveForums.Controllers.EmailController.SendEmail(fi.ModApproveTemplateId, PortalId, ForumModuleId, TabId, tmpForumId, tmpTopicId, tmpReplyId, string.Empty, ti.Author);
                                    }
                                    DotNetNuke.Modules.ActiveForums.Controllers.TopicController.QueueApprovedTopicAfterAction(PortalId, ForumTabId, ForumModuleId, fi.ForumGroupId, ForumId, tmpTopicId, -1, ti.Content.AuthorId);
                                }
                            }
                            else if (tmpForumId > 0 & tmpTopicId > 0 & tmpReplyId > 0)
                            {
                                DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(tmpTopicId);
                                DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo ri = new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController().GetById(tmpReplyId);
                                if (ri != null)
                                {
                                    ri.IsApproved = true;
                                    new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController().Reply_Save(PortalId, ForumModuleId, ri);
                                    DotNetNuke.Modules.ActiveForums.Controllers.TopicController.SaveToForum(ModuleId, tmpForumId, tmpTopicId, tmpReplyId);
                                    //TODO: Add Audit log for who approved topic
                                    if (fi.ModApproveTemplateId > 0 & ri.Author.AuthorId > 0)
                                    {
                                        DotNetNuke.Modules.ActiveForums.Controllers.EmailController.SendEmail(fi.ModApproveTemplateId, PortalId, ForumModuleId, TabId, tmpForumId, tmpTopicId, tmpReplyId, string.Empty, ri.Author);
                                    }

                                    DotNetNuke.Modules.ActiveForums.Controllers.ReplyController.QueueUnapprovedReplyAfterAction(PortalId, ForumTabId, ForumModuleId, fi.ForumGroupId, tmpForumId, tmpTopicId, -tmpReplyId, ri.Content.AuthorId);

                                }

                            }


                            break;
                        }
                }
                
                DataCache.CacheClearPrefix(ModuleId, string.Format(CacheKeys.ForumViewPrefix, ModuleId));
            }
            BuildModList();
            litTopics.RenderControl(e.Output);
        }
        #endregion
        #region Private Members
        private void BuildModList()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            DataSet ds = DataProvider.Instance().Mod_Pending(PortalId, ModuleId, ForumId, UserId);
            DataTable dtContent = ds.Tables[0];
            DataTable dtAttach = ds.Tables[1];
            string tmpForum = string.Empty;

            sb.Append("<div id=\"afgrid\" style=\"position:relative;\"><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\">");
            foreach (DataRow dr in dtContent.Rows)
            {
                string forumKey = dr["ForumId"].ToString() + dr["ForumName"].ToString();
                if (forumKey != tmpForum)
                {
                    if (ForumId == -1)
                    {
                        SetPermissions(Convert.ToInt32(dr["ForumId"]));
                    }
                    if (bCanMod)
                    {
                        if (!(tmpForum == string.Empty))
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
                if (bCanMod)
                {
                    sb.Append("<div class=\"afmodrow\">");
                    sb.Append("<table width=\"99%\">");
                    sb.Append("<tr><td style=\"white-space:nowrap;\">" + Utilities.GetUserFormattedDateTime(Convert.ToDateTime(dr["DateCreated"]), PortalId, UserId) + "</td>");
                    sb.Append("<td align=\"right\">");
                    if (bModApprove)
                    {
                        sb.Append("<span class=\"afminibtn\" onclick=\"afmodApprove(" + dr["ForumId"].ToString() + "," + dr["TopicId"].ToString() + "," + dr["ReplyId"].ToString() + ");\" onmouseover=\"this.className='afminibtn_over';\" onmouseout=\"this.className='afminibtn';\">[RESX:Approve]</span>");
                    }
                    //If bModApprove And bModMove And CInt(dr("ReplyId")) = 0 Then
                    //    sb.Append("<span class=""afminibtn"" onmouseover=""this.className='afminibtn_over';"" onmouseout=""this.className='afminibtn';"">[RESX:MoveApprove]</span>")
                    //End If
                    if (bModApprove || bModEdit)
                    {
                        sb.Append("<span class=\"afminibtn\" onclick=\"javascript:if(confirm('[RESX:Confirm:Reject]')){afmodReject(" + dr["ForumId"].ToString() + "," + dr["TopicId"].ToString() + "," + dr["ReplyId"].ToString() + "," + dr["AuthorId"].ToString() + ");};\" onmouseover=\"this.className='afminibtn_over';\" onmouseout=\"this.className='afminibtn';\">[RESX:Reject]</span>");
                    }
                    if (bModDelete)
                    {
                        sb.Append("<span class=\"afminibtn\" onclick=\"javascript:if(confirm('[RESX:Confirm:Delete]')){afmodDelete(" + dr["ForumId"].ToString() + "," + dr["TopicId"].ToString() + "," + dr["ReplyId"].ToString() + ");};\" onmouseover=\"this.className='afminibtn_over';\" onmouseout=\"this.className='afminibtn';\">[RESX:Delete]</span>");
                    }
                    if (bModEdit)
                    {
                        sb.Append("<span class=\"afminibtn\" onclick=\"afmodEdit('" + TopicEditUrl(Convert.ToInt32(dr["ForumId"]), Convert.ToInt32(dr["TopicId"]), Convert.ToInt32(dr["ReplyId"])) + "');\" onmouseover=\"this.className='afminibtn_over';\" onmouseout=\"this.className='afminibtn';\">[RESX:Edit]</span>");
                    }


                    sb.Append("</td></tr>");
                    sb.Append("<tr><td style=\"width:90px\" valign=\"top\">" + dr["AuthorName"].ToString() + "</td>");
                    sb.Append("<td><div class=\"afrowsub\">[RESX:Subject]: " + dr["Subject"].ToString() + "</div><div class=\"afrowbod\">" + dr["Body"].ToString() + "</div>");
                    sb.Append(GetAttachments(Convert.ToInt32(dr["ContentId"]), PortalId, ModuleId, dtAttach) + "</td></tr>");
                    sb.Append("</table></div>");
                }

            }
            sb.Append("</table></div>");

            litTopics.Text = Utilities.LocalizeControl(sb.ToString());
        }
        private string TopicEditUrl(int ForumId, int TopicId, int ReplyId)
        {
            if (ReplyId == 0)
            {
                return NavigateUrl(TabId, "", new string[] { ParamKeys.ViewType + "=post", "action=te", ParamKeys.ForumId + "=" + ForumId, ParamKeys.TopicId + "=" + TopicId });
            }
            else
            {
                return NavigateUrl(TabId, "", new string[] { ParamKeys.ViewType + "=post", "action=re", ParamKeys.ForumId + "=" + ForumId, ParamKeys.TopicId + "=" + TopicId, "postid=" + ReplyId });
            }
        }
        private void SetPermissions(int fId)
        {
            DotNetNuke.Modules.ActiveForums.Entities.ForumInfo f = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(fId,ForumModuleId );
            bModDelete = false;
            bModApprove = false;
            bModEdit = false;
            bModMove = false;
            bCanMod = false;
            if (f != null)
            {
                bModDelete = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(f.Security.ModDelete, ForumUser.UserRoles);
                bModApprove = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(f.Security.ModApprove, ForumUser.UserRoles);
                bModMove = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(f.Security.ModMove, ForumUser.UserRoles);
                bModEdit = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(f.Security.ModEdit, ForumUser.UserRoles);
                if (bModDelete || bModApprove || bModMove || bModEdit)
                {
                    bCanMod = true;
                }

            }

        }
        private string GetAttachments(int ContentId, int PortalID, int ModuleID, DataTable dtAttach)
        {
            string strHost = DotNetNuke.Common.Globals.AddHTTP(DotNetNuke.Common.Globals.GetDomainName(Request)) + "/";
            if (Request.IsSecureConnection)
            {
                strHost = strHost.Replace("http://", "https://");
            }
            //TODO: Add option for folder storage
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            string vpath = null;
            vpath = PortalSettings.HomeDirectory + "activeforums_Attach/";
            string fpath = null;
            fpath = Utilities.MapPath(PortalSettings.HomeDirectory + "activeforums_Attach/");
            dtAttach.DefaultView.RowFilter = "ContentId = " + ContentId;
            foreach (DataRow dr in dtAttach.DefaultView.ToTable().Rows)
            {
                sb.Append("<br />");
                int attachId = Convert.ToInt32(dr["AttachId"]);
                string Filename = dr["Filename"].ToString();
                string contentType = dr["ContentType"].ToString();
                if (dr.IsNull("FileData"))
                {
                    sb.Append($"<a href=\"/DesktopModules/ActiveForums/viewer.aspx?portalid={PortalId}&moduleid={ModuleId}&attachmentid={attachId}\" target=\"_blank\"><img src=\"/DesktopModules/ActiveForums/images/attach.gif\" border=\"0\" align=\"absmiddle\">Attachment: " + Filename + "</a><br>");
                }
                else
                {
                    switch (contentType.ToLower())
                    {
                        case "image/jpeg":
                        case "image/pjpeg":
                        case "image/gif":
                        case "image/png":
                            sb.Append("<br /><span class=\"afimage\"><img src=\"" + strHost + "DesktopModules/ActiveForums/viewer.aspx?portalid=" + PortalID + "&moduleid=" + ModuleID + "&attachmentid=" + attachId + "\" border=0 align=center></span><br><br>");
                            break;
                        default:
                            sb.Append("<span class=\"afattachlink\"><a href=\"" + strHost + "DesktopModules/ActiveForums/viewer.aspx?portalid=" + PortalID + "&moduleid=" + ModuleID + "&attachmentid=" + attachId + "\" target=\"_blank\"><img src=\"" + strHost + "DesktopModules/ActiveForums/images/attach.gif\" border=\"0\" align=\"absmiddle\">Attachment: " + Filename + "</a></span><br />");
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
