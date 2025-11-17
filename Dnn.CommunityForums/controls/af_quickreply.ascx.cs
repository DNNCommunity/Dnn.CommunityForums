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

namespace DotNetNuke.Modules.ActiveForums
{
    using System;
    using System.Collections.Generic;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    using DotNetNuke.Collections;
    using DotNetNuke.Framework.Providers;
    using DotNetNuke.Modules.ActiveForums.Controls;
    using DotNetNuke.Modules.ActiveForums.Enums;
    using DotNetNuke.Web.Client;
    using DotNetNuke.Web.Client.ClientResourceManagement;

    using log4net.Plugin;

    public partial class af_quickreplyform : ForumBase
    {
        private const string TargetCollapsible = "groupQR";
        protected System.Web.UI.HtmlControls.HtmlGenericControl contactByFaxOnly = new HtmlGenericControl();
        protected System.Web.UI.WebControls.CheckBox contactByFaxOnlyCheckBox = new CheckBox();
        protected System.Web.UI.HtmlControls.HtmlGenericControl qR = new HtmlGenericControl();
        protected System.Web.UI.WebControls.PlaceHolder plhMessage = new PlaceHolder();
        protected System.Web.UI.WebControls.Label reqBody = new Label();
        protected System.Web.UI.HtmlControls.HtmlGenericControl btnToolBar = new HtmlGenericControl();
        protected System.Web.UI.WebControls.LinkButton btnSubmitLink = new LinkButton();
        protected TextBox txtUsername = new TextBox();
        protected TextBox txtBody = new TextBox();
        protected System.Web.UI.WebControls.RequiredFieldValidator reqUsername = new System.Web.UI.WebControls.RequiredFieldValidator();
        protected UI.WebControls.CaptchaControl ctlCaptcha = new UI.WebControls.CaptchaControl();
        protected System.Web.UI.WebControls.Label reqSecurityCode = new System.Web.UI.WebControls.RequiredFieldValidator();
        protected System.Web.UI.WebControls.RequiredFieldValidator reqCaptcha = new System.Web.UI.WebControls.RequiredFieldValidator();
        
        protected PlaceHolder plhEditor = new PlaceHolder();
        protected Control txtEditor = null;

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

        [Obsolete("Deprecated in Community Forums. Scheduled removal in v10.0.0.0. Not Used")]
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
                    this.Response.Redirect("about:blank", false);
                    this.Context.ApplicationInstance.CompleteRequest();
                }

                if (this.Request.IsAuthenticated)
                {
                    this.btnSubmitLink.OnClientClick = "afQuickSubmit(); return false;";
                }
                else
                {
                    this.reqUsername.Enabled = true;
                    this.reqUsername.Text = "<img src=\"" + this.ImagePath + "/images/warning.png\" />";
                    this.reqBody.Text = "<img src=\"" + this.ImagePath + "/images/warning.png\" />";
                    this.reqSecurityCode.Text = "<img src=\"" + this.ImagePath + "/images/warning.png\" />";
                    this.btnSubmitLink.Click += this.ambtnSubmit_Click;
                }

