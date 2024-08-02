// Copyright (c) 2013-2024 by DNN Community
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
    using System.Web.UI;

    public partial class af_modban : ForumBase
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.LocalResourceFile = "~/DesktopModules/ActiveForums/App_LocalResources/af_modban.ascx.resx";
            this.btnBan.Click += new System.EventHandler(this.btnBan_Click);
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                if (!this.Request.IsAuthenticated)
                {
                    this.Response.Redirect(Utilities.NavigateURL(this.TabId));
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
            html = html.Replace("[AF:LINK:FORUMMAIN]", "<a href=\"" + Utilities.NavigateURL(this.TabId) + "\">[RESX:FORUMS]</a>");
            html = html.Replace("[AF:LINK:FORUMGROUP]", "<a href=\"" + Utilities.NavigateURL(this.TabId, string.Empty, ParamKeys.GroupId + "=" + this.ForumInfo.ForumGroupId) + "\">" + this.ForumInfo.GroupName + "</a>");
            html = html.Replace("[AF:LINK:FORUMNAME]", "<a href=\"" + Utilities.NavigateURL(this.TabId, string.Empty, new string[] { ParamKeys.ForumId + "=" + this.ForumId, ParamKeys.ViewType + "=" + Views.Topics }) + "\">" + this.ForumInfo.ForumName + "</a>");
            html = Utilities.LocalizeControl(html);
            writer.Write(html);
        }

        private void btnCancel_Click(object sender, System.EventArgs e)
        {
            this.Response.Redirect(Utilities.NavigateURL(this.TabId, string.Empty, new string[] { ParamKeys.ForumId + "=" + this.ForumId, ParamKeys.ViewType + "=" + Views.Topic, ParamKeys.TopicId + "=" + this.TopicId }));
        }

        private void btnBan_Click(object sender, System.EventArgs e)
        {
            if (!this.Request.IsAuthenticated)
            {
                this.Response.Redirect(Utilities.NavigateURL(this.TabId, string.Empty, new string[] { ParamKeys.ForumId + "=" + this.ForumId, ParamKeys.ViewType + "=" + Views.Topic, ParamKeys.TopicId + "=" + this.TopicId }));
            }
            else
            {
                DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.BanUser(portalId: this.PortalId, moduleId: this.ForumModuleId, moduleTitle: this.ModuleContext.Configuration.ModuleTitle, tabId: this.TabId, forumId: this.ForumId, topicId: this.TopicId, replyId: this.ReplyId, bannedBy: this.UserInfo, authorId: this.AuthorId);
                this.Response.Redirect(Utilities.NavigateURL(this.TabId, string.Empty, new string[] { ParamKeys.ForumId + "=" + this.ForumId, this.ReplyId > 0 ? ParamKeys.TopicId + "=" + this.TopicId : string.Empty, ParamKeys.ViewType + "=confirmaction", ParamKeys.ConfirmActionId + "=" + ConfirmActions.UserBanned + (this.SocialGroupId > 0 ? "&" + Literals.GroupId + "=" + this.SocialGroupId : string.Empty) }));
            }
        }
    }
}
