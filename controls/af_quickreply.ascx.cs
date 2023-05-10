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
using DotNetNuke.Modules.ActiveForums.DAL2;

namespace DotNetNuke.Modules.ActiveForums
{

    public partial class af_quickreplyform : ForumBase
    {
        #region Private Members
        private bool _CanReply = false;
        private string _ThemePath = string.Empty;
        private bool _UseFilter = true;
        private string _Subject = string.Empty;
        private bool _ModAppove = false;
        private bool _CanTrust = false;
        private bool _IsTrusted = false;
        private bool _TrustDefault = false;
        private bool _AllowHTML = false;
        private bool _AllowScripts = false;
        private bool _AllowSubscribe = false;
        private int _ForumId = -1;
        private bool _CanSubscribe = false;

        #endregion
        #region Public Members
        public string SubscribedChecked = string.Empty;
        //public IClientCapability device = ClientCapabilityProvider.CurrentClientCapability;
        #endregion
        #region Public Properties
        public bool UseFilter
        {
            get
            {
                return _UseFilter;
            }
            set
            {
                _UseFilter = value;
            }
        }
        public string Subject
        {
            get
            {
                return _Subject;
            }
            set
            {
                _Subject = value;
            }
        }
        public bool ModApprove
        {
            get
            {
                return _ModAppove;
            }
            set
            {
                _ModAppove = value;
            }
        }
        public bool CanTrust
        {
            get
            {
                return _CanTrust;
            }
            set
            {
                _CanTrust = value;
            }
        }
        public bool IsTrusted
        {
            get
            {
                return _IsTrusted;
            }
            set
            {
                _IsTrusted = value;
            }
        }
        public bool TrustDefault
        {
            get
            {
                return _TrustDefault;
            }
            set
            {
                _TrustDefault = value;
            }
        }
        public bool AllowHTML
        {
            get
            {
                return _AllowHTML;
            }
            set
            {
                _AllowHTML = value;
            }
        }
        public bool AllowScripts
        {
            get
            {
                return _AllowScripts;
            }
            set
            {
                _AllowScripts = value;
            }
        }
        public bool AllowSubscribe
        {
            get
            {
                return _AllowSubscribe;
            }
            set
            {
                _AllowSubscribe = value;
            }
        }
        public bool CanSubscribe
        {
            get
            {
                return _CanSubscribe;
            }
            set
            {
                _CanSubscribe = value;
            }
        }
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

                    AllowSubscribe = Permissions.HasPerm(forum.Security.Subscribe, ForumUser.UserRoles);
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
                trSubscribe.Visible = AllowSubscribe;
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
                    if (UserPrefTopicSubscribe || Subscriptions.IsSubscribed(PortalId, ForumModuleId, ForumId, TopicId, SubscriptionTypes.Instant, this.UserId))
                    {
                        SubscribedChecked = " checked=true";
                    }
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
            SettingsInfo ms = DataCache.MainSettings(ForumModuleId);
            if (!Utilities.HasFloodIntervalPassed(floodInterval: MainSettings.FloodInterval, user: ForumUser, forumInfo: forum))
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
            bool UserIsTrusted = Utilities.IsTrusted((int)forum.DefaultTrustValue, ui.TrustLevel, Permissions.HasPerm(forum.Security.Trust, ForumUser.UserRoles), forum.AutoTrustLevel, ui.PostCount);
            bool isApproved = Convert.ToBoolean(((forum.IsModerated == true) ? false : true));
            if (UserIsTrusted || Permissions.HasPerm(forum.Security.ModApprove, ForumUser.UserRoles))
            {
                isApproved = true;
            }
            ReplyInfo reply = new ReplyInfo();
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

