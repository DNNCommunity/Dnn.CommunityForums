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
using System.Text.RegularExpressions;
//using DotNetNuke.Services.ClientCapability;
using DotNetNuke.Services.Social.Notifications;
using DotNetNuke.Services.Localization;
using DotNetNuke.Modules.ActiveForums.Controls;
using DotNetNuke.Modules.ActiveForums.Entities;
using TopicInfo = DotNetNuke.Modules.ActiveForums.Entities.TopicInfo;

namespace DotNetNuke.Modules.ActiveForums
{

    public partial class af_quickreplyform : ForumBase
    {
        #region Private Members
        private bool _CanReply = false;
        private string _ThemePath = string.Empty;
        private int _ForumId = -1;

        #endregion
        //#region Public Members
        //public string SubscribedChecked = string.Empty;
        ////public IClientCapability device = ClientCapabilityProvider.CurrentClientCapability;
        //#endregion
        #region Public Properties
        public bool UseFilter { get; set; } = true;
        public string Subject { get; set; } = string.Empty;
        public bool ModApprove { get; set; } = false;
        public bool CanTrust { get; set; } = false;
        public bool IsTrusted { get; set; } = false;
        public bool TrustDefault { get; set; } = false;
        public bool AllowHTML { get; set; } = false;
        public bool AllowScripts { get; set; } = false;
        public bool AllowSubscribe { get; set; } = false;
        public bool CanSubscribe { get; set; } = false;
        #endregion
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
                    reqUserName.Enabled = true;
                    reqUserName.Text = "<img src=\"" + ImagePath + "/images/warning.png\" />";
                    reqBody.Text = "<img src=\"" + ImagePath + "/images/warning.png\" />";
                    reqSecurityCode.Text = "<img src=\"" + ImagePath + "/images/warning.png\" />";
                    btnSubmitLink.Click += ambtnSubmit_Click;

                    AllowSubscribe = false;
                }



                BoldText = Utilities.GetSharedResource("[RESX:Bold]");
                ItalicsText = Utilities.GetSharedResource("[RESX:Italics]");
                UnderlineText = Utilities.GetSharedResource("[RESX:Underline]");
                QuoteText = Utilities.GetSharedResource("[RESX:Quote]");
                BoldDesc = Utilities.GetSharedResource("[RESX:BoldDesc]");
                ItalicsDesc = Utilities.GetSharedResource("[RESX:ItalicsDesc]");
                UnderlineDesc = Utilities.GetSharedResource("[RESX:UnderlineDesc]");
                QuoteDesc = Utilities.GetSharedResource("[RESX:QuoteDesc]");
                CodeText = Utilities.GetSharedResource("[RESX:Code]");
                CodeDesc = Utilities.GetSharedResource("[RESX:CodeDesc]");
                ImageText = Utilities.GetSharedResource("[RESX:Image]");
                ImageDesc = Utilities.GetSharedResource("[RESX:ImageDesc]");

                if (UseFilter)
                {
                    btnToolBar.Visible = true;
                }
                else
                {
                    btnToolBar.Visible = false;
                }
                Subject = Utilities.GetSharedResource("[RESX:SubjectPrefix]") + " " + Subject;
                //trSubscribe.Visible = AllowSubscribe;
                tdSubscribe.Visible = AllowSubscribe;
                if (AllowSubscribe)
                {
                    var subControl = new ToggleSubscribe(ForumModuleId, ForumId, TopicId, 1);
                    subControl.Checked = (UserPrefTopicSubscribe || Subscriptions.IsSubscribed(PortalId, ForumModuleId, ForumId, TopicId, SubscriptionTypes.Instant, this.UserId));
                    subControl.Text = "[RESX:TopicSubscribe:" + (UserPrefTopicSubscribe || Subscriptions.IsSubscribed(PortalId, ForumModuleId, ForumId, TopicId, SubscriptionTypes.Instant, this.UserId)).ToString().ToUpper() + "]";
                    tdSubscribe.InnerHtml = subControl.Render();
                }
                if (!Request.IsAuthenticated && CanReply)
                {
                    trUsername.Visible = true;
                    bolIsAnon = true;
                    trCaptcha.Visible = true;
                }
                else
                {
                    trUsername.Visible = false;
                    trCaptcha.Visible = false;
                    //if (UserPrefTopicSubscribe || Subscriptions.IsSubscribed(PortalId, ForumModuleId, ForumId, TopicId, SubscriptionTypes.Instant, this.UserId))
                    //{
                    //    SubscribedChecked = " checked=true";
                    //}
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

        #region  Web Form Designer Generated Code

        //This call is required by the Web Form Designer.
        [System.Diagnostics.DebuggerStepThrough()]
        private void InitializeComponent()
        {

        }

        private bool bolIsAnon = false;
        public string txtBodyID;
        public string DisplayMode;
        public string BoldText;
        public string ItalicsText;
        public string UnderlineText;
        public string QuoteText;
        public string BoldDesc;
        public string ItalicsDesc;
        public string UnderlineDesc;
        public string QuoteDesc;
        public string CodeText;
        public string CodeDesc;
        public string ImageText;
        public string ImageDesc;
        public string SubmitText = Utilities.GetSharedResource("Submit.Text");


        //NOTE: The following placeholder declaration is required by the Web Form Designer.
        //Do not delete or move it.
        private object designerPlaceholderDeclaration;

        protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

            //CODEGEN: This method call is required by the Web Form Designer
            //Do not modify it using the code editor.
            InitializeComponent();
        }

