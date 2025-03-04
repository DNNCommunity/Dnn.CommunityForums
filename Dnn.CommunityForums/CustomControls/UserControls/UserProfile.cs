// Copyright (c) by DNN Community
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
    using System.ComponentModel;
    using System.Text.RegularExpressions;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using DotNetNuke.Modules.ActiveForums.Enums;

    [DefaultProperty("Text"), ToolboxData("<{0}:UserProfile runat=server></{0}:UserProfile>")]
    public class UserProfile : ProfileBase
    {
        #region Enum
        public enum ProfileModes : int
        {
            View,
            Edit,
        }

        #endregion
        #region Private Members
        private ProfileModes profileMode = ProfileModes.View;

        #endregion
        #region Public Properties
        public ProfileModes ProfileMode
        {
            get
            {
                return this.profileMode;
            }

            set
            {
                this.profileMode = value;
            }
        }

        #endregion
        #region Protected Members
        protected PlaceHolder plh = new PlaceHolder();
        protected PlaceHolder plhTabs = new PlaceHolder();
        protected PlaceHolder plhProfileAdminSettings = new PlaceHolder();
        protected PlaceHolder plhProfilePrefs = new PlaceHolder();
        protected PlaceHolder plhProfileUserAccount = new PlaceHolder();
        protected PlaceHolder plhTracker = new PlaceHolder();
        protected PlaceHolder plhProfileEditButton = new PlaceHolder();
        protected PlaceHolder plhProfileSaveButton = new PlaceHolder();
        protected PlaceHolder plhProfileCancelButton = new PlaceHolder();
        protected DotNetNuke.Modules.ActiveForums.Controls.ActiveTabs amTabs = new DotNetNuke.Modules.ActiveForums.Controls.ActiveTabs();

        protected global::System.Web.UI.WebControls.TextBox txtSignature;
        protected global::DotNetNuke.Modules.ActiveForums.Controls.ImageButton btnProfileEdit;
        protected global::DotNetNuke.Modules.ActiveForums.Controls.ImageButton btnProfileSave;
        protected global::DotNetNuke.Modules.ActiveForums.Controls.ImageButton btnProfileCancel;
        protected global::System.Web.UI.HtmlControls.HtmlTableRow trAvatarLinks;
        protected global::System.Web.UI.HtmlControls.HtmlTable tblAvatars;
        protected global::System.Web.UI.WebControls.Label lblAvatarError;

        #endregion
        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.btnProfileEdit.Click += new System.EventHandler(this.btnProfileEdit_Click);
            this.btnProfileCancel.Click += new System.EventHandler(this.btnProfileCancel_Click);
            this.btnProfileSave.Click += new System.EventHandler(this.btnProfileSave_Click);

            this.AppRelativeVirtualPath = "~/";
            try
            {
                if (this.Request.QueryString["mode"] != null)
                {
                    if (this.Request.QueryString["mode"].ToLowerInvariant() == "edit" && this.CanEditMode())
                    {
                        this.ProfileMode = ProfileModes.Edit;
                    }
                    else
                    {
                        this.GoViewURL();
                    }
                }
                else
                {
                    this.ProfileMode = ProfileModes.View;
                }
            }
            catch (Exception ex)
            {
                this.ProfileMode = ProfileModes.View;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            string sTemplate = DotNetNuke.Modules.ActiveForums.Controllers.TemplateController.Template_Get(this.ForumModuleId, Enums.TemplateType._userProfile, SettingsBase.GetModuleSettings(this.ForumModuleId).ForumFeatureSettings.TemplateFileNameSuffix);

            if (this.ProfileMode == ProfileModes.Edit)
            {
                sTemplate = Globals.DnnControlsRegisterTag + sTemplate;
            }

            Literal lit = new Literal();

            var user = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ForumModuleId).GetByUserId(this.PortalId, this.UID);
            user.UserForums = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForumsForUser(this.PortalId, this.ForumModuleId, this.ForumUser, "CanRead");
            var author = new DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo(this.PortalId, this.ForumModuleId, this.UID);
            sTemplate = TemplateUtils.ParseProfileTemplate(this.ForumModuleId, sTemplate, author, this.ImagePath, this.ForumUser.CurrentUserType, false, false, string.Empty, this.UserInfo.UserID, this.TimeZoneOffset);
            sTemplate = this.RenderModals(sTemplate);

            sTemplate = sTemplate.Replace("[AM:CONTROLS:AdminProfileSettings]", "<asp:placeholder id=\"plhProfileAdminSettings\" runat=\"server\" />");
            sTemplate = sTemplate.Replace("[AM:CONTROLS:ProfileMyPreferences]", "<asp:placeholder id=\"plhProfilePrefs\" runat=\"server\" />");
            sTemplate = sTemplate.Replace("[AM:CONTROLS:ProfileUserAccount]", "<asp:placeholder id=\"plhProfileUserAccount\" runat=\"server\" />");
            sTemplate = sTemplate.Replace("[AM:CONTROLS:ProfileForumTracker]", "<asp:placeholder id=\"plhTracker\" runat=\"server\" />");
            sTemplate = sTemplate.Replace("[AF:PROFILE:VIEWUSERPOSTS]", "<a href=\"" + this.NavigateUrl(this.TabId, string.Empty, $"{ParamKeys.ViewType}={Views.Search}&{ParamKeys.UserId}={this.UID}") + "\">[RESX:ViewPostsByUser]</a>");

            sTemplate = this.GetTabsSection(sTemplate);
            Control ctl = this.ParseControl(sTemplate);
            this.Controls.Add(ctl);
            while (!(ctl.Controls.Count == 0))
            {
                this.Controls.Add(ctl.Controls[0]);
            }

            // Begin Load Tab Control
            this.plhTabs = (PlaceHolder)this.FindControl("plhTabs");
            if (this.plhTabs != null & this.amTabs != null)
            {
                this.plhTabs.Controls.Add(this.amTabs);
            }

            this.LinkControls(this.Controls);
            if (this.plhProfileEditButton != null)
            {
                this.btnProfileEdit = new ImageButton();
                this.btnProfileEdit.ID = "btnProfileEdit";
                this.btnProfileEdit.CssClass = "amimagebutton";
                this.btnProfileEdit.PostBack = true;
                this.btnProfileEdit.Height = 50;
                this.btnProfileEdit.Width = 50;
                this.btnProfileEdit.ImageLocation = "TOP";
                this.btnProfileEdit.Text = "[RESX:Button:Edit]";
                this.btnProfileEdit.ImageUrl = this.Page.ResolveUrl(DotNetNuke.Modules.ActiveForums.Globals.ModuleImagesPath + "edit32.png");
                this.btnProfileEdit.Visible = false;
                this.plhProfileEditButton.Controls.Add(this.btnProfileEdit);
            }

            if (this.plhProfileCancelButton != null)
            {
                this.btnProfileCancel = new ImageButton();
                this.btnProfileCancel.ID = "btnProfileCancel";
                this.btnProfileCancel.CssClass = "amimagebutton";
                this.btnProfileCancel.PostBack = true;
                this.btnProfileCancel.Height = 50;
                this.btnProfileCancel.Width = 50;
                this.btnProfileCancel.ImageLocation = "TOP";
                this.btnProfileCancel.Text = "[RESX:Button:Cancel]";
                this.btnProfileCancel.ImageUrl = this.Page.ResolveUrl(DotNetNuke.Modules.ActiveForums.Globals.ModuleImagesPath + "cancel32.png");
                this.btnProfileCancel.Visible = false;
                this.plhProfileCancelButton.Controls.Add(this.btnProfileCancel);
            }

            if (this.plhProfileSaveButton != null)
            {
                this.btnProfileSave = new ImageButton();
                this.btnProfileSave.ID = "btnProfileSave";
                this.btnProfileSave.CssClass = "amimagebutton";
                this.btnProfileSave.PostBack = true;
                this.btnProfileSave.Height = 50;
                this.btnProfileSave.Width = 50;
                this.btnProfileSave.ImageLocation = "TOP";
                this.btnProfileSave.Text = "[RESX:Button:Save]";
                this.btnProfileSave.ImageUrl = this.Page.ResolveUrl(DotNetNuke.Modules.ActiveForums.Globals.ModuleImagesPath + "save32.png");
                this.btnProfileSave.Visible = false;
                this.plhProfileSaveButton.Controls.Add(this.btnProfileSave);
            }

            if (this.plhProfileAdminSettings != null)
            {
                ProfileBase tmpCtl = (ProfileBase)this.LoadControl("<% (DotNetNuke.Modules.ActiveForums.Globals.ModulePath) %>controls/profile_adminsettings.ascx");
                tmpCtl.ModuleConfiguration = this.ModuleConfiguration;
                tmpCtl.ForumUserInfo = user;
                this.plhProfileAdminSettings.Controls.Add(tmpCtl);
            }

            if (this.plhProfilePrefs != null)
            {
                ProfileBase tmpCtl = (ProfileBase)this.LoadControl("<% (DotNetNuke.Modules.ActiveForums.Globals.ModulePath) %>controls/profile_mypreferences.ascx");
                tmpCtl.ModuleConfiguration = this.ModuleConfiguration;
                tmpCtl.ForumUserInfo = user;
                this.plhProfilePrefs.Controls.Add(tmpCtl);
            }

            if (this.plhProfileUserAccount != null)
            {
                ProfileBase tmpCtl = (ProfileBase)this.LoadControl("<% (DotNetNuke.Modules.ActiveForums.Globals.ModulePath) %>controls/profile_useraccount.ascx");
                tmpCtl.ModuleConfiguration = this.ModuleConfiguration;
                tmpCtl.ForumUserInfo = user;
                this.plhProfileUserAccount.Controls.Add(tmpCtl);
            }

            if (this.plhTracker != null)
            {
                ForumView ctlForums = new ForumView();
                ctlForums.ModuleConfiguration = this.ModuleConfiguration;
                ctlForums.DisplayTemplate = DotNetNuke.Modules.ActiveForums.Controllers.TemplateController.Template_Get(this.ForumModuleId, (Enums.TemplateType)Enum.Parse(typeof(Enums.TemplateType),  "ForumTracking", true), SettingsBase.GetModuleSettings(this.ForumModuleId).ForumFeatureSettings.TemplateFileNameSuffix);
                ctlForums.CurrentUserId = this.UID;
                ctlForums.ForumIds = user.UserForums;
                this.plhTracker.Controls.Add(ctlForums);
            }

            if (this.btnProfileEdit != null)
            {
                if (!(this.ForumUser.CurrentUserType == CurrentUserTypes.Anon) && (this.UID == this.UserId || this.ForumUser.CurrentUserType == CurrentUserTypes.Admin || this.ForumUser.CurrentUserType == CurrentUserTypes.Admin || this.ForumUser.CurrentUserType == CurrentUserTypes.SuperUser))
                {
                    if (this.ProfileMode == ProfileModes.View)
                    {
                        this.btnProfileEdit.Visible = true;
                        this.btnProfileCancel.Visible = false;
                        this.btnProfileSave.Visible = false;
                    }
                    else
                    {
                        this.btnProfileEdit.Visible = false;
                        this.btnProfileCancel.Visible = true;
                        this.btnProfileSave.Visible = true;
                    }
                }
            }
        }

        private void btnProfileEdit_Click(object sender, System.EventArgs e)
        {
            if (!(this.ForumUser.CurrentUserType == CurrentUserTypes.Anon) && (this.UID == this.UserId || this.ForumUser.CurrentUserType == CurrentUserTypes.Admin || this.ForumUser.CurrentUserType == CurrentUserTypes.Admin || this.ForumUser.CurrentUserType == CurrentUserTypes.SuperUser))
            {
                this.Response.Redirect(this.NavigateUrl(this.TabId, string.Empty, new string[] { $"{ParamKeys.ViewType}={Views.Profile}", $"{ParamKeys.UserId}={this.UID}", $"{ParamKeys.Mode}={Modes.Edit}" }), false);
                this.Context.ApplicationInstance.CompleteRequest();
            }
        }

        private void btnProfileCancel_Click(object sender, System.EventArgs e)
        {
            this.GoViewURL();
        }

        private void btnProfileSave_Click(object sender, System.EventArgs e)
        {
            if (this.SaveProfile())
            {
                this.GoViewURL();
            }
        }

        #endregion
        #region Private Methods
        private void GoViewURL()
        {
            this.Response.Redirect(this.NavigateUrl(this.TabId, string.Empty, new string[] { $"{ParamKeys.ViewType}={Views.Profile}", $"{ParamKeys.UserId}={this.UID}" }), false);
            this.Context.ApplicationInstance.CompleteRequest();
        }

        private bool CanEditMode()
        {
            if (!(this.ForumUser.CurrentUserType == CurrentUserTypes.Anon) && (this.UID == this.UserId || this.ForumUser.CurrentUserType == CurrentUserTypes.Admin || this.ForumUser.CurrentUserType == CurrentUserTypes.Admin || this.ForumUser.CurrentUserType == CurrentUserTypes.SuperUser))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void LinkControls(ControlCollection ctrls)
        {
            foreach (Control ctrl in ctrls)
            {
                switch (ctrl.ID)
                {
                    case "plhProfileAdminSettings":
                        this.plhProfileAdminSettings = (PlaceHolder)ctrl;
                        break;
                    case "plhProfilePrefs":
                        this.plhProfilePrefs = (PlaceHolder)ctrl;
                        break;
                    case "plhProfileUserAccount":
                        this.plhProfileUserAccount = (PlaceHolder)ctrl;
                        break;
                    case "plhTracker":
                        this.plhTracker = (PlaceHolder)ctrl;
                        break;
                    case "plhProfileEditButton":
                        this.plhProfileEditButton = (PlaceHolder)ctrl;
                        break;
                    case "plhProfileSaveButton":
                        this.plhProfileSaveButton = (PlaceHolder)ctrl;
                        break;
                    case "plhProfileCancelButton":
                        this.plhProfileCancelButton = (PlaceHolder)ctrl;
                        break;
                    case "trAvatarLinks":
                        this.trAvatarLinks = (System.Web.UI.HtmlControls.HtmlTableRow)ctrl;
                        break;
                    case "tblAvatars":
                        this.tblAvatars = (System.Web.UI.HtmlControls.HtmlTable)ctrl;
                        break;
                    case "txtSignature":
                        this.txtSignature = (TextBox)ctrl;
                        break;
                    case "btnProfileEdit":
                        this.btnProfileEdit = (ImageButton)ctrl;
                        break;
                    case "btnProfileCancel":
                        this.btnProfileCancel = (ImageButton)ctrl;
                        break;
                    case "btnProfileSave":
                        this.btnProfileSave = (ImageButton)ctrl;
                        break;
                    case "lblAvatarError":
                        this.lblAvatarError = (Label)ctrl;
                        break;
                }

                if (ctrl.Controls.Count > 0)
                {
                    this.LinkControls(ctrl.Controls);
                }
            }
        }

        private bool SaveProfile()
        {
            if (this.CanEditMode())
            {
                var user = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ForumModuleId).GetByUserId(this.PortalId, this.UID);
                if (user != null)
                {
                    if (this.MainSettings.AllowSignatures == 1)
                    {
                        user.Signature = Utilities.XSSFilter(this.txtSignature.Text, true);
                        user.Signature = Utilities.StripHTMLTag(user.Signature);
                        user.Signature = System.Net.WebUtility.HtmlEncode(user.Signature);
                    }
                    else if (this.MainSettings.AllowSignatures == 2)
                    {
                        user.Signature = Utilities.XSSFilter(this.txtSignature.Text, false);
                    }
                }

                DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.Save(user);
            }

            return true;
        }

        private Control RecursiveFind(Control ctl, string controlId)
        {
            if (ctl.ID == controlId)
            {
                return ctl;
            }

            foreach (Control tmpctl in ctl.Controls)
            {
                Control t = this.RecursiveFind(tmpctl, controlId);
                if (t != null)
                {
                    return t;
                }
            }

            return null;
        }

        private string RenderModals(string template)
        {
            string sOut = template;

            // [AM:CONTROLS:MODAL:MyPreferences:Private]
            string pattern = "(\\[AM:CONTROLS:MODAL:(.+?)\\])";
            Regex regExp = new Regex(pattern);
            MatchCollection matches = null;
            matches = regExp.Matches(sOut);
            bool bAddModal = false;
            foreach (Match match in matches)
            {
                string matchValue = match.Groups[2].Value;
                if (matchValue.Contains(":"))
                {
                    string sec = matchValue.Split(':')[1].ToLowerInvariant();
                    switch (sec.ToLowerInvariant())
                    {
                        case "anon":
                            bAddModal = true;
                            break;
                        case "private":
                            if (!(this.ForumUser.CurrentUserType == CurrentUserTypes.Anon))
                            {
                                if (this.UserId == this.UID || this.ForumUser.CurrentUserType == CurrentUserTypes.Admin || this.ForumUser.CurrentUserType == CurrentUserTypes.SuperUser)
                                {
                                    bAddModal = true;
                                }
                            }

                            break;
                        case "auth":
                            if (!(this.ForumUser.CurrentUserType == CurrentUserTypes.Anon))
                            {
                                bAddModal = true;
                            }

                            break;
                        case "forummod":
                            if (!(this.ForumUser.CurrentUserType == CurrentUserTypes.Anon) && !(this.ForumUser.CurrentUserType == CurrentUserTypes.Auth))
                            {
                                bAddModal = true;
                            }

                            break;
                        case "admin":
                            if (this.ForumUser.CurrentUserType == CurrentUserTypes.Admin || this.ForumUser.CurrentUserType == CurrentUserTypes.SuperUser)
                            {
                                bAddModal = true;
                            }

                            break;
                        case "superuser":
                            if (this.ForumUser.CurrentUserType == CurrentUserTypes.SuperUser)
                            {
                                bAddModal = true;
                            }

                            break;
                        default:
                            bAddModal = this.UserInfo.IsInRole(sec);
                            break;
                    }

                    matchValue = matchValue.Split(':')[0];
                }
                else
                {
                    bAddModal = true;
                }

                if (bAddModal == true)
                {
                    string sModalDivId = "afmodal" + matchValue;
                    string sModalText = "[RESX:Label:" + matchValue + "]";
                    string sModalContent = string.Empty;
                    template = template.Replace(match.Value, "<a href=\"javascript:void(0);\" onclick=\"amOpenModal('" + sModalDivId + "','" + sModalText + "',350, 300);\">" + sModalText + "</a>" + match.Value);
                    if (sOut.Contains(match.Value.Replace("[AM", "[/AM")))
                    {
                        string tmp = TemplateUtils.GetTemplateSection(sOut, match.Value, match.Value.Replace("[AM", "[/AM"));
                        sModalContent = "<div id=\"" + sModalDivId + "\" style=\"display:none;\">" + tmp + "</div>";
                    }

                    template = TemplateUtils.ReplaceSubSection(template, string.Empty, match.Value, match.Value.Replace("[AM", "[/AM"));
                    template = template + sModalContent;
                }
            }

            return template;
        }

        private string GetTabsSection(string template)
        {
            string sOut = string.Empty;
            sOut = TemplateUtils.GetTemplateSection(template, "[AM:CONTROLS:TABS]", "[/AM:CONTROLS:TABS]");
            string pattern = "(\\[AM:CONTROLS:TAB:(.+?)\\])";
            Regex regExp = new Regex(pattern);
            MatchCollection matches = null;
            matches = regExp.Matches(sOut);
            this.amTabs = new DotNetNuke.Modules.ActiveForums.Controls.ActiveTabs();
            foreach (Match match in matches)
            {
                bool bAddTab = false;
                string matchValue = match.Groups[2].Value;
                CurrentUserTypes access = CurrentUserTypes.Anon;
                if (matchValue.Contains(":"))
                {
                    string sec = matchValue.Split(':')[1].ToLowerInvariant();
                    if (sec == "private")
                    {
                        if (!(this.ForumUser.CurrentUserType == CurrentUserTypes.Anon))
                        {
                            if (this.UserId == this.UID || this.ForumUser.CurrentUserType == CurrentUserTypes.Admin || this.ForumUser.CurrentUserType == CurrentUserTypes.SuperUser)
                            {
                                bAddTab = true;
                                access = CurrentUserTypes.Admin;
                            }
                        }
                    }
                    else
                    {
                        switch (sec)
                        {
                            case "auth":
                                access = CurrentUserTypes.Auth;
                                break;
                            case "forummod":
                                access = CurrentUserTypes.ForumMod;
                                break;
                            case "admin":
                                access = CurrentUserTypes.Admin;
                                break;
                            case "superuser":
                                access = CurrentUserTypes.SuperUser;
                                break;
                        }

                        switch (access)
                        {
                            case CurrentUserTypes.Anon:
                                bAddTab = true;
                                break;
                            case CurrentUserTypes.Auth:
                                if (!(this.ForumUser.CurrentUserType == CurrentUserTypes.Anon))
                                {
                                    bAddTab = true;
                                }

                                break;
                            case CurrentUserTypes.ForumMod:
                                if (!(this.ForumUser.CurrentUserType == CurrentUserTypes.Anon) && !(this.ForumUser.CurrentUserType == CurrentUserTypes.Auth))
                                {
                                    bAddTab = true;
                                }

                                break;
                            case CurrentUserTypes.Admin:
                                if (this.ForumUser.CurrentUserType == CurrentUserTypes.Admin || this.ForumUser.CurrentUserType == CurrentUserTypes.SuperUser)
                                {
                                    bAddTab = true;
                                }

                                break;
                            case CurrentUserTypes.SuperUser:
                                if (this.ForumUser.CurrentUserType == CurrentUserTypes.SuperUser)
                                {
                                    bAddTab = true;
                                }

                                break;
                        }
                    }

                    matchValue = matchValue.Split(':')[0];
                }
                else
                {
                    bAddTab = true;
                }

                if (bAddTab)
                {
                    DotNetNuke.Modules.ActiveForums.Controls.Tab tb = new DotNetNuke.Modules.ActiveForums.Controls.Tab();
                    tb.ControlKey = matchValue;
                    tb.Text = "[RESX:Label:" + matchValue + "]";
                    if (sOut.Contains(match.Value.Replace("[AM", "[/AM")))
                    {
                        DotNetNuke.Modules.ActiveForums.Controls.TabContent tbc = new DotNetNuke.Modules.ActiveForums.Controls.TabContent();
                        string tmp = TemplateUtils.GetTemplateSection(sOut, match.Value, match.Value.Replace("[AM", "[/AM"));
                        if (tmp.Contains("<dnn:"))
                        {
                            tmp = Globals.DnnControlsRegisterTag + tmp;
                        }

                        if (tmp.Contains("<social:"))
                        {
                            tmp = Globals.SocialRegisterTag + tmp;
                        }

                        Control ctl = this.ParseControl(tmp);
                        tbc.Controls.Add(ctl);
                        tb.Content = tbc;
                    }

                    this.amTabs.Tabs.Add(tb);
                }
            }

            template = TemplateUtils.ReplaceSubSection(template, "<asp:placeholder id=\"plhTabs\" runat=\"server\" />", "[AM:CONTROLS:TABS]", "[/AM:CONTROLS:TABS]");
            return template;
        }
        #endregion
    }
}