                this.btnToolBar.Visible = this.UseFilter;

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
            string template = DotNetNuke.Modules.ActiveForums.Controllers.TemplateController.Template_Get(this.ForumModuleId, Enums.TemplateType.QuickReply, this.ForumInfo.FeatureSettings.TemplateFileNameSuffix);

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
                DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forumInfo = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Forums_Get(this.PortalId, this.ForumModuleId, this.ForumId, false, this.TopicId);
                if (forumInfo.FeatureSettings.EditorType.Equals(EditorType.DNNCKEDITOR4PLUSFORUMSPLUGINS))
                {
                    var editorProvider = ProviderConfiguration.GetProviderConfiguration("htmlEditor");
                    if (editorProvider != null && !string.IsNullOrEmpty(editorProvider.DefaultProvider) && (editorProvider.DefaultProvider.Contains("CKHtmlEditorProvider") || editorProvider.DefaultProvider.Contains("DNNConnect.CKE")))
                    {
                        Unit editorWidth;
                        Unit editorHeight;
                        if (Convert.ToString(this.ForumInfo.FeatureSettings.EditorWidth) != null)
                        {
                            editorWidth = Convert.ToString(this.ForumInfo.FeatureSettings.EditorWidth).IndexOf("%", 0, StringComparison.Ordinal) + 1 > 0 ? Unit.Percentage(Convert.ToDouble(Convert.ToString(this.ForumInfo.FeatureSettings.EditorWidth).TrimEnd('%'))) : Unit.Parse(this.ForumInfo.FeatureSettings.EditorWidth);
                        }
                        else
                        {
                            editorWidth = Unit.Pixel(600);
                        }

                        editorHeight = Convert.ToString(this.ForumInfo.FeatureSettings.EditorHeight) != null ? Unit.Parse(this.ForumInfo.FeatureSettings.EditorHeight) : Unit.Pixel(400);
                        var editor = new UI.UserControls.TextEditor();
                        this.txtEditor = editor;
                        editor = (UI.UserControls.TextEditor)this.LoadControl("~/controls/TextEditor.ascx");
                        editor.ID = "txtBody";
                        editor.ChooseMode = false;
                        editor.DefaultMode = "R";
                        editor.ChooseRender = false;
                        editor.Mode = "RICH";
                        editor.Width = editorWidth;
                        editor.Height = editorHeight;
                        editor.HtmlEncode = false; // Turn Encoding off or passed already Encoded HTML.
                        this.plhEditor = new PlaceHolder();
                        this.plhEditor = (PlaceHolder)this.qR.FindControl("plhEditor");
                        this.plhEditor.Controls.Add(editor);
                        this.txtBody.Visible = false;
                        this.btnToolBar.Visible = false;
                        //////////System.Text.StringBuilder sb = new System.Text.StringBuilder();
                        ////////////sb.Append("var afeditor = '" + editor.ClientID + "';");
                        //////////sb.Append("editorConfigeditortxtBody.customConfig = '" + this.Page.ResolveUrl(Globals.ModulePath + "Resources/ckeditor-4.22.1-additional-plugins/customConfig.js") + "';");
                        //////////var extraPlugins = new string[] { "mentions", "ajax", "autocomplete", "textmatch", "textwatcher", "xml" };
                        //////////foreach (string plugin in extraPlugins)
                        //////////{
                        //////////    sb.Append($"editorConfigeditortxtBody.extraPlugins += `,{plugin}`;");
                        //////////}

                        //////////var userTag = this.GetTagForUserMentions();
                        //////////var avatarTag = this.GetAvatarTagForUserMentions();

                        //////////sb.Append("editorConfigeditortxtBody.mentions = [");
                        //////////sb.Append(" { feed: function( opts, callback ) { " + "var sf = $.ServicesFramework(" + this.ForumModuleId + ");" + "var url = dnn.getVar('sf_siteRoot', '/') + 'API/ActiveForums/User/GetUsersForEditorMentions?forumId=" + this.ForumInfo.ForumID.ToString() + "&query=';" + "var xhr = new XMLHttpRequest();xhr.onreadystatechange = function() { if ( xhr.readyState == 4 ) { if ( xhr.status == 200 ) { callback( JSON.parse( this.responseText ) ); } else { callback( [] ); } } }; xhr.open( 'GET', url + opts.query ); xhr.setRequestHeader('RequestVerificationToken',$('[name=\"__RequestVerificationToken\"]').val()); xhr.setRequestHeader('ModuleId'," + this.ForumModuleId + "); xhr.setRequestHeader('TabId'," + this.TabId + "); xhr.send(); }, marker: '@', minChars: 3, throttle: 100, followingSpace: true, itemTemplate: '<li data-id=\"{id}\" class=\"dcf-mentions-user\">" + avatarTag + "{name}</li>', outputTemplate: `<a href=\"" + userTag + "\">" + avatarTag + "&nbsp;{name}</a>` },");
                        //////////sb.Append("];");

                        ////////////        if (this.ForumUser.IsAnonymous || (!this.ForumUser.IsAdmin && !this.ForumUser.IsSuperUser && !this.ForumInfo.GetIsMod(this.ForumUser)))
                        ////////////        {
                        ////////////            sb.Append("CKEDITOR.config.toolbar = [{ name: 'basicstyles', items: [ 'Bold', 'Italic', 'Underline' ] },{ name: 'clipboard', items: [ 'Cut', 'Copy', 'Paste' ] },{ name: 'undo', items: [ 'Undo', 'Redo' ] },{ name: 'paragraph', items: [ 'NumberedList', 'BulletedList', '-', 'Outdent', 'Indent' ] }];");

                        ////////////            sb.Append("CKEDITOR.config.toolbarCanCollapse = false;");
                        ////////////            sb.Append("CKEDITOR.config.toolbarStartupExpanded = true;");
                        ////////////            sb.Append("CKEDITOR.config.removePlugins = 'elementspath';");
                        ////////////            sb.Append("CKEDITOR.config.resize_enabled = false;");
                        ////////////        }

                        //////////Type editorAssemblyType = null;
                        //////////var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                        //////////foreach (var assembly in assemblies)
                        //////////{
                        //////////    if (assembly.FullName.StartsWith("DNNConnect.CKE"))
                        //////////    {
                        //////////        editorAssemblyType = assembly.GetType("DNNConnect.CKEditorProvider.Web.EditorControl");
                        //////////        break;
                        //////////    }
                        //////////}
                        //////////this.Page.ClientScript.RegisterClientScriptBlock(editorAssemblyType, $"{this.txtBody.ClientID}_txtBody_CKE_Config", sb.ToString(), true);
                    }
                }
                //if (this.txtBody != null)
                //{
                //    var usertag = Utilities.NavigateURL(this.PortalSettings.UserTabId, string.Empty, new[] { "userId={id}" });
                //    var avatartag = Utilities.ResolveUrl(this.PortalSettings, "<img class=\"af-avatar\" src=\"https://" + this.PortalSettings.DefaultPortalAlias + "/DnnImageHandler.ashx?mode=profilepic&userId={id}&h=20&w=20\" />");
                //    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                //    sb.Append("window.onload = function () { CKEDITOR.replace( '" + this.txtBody.ClientID + "', {versionCheck: false});};");
                //    sb.Append("CKEDITOR.config.mentions = [");
                //    sb.Append(" { feed: function( opts, callback ) { " + "var sf = $.ServicesFramework(" + this.ForumModuleId + ");" + "var url = dnn.getVar('sf_siteRoot', '/') + 'API/ActiveForums/User/GetUsersForEditorMentions?forumId=" + this.ForumInfo.ForumID.ToString() + "&query=';" + "var xhr = new XMLHttpRequest();xhr.onreadystatechange = function() { if ( xhr.readyState == 4 ) { if ( xhr.status == 200 ) { callback( JSON.parse( this.responseText ) ); } else { callback( [] ); } } }; xhr.open( 'GET', url + opts.query ); xhr.setRequestHeader('RequestVerificationToken',$('[name=\"__RequestVerificationToken\"]').val()); xhr.setRequestHeader('ModuleId'," + this.ForumModuleId + "); xhr.setRequestHeader('TabId'," + this.TabId + "); xhr.send(); }, marker: '@', minChars: 1, followingSpace: true, itemTemplate: '<li data-id=\"{id}\" class=\"dcf-mentions-user\">" + avatartag + "{name}</li>', outputTemplate: `<a href=\"" + usertag + "\">" + avatartag + "&nbsp;{name}</a>` }");

