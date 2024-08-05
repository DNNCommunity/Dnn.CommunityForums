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

namespace DotNetNuke.Modules.ActiveForums.Controls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using DotNetNuke.Modules.ActiveForums.API;
    using DotNetNuke.Modules.ActiveForums.Data;

    [DefaultProperty("Text"), ToolboxData("<{0}:QuickReply runat=server></{0}:QuickReply>")]
    public class QuickReply : ControlsBase
    {
        #region Protected Members
        protected Controls.Toggle tgQR;
        protected Controls.ToggleDisplay tgdQR;
        protected PlaceHolder plhMessage;
        protected System.Web.UI.WebControls.RequiredFieldValidator reqUserName;
        protected System.Web.UI.HtmlControls.HtmlInputText txtUserName;
        protected Label reqBody;
        protected Label reqSecurityCode;
        protected CheckBox chkSubscribe;
        protected Controls.ImageButton ambtnSubmit;
        protected System.Web.UI.HtmlControls.HtmlInputButton btnSubmit;
        protected System.Web.UI.HtmlControls.HtmlTableRow trUsername;
        protected System.Web.UI.HtmlControls.HtmlTableRow trCaptcha;
        protected System.Web.UI.HtmlControls.HtmlTableRow trSubscribe;

        #endregion
        #region Private Members
        private int moduleId = -1;
        private int portalId = -1;
        private string subject = string.Empty;

        #endregion
        #region Public Properties
        public string Subject { get; set; } = string.Empty;

        #endregion
        #region Event Handlers
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.ambtnSubmit.Click += new System.EventHandler(this.ambtnSubmit_Click);

            this.moduleId = this.ControlConfig.ModuleId;
            this.portalId = this.ControlConfig.PortalId;
            if (this.ForumInfo == null)
            {
                this.ForumInfo = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Forums_Get(this.portalId, this.moduleId, this.ForumId, false, this.TopicId);
            }

            string sTemp = string.Empty;
            if (this.ControlConfig != null)
            {
                object obj = DataCache.SettingsCacheRetrieve(this.moduleId, string.Format(CacheKeys.QuickReply, this.moduleId));
                if (obj == null)
                {
                    sTemp = this.ParseTemplate();
                    DataCache.SettingsCacheStore(this.moduleId, string.Format(CacheKeys.QuickReply, this.moduleId), sTemp);
                }
                else
                {
                    sTemp = Convert.ToString(obj);
                }

                sTemp = Utilities.LocalizeControl(sTemp);
                string subscribedChecked = string.Empty;
                if (this.ControlConfig.User.PrefTopicSubscribe || new DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController().Subscribed(this.portalId, this.moduleId, this.UserId, this.ForumId, this.TopicId))
                {
                    subscribedChecked = " checked=true";
                }

                sTemp = sTemp.Replace("[AF:CONTROL:SUBSCRIBECHECK]", "<input type=\"checkbox\" id=\"chkSubscribe\" name=\"chkSubscribe\" value=\"1\" " + subscribedChecked + "\" />");

                // Security
                sTemp = sTemp.Replace("[CREATEROLES]", "1;");
                sTemp = sTemp.Replace("[USERROLES]", this.ForumUser.UserRoles);
                sTemp = sTemp.Replace("[THEMEPATH]", this.ThemePath);
                sTemp = sTemp.Replace("[SUBJECT]", this.Subject);
                sTemp = sTemp.Replace("[REPLYROLES]", this.ForumInfo.Security.Reply);
                if (!HttpContext.Current.Request.IsAuthenticated)
                {
                    sTemp = "<%@ Register TagPrefix=\"dnn\" Assembly=\"DotNetNuke\" Namespace=\"DotNetNuke.UI.WebControls\"%>" + sTemp;
                }

                if (!sTemp.Contains(Globals.ForumsControlsRegisterAFTag))
                {
                    sTemp = Globals.ForumsControlsRegisterAFTag + sTemp;
                }

                Control ctl = this.Page.ParseControl(sTemp);
                this.LinkControls(ctl.Controls);
                this.Controls.Add(ctl);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                bool bolShow = true;
                if (HttpContext.Current.Request.Cookies["QRShow"] != null)
                {
                    bolShow = Convert.ToBoolean(HttpContext.Current.Request.Cookies["QRShow"].Value);
                }

                this.tgdQR.Visible = bolShow;
                this.tgQR.Visible = bolShow;
                if (!HttpContext.Current.Request.Browser.IsMobileDevice)
                {
                    this.ambtnSubmit.ImageUrl = this.ThemePath + "save32.png";
                    this.ambtnSubmit.ImageLocation = "TOP";
                    this.btnSubmit.Visible = false;
                    this.ambtnSubmit.Visible = true;
                    if (!HttpContext.Current.Request.IsAuthenticated)
                    {
                        this.trSubscribe.Visible = false;
                        this.ambtnSubmit.PostBack = true;
                        this.ambtnSubmit.ClientSideScript = string.Empty;
                        this.reqUserName.Enabled = true;
                        this.reqUserName.Text = "<img src=\"" + this.ThemePath + "warning.png\" />";
                        this.reqBody.Text = "<img src=\"" + this.ThemePath + "warning.png\" />";
                        this.reqSecurityCode.Text = "<img src=\"" + this.ThemePath + "warning.png\" />";
                        this.ambtnSubmit.Click += this.ambtnSubmit_Click;
                    }
                    else
                    {
                        this.trUsername.Visible = false;
                        this.reqUserName.Visible = false;
                        this.reqUserName.Enabled = false;
                        this.txtUserName.Visible = false;
                        this.trCaptcha.Visible = false;
                    }
                }
                else
                {
                    this.btnSubmit.Visible = true;
                    this.ambtnSubmit.Visible = false;
                }

                if (!string.IsNullOrEmpty(HttpContext.Current.Request.Form["txtBody"]) && HttpContext.Current.Request.IsAuthenticated & ((!string.IsNullOrEmpty(HttpContext.Current.Request.Form["hidReply1"]) && string.IsNullOrEmpty(HttpContext.Current.Request.Form["hidReply2"])) | HttpContext.Current.Request.Browser.IsMobileDevice))
                {
                    this.SaveQuickReply();
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void ambtnSubmit_Click(object sender, System.EventArgs e)
        {
        }

        #endregion
        #region Private Methods
        private void SaveQuickReply()
        {
            DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forumInfo = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Forums_Get(this.portalId, this.moduleId, this.ForumId, false, this.TopicId);
            if (!Utilities.HasFloodIntervalPassed(floodInterval: this.MainSettings.FloodInterval, forumUser: this.ForumUser, forumInfo: forumInfo))
            {
                this.plhMessage.Controls.Add(new InfoMessage { Message = "<div class=\"afmessage\">" + string.Format(this.GetSharedResource("[RESX:Error:FloodControl]"), this.MainSettings.FloodInterval) + "</div>" });
                return;
            }

            bool userIsTrusted = Utilities.IsTrusted((int)forumInfo.DefaultTrustValue, this.ControlConfig.User.TrustLevel, DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(forumInfo.Security.Trust, this.ForumUser.UserRoles), forumInfo.AutoTrustLevel, this.ControlConfig.User.PostCount);
            bool isApproved = Convert.ToBoolean((forumInfo.IsModerated == true) ? false : true);
            if (userIsTrusted || DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(forumInfo.Security.Moderate, this.ForumUser.UserRoles))
            {
                isApproved = true;
            }

            DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo ri = new DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo();
            Data.Topics db = new Data.Topics();

            // im rc As New ReplyController
            int replyId = -1;
            string sUsername = string.Empty;
            if (HttpContext.Current.Request.IsAuthenticated)
            {
                sUsername = this.ControlConfig.User.DisplayName;
            }
            else
            {
                sUsername = Utilities.CleanString(this.portalId, this.txtUserName.Value, false, EditorTypes.TEXTBOX, true, false, this.moduleId, this.ThemePath, false);
            }

            string sBody = string.Empty;

            // TODO: Check for allowhtml
            bool allowHtml = false;

            sBody = Utilities.CleanString(this.portalId, HttpContext.Current.Request.Form["txtBody"], allowHtml, EditorTypes.TEXTBOX, forumInfo.UseFilter, forumInfo.AllowScript, this.moduleId, this.ThemePath, forumInfo.AllowEmoticons);
            ri.TopicId = this.TopicId;
            ri.ReplyToId = this.TopicId;
            ri.Content.AuthorId = this.UserId;
            ri.Content.AuthorName = sUsername;
            ri.Content.Body = sBody;
            ri.Content.IsDeleted = false;
            ri.Content.Subject = this.Subject;
            ri.Content.Summary = string.Empty;
            ri.IsApproved = isApproved;
            ri.IsDeleted = false;
            ri.Content.IPAddress = HttpContext.Current.Request.UserHostAddress;
            replyId = db.Reply_Save(ri);

            if (isApproved)
            {
                DotNetNuke.Modules.ActiveForums.Controllers.ReplyController.QueueApprovedReplyAfterAction(this.portalId, this.TabId, this.moduleId, forumInfo.ForumGroupId, this.ForumId, this.TopicId, replyId, ri.Content.AuthorId);

                // Redirect to show post
                string fullURL = Utilities.NavigateURL(this.PageId, string.Empty, new string[] { ParamKeys.ForumId + "=" + this.ForumId, ParamKeys.ViewType + "=" + Views.Topic, ParamKeys.TopicId + "=" + this.TopicId, ParamKeys.ContentJumpId + "=" + replyId });
                HttpContext.Current.Response.Redirect(fullURL, false);
            }
            else
            {            
                DotNetNuke.Modules.ActiveForums.Controllers.ModerationController.SendModerationNotification(this.portalId, this.TabId, this.moduleId, forumInfo.ForumGroupId, this.ForumId, this.TopicId, replyId, ri.Content.AuthorId, HttpContext.Current.Request.Url.ToString());
                string[] @params = { ParamKeys.ForumId + "=" + this.ForumId, ParamKeys.ViewType + "=confirmaction", "afmsg=pendingmod", ParamKeys.TopicId + "=" + this.TopicId };
                HttpContext.Current.Response.Redirect(Utilities.NavigateURL(this.PageId, string.Empty, @params), false);
            }
        }

        private string ParseTemplate()
        {
            return this.DisplayTemplate;
        }

        private void LinkControls(ControlCollection ctrls)
        {
            foreach (Control ctrl in ctrls)
            {
                switch (ctrl.ID)
                {
                    case "tgQR":
                        this.tgQR = (Toggle)ctrl;
                        this.tgQR.ImagePath = this.ThemePath;
                        break;
                    case "tgdQR":
                        this.tgdQR = (ToggleDisplay)ctrl;
                        break;
                    case "plhMessage":
                        this.plhMessage = (PlaceHolder)ctrl;
                        break;
                    case "reqUserName":
                        this.reqUserName = (System.Web.UI.WebControls.RequiredFieldValidator)ctrl;
                        break;
                    case "txtUserName":
                        this.txtUserName = (System.Web.UI.HtmlControls.HtmlInputText)ctrl;
                        break;
                    case "reqBody":
                        this.reqBody = (Label)ctrl;
                        break;
                    case "reqSecurityCode":
                        this.reqSecurityCode = (Label)ctrl;
                        break;
                    case "chkSubscribe":
                        this.chkSubscribe = (CheckBox)ctrl;
                        break;
                    case "ambtnSubmit":
                        this.ambtnSubmit = (ImageButton)ctrl;
                        break;
                    case "btnSubmit":
                        this.btnSubmit = (System.Web.UI.HtmlControls.HtmlInputButton)ctrl;
                        break;
                    case "trUsername":
                        this.trUsername = (System.Web.UI.HtmlControls.HtmlTableRow)ctrl;
                        break;
                    case "trCaptcha":
                        this.trCaptcha = (System.Web.UI.HtmlControls.HtmlTableRow)ctrl;
                        break;
                    case "trSubscribe":
                        this.trSubscribe = (System.Web.UI.HtmlControls.HtmlTableRow)ctrl;
                        break;
                }

                if (ctrl is Controls.ControlsBase)
                {
                    ((Controls.ControlsBase)ctrl).ControlConfig = this.ControlConfig;
                }

                if (ctrl.Controls.Count > 0)
                {
                    this.LinkControls(ctrl.Controls);
                }
            }
        }
        #endregion
    }
}