        #endregion
        protected void ContactByFaxOnlyCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            // if someone activates this checkbox send him home :-)
            Response.Redirect("http://localhost/", true);
        }
        private void SaveQuickReply()
        {
            SettingsInfo ms = SettingsBase.GetModuleSettings(ForumModuleId);
            int iFloodInterval = MainSettings.FloodInterval;
            if (iFloodInterval > 0)
            {
                plhMessage.Controls.Add(new InfoMessage { Message = "<div class=\"afmessage\">" + string.Format(GetSharedResource("[RESX:Error:FloodControl]"), MainSettings.FloodInterval) + "</div>" });
                return;
            }
            if (!Request.IsAuthenticated)
            {
                if ((!ctlCaptcha.IsValid) || txtUserName.Value == "")
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
                sUsername = Utilities.CleanString(PortalId, txtUserName.Value, false, EditorTypes.TEXTBOX, true, false, ForumModuleId, ThemePath, false);
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
            ReplyId = rc.Reply_Save(PortalId, ModuleId, ri);
            rc.UpdateModuleLastContentModifiedOnDate(ModuleId);
            DataCache.ContentCacheClear(ModuleId, string.Format(CacheKeys.TopicViewForUser, ModuleId, ri.TopicId, ri.Content.AuthorId));
            DataCache.CacheClearPrefix(ModuleId,string.Format(CacheKeys.ForumViewPrefix, ModuleId));


            DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = new TopicsController().Topics_Get(PortalId, ForumModuleId, TopicId, ForumId, -1, false);
            string fullURL = new ControlUtils().BuildUrl(ForumTabId, ForumModuleId, ForumInfo.ForumGroup.PrefixURL, ForumInfo.PrefixURL, ForumInfo.ForumGroupId, ForumInfo.ForumID, TopicId, ti.TopicUrl, -1, -1, string.Empty, -1, ReplyId, SocialGroupId);

            if (fullURL.Contains("~/"))
            {
                fullURL = Utilities.NavigateUrl(TabId, "", new string[] { ParamKeys.TopicId + "=" + TopicId, ParamKeys.ContentJumpId + "=" + ReplyId });
            }
            if (fullURL.EndsWith("/"))
            {
                fullURL += Utilities.UseFriendlyURLs(ForumModuleId) ? String.Concat("#", ReplyId) : String.Concat("?", ParamKeys.ContentJumpId, "=", ReplyId);            }
            if (isApproved)
            {

                //Send Subscriptions

                try
                {
                    Subscriptions.SendSubscriptions(PortalId, ForumModuleId, TabId, ForumId, TopicId, ReplyId, UserId);
                    try
                    {
                        Social amas = new Social();
                        amas.AddReplyToJournal(PortalId, ForumModuleId, TabId, ForumId, TopicId, ReplyId, UserId, fullURL, Subject, string.Empty, sBody, ForumInfo.Security.Read, SocialGroupId);
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
                List<DotNetNuke.Entities.Users.UserInfo> mods = Utilities.GetListOfModerators(PortalId, ForumId);
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
                notification.SenderUserID = UserInfo.UserID;
                notification.Context = notificationKey;

                NotificationsController.Instance.SendNotification(notification, PortalId, null, mods);

                var @params = new List<string> { ParamKeys.ForumId + "=" + ForumId, ParamKeys.ViewType + "=confirmaction", "afmsg=pendingmod", ParamKeys.TopicId + "=" + TopicId };
                if (SocialGroupId > 0)
                {
                    @params.Add("GroupId=" + SocialGroupId);
                }
                Response.Redirect(Utilities.NavigateUrl(TabId, "", @params.ToArray()), false);
            }
            else
            {
                //Dim fullURL As String = Utilities.NavigateUrl(TabId, "", New String() {ParamKeys.ForumId & "=" & ForumId, ParamKeys.ViewType & "=" & Views.Topic, ParamKeys.TopicId & "=" & TopicId, ParamKeys.ContentJumpId & "=" & ReplyId})
                //If MainSettings.UseShortUrls Then
                //    fullURL = Utilities.NavigateUrl(TabId, "", New String() {ParamKeys.TopicId & "=" & TopicId, ParamKeys.ContentJumpId & "=" & ReplyId})
                //End If

                try
                {
                    Social amas = new Social();
                    amas.AddReplyToJournal(PortalId, ForumModuleId,TabId, ForumId, TopicId, ReplyId, UserId, fullURL, Subject, string.Empty, sBody, ForumInfo.Security.Read, SocialGroupId);
                }
                catch (Exception ex)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                }
                Response.Redirect(fullURL, false);
            }

            //End If


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
            if (!Request.IsAuthenticated && Utilities.InputIsValid(txtUserName.Value.Trim()) == false)
            {
                reqUserName.Visible = true;
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