                //    var tagtag = Utilities.NavigateURL(this.ForumInfo.GetTabId(), string.Empty, new[] { $"{ParamKeys.ViewType}={Views.Search}", string.Concat(ParamKeys.Tags, "={name}") });
                //    sb.Append(",");
                //    sb.Append("{ feed: function( opts, callback ) { " + "var sf = $.ServicesFramework(" + this.ForumModuleId + ");" + "var url = dnn.getVar('sf_siteRoot', '/') + 'API/ActiveForums/Tag/Matches?forumId=" + this.ForumInfo.ForumID.ToString() + "&query=';" + "var xhr = new XMLHttpRequest();xhr.onreadystatechange = function() { if ( xhr.readyState == 4 ) { if ( xhr.status == 200 ) { callback( JSON.parse( this.responseText ) ); } else { callback( [] ); } } }; xhr.open( 'GET', url + opts.query ); xhr.setRequestHeader('RequestVerificationToken',$('[name=\"__RequestVerificationToken\"]').val()); xhr.setRequestHeader('ModuleId'," + this.ForumModuleId + "); xhr.setRequestHeader('TabId'," + this.TabId + "); xhr.send(); }, marker: '#', minChars: 2, followingSpace: true, itemTemplate: '<li data-id=\"{id}\" class=\"dcf-mentions-tag\">" + "{name}</li>', outputTemplate: `<a href=\"" + tagtag + "\" class=\"dcf-tag-link\">{name}</a>` },");

