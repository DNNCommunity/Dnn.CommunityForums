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
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.Services.Social.Notifications;
using DotNetNuke.UI.UserControls;
namespace DotNetNuke.Modules.ActiveForums
{
    public partial class af_modban : ForumBase
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.LocalResourceFile = "~/DesktopModules/ActiveForums/App_LocalResources/af_modban.ascx.resx"; 
            btnBan.Click += new System.EventHandler(btnBan_Click);
            btnCancel.Click += new System.EventHandler(btnCancel_Click);
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                if (!Request.IsAuthenticated)
                {
                    Response.Redirect(Utilities.NavigateURL(TabId));
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }
        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            System.IO.StringWriter stringWriter = new System.IO.StringWriter();
            HtmlTextWriter htmlWriter = new HtmlTextWriter(stringWriter);
            base.Render(htmlWriter);
            string html = stringWriter.ToString();
            html = html.Replace("[AF:LINK:FORUMMAIN]", "<a href=\"" + Utilities.NavigateURL(TabId) + "\">[RESX:FORUMS]</a>");
            html = html.Replace("[AF:LINK:FORUMGROUP]", "<a href=\"" + Utilities.NavigateURL(TabId, "", ParamKeys.GroupId + "=" + ForumInfo.ForumGroupId) + "\">" + ForumInfo.GroupName + "</a>");
            html = html.Replace("[AF:LINK:FORUMNAME]", "<a href=\"" + Utilities.NavigateURL(TabId, "", new string[] { ParamKeys.ForumId + "=" + ForumId, ParamKeys.ViewType + "=" + Views.Topics }) + "\">" + ForumInfo.ForumName + "</a>");
            html = Utilities.LocalizeControl(html);
            writer.Write(html);
        } 
        private void btnCancel_Click(object sender, System.EventArgs e)
        {
            Response.Redirect(Utilities.NavigateURL(TabId, "", new string[] { ParamKeys.ForumId + "=" + ForumId, ParamKeys.ViewType + "=" + Views.Topic, ParamKeys.TopicId + "=" + TopicId }));
        }
        private void btnBan_Click(object sender, System.EventArgs e)
        {
            if (!Request.IsAuthenticated)
            {
                Response.Redirect(Utilities.NavigateURL(TabId, "", new string[] { ParamKeys.ForumId + "=" + ForumId, ParamKeys.ViewType + "=" + Views.Topic, ParamKeys.TopicId + "=" + TopicId }));
            }
            else
            {
                DotNetNuke.Entities.Users.UserInfo user = DotNetNuke.Entities.Users.UserController.GetUserById(portalId: PortalId, userId: AuthorId);
                DotNetNuke.Modules.ActiveForums.Entities.TopicInfo topic = new TopicsController().Topics_Get(PortalId, ModuleId, TopicId, ForumId, -1, false);
                string sBody = string.Empty;
                string authorName = string.Empty;
                string sSubject = string.Empty;
                string sTopicURL = string.Empty;
                sTopicURL = topic.TopicUrl;
                if (ReplyId > 0 & TopicId != ReplyId)
                {
                    ReplyController rc = new ReplyController();
                    DotNetNuke.Modules.ActiveForums.ReplyInfo reply = rc.Reply_Get(PortalId, ModuleId, TopicId, ReplyId);
                    sBody = reply.Content.Body;
                    sSubject = reply.Content.Subject;
                    authorName = reply.Author.DisplayName;

                }
                else
                {
                    sBody = topic.Content.Body;
                    sSubject = topic.Content.Subject;
                    authorName = topic.Author.DisplayName;

                }
                string subject = Utilities.GetSharedResource("RESX:BanAlertSubject");
                subject = subject.Replace("[Username]", user.Username);
                subject = subject.Replace("[DisplayName]", user.DisplayName);
                subject = subject.Replace("[BannedBy]", UserInfo.DisplayName);
                string body = Utilities.GetSharedResource("RESX:BanAlertBody");
                body = body.Replace("[Subject]", sSubject);
                body = body.Replace("[Post]", sBody);

                List<DotNetNuke.Entities.Users.UserInfo> mods = Utilities.GetListOfModerators(PortalId, ForumModuleId, ForumId);

                string notificationKey = string.Format("{0}:{1}:{2}:{3}:{4}", TabId, ForumModuleId, ForumId, TopicId, ReplyId);

                Notification notification = new Notification(); 
                NotificationType notificationType = NotificationsController.Instance.GetNotificationType("DCF-UserBanned");

                notification.NotificationTypeID = notificationType.NotificationTypeId;
                notification.Subject = subject;
                notification.Body = body;
              
                notification.IncludeDismissAction = true;
                notification.SenderUserID = UserInfo.UserID;
                notification.Context = notificationKey;

                NotificationsController.Instance.SendNotification(notification, PortalId, null, mods);

                var log = new DotNetNuke.Services.Log.EventLog.LogInfo { LogTypeKey = DotNetNuke.Abstractions.Logging.EventLogType.ADMIN_ALERT.ToString() };
                log.LogProperties.Add(new LogDetailInfo("Module", ModuleContext.Configuration.ModuleTitle));
                string userBannedMsg = String.Format(Utilities.GetSharedResource("RESX:UserBanned"), user.Username);
                log.AddProperty("Message", userBannedMsg);
                DotNetNuke.Services.Log.EventLog.LogController.Instance.AddLog(log);

                DotNetNuke.Modules.ActiveForums.Controllers.UserController.BanUser(PortalId: PortalId, ModuleId: ForumModuleId, UserId: AuthorId);

                Response.Redirect(Utilities.NavigateURL(TabId, "", new string[] { ParamKeys.ForumId + "=" + ForumId, (ReplyId > 0 ? ParamKeys.TopicId + "=" + TopicId : string.Empty), ParamKeys.ViewType + "=confirmaction", ParamKeys.ConfirmActionId + "=" + ConfirmActions.UserBanned + (SocialGroupId > 0 ? "&" + Literals.GroupId + "=" + SocialGroupId : string.Empty) }));
            }
        }
    }
}
