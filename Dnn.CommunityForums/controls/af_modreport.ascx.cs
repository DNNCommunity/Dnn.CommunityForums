//
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
namespace DotNetNuke.Modules.ActiveForums
{
    using System;
    using System.Collections.Generic;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Services.Social.Notifications;

    public partial class af_modreport : ForumBase
    {

        #region Controls
        #endregion

        #region Event Handlers
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {

                if (this.Request.IsAuthenticated)
                {

                    string strReasons = this.GetSharedResource("[RESX:ReasonOptions]");
                    int i = 0;
                    foreach (string strReason in strReasons.Split(new char[] { ';' }))
                    {
                        if (!(strReason == ""))
                        {
                            this.drpReasons.Items.Insert(i, new ListItem(strReason, strReason));
                            i += 1;
                        }
                    }

                }
                else
                {
                    this.Response.Redirect(this.NavigateUrl(this.TabId));
                }

            }
            catch (Exception exc)
            {
                // DotNetNuke.Services.Exceptions.Exceptions.ProcessModuleLoadException(Me, exc, False)
            }
        }

        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            System.IO.StringWriter stringWriter = new System.IO.StringWriter();
            HtmlTextWriter htmlWriter = new HtmlTextWriter(stringWriter);
            base.Render(htmlWriter);
            string html = stringWriter.ToString();
            html = html.Replace("[AF:LINK:FORUMMAIN]", "<a href=\"" + this.NavigateUrl(this.TabId) + "\">[RESX:FORUMS]</a>");
            html = html.Replace("[AF:LINK:FORUMGROUP]", "<a href=\"" + this.NavigateUrl(this.TabId, "", ParamKeys.GroupId + "=" + this.ForumInfo.ForumGroupId) + "\">" + this.ForumInfo.GroupName + "</a>");
            html = html.Replace("[AF:LINK:FORUMNAME]", "<a href=\"" + this.NavigateUrl(this.TabId, "", new string[] { ParamKeys.ForumId + "=" + this.ForumId, ParamKeys.ViewType + "=" + Views.Topics }) + "\">" + this.ForumInfo.ForumName + "</a>");

            html = Utilities.LocalizeControl(html);

            writer.Write(html);
        }
        #endregion

        #region  Web Form Designer Generated Code

        // This call is required by the Web Form Designer.
        [System.Diagnostics.DebuggerStepThrough()]
        private void InitializeComponent()
        {
        }

        protected Panel pnlMessage;

        // NOTE: The following placeholder declaration is required by the Web Form Designer.
        // Do not delete or move it.
        private object designerPlaceholderDeclaration;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // CODEGEN: This method call is required by the Web Form Designer
            // Do not modify it using the code editor.
            this.LocalResourceFile = Globals.ModulePath + "app_localresources/af_modalert.ascx.resx";
            this.InitializeComponent();

            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);

        }

        #endregion

        private void btnCancel_Click(object sender, System.EventArgs e)
        {
            this.Response.Redirect(Utilities.NavigateURL(this.TabId, "", new string[] { ParamKeys.ForumId + "=" + this.ForumId, ParamKeys.ViewType + "=" + Views.Topic, ParamKeys.TopicId + "=" + this.TopicId }));
        }

        private void btnSend_Click(object sender, System.EventArgs e)
        {
            if (this.Request.IsAuthenticated)
            {
                string Comments = this.drpReasons.SelectedItem.Value + "<br>";
                Comments += Utilities.CleanString(this.PortalId, this.txtComments.Text, false, EditorTypes.TEXTBOX, false, false, this.ModuleId, string.Empty, false);
                string sUrl = this.SocialGroupId > 0
                    ? Utilities.NavigateURL(Convert.ToInt32(this.Request.QueryString["TabId"]), "", new string[] { ParamKeys.ForumId + "=" + this.ForumId, ParamKeys.TopicId + "=" + this.TopicId, ParamKeys.ViewType + "=confirmaction", ParamKeys.ConfirmActionId + "=" + ConfirmActions.AlertSent + "&" + Literals.GroupId + "=" + this.SocialGroupId })
                    : Utilities.NavigateURL(Convert.ToInt32(this.Request.QueryString["TabId"]), "", new string[] { ParamKeys.ForumId + "=" + this.ForumId, ParamKeys.TopicId + "=" + this.TopicId, ParamKeys.ViewType + "=confirmaction", ParamKeys.ConfirmActionId + "=" + ConfirmActions.AlertSent });
                DotNetNuke.Modules.ActiveForums.Entities.TopicInfo topic = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(this.TopicId);
                string sBody = string.Empty;
                string authorName = string.Empty;
                string sSubject = string.Empty;
                string sTopicURL = topic.TopicUrl;
                if (this.ReplyId > 0 & this.TopicId != this.ReplyId)
                {
                    DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo reply = DotNetNuke.Modules.ActiveForums.Controllers.ReplyController.GetReply(this.ReplyId);
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

                ControlUtils ctlUtils = new ControlUtils();
                string fullURL = ctlUtils.BuildUrl(this.TabId, this.ForumModuleId, this.ForumInfo.ForumGroup.PrefixURL, this.ForumInfo.PrefixURL, this.ForumInfo.ForumGroupId, this.ForumInfo.ForumID, this.TopicId, sTopicURL, -1, -1, string.Empty, 1, this.ReplyId, this.SocialGroupId);
                string subject = Utilities.GetSharedResource("AlertSubject");
                subject = subject.Replace("[DisplayName]", authorName);
                subject = subject.Replace("[Subject]", sSubject);
                subject = subject.Replace("[FlaggedBy]", this.UserInfo.DisplayName);
                string body = Utilities.GetSharedResource("AlertBody");
                body = body.Replace("[Post]", sBody);
                body = body.Replace("[Comment]", Comments);
                body = body.Replace("[URL]", fullURL);
                body = body.Replace("[Reason]", this.drpReasons.SelectedItem.Value);

                Notification notification = new Notification();
                NotificationType notificationType = NotificationsController.Instance.GetNotificationType(Globals.ContentAlertNotificationType);
                notification.NotificationTypeID = notificationType.NotificationTypeId;
                notification.Subject = subject;
                notification.Body = body;
                notification.IncludeDismissAction = false;
                notification.SenderUserID = this.UserInfo.UserID;
                notification.Context = DotNetNuke.Modules.ActiveForums.Controllers.ModerationController.BuildNotificationContextKey(this.TabId, this.ForumModuleId, this.ForumId, this.TopicId, this.ReplyId);

                var modRoles = DotNetNuke.Modules.ActiveForums.Controllers.ModerationController.GetModeratorRoles(this.PortalId, this.ForumModuleId, this.ForumId);
                NotificationsController.Instance.SendNotification(notification, this.PortalId, modRoles, null);

                this.Response.Redirect(sUrl);
            }
        }
    }

}