                //    sb.Append("];");
                //    var lang = string.Empty;
                //    if (this.Request.QueryString["language"] != null)
                //    {
                //        lang = this.Request.QueryString["language"];
                //    }

                //    if (string.IsNullOrEmpty(lang))
                //    {
                //        lang = this.UserInfo.Profile.PreferredLocale;
                //    }

                //    if (string.IsNullOrEmpty(lang))
                //    {
                //        lang = this.PortalSettings.DefaultLanguage;
                //    }

                //    if (string.IsNullOrEmpty(lang))
                //    {
                //        lang = "en-US";
                //    }

                //    sb.Append("CKEDITOR.config.language = '" + lang.Substring(0, 2) + "';");

                //    sb.Append("CKEDITOR.config.toolbar = [{ name: 'basicstyles', items: [ 'Bold', 'Italic', 'Underline' ] },{ name: 'clipboard', items: [ 'Cut', 'Copy', 'Paste' ] },{ name: 'undo', items: [ 'Undo', 'Redo' ] }, { name: 'links', items: [ 'Link' ] },  { name: 'insert', items: [ 'Image', 'Smiley', 'Code' ] } ];");
                //    sb.Append("CKEDITOR.config.toolbarCanCollapse = false;");
                //    sb.Append("CKEDITOR.config.toolbarStartupExpanded = true;");
                //    sb.Append("CKEDITOR.config.extraPlugins = 'codeTag';");
                //    sb.Append("CKEDITOR.config.removeButtons = 'Form,Checkbox,Radio,TextField,Textarea,Select,Button,ImageButton,HiddenField,PasteFromWord,Print,Preview,ExportPdf,NewPage,Save,Replace,Find,BGColor,TextColor,HorizontalRule,Anchor,Unlink,BidiLtr,BidiRtl,Language,CreateDiv,CopyFormatting,RemoveFormat,Subscript,Superscript,Strike,Format,Source,Templates,SelectAll,Scayt,PasteText,Styles,Font,FontSize,About,Maximize,Table,SpecialChar,PageBreak,Iframe,JustifyLeft,JustifyCenter,JustifyRight,JustifyBlock,Indent,Outdent,NumberedList,BulletedList';");
                //    sb.Append("CKEDITOR.config.removePlugins = 'elementspath';");
                //    sb.Append("CKEDITOR.config.resize_enabled = false;");

