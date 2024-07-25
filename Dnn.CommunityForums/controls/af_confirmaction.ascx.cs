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
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    public partial class af_confirmaction_new : ForumBase
    {

        #region Event Handlers
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                // Put user code to initialize the page here
                if (this.Request.Params["afmsg"] != null)
                {
                    switch (this.Request.Params["afmsg"].ToUpper())
                    {
                        case "MOVE":
                            this.lblMessage.Text = Utilities.GetSharedResource("[RESX:Topic:Moved]", "Messages.ascx");
                            break;
                        case "MODALERT":
                            this.lblMessage.Text = Utilities.GetSharedResource("[RESX:Topic:Alert]", "Messages.ascx");
                            break;
                        case "PENDINGMOD":
                            this.lblMessage.Text = Utilities.GetSharedResource("[RESX:Topic:PendingMod]", "Messages.ascx");
                            this.hypPost.Visible = false;
                            break;
                        case "EMAILSENT":
                            this.lblMessage.Text = Utilities.GetSharedResource("[RESX:Email:Sent]", "Messages.ascx");
                            break;
                        case "POSTSUBMIT":
                            this.lblMessage.Text = Utilities.GetSharedResource("[RESX:Topic:Submit]", "Messages.ascx");
                            break;
                    }
                }
                else if (this.Request.QueryString[ParamKeys.ConfirmActionId] != null)
                {
                    ConfirmActions action = (ConfirmActions)Enum.Parse(typeof(ConfirmActions), this.Request.QueryString[ParamKeys.ConfirmActionId], true);
                    switch (action)
                    {
                        case ConfirmActions.AlertSent:
                            this.lblMessage.Text = Utilities.GetSharedResource("[RESX:Topic:Alert]", "Messages.ascx");
                            break;
                        case ConfirmActions.MessageDeleted:
                            this.lblMessage.Text = Utilities.GetSharedResource("[RESX:Topic:Deleted]", "Messages.ascx");
                            break;
                        case ConfirmActions.MessageMoved:
                            this.lblMessage.Text = Utilities.GetSharedResource("[RESX:Topic:Moved]", "Messages.ascx");
                            break;
                        case ConfirmActions.MessagePending:
                            this.lblMessage.Text = Utilities.GetSharedResource("[RESX:Topic:PendingMod]", "Messages.ascx");
                            break;
                        case ConfirmActions.TopicSaved:
                            this.lblMessage.Text = Utilities.GetSharedResource("[RESX:Topic:Saved]", "Messages.ascx");
                            break;
                        case ConfirmActions.TopicDeleted:
                            this.lblMessage.Text = Utilities.GetSharedResource("[RESX:Topic:Deleted]", "Messages.ascx");
                            break;
                        case ConfirmActions.ReplyDeleted:
                            this.lblMessage.Text = Utilities.GetSharedResource("[RESX:Reply:Deleted]", "Messages.ascx");
                            break;
                        case ConfirmActions.ReplySaved:
                            this.lblMessage.Text = Utilities.GetSharedResource("[RESX:Reply:Saved]", "Messages.ascx");
                            break;
                        case ConfirmActions.SendToComplete:
                            this.lblMessage.Text = Utilities.GetSharedResource("[RESX:Email:Sent]", "Messages.ascx");
                            break;
                        case ConfirmActions.SendToFailed:
                            this.lblMessage.Text = Utilities.GetSharedResource("[RESX:Email:Failed]", "Messages.ascx");
                            break;
                        case ConfirmActions.UserBanned:
                            this.lblMessage.Text = Utilities.GetSharedResource("[RESX:User:Banned]", "Messages.ascx");
                            break;
                    }

                }

                this.hypForums.NavigateUrl = this.NavigateUrl(this.TabId, "", new string[] { ParamKeys.ViewType + "=" + Views.Topics, ParamKeys.ForumId + "=" + this.ForumId });
                this.hypPost.NavigateUrl = this.NavigateUrl(this.TabId, "", new string[] { ParamKeys.ViewType + "=" + Views.Topic, ParamKeys.ForumId + "=" + this.ForumId, ParamKeys.TopicId + "=" + this.TopicId });
                if (this.TopicId == -1)
                {
                    this.hypPost.Visible = false;
                }
                else
                {
                    this.hypPost.Visible = true;
                }

                this.hypHome.NavigateUrl = this.NavigateUrl(this.TabId);
            }
            catch (Exception exc)
            {
                DotNetNuke.Services.Exceptions.Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
        #endregion

        #region  Web Form Designer Generated Code

        // This call is required by the Web Form Designer.
        [System.Diagnostics.DebuggerStepThrough()]
        private void InitializeComponent()
        {
        }

        protected System.Web.UI.WebControls.Panel pnlMessage;

        // NOTE: The following placeholder declaration is required by the Web Form Designer.
        // Do not delete or move it.
        private object designerPlaceholderDeclaration;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // CODEGEN: This method call is required by the Web Form Designer
            // Do not modify it using the code editor.
            this.LocalResourceFile = Globals.SharedResourceFile;
            this.InitializeComponent();
        }

        #endregion

    }

}
