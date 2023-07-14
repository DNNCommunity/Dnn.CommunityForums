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
using System.Collections;
using System.Collections.Generic;
using System.Data;

using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.ClientCapability;
using DotNetNuke.Services.Social.Notifications;
using DotNetNuke.Modules.ActiveForums.DAL2;

namespace DotNetNuke.Modules.ActiveForums
{

    public partial class af_modban : ForumBase
    {


        #region Controls
        #endregion

        #region Event Handlers
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {

                if (!Request.IsAuthenticated)
                {
                    Response.Redirect(NavigateUrl(TabId));
                }


            }
            catch (Exception exc)
            {
                //DotNetNuke.Services.Exceptions.Exceptions.ProcessModuleLoadException(Me, exc, False)
            }
        }
        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            System.IO.StringWriter stringWriter = new System.IO.StringWriter();
            HtmlTextWriter htmlWriter = new HtmlTextWriter(stringWriter);
            base.Render(htmlWriter);
            string html = stringWriter.ToString();
            html = html.Replace("[AF:LINK:FORUMMAIN]", "<a href=\"" + NavigateUrl(TabId) + "\">[RESX:FORUMS]</a>");
            html = html.Replace("[AF:LINK:FORUMGROUP]", "<a href=\"" + NavigateUrl(TabId, "", ParamKeys.GroupId + "=" + ForumInfo.ForumGroupId) + "\">" + ForumInfo.GroupName + "</a>");
            html = html.Replace("[AF:LINK:FORUMNAME]", "<a href=\"" + NavigateUrl(TabId, "", new string[] { ParamKeys.ForumId + "=" + ForumId, ParamKeys.ViewType + "=" + Views.Topics }) + "\">" + ForumInfo.ForumName + "</a>");

            html = Utilities.LocalizeControl(html);

            writer.Write(html);
        }
        #endregion

        #region  Web Form Designer Generated Code

        //This call is required by the Web Form Designer.
        [System.Diagnostics.DebuggerStepThrough()]
        private void InitializeComponent()
        {
        }
        protected Panel pnlMessage;

        //NOTE: The following placeholder declaration is required by the Web Form Designer.
        //Do not delete or move it.
        private object designerPlaceholderDeclaration;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            //CODEGEN: This method call is required by the Web Form Designer
            //Do not modify it using the code editor.
            this.LocalResourceFile = "~/DesktopModules/ActiveForums/App_LocalResources/af_modban.ascx.resx";
            InitializeComponent();

            btnBan.Click += new System.EventHandler(btnBan_Click);
            btnCancel.Click += new System.EventHandler(btnCancel_Click);

        }

        #endregion


        private void btnCancel_Click(object sender, System.EventArgs e)
        {
            Response.Redirect(DotNetNuke.Common.Globals.NavigateURL(TabId, "", new string[] { ParamKeys.ForumId + "=" + ForumId, ParamKeys.ViewType + "=" + Views.Topic, ParamKeys.TopicId + "=" + TopicId }));
        }

        private void btnBan_Click(object sender, System.EventArgs e)
        {
            if (Request.IsAuthenticated)
            {
                string sUrl;
                if (SocialGroupId > 0)
                {
                    sUrl = NavigateUrl(Convert.ToInt32(Request.QueryString["TabId"]), "", new string[] { ParamKeys.ForumId + "=" + ForumId,(ReplyId > 0 ? ParamKeys.TopicId + "=" + TopicId : string.Empty), ParamKeys.ViewType + "=confirmaction", ParamKeys.ConfirmActionId + "=" + ConfirmActions.UserBanned + "&" + ParamKeys.GroupIdName + "=" + SocialGroupId });
                }
                else
                {
                    sUrl = NavigateUrl(Convert.ToInt32(Request.QueryString["TabId"]), "", new string[] { ParamKeys.ForumId + "=" + ForumId, (ReplyId > 0 ? ParamKeys.TopicId + "=" + TopicId : string.Empty), ParamKeys.ViewType + "=confirmaction", ParamKeys.ConfirmActionId + "=" + ConfirmActions.UserBanned });
                }

                int userToBan = -1;
                if (ReplyId > 0)
                {
                    ReplyController replyController = new ReplyController();
                    ReplyInfo reply = replyController.Reply_Get( PortalId: PortalId, ModuleId: ForumModuleId, TopicId: TopicId, ReplyId: ReplyId);
                    userToBan = reply.Author.AuthorId;
                }
                else
                {
                    TopicsController topicController = new TopicsController();
                    TopicInfo topic = topicController.Topics_Get(PortalId, ForumModuleId, TopicId, ForumId, -1, false);
                    userToBan = topic.Author.AuthorId;
                }

                if (userToBan > 0)
                {
                    DataProvider.Instance().Topics_Delete_For_User(ModuleId: ForumModuleId, UserId: userToBan, DelBehavior: DataCache.MainSettings(ForumModuleId).DeleteBehavior);
                    DotNetNuke.Entities.Users.UserInfo user = DotNetNuke.Entities.Users.UserController.GetUserById(portalId: PortalId, userId: userToBan);
                    user.Membership.Approved = false;
                    DotNetNuke.Entities.Users.UserController.UpdateUser(portalId: PortalId, user: user, loggedAction: true);

                    DataCache.CacheClearPrefix(string.Format("AF-FV-{0}-{1}", PortalId, ModuleId));
                }
                Response.Redirect(sUrl);
            }
        }
    }

}
