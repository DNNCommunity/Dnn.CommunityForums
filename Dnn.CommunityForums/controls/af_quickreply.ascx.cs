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
using System.Web.UI;
using DotNetNuke.Modules.ActiveForums.Controls; 
using DotNetNuke.Services.Social.Notifications;  
using System.Web.UI.WebControls; 
using System.Web.UI.HtmlControls;
using DotNetNuke.Modules.ActiveForums.Data;
using DotNetNuke.UI.UserControls;
using System.Reflection;

namespace DotNetNuke.Modules.ActiveForums
{

    public partial class af_quickreplyform : ForumBase
    {
        private const string TargetCollapsible = "groupQR";
        protected System.Web.UI.HtmlControls.HtmlGenericControl ContactByFaxOnly = new HtmlGenericControl();
        protected System.Web.UI.WebControls.CheckBox ContactByFaxOnlyCheckBox = new CheckBox();
        protected System.Web.UI.HtmlControls.HtmlGenericControl QR = new HtmlGenericControl();
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
                if (ContactByFaxOnlyCheckBox.Checked)
                {
                    // if someone activates this checkbox send him home :-)
                    Response.Redirect("about:blank");
                }


                if (Request.IsAuthenticated)
                {
                    btnSubmitLink.OnClientClick = "afQuickSubmit(); return false;";

                    AllowSubscribe = Permissions.HasPerm(ForumInfo.Security.Subscribe, ForumUser.UserRoles);
                }
                else
                {
                    reqUsername.Enabled = true;
                    reqUsername.Text = "<img src=\"" + ImagePath + "/images/warning.png\" />";
                    reqBody.Text = "<img src=\"" + ImagePath + "/images/warning.png\" />";
                    reqSecurityCode.Text = "<img src=\"" + ImagePath + "/images/warning.png\" />";
                    btnSubmitLink.Click += ambtnSubmit_Click;

                    AllowSubscribe = false;
                }
                btnToolBar.Visible = UseFilter;
                divSubscribe.Visible = AllowSubscribe;
                if (AllowSubscribe)
                {
                    var subControl = new ToggleSubscribe(ForumModuleId, ForumId, TopicId, 1);
                    subControl.Checked = (Subscriptions.IsSubscribed(PortalId, ForumModuleId, ForumId, TopicId, SubscriptionTypes.Instant, this.UserId));
                    subControl.Text = "[RESX:TopicSubscribe:" + (Subscriptions.IsSubscribed(PortalId, ForumModuleId, ForumId, TopicId, SubscriptionTypes.Instant, this.UserId)).ToString().ToUpper() + "]";
                    divSubscribe.InnerHtml = subControl.Render();
                }
                if (Utilities.InputIsValid(Request.Form["txtBody"]) && Request.IsAuthenticated & ((!(string.IsNullOrEmpty(Request.Form["hidReply1"])) && string.IsNullOrEmpty(Request.Form["hidReply2"])) | Request.Browser.IsMobileDevice))
                {
                    SaveQuickReply();
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
            string template = TemplateCache.GetCachedTemplate(ForumModuleId, "QuickReply", ForumInfo.QuickReplyFormId);

            try
            {
                template = Globals.ForumsControlsRegisterAMTag + template;
                if (template.Contains("[SUBJECT]"))
                {
                    template = template.Replace("[SUBJECT]", Subject);
                }
                if (template.Contains("[AF:CONTROLS:GROUPTOGGLE]"))
                {
                    template = template.Replace(oldValue: "[AF:CONTROLS:GROUPTOGGLE]", newValue: DotNetNuke.Modules.ActiveForums.Injector.InjectCollapsibleOpened(target: TargetCollapsible, title:string.Empty));
                }
                if (!Request.IsAuthenticated)
                {
                    if (template.Contains("[AF:UI:ANON]"))
                    {
                        template = template.Replace("[AF:INPUT:USERNAME]", "<asp:textbox id=\"txtUsername\" cssclass=\"aftextbox\" runat=\"server\" />");
                        template = template.Replace("[AF:REQ:USERNAME]", "<asp:requiredfieldvalidator id=\"reqUsername\" validationgroup=\"afform\" ControlToValidate=\"txtUsername\" runat=\"server\" />");
                        template = Globals.DnnControlsRegisterTag + template;
                        template = template.Replace("[AF:INPUT:CAPTCHA]", "<dnn:captchacontrol  id=\"ctlCaptcha\" captchawidth=\"130\" captchaheight=\"40\" cssclass=\"Normal\" runat=\"server\" errorstyle-cssclass=\"NormalRed\"  />");
                        if (!RequireCaptcha)
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
                LinkControls(ctl.Controls);
                QR.Controls.Add(ctl);
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
                        plhMessage = (PlaceHolder)ctrl;
                        break;
                    case "reqUsername":
                        reqUsername = (System.Web.UI.WebControls.RequiredFieldValidator)ctrl;
                        break;
                    case "txtUsername":
                        txtUsername = (System.Web.UI.WebControls.TextBox)ctrl;
                        break;
                    case "reqBody":
                        reqBody = (Label)ctrl;
                        break;
                    case "btnToolBar":
                        btnToolBar = (System.Web.UI.HtmlControls.HtmlGenericControl)ctrl;
                        break; 
                    case "reqSecurityCode":
                        reqSecurityCode = (Label)ctrl;
                        break;
                    case "ctlCaptcha":
                        ctlCaptcha = (DotNetNuke.UI.WebControls.CaptchaControl)ctrl;
                        break; 
                    case "divSubscribe": 
                        divSubscribe = (System.Web.UI.HtmlControls.HtmlGenericControl)ctrl;
                        break; 
                    case "btnSubmitLink":
                        btnSubmitLink = (System.Web.UI.WebControls.LinkButton)ctrl;
                        break;
                }
                if (ctrl is Controls.ControlsBase)
                {
                    ((Controls.ControlsBase)ctrl).ControlConfig = this.ControlConfig;
                }
                if (ctrl.Controls.Count > 0)
                {
                    LinkControls(ctrl.Controls);
                }
            }
        }
        protected void ContactByFaxOnlyCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            // if someone activates this checkbox send him home :-)
            Response.Redirect("http://localhost/", true);
        }
        private void SaveQuickReply()
        {
            ForumController fc = new ForumController();
            Forum forumInfo = fc.Forums_Get(PortalId, ForumModuleId, ForumId, false, TopicId);
            if (!Utilities.HasFloodIntervalPassed(floodInterval: MainSettings.FloodInterval, user: ForumUser, forumInfo: forumInfo))
            {
                UserProfileController upc = new UserProfileController();
                UserProfileInfo upi = upc.Profiles_Get(PortalId, ModuleId, this.UserId);
                if (upi != null)
                {
                    if (SimulateDateDiff.DateDiff(SimulateDateDiff.DateInterval.Second, upi.DateLastPost, DateTime.UtcNow) < MainSettings.FloodInterval)
                    {
                        Controls.InfoMessage im = new Controls.InfoMessage();
                        im.Message = "<div class=\"afmessage\">" + string.Format(Utilities.GetSharedResource("[RESX:Error:FloodControl]"), MainSettings.FloodInterval) + "</div>";
                        plhMessage.Controls.Add(im);
                        return;
                    }
                }
            }
            if (!Request.IsAuthenticated)
            {
                if ((!ctlCaptcha.IsValid) || txtUsername.Text == "")
                {
                    return;
                }
            }
            UserProfileInfo ui = new UserProfileInfo();
            if (UserId > 0)
            {
                ui = ForumUser.Profile;
            }
            else
            {
                ui.TopicCount = 0;
                ui.ReplyCount = 0;
                ui.RewardPoints = 0;
                ui.IsMod = false;
                ui.TrustLevel = -1;

            }
            bool UserIsTrusted = false;
            UserIsTrusted = Utilities.IsTrusted((int)ForumInfo.DefaultTrustValue, ui.TrustLevel, Permissions.HasPerm(ForumInfo.Security.Trust, ForumUser.UserRoles), ForumInfo.AutoTrustLevel, ui.PostCount);
            bool isApproved = false;
            isApproved = Convert.ToBoolean(((ForumInfo.IsModerated == true) ? false : true));
            if (UserIsTrusted || Permissions.HasPerm(ForumInfo.Security.ModApprove, ForumUser.UserRoles))
            {
                isApproved = true;
            }
            DotNetNuke.Modules.ActiveForums.ReplyInfo ri = new DotNetNuke.Modules.ActiveForums.ReplyInfo();
            ReplyController rc = new ReplyController();
            int ReplyId = -1;
            string sUsername = string.Empty;
            if (Request.IsAuthenticated)
            {
                switch (MainSettings.UserNameDisplay.ToUpperInvariant())
                {
                    case "USERNAME":
                        sUsername = UserInfo.Username.Trim(' ');
                        break;
                    case "FULLNAME":
                        sUsername = Convert.ToString(UserInfo.FirstName + " " + UserInfo.LastName).Trim(' ');
                        break;
                    case "FIRSTNAME":
                        sUsername = UserInfo.FirstName.Trim(' ');
                        break;
                    case "LASTNAME":
                        sUsername = UserInfo.LastName.Trim(' ');
                        break;
                    case "DISPLAYNAME":
                        sUsername = UserInfo.DisplayName.Trim(' ');
                        break;
                    default:
                        sUsername = UserInfo.DisplayName;
                        break;
                }

            }
            else
            {
                sUsername = Utilities.CleanString(PortalId, txtUsername.Text, false, EditorTypes.TEXTBOX, true, false, ForumModuleId, ThemePath, false);
            }

            string sBody = string.Empty;
            if (AllowHTML)
            {
                AllowHTML = IsHtmlPermitted(ForumInfo.EditorPermittedUsers, IsTrusted, Permissions.HasPerm(ForumInfo.Security.ModEdit, ForumUser.UserRoles));
            }
            sBody = Utilities.CleanString(PortalId, Request.Form["txtBody"], AllowHTML, EditorTypes.TEXTBOX, UseFilter, AllowScripts, ForumModuleId, ThemePath, ForumInfo.AllowEmoticons);
            DateTime createDate = DateTime.UtcNow;
            ri.TopicId = TopicId;
            ri.ReplyToId = TopicId;
            ri.Content.AuthorId = UserId;
            ri.Content.AuthorName = sUsername;
            ri.Content.Body = sBody;
            ri.Content.DateCreated = createDate;
            ri.Content.DateUpdated = createDate;
            ri.Content.IsDeleted = false;
            ri.Content.Subject = Subject;
            ri.Content.Summary = string.Empty;
            ri.IsApproved = isApproved;
            ri.IsDeleted = false;
            ri.Content.IPAddress = Request.UserHostAddress;
            if (UserPrefTopicSubscribe)
            {
                new DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController().Subscribe(PortalId, ForumModuleId, UserId, ForumId, ri.TopicId);
            }
            ReplyId = rc.Reply_Save(PortalId, ModuleId, ri);
            rc.UpdateModuleLastContentModifiedOnDate(ModuleId);
            DataCache.ContentCacheClear(ModuleId, string.Format(CacheKeys.TopicViewForUser, ModuleId, ri.TopicId, ri.Content.AuthorId));
            DataCache.CacheClearPrefix(ModuleId, string.Format(CacheKeys.ForumViewPrefix, ModuleId));


            DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = new TopicsController().Topics_Get(PortalId, ForumModuleId, TopicId, ForumId, -1, false);
            string fullURL = new ControlUtils().BuildUrl(TabId, ForumModuleId, ForumInfo.ForumGroup.PrefixURL, ForumInfo.PrefixURL, ForumInfo.ForumGroupId, ForumInfo.ForumID, TopicId, ti.TopicUrl, -1, -1, string.Empty, -1, ReplyId, SocialGroupId);

            if (fullURL.Contains("~/"))
            {
                fullURL = Utilities.NavigateURL(TabId, "", new string[] { ParamKeys.TopicId + "=" + TopicId, ParamKeys.ContentJumpId + "=" + ReplyId });
            }
            if (fullURL.EndsWith("/"))
            {
                fullURL += Utilities.UseFriendlyURLs(ForumModuleId) ? String.Concat("#", ReplyId) : String.Concat("?", ParamKeys.ContentJumpId, "=", ReplyId);
            }
            if (isApproved)
            {

                //Send Subscriptions

                try
                {
                    Subscriptions.SendSubscriptions(PortalId, ForumModuleId, TabId, ForumId, TopicId, ReplyId, UserId);
                    try
                    {
                        Social amas = new Social();
                        amas.AddReplyToJournal(PortalId, ForumModuleId, TabId, ForumId, TopicId, ReplyId, UserId, fullURL, ri.Content.Subject, string.Empty, sBody, ForumInfo.Security.Read, SocialGroupId);
                    }
                    catch (Exception ex)
                    {
                        DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                    }
                }
                catch (Exception ex)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.ProcessModuleLoadException(this, ex);
                }
                //Redirect to show post

                Response.Redirect(fullURL, false);
            }
            else if (isApproved == false)
            {
                List<DotNetNuke.Entities.Users.UserInfo> mods = Utilities.GetListOfModerators(PortalId, ForumModuleId, ForumId);
                NotificationType notificationType = NotificationsController.Instance.GetNotificationType("AF-ForumModeration");
                string subject = Utilities.GetSharedResource("NotificationSubjectReply");
                subject = subject.Replace("[DisplayName]", UserInfo.DisplayName);
                subject = subject.Replace("[TopicSubject]", ti.Content.Subject);
                string body = Utilities.GetSharedResource("NotificationBodyReply");
                body = body.Replace("[Post]", sBody);
                string notificationKey = string.Format("{0}:{1}:{2}:{3}:{4}", TabId, ForumModuleId, ForumId, TopicId, ReplyId);

                Notification notification = new Notification();
                notification.NotificationTypeID = notificationType.NotificationTypeId;
                notification.Subject = subject;
                notification.Body = body;
                notification.IncludeDismissAction = false;
                notification.SenderUserID = UserId;
                notification.Context = notificationKey;

                NotificationsController.Instance.SendNotification(notification, PortalId, null, mods);

                var @params = new List<string> { ParamKeys.ForumId + "=" + ForumId, ParamKeys.ViewType + "=confirmaction", "afmsg=pendingmod", ParamKeys.TopicId + "=" + TopicId };
                if (SocialGroupId > 0)
                {
                    @params.Add("GroupId=" + SocialGroupId);
                }
                Response.Redirect(Utilities.NavigateURL(TabId, "", @params.ToArray()), false);
            }
            else
            {
                try
                {
                    Social amas = new Social();
                    amas.AddReplyToJournal(PortalId, ForumModuleId, TabId, ForumId, TopicId, ReplyId, UserId, fullURL, Subject, string.Empty, sBody, ForumInfo.Security.Read, SocialGroupId);
                }
                catch (Exception ex)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                }
                Response.Redirect(fullURL, false);
            }
             


        }


        private void ambtnSubmit_Click(object sender, System.EventArgs e)
        {

            Page.Validate();
            bool tmpVal = true;
            if (Utilities.InputIsValid(Request.Form["txtBody"].Trim()) == false)
            {
                reqBody.Visible = true;
                tmpVal = false;
            }
            if (!Request.IsAuthenticated && Utilities.InputIsValid(txtUsername.Text.Trim()) == false)
            {
                reqUsername.Visible = true;
                tmpVal = false;
            }
            if (!ctlCaptcha.IsValid)
            {
                reqSecurityCode.Visible = true;
            }
            if (Page.IsValid && tmpVal)
            {
                SaveQuickReply();
            }

        }
    }

}
