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
    using System.Reflection;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    using DotNetNuke.Modules.ActiveForums.Controls;
    using DotNetNuke.Modules.ActiveForums.Data;
    using DotNetNuke.Services.Social.Notifications;
    using DotNetNuke.UI.UserControls;

    public partial class af_quickreplyform : ForumBase
    {
        private const string TargetCollapsible = "groupQR";
        protected System.Web.UI.HtmlControls.HtmlGenericControl contactByFaxOnly = new HtmlGenericControl();
        protected System.Web.UI.WebControls.CheckBox contactByFaxOnlyCheckBox = new CheckBox();
        protected System.Web.UI.HtmlControls.HtmlGenericControl qR = new HtmlGenericControl();
        protected System.Web.UI.WebControls.PlaceHolder plhMessage = new PlaceHolder();
        protected System.Web.UI.WebControls.Label reqBody = new Label();
        protected System.Web.UI.HtmlControls.HtmlGenericControl btnToolBar = new HtmlGenericControl();
        protected System.Web.UI.HtmlControls.HtmlGenericControl divSubscribe = new HtmlGenericControl();
        protected System.Web.UI.WebControls.LinkButton btnSubmitLink = new LinkButton();
        protected TextBox txtUsername = new TextBox();
        protected System.Web.UI.WebControls.RequiredFieldValidator reqUsername = new System.Web.UI.WebControls.RequiredFieldValidator();
        protected UI.WebControls.CaptchaControl ctlCaptcha = new UI.WebControls.CaptchaControl();
        protected System.Web.UI.WebControls.Label reqSecurityCode = new System.Web.UI.WebControls.RequiredFieldValidator();
        protected System.Web.UI.WebControls.RequiredFieldValidator reqCaptcha = new System.Web.UI.WebControls.RequiredFieldValidator();

        public string SubmitText = Utilities.GetSharedResource("Submit.Text");

        public bool RequireCaptcha { get; set; } = true;

        public bool UseFilter { get; set; } = true;

        public string Subject { get; set; } = string.Empty;

        public bool ModApprove { get; set; } = false;

        public bool CanTrust { get; set; } = false;

        public bool IsTrusted { get; set; } = false;

        public bool TrustDefault { get; set; } = false;

        public bool AllowHTML { get; set; } = false;

        public bool AllowScripts { get; set; } = false;

        public bool AllowSubscribe { get; set; } = false;

        #region Event Handlers
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                if (this.contactByFaxOnlyCheckBox.Checked)
                {
                    // if someone activates this checkbox send him home :-)
                    this.Response.Redirect("about:blank");
                }

                if (this.Request.IsAuthenticated)
                {
                    this.btnSubmitLink.OnClientClick = "afQuickSubmit(); return false;";

                    this.AllowSubscribe = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this.ForumInfo.Security.Subscribe, this.ForumUser.UserRoles);
                }
                else
                {
                    this.reqUsername.Enabled = true;
                    this.reqUsername.Text = "<img src=\"" + this.ImagePath + "/images/warning.png\" />";
                    this.reqBody.Text = "<img src=\"" + this.ImagePath + "/images/warning.png\" />";
                    this.reqSecurityCode.Text = "<img src=\"" + this.ImagePath + "/images/warning.png\" />";
                    this.btnSubmitLink.Click += this.ambtnSubmit_Click;

                    this.AllowSubscribe = false;
                }

                this.btnToolBar.Visible = this.UseFilter;
                this.divSubscribe.Visible = this.AllowSubscribe;
                this.divSubscribe.Visible = !this.UserPrefTopicSubscribe; /* if user has preference set for auto subscribe, no need to show them the subscribe option */
                if (this.divSubscribe.Visible)
                {
                    var subControl = new ToggleSubscribe(this.ForumModuleId, this.ForumId, this.TopicId, 1);
                    subControl.Checked = new DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController().Subscribed(this.PortalId, this.ForumModuleId, this.UserId, this.ForumId, this.TopicId);
                    subControl.Text = "[RESX:Subscribe]";
                    this.divSubscribe.InnerHtml = subControl.Render();
                }

                if (Utilities.InputIsValid(this.Request.Form["txtBody"]) && this.Request.IsAuthenticated & ((!string.IsNullOrEmpty(this.Request.Form["hidReply1"]) && string.IsNullOrEmpty(this.Request.Form["hidReply2"])) | this.Request.Browser.IsMobileDevice))
                {
                    this.SaveQuickReply();
                }
            }
            catch (Exception exc)
            {
                DotNetNuke.Services.Exceptions.Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        #endregion

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            string template = TemplateCache.GetCachedTemplate(this.ForumModuleId, "QuickReply", this.ForumInfo.QuickReplyFormId);

            try
            {
                template = Globals.ForumsControlsRegisterAMTag + template;
                if (template.Contains("[SUBJECT]"))
                {
                    template = template.Replace("[SUBJECT]", this.Subject);
                }

                if (template.Contains("[AF:CONTROLS:GROUPTOGGLE]"))
                {
                    template = template.Replace(oldValue: "[AF:CONTROLS:GROUPTOGGLE]", newValue: DotNetNuke.Modules.ActiveForums.Injector.InjectCollapsibleOpened(target: TargetCollapsible, title: string.Empty));
                }

                if (!this.Request.IsAuthenticated)
                {
                    if (template.Contains("[AF:UI:ANON]"))
                    {
                        template = template.Replace("[AF:INPUT:USERNAME]", "<asp:textbox id=\"txtUsername\" cssclass=\"aftextbox\" runat=\"server\" />");
                        template = template.Replace("[AF:REQ:USERNAME]", "<asp:requiredfieldvalidator id=\"reqUsername\" validationgroup=\"afform\" ControlToValidate=\"txtUsername\" runat=\"server\" />");
                        template = Globals.DnnControlsRegisterTag + template;
                        template = template.Replace("[AF:INPUT:CAPTCHA]", "<dnn:captchacontrol  id=\"ctlCaptcha\" captchawidth=\"130\" captchaheight=\"40\" cssclass=\"Normal\" runat=\"server\" errorstyle-cssclass=\"NormalRed\"  />");
                        if (!this.RequireCaptcha)
                        {
                            template = template.Replace("[RESX:SecurityCode]:[AF:REQ:SECURITYCODE]", string.Empty);
                        }

                        template = template.Replace("[AF:UI:ANON]", string.Empty);
                        template = template.Replace("[/AF:UI:ANON]", string.Empty);

                    }
                }
                else
                {
                    template = TemplateUtils.ReplaceSubSection(template, string.Empty, "[AF:UI:ANON]", "[/AF:UI:ANON]");
                }

                template = template.Replace("[AF:BUTTON:SUBMIT]", "<asp:linkbutton ID=\"btnSubmitLink\" runat=\"server\" CssClass=\"dnnPrimaryAction\">[RESX:Submit]</asp:linkbutton>");
                template = Utilities.LocalizeControl(template);
                Control ctl = this.ParseControl(template);
                this.LinkControls(ctl.Controls);
                this.qR.Controls.Add(ctl);
            }
            catch (Exception)
            {

                throw;
            }

        }

        private void LinkControls(ControlCollection ctrls)
        {
            foreach (Control ctrl in ctrls)
            {
                switch (ctrl.ID)
                {
                    case "plhMessage":
                        this.plhMessage = (PlaceHolder)ctrl;
                        break;
                    case "reqUsername":
                        this.reqUsername = (System.Web.UI.WebControls.RequiredFieldValidator)ctrl;
                        break;
                    case "txtUsername":
                        this.txtUsername = (System.Web.UI.WebControls.TextBox)ctrl;
                        break;
                    case "reqBody":
                        this.reqBody = (Label)ctrl;
                        break;
                    case "btnToolBar":
                        this.btnToolBar = (System.Web.UI.HtmlControls.HtmlGenericControl)ctrl;
                        break;
                    case "reqSecurityCode":
                        this.reqSecurityCode = (Label)ctrl;
                        break;
                    case "ctlCaptcha":
                        this.ctlCaptcha = (DotNetNuke.UI.WebControls.CaptchaControl)ctrl;
                        break;
                    case "divSubscribe":
                        this.divSubscribe = (System.Web.UI.HtmlControls.HtmlGenericControl)ctrl;
                        break;
                    case "btnSubmitLink":
                        this.btnSubmitLink = (System.Web.UI.WebControls.LinkButton)ctrl;
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

        protected void ContactByFaxOnlyCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            // if someone activates this checkbox send him home :-)
            this.Response.Redirect("http://localhost/", true);
        }

        private void SaveQuickReply()
        {
            DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forumInfo = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Forums_Get(this.PortalId, this.ForumModuleId, this.ForumId, false, this.TopicId);
            if (!Utilities.HasFloodIntervalPassed(floodInterval: this.MainSettings.FloodInterval, user: this.ForumUser, forumInfo: forumInfo))
            {
                UserProfileController upc = new UserProfileController();
                UserProfileInfo upi = upc.Profiles_Get(this.PortalId, this.ModuleId, this.UserId);
                if (upi != null)
                {
                    if (SimulateDateDiff.DateDiff(SimulateDateDiff.DateInterval.Second, upi.DateLastPost, DateTime.UtcNow) < this.MainSettings.FloodInterval)
                    {
                        Controls.InfoMessage im = new Controls.InfoMessage();
                        im.Message = "<div class=\"afmessage\">" + string.Format(Utilities.GetSharedResource("[RESX:Error:FloodControl]"), this.MainSettings.FloodInterval) + "</div>";
                        this.plhMessage.Controls.Add(im);
                        return;
                    }
                }
            }

            if (!this.Request.IsAuthenticated)
            {
                if ((!this.ctlCaptcha.IsValid) || this.txtUsername.Text == "")
                {
                    return;
                }
            }

            UserProfileInfo ui = new UserProfileInfo();
            if (this.UserId > 0)
            {
                ui = this.ForumUser.Profile;
            }
            else
            {
                ui.TopicCount = 0;
                ui.ReplyCount = 0;
                ui.RewardPoints = 0;
                ui.IsMod = false;
                ui.TrustLevel = -1;

            }

            bool userIsTrusted = false;
            userIsTrusted = Utilities.IsTrusted((int)this.ForumInfo.DefaultTrustValue, ui.TrustLevel, DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this.ForumInfo.Security.Trust, this.ForumUser.UserRoles), this.ForumInfo.AutoTrustLevel, ui.PostCount);
            bool isApproved = false;
            isApproved = Convert.ToBoolean((this.ForumInfo.IsModerated == true) ? false : true);
            if (userIsTrusted || DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this.ForumInfo.Security.ModApprove, this.ForumUser.UserRoles))
            {
                isApproved = true;
            }

            DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo ri = new DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo();
            ri.ReplyToId = this.TopicId;
            ri.TopicId = this.TopicId;
            ri.StatusId = -1;
            ri.Content = new DotNetNuke.Modules.ActiveForums.Entities.ContentInfo();
            string sUsername = string.Empty;
            if (this.Request.IsAuthenticated)
            {
                switch (this.MainSettings.UserNameDisplay.ToUpperInvariant())
                {
                    case "USERNAME":
                        sUsername = this.UserInfo.Username.Trim(' ');
                        break;
                    case "FULLNAME":
                        sUsername = Convert.ToString(this.UserInfo.FirstName + " " + this.UserInfo.LastName).Trim(' ');
                        break;
                    case "FIRSTNAME":
                        sUsername = this.UserInfo.FirstName.Trim(' ');
                        break;
                    case "LASTNAME":
                        sUsername = this.UserInfo.LastName.Trim(' ');
                        break;
                    case "DISPLAYNAME":
                        sUsername = this.UserInfo.DisplayName.Trim(' ');
                        break;
                    default:
                        sUsername = this.UserInfo.DisplayName;
                        break;
                }

            }
            else
            {
                sUsername = Utilities.CleanString(this.PortalId, this.txtUsername.Text, false, EditorTypes.TEXTBOX, true, false, this.ForumModuleId, this.ThemePath, false);
            }

            string sBody = string.Empty;
            if (this.AllowHTML)
            {
                this.AllowHTML = this.IsHtmlPermitted(this.ForumInfo.EditorPermittedUsers, this.IsTrusted, DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this.ForumInfo.Security.ModEdit, this.ForumUser.UserRoles));
            }

            sBody = Utilities.CleanString(this.PortalId, this.Request.Form["txtBody"], this.AllowHTML, EditorTypes.TEXTBOX, this.UseFilter, this.AllowScripts, this.ForumModuleId, this.ThemePath, this.ForumInfo.AllowEmoticons);
            ri.Content.AuthorId = this.UserId;
            ri.Content.AuthorName = sUsername;
            ri.Content.Body = sBody;
            ri.Content.IsDeleted = false;
            ri.Content.Subject = this.Subject;
            ri.Content.Summary = string.Empty;
            ri.IsApproved = isApproved;
            ri.IsDeleted = false;
            ri.Content.IPAddress = this.Request.UserHostAddress;
            if (this.UserPrefTopicSubscribe)
            {
                new DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController().Subscribe(this.PortalId, this.ForumModuleId, this.UserId, this.ForumId, ri.TopicId);
            }

            int replyId = new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController().Reply_Save(this.PortalId, this.ModuleId, ri);
            ri = new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController().GetById(replyId);
            DotNetNuke.Modules.ActiveForums.Controllers.ReplyController.QueueApprovedReplyAfterAction(this.PortalId, this.TabId, this.ModuleId, ri.Forum.ForumGroupId, this.ForumId, this.TopicId, replyId, ri.Content.AuthorId);
            DataCache.CacheClearPrefix(this.ModuleId, string.Format(CacheKeys.TopicViewPrefix, this.ModuleId));
            DataCache.CacheClearPrefix(this.ModuleId, string.Format(CacheKeys.TopicsViewPrefix, this.ModuleId));
            DataCache.CacheClearPrefix(this.ModuleId, string.Format(CacheKeys.ForumViewPrefix, this.ModuleId));

            DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(this.TopicId);
            string fullURL = new ControlUtils().BuildUrl(this.TabId, this.ForumModuleId, this.ForumInfo.ForumGroup.PrefixURL, this.ForumInfo.PrefixURL, this.ForumInfo.ForumGroupId, this.ForumInfo.ForumID, this.TopicId, ti.TopicUrl, -1, -1, string.Empty, -1, replyId, this.SocialGroupId);

            if (fullURL.Contains("~/"))
            {
                fullURL = Utilities.NavigateURL(this.TabId, "", new string[] { ParamKeys.TopicId + "=" + this.TopicId, ParamKeys.ContentJumpId + "=" + replyId });
            }

            if (fullURL.EndsWith("/"))
            {
                fullURL += Utilities.UseFriendlyURLs(this.ForumModuleId) ? String.Concat("#", replyId) : String.Concat("?", ParamKeys.ContentJumpId, "=", replyId);
            }

            if (isApproved)
            {
                // Redirect to show post
                this.Response.Redirect(fullURL, false);
            }
            else
            {
                DotNetNuke.Modules.ActiveForums.Controllers.ReplyController.QueueUnapprovedReplyAfterAction(this.PortalId, this.TabId, this.ModuleId, ri.Forum.ForumGroupId, this.ForumId, this.TopicId, replyId, ri.Content.AuthorId);

                var @params = new List<string> { ParamKeys.ForumId + "=" + this.ForumId, ParamKeys.ViewType + "=confirmaction", "afmsg=pendingmod", ParamKeys.TopicId + "=" + this.TopicId };
                if (this.SocialGroupId > 0)
                {
                    @params.Add("GroupId=" + this.SocialGroupId);
                }

                this.Response.Redirect(Utilities.NavigateURL(this.TabId, "", @params.ToArray()), false);
            }
        }

        private void ambtnSubmit_Click(object sender, System.EventArgs e)
        {

            this.Page.Validate();
            bool tmpVal = true;
            if (Utilities.InputIsValid(this.Request.Form["txtBody"].Trim()) == false)
            {
                this.reqBody.Visible = true;
                tmpVal = false;
            }

            if (!this.Request.IsAuthenticated && Utilities.InputIsValid(this.txtUsername.Text.Trim()) == false)
            {
                this.reqUsername.Visible = true;
                tmpVal = false;
            }

            if (!this.ctlCaptcha.IsValid)
            {
                this.reqSecurityCode.Visible = true;
            }

            if (this.Page.IsValid && tmpVal)
            {
                this.SaveQuickReply();
            }

        }
    }

}
