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
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Web.UI;

    using DotNetNuke.Modules.ActiveForums.Entities;

    using TopicInfo = DotNetNuke.Modules.ActiveForums.Entities.TopicInfo;

    public partial class af_sendto : ForumBase
    {
        private bool bcUpdated = false;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            string warnImg = "<img src=\"" + this.ImagePath + "/images/warning.png\" />";
            this.reqEmail.Text = warnImg;
            this.reqMessage.Text = warnImg;
            this.reqName.Text = warnImg;
            this.reqSubject.Text = warnImg;
            this.regEmail.Text = warnImg;
            this.regEmail.ValidationExpression = "\\b[a-zA-Z0-9._%\\-+']+@[a-zA-Z0-9.\\-]+\\.[a-zA-Z]{2,4}\\b";
            string topicSubject = string.Empty;
            if (this.TopicId > 0)
            {
                TopicsController tc = new TopicsController();
                DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(this.TopicId);
                if (ti != null)
                {
                    if (DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this.ForumInfo.Security.Read, this.ForumUser.UserRoles))
                    {
                        if (!this.Page.IsPostBack)
                        {
                            string subjectDefault = this.GetSharedResource("[RESX:EmailSubjectDefault]");
                            topicSubject = ti.Content.Subject;
                            subjectDefault = subjectDefault.Replace("[SUBJECT]", ti.Content.Subject);

                            this.txtRecipSubject.Text = subjectDefault;
                            string messageDefault = this.GetSharedResource("[RESX:EmailMessageDefault]");
                            string sURL = this.NavigateUrl(this.TabId, string.Empty, new string[] { ParamKeys.ForumId + "=" + this.ForumId, ParamKeys.ViewType + "=" + Views.Topic, ParamKeys.TopicId + "=" + this.TopicId });
                            if (this.MainSettings.UseShortUrls)
                            {
                                sURL = this.NavigateUrl(this.TabId, string.Empty, new string[] { ParamKeys.TopicId + "=" + this.TopicId });
                            }

                            messageDefault = messageDefault.Replace("[TOPICLINK]", sURL);
                            messageDefault = messageDefault.Replace("[DISPLAYNAME]", UserProfiles.GetDisplayName(this.PortalSettings, this.ModuleId, false, false, false, this.UserId, this.UserInfo.Username, this.UserInfo.FirstName, this.UserInfo.LastName, this.UserInfo.DisplayName));
                            this.txtMessage.Text = messageDefault;
                        }
                    }

                    if (this.MainSettings.UseSkinBreadCrumb)
                    {
                        string sCrumb = "<a href=\"" + this.NavigateUrl(this.TabId, string.Empty, ParamKeys.GroupId + "=" + this.ForumGroupId) + "\">" + this.ForumInfo.GroupName + "</a>|";
                        if (this.MainSettings.UseShortUrls)
                        {
                            sCrumb += "<a href=\"" + this.NavigateUrl(this.TabId, string.Empty, ParamKeys.ForumId + "=" + this.ForumId) + "\">" + this.ForumInfo.ForumName + "</a>";
                            sCrumb += "|<a href=\"" + this.NavigateUrl(this.TabId, string.Empty, ParamKeys.TopicId + "=" + this.TopicId) + "\">" + topicSubject + "</a>";
                        }
                        else
                        {
                            sCrumb += "<a href=\"" + this.NavigateUrl(this.TabId, string.Empty, new string[] { ParamKeys.ForumId + "=" + this.ForumId, ParamKeys.ViewType + "=" + Views.Topics }) + "\">" + this.ForumInfo.ForumName + "</a>";
                            sCrumb += "|<a href=\"" + this.NavigateUrl(this.TabId, string.Empty, new string[] { ParamKeys.ForumId + "=" + this.ForumId, ParamKeys.ViewType + "=" + Views.Topic, ParamKeys.TopicId + "=" + this.TopicId }) + "\">" + topicSubject + "</a>";
                        }

                        if (Environment.UpdateBreadCrumb(this.Page.Controls, sCrumb))
                        {
                            this.bcUpdated = true;
                        }
                    }
                }
                else
                {
                    this.Response.Redirect(this.NavigateUrl(this.TabId));
                }
            }
        }

        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            System.IO.StringWriter stringWriter = new System.IO.StringWriter();
            HtmlTextWriter htmlWriter = new HtmlTextWriter(stringWriter);
            base.Render(htmlWriter);
            string html = stringWriter.ToString();
            if (this.bcUpdated)
            {
                html = html.Replace("<div class=\"afcrumb\">[AF:LINK:FORUMMAIN] > [AF:LINK:FORUMGROUP] > [AF:LINK:FORUMNAME]</div>", string.Empty);
            }
            else
            {
                html = html.Replace("[AF:LINK:FORUMMAIN]", "<a href=\"" + this.NavigateUrl(this.TabId) + "\">[RESX:FORUMS]</a>");
                html = html.Replace("[AF:LINK:FORUMGROUP]", "<a href=\"" + this.NavigateUrl(this.TabId, string.Empty, ParamKeys.GroupId + "=" + this.ForumInfo.ForumGroupId) + "\">" + this.ForumInfo.GroupName + "</a>");
                html = html.Replace("[AF:LINK:FORUMNAME]", "<a href=\"" + this.NavigateUrl(this.TabId, string.Empty, new string[] { ParamKeys.ForumId + "=" + this.ForumId, ParamKeys.ViewType + "=" + Views.Topics }) + "\">" + this.ForumInfo.ForumName + "</a>");
            }

            html = Utilities.LocalizeControl(html);

            writer.Write(html);
        }

        private void btnCancel_Click(object sender, System.EventArgs e)
        {
            this.Response.Redirect(this.NavigateUrl(Convert.ToInt32(this.Request.QueryString["TabId"]), string.Empty, new string[] { ParamKeys.ForumId + "=" + this.ForumId, ParamKeys.ViewType + "=" + Views.Topic, ParamKeys.TopicId + "=" + this.TopicId }));
        }

        private void btnSend_Click(object sender, System.EventArgs e)
        {
            this.Page.Validate();
            if (this.Page.IsValid)
            {
                string sSubject = this.txtRecipSubject.Text;
                string sEmail = this.txtRecipEmail.Text;
                string sEmailName = this.txtRecipName.Text;
                string sMessage = this.txtMessage.Text;
                sSubject = Utilities.CleanString(this.PortalId, sSubject.Trim(), false, EditorTypes.TEXTBOX, false, false, this.ModuleId, string.Empty, false);
                sMessage = Utilities.CleanString(this.PortalId, sMessage.Trim(), false, EditorTypes.TEXTBOX, false, false, this.ModuleId, string.Empty, false);
                string sUrl = this.NavigateUrl(Convert.ToInt32(this.Request.QueryString["TabId"]), string.Empty, new string[] { ParamKeys.ForumId + "=" + this.ForumId, ParamKeys.TopicId + "=" + this.TopicId, ParamKeys.ViewType + "=confirmaction", ParamKeys.ConfirmActionId + "=" + ConfirmActions.SendToComplete });
                try
                {
                    if (!(sMessage == string.Empty) && !(sSubject == string.Empty))
                    {
                        DotNetNuke.Modules.ActiveForums.Controllers.EmailController.SendNotification(this.PortalId, moduleId: this.ModuleId, fromEmail: this.UserInfo.Email, toEmail: sEmail, subject: sSubject, body: sMessage);
                    }
                }
                catch (Exception ex)
                {
                    // Response.Redirect(NavigateUrl(CInt(Request.QueryString["TabId"]), "", New String() {ParamKeys.ForumId & "=" & ForumId, ParamKeys.TopicId & "=" & TopicId, ParamKeys.ViewType & "=confirmaction", ParamKeys.ConfirmActionId & "=" & ConfirmActions.SendToFailed}))
                }

                this.Response.Redirect(sUrl);
            }
        }
    }
}