            //Dim sSubject As String = Server.HtmlEncode(Request.Form("txtSubject"))
            //If (UseFilter) Then
            //    sSubject = Utilities.FilterWords(PortalId,  ForumModuleId, ThemePath, sSubject)
            //End If
            string sBody = string.Empty;
            if (AllowHTML)
            {
                AllowHTML = IsHtmlPermitted(forum.EditorPermittedUsers, IsTrusted, Permissions.HasPerm(forum.Security.ModEdit, ForumUser.UserRoles));
            }
            sBody = Utilities.CleanString(PortalId, Request.Form["txtBody"], AllowHTML, EditorTypes.TEXTBOX, UseFilter, AllowScripts, ForumModuleId, ThemePath, forum.AllowEmoticons);
            DateTime createDate = DateTime.UtcNow;
            reply.TopicId = TopicId;
            reply.ReplyToId = TopicId;
            reply.Content.AuthorId = UserId;
            reply.Content.AuthorName = sUsername;
            reply.Content.Body = sBody;
            reply.Content.DateCreated = createDate;
            reply.Content.DateUpdated = createDate;
            reply.Content.IsDeleted = false;
            reply.Content.Subject = Subject;
            reply.Content.Summary = string.Empty;
            reply.IsApproved = isApproved;
            reply.IsDeleted = false;
            reply.Content.IPAddress = Request.UserHostAddress;
            ReplyId = rc.Reply_Save(PortalId, reply);
            //Check if is subscribed
            string cachekey = string.Format("AF-FV-{0}-{1}", PortalId, ModuleId);
            DataCache.CacheClearPrefix(cachekey);


            // Subscribe or unsubscribe if needed
            if (AllowSubscribe && UserId > 0)
            {
                var subscribe = Request.Params["chkSubscribe"] == "1";
                var currentlySubscribed = Subscriptions.IsSubscribed(PortalId, ForumModuleId, ForumId, TopicId, SubscriptionTypes.Instant, UserId);

                if (subscribe != currentlySubscribed)
                {
                    // Will need to update this to support multiple subscrition types later
                    // Subscription_Update works as a toggle, so you only call it if you want to change the value.
                    var sc = new SubscriptionController();
                    sc.Subscription_Update(PortalId, ForumModuleId, ForumId, TopicId, 1, UserId, ForumUser.UserRoles);
                }
            }



            ControlUtils ctlUtils = new ControlUtils();
            TopicsController tc = new TopicsController();
            TopicInfo ti = tc.Topics_Get(PortalId, ForumModuleId, TopicId, ForumId, -1, false);
            
            if (isApproved)
            {
                DotNetNuke.Modules.ActiveForums.ReplyController.QueueApprovedReplyAfterAction(PortalId, TabId, ModuleId, forum.ForumGroupId, ForumId, TopicId, ReplyId, reply.Content.AuthorId);
                string fullURL = ctlUtils.BuildUrl(ForumTabId, ForumModuleId, forum.ForumGroup.PrefixURL, forum.PrefixURL, forum.ForumGroupId, forum.ForumID, TopicId, ti.TopicUrl, -1, -1, string.Empty, -1, ReplyId, SocialGroupId);
                if (fullURL.Contains("~/"))
                {
                    fullURL = Utilities.NavigateUrl(TabId,PortalId, "", new string[] { ParamKeys.TopicId + "=" + TopicId, ParamKeys.ContentJumpId + "=" + ReplyId });
                }
                if (fullURL.EndsWith("/"))
                {
                    fullURL += "?" + ParamKeys.ContentJumpId + "=" + ReplyId;
                }
                Response.Redirect(fullURL, false);
            }
            else 
            {
                DotNetNuke.Modules.ActiveForums.ReplyController.QueueUnapprovedReplyAfterAction(PortalId, TabId, ModuleId, forum.ForumGroupId, ForumId, TopicId, ReplyId, reply.Content.AuthorId);
                var @params = new List<string> { ParamKeys.ForumId + "=" + ForumId, ParamKeys.ViewType + "=confirmaction", "afmsg=pendingmod", ParamKeys.TopicId + "=" + TopicId };
                if (SocialGroupId > 0)
                {
                    @params.Add("GroupId=" + SocialGroupId);
                }
                Response.Redirect(Utilities.NavigateUrl(TabId,PortalId, "", @params.ToArray()), false);
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