                //    ClientResourceManager.RegisterScript(ctl.Page, string.Concat(Globals.ModulePath, "resources/ckeditor-4.22.1/ckeditor.js"), FileOrder.Js.DefaultPriority + 2, "DnnPageHeaderProvider");
                //    ctl.Page.ClientScript.RegisterClientScriptBlock(ctl.Page.GetType(), "QuickReply", sb.ToString(), true); 
                //}
            }
            catch (Exception)
            {
                throw;
            }
        }
        
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            this.InjectMentionsPluginsForCkEditor4();
        }

        private void InjectMentionsPluginsForCkEditor4()
        {
            DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forumInfo = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Forums_Get(this.PortalId, this.ForumModuleId, this.ForumId, false, this.TopicId);
            if (forumInfo.FeatureSettings.EditorType.Equals(EditorType.DNNCKEDITOR4PLUSFORUMSPLUGINS))
            {
                var editorProvider = ProviderConfiguration.GetProviderConfiguration("htmlEditor");
                if (editorProvider != null && !string.IsNullOrEmpty(editorProvider.DefaultProvider) && (editorProvider.DefaultProvider.Contains("CKHtmlEditorProvider") || editorProvider.DefaultProvider.Contains("DNNConnect.CKE")))
                {
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    sb.Append("editorConfigeditortxtBody.customConfig = '" + this.Page.ResolveUrl(Globals.ModulePath + "Resources/ckeditor-4.22.1-additional-plugins/customConfig.js") + "';");
                    var extraPlugins = new string[] { "mentions", "ajax", "autocomplete", "textmatch", "textwatcher", "xml", "codeTag"};
                    foreach (string plugin in extraPlugins)
                    {
                        sb.Append($"editorConfigeditortxtBody.extraPlugins += `,{plugin}`;");
                    }

                    var userTag = this.GetTagForUserMentions();
                    var avatarTag = this.GetAvatarTagForUserMentions();

                    sb.Append("editorConfigeditortxtBody.mentions = [");
                    sb.Append(" { feed: function( opts, callback ) { " + "var sf = $.ServicesFramework(" + this.ForumModuleId + ");" + "var url = dnn.getVar('sf_siteRoot', '/') + 'API/ActiveForums/User/GetUsersForEditorMentions?forumId=" + this.ForumInfo.ForumID.ToString() + "&query=';" + "var xhr = new XMLHttpRequest();xhr.onreadystatechange = function() { if ( xhr.readyState == 4 ) { if ( xhr.status == 200 ) { callback( JSON.parse( this.responseText ) ); } else { callback( [] ); } } }; xhr.open( 'GET', url + opts.query ); xhr.setRequestHeader('RequestVerificationToken',$('[name=\"__RequestVerificationToken\"]').val()); xhr.setRequestHeader('ModuleId'," + this.ForumModuleId + "); xhr.setRequestHeader('TabId'," + this.TabId + "); xhr.send(); }, marker: '@', minChars: 3, throttle: 100, followingSpace: true, itemTemplate: '<li data-id=\"{id}\" class=\"dcf-mentions-user\">" + avatarTag + "{name}</li>', outputTemplate: `<a href=\"" + userTag + "\">" + avatarTag + "&nbsp;{name}</a>` },");
                    sb.Append("];");
                    sb.Append("editorConfigeditortxtBody.toolbar = [{ name: 'basicstyles', items: [ 'Bold', 'Italic', 'Underline' ] },{ name: 'clipboard', items: [ 'Cut', 'Copy', 'Paste' ] },{ name: 'undo', items: [ 'Undo', 'Redo' ] }, { name: 'links', items: [ 'Link' ] },  { name: 'insert', items: [ 'Image', 'Smiley', 'Code' ] } ];");
                    sb.Append("editorConfigeditortxtBody.toolbarCanCollapse = false;");
                    sb.Append("editorConfigeditortxtBody.toolbarStartupExpanded = true;");
                    sb.Append("editorConfigeditortxtBody.removePlugins = 'elementspath,wordcount';");
                    sb.Append("editorConfigeditortxtBody.resize_enabled = true;");
                    sb.Append("editorConfigeditortxtBody.removeButtons = 'Form,Checkbox,Radio,TextField,Textarea,Select,Button,ImageButton,HiddenField,PasteFromWord,Print,Preview,ExportPdf,NewPage,Save,Replace,Find,BGColor,TextColor,HorizontalRule,Anchor,Unlink,BidiLtr,BidiRtl,Language,CreateDiv,CopyFormatting,RemoveFormat,Subscript,Superscript,Strike,Format,Source,Templates,SelectAll,Scayt,PasteText,Styles,Font,FontSize,About,Maximize,Table,SpecialChar,PageBreak,Iframe,JustifyLeft,JustifyCenter,JustifyRight,JustifyBlock,Indent,Outdent,NumberedList,BulletedList';");
                    this.Page.ClientScript.RegisterClientScriptBlock(this.GetType(), $"{this.txtEditor.ClientID}_txtBody_CKE_Config", sb.ToString(), true);
                }
            }
        }

        private string GetAvatarTagForUserMentions()
        {
            return Utilities.ResolveUrl(this.PortalSettings, "<img class=\"af-avatar\" src=\"https://" + this.PortalSettings.DefaultPortalAlias + "/DnnImageHandler.ashx?mode=profilepic&userId={id}&h=20&w=20\" />");
        }

        private string GetTagForUserMentions()
        {
            return Utilities.NavigateURL(this.PortalSettings.UserTabId, string.Empty, new[] { "userId={id}" });
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
                    case "txtBody":
                        this.txtBody = (System.Web.UI.WebControls.TextBox)ctrl;
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
            this.Response.Redirect("about:blank", false);
            this.Context.ApplicationInstance.CompleteRequest();
        }

        private void SaveQuickReply()
        {
            DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forumInfo = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Forums_Get(this.PortalId, this.ForumModuleId, this.ForumId, false, this.TopicId);
            if (!Utilities.HasFloodIntervalPassed(floodInterval: this.ForumInfo.MainSettings.FloodInterval, forumUser: this.ForumUser, forumInfo: forumInfo))
            {
                Controls.InfoMessage im = new Controls.InfoMessage();
                im.Message = "<div class=\"afmessage\">" + string.Format(Utilities.GetSharedResource("[RESX:Error:FloodControl]"), this.ModuleSettings.FloodInterval) + "</div>";
                this.plhMessage.Controls.Add(im);
                return;
            }

            if (!this.Request.IsAuthenticated)
            {
                if ((!this.ctlCaptcha.IsValid) || this.txtUsername.Text == string.Empty)
                {
                    return;
                }
            }

            DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo user = new DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo(this.ForumModuleId);
            if (this.UserId > 0)
            {
                user = this.ForumUser;
            }
            else
            {
                user.TopicCount = 0;
                user.ReplyCount = 0;
                user.RewardPoints = 0;
                user.TrustLevel = -1;
            }

            bool UserIsTrusted = Utilities.IsTrusted((int)this.ForumInfo.FeatureSettings.DefaultTrustValue, user.TrustLevel, DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(this.ForumInfo.Security.TrustRoleIds, this.ForumUser.UserRoleIds), this.ForumInfo.FeatureSettings.AutoTrustLevel, user.PostCount);
            bool isApproved = Convert.ToBoolean((this.ForumInfo.FeatureSettings.IsModerated == true) ? false : true);
            if (UserIsTrusted || DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(this.ForumInfo.Security.ModerateRoleIds, this.ForumUser.UserRoleIds))
            {
                isApproved = true;
            }

            DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo ri = new DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo();
            ri.ReplyToId = this.TopicId;
            ri.TopicId = this.TopicId;
            ri.ForumId = this.ForumId;
            ri.StatusId = -1;
            ri.Content = new DotNetNuke.Modules.ActiveForums.Entities.ContentInfo();
            string sUsername = string.Empty;
            if (this.Request.IsAuthenticated)
            {
                switch (this.ModuleSettings.UserNameDisplay.ToUpperInvariant())
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
                sUsername = Utilities.CleanString(this.PortalId, this.txtUsername.Text, false, EditorType.TEXTBOX, true, false, this.ForumModuleId, this.ThemePath, false);
            }

            string sBody = string.Empty;
            if (this.AllowHTML)
            {
                this.AllowHTML = this.IsHtmlPermitted(this.ForumInfo.FeatureSettings.EditorPermittedUsers, this.IsTrusted, DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(this.ForumInfo.Security.ModerateRoleIds, this.ForumUser.UserRoleIds));
            }

            sBody = Utilities.CleanString(this.PortalId, CodeParser.ConvertCodeBrackets(this.Request.Form["txtBody"]), this.AllowHTML, EditorType.DNNCKEDITOR4PLUSFORUMSPLUGINS, this.UseFilter, this.AllowScripts, this.ForumModuleId, this.ThemePath, this.ForumInfo.FeatureSettings.AllowEmoticons);
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

            var rc = new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController(this.ForumModuleId);
            int replyId = rc.Reply_Save(this.PortalId, this.ModuleId, ri);
            ri = rc.GetById(replyId);
            DataCache.ContentCacheClearForForum(this.ModuleId, this.ForumId);
            DataCache.ContentCacheClearForReply(this.ModuleId, replyId);
            DataCache.ContentCacheClearForTopic(this.ModuleId, ri.TopicId);

            DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(this.ForumModuleId).GetById(this.TopicId);
            string fullURL = new ControlUtils().BuildUrl(this.PortalId, this.TabId, this.ForumModuleId, this.ForumInfo.ForumGroup.PrefixURL, this.ForumInfo.PrefixURL, this.ForumInfo.ForumGroupId, this.ForumInfo.ForumID, this.TopicId, ti.TopicUrl, -1, -1, string.Empty, -1, replyId, this.SocialGroupId);

            if (fullURL.Contains("~/"))
            {
                fullURL = Utilities.NavigateURL(this.TabId, string.Empty, new string[] { ParamKeys.TopicId + "=" + this.TopicId, ParamKeys.ContentJumpId + "=" + replyId });
            }

            if (fullURL.EndsWith("/"))
            {
                fullURL += Utilities.UseFriendlyURLs(this.ForumModuleId) ? String.Concat("#", replyId) : String.Concat("?", ParamKeys.ContentJumpId, "=", replyId);
            }

            if (isApproved)
            {
                DotNetNuke.Modules.ActiveForums.Controllers.ReplyController.QueueApprovedReplyAfterAction(portalId: this.PortalId, tabId: this.TabId, moduleId: this.ModuleId, forumGroupId: ri.Forum.ForumGroupId, forumId: this.ForumId, topicId: this.TopicId, replyId: replyId, contentId: ri.ContentId, authorId: ri.Content.AuthorId, userId: this.ForumUser.UserId);

                // Redirect to show post
                this.Response.Redirect(fullURL, false);
                this.Context.ApplicationInstance.CompleteRequest();
            }
            else
            {
                DotNetNuke.Modules.ActiveForums.Controllers.ReplyController.QueueUnapprovedReplyAfterAction(portalId: this.PortalId, tabId: this.TabId, moduleId: this.ModuleId, forumGroupId: ri.Forum.ForumGroupId, forumId: this.ForumId, topicId: this.TopicId, replyId: replyId, contentId: ri.ContentId, authorId: ri.Content.AuthorId, userId: this.ForumUser.UserId);

                var @params = new List<string> { ParamKeys.ForumId + "=" + this.ForumId, ParamKeys.ViewType + "=confirmaction", "afmsg=pendingmod", ParamKeys.TopicId + "=" + this.TopicId };
                if (this.SocialGroupId > 0)
                {
                    @params.Add("GroupId=" + this.SocialGroupId);
                }

                this.Response.Redirect(Utilities.NavigateURL(this.TabId, string.Empty, @params.ToArray()), false);
                this.Context.ApplicationInstance.CompleteRequest();
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
