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
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    [DefaultProperty("Text"), ToolboxData("<{0}:SubmitForm runat=server></{0}:SubmitForm>")]
    public class SubmitForm : TopicBase
    {
        #region EditorModes
        public enum EditorModes : int
        {
            NewTopic,
            Reply,
            ReplyWithBody,
            Quote,
            EditTopic,
            EditReply,
        }

        #endregion
        #region Private Members
        private string subject = string.Empty;
        private string summary = string.Empty;
        private string body = string.Empty;
        private string clientId;
        private string topicIcon;
        private bool locked;
        private bool @checked;
        private bool pinned;
        private DateTime announceStart = DateTime.MinValue;
        private DateTime announceEnd = DateTime.MinValue;
        private string pollQuestion;
        private string pollType;
        private string pollOptions;
        private int statusId = -1;
        private string authorName = string.Empty;
        private string topicReviewTemplate = string.Empty;
        private int topicPriority;
        private bool canModEdit;
        private bool canModerate;
        private bool canEdit;
        private bool canReply;
        private bool canCreate;
        private bool canAttach;
        private bool canTrust;
        private bool canLock;
        private bool canPin;
        private bool canAnnounce;
        private bool canSubscribe;

        #endregion
        #region Public Properties
        public string Template { get; set; } = string.Empty;

        public string AuthorName
        {
            get
            => this.txtUsername.Text;
            set
            {
                this.authorName = value;
                this.txtUsername.Text = value;
            }
        }

        public string Subject
        {
            get => this.txtSubject.Text;
            set
            {
                this.subject = value;
                this.txtSubject.Text = value;
            }
        }

        public string TopicSubject { get; set; } = string.Empty;

        public string Summary
        {
            get => this.txtSummary.Text;
            set
            {
                this.summary = value;
                this.txtSummary.Text = value;
            }
        }

        public string Body
        {
            get
            {
                string tempBody = null;
                if (this.plhEditor.Controls.Count > 0)
                {
                    switch (this.EditorType)
                    {
                        // Case EditorTypes.ACTIVEEDITOR
                        //    Dim txtEditor As New ActiveEditorControls.ActiveEditor
                        //    txtEditor = CType(plhEditor.Controls.Item(0), ActiveEditorControls.ActiveEditor)
                        //    Body = txtEditor.Text
                        case EditorTypes.HTMLEDITORPROVIDER:
                            tempBody = ((UI.UserControls.TextEditor)this.plhEditor.FindControl("txtBody")).Text;
                            break;
                        case EditorTypes.TEXTBOX:
                            tempBody = ((TextBox)this.txtEditor).Text;
                            break;
                    }
                }
                else
                {
                    return this.body;
                }

                return tempBody;
            }

            set
            {
                this.body = value;
            }
        }

        public ImageButton PostButton { get; set; } = new ImageButton();

        public ImageButton CancelButton { get; set; } = new ImageButton();

        public EditorTypes EditorType { get; set; }

        public DotNetNuke.Modules.ActiveForums.Entities.ForumInfo ForumInfo { get; set; }

        public string TopicIcon
        {
            get => string.IsNullOrEmpty(this.afposticons.PostIcon) ? string.Empty : this.afposticons.PostIcon;
            set
            {
                this.topicIcon = value;
                this.afposticons.PostIcon = value;
            }
        }

        public bool Locked
        {
            get => this.chkLocked.Checked;
            set
            {
                this.locked = value;
                this.chkLocked.Checked = value;
            }
        }

        public bool Pinned
        {
            get => this.chkPinned.Checked;
            set
            {
                this.pinned = value;
                this.chkPinned.Checked = value;
            }
        }

        public bool IsApproved
        {
            get => this.chkApproved.Checked;
            set
            {
                this.@checked = value;
                this.chkApproved.Checked = value;
            }
        }

        public bool IsAnnounce
        {
            get => this.chkAnnounce.Checked;
            set => this.chkAnnounce.Checked = value;
        }

        public int TopicPriority
        {
            get => int.Parse(this.txtTopicPriority.Text);
            set
            {
                this.txtTopicPriority.Text = value.ToString();
                this.topicPriority = value;
            }
        }

        public DateTime AnnounceStart
        {/* for announce, only set/get date without time */
            get
            {
                if (this.calStartDate.SelectedDate == string.Empty && this.announceStart == DateTime.MinValue)
                {
                    return Utilities.NullDate();
                }

                if (!string.IsNullOrEmpty(this.calStartDate.SelectedDate))
                {
                    return Convert.ToDateTime(this.calStartDate.SelectedDate).Date;
                }

                return this.announceStart;
            }

            set
            {
                this.announceStart = value;
                this.calStartDate.SelectedDate = value.Date.ToString();
            }
        }

        public DateTime AnnounceEnd
        {/* for announce, only want date without time */
            get
            {
                if (this.calEndDate.SelectedDate == string.Empty && this.announceEnd == DateTime.MinValue)
                {
                    return Utilities.NullDate();
                }

                if (!string.IsNullOrEmpty(this.calEndDate.SelectedDate))
                {
                    return Convert.ToDateTime(this.calEndDate.SelectedDate).Date;
                }

                return this.announceEnd;
            }

            set
            {
                this.announceEnd = value;
                this.calEndDate.SelectedDate = Convert.ToDateTime(value).Date.ToString();
            }
        }

        public string EditorClientId
        {
            get
            => this.clientId;
        }

        public string AttachmentsClientId { get; set; }

        public EditorModes EditorMode { get; set; }

        public string Tags { get; set; }

        public string Categories { get; set; } = string.Empty;

        public string PollQuestion
        {
            get => this.afpolledit.PollQuestion;
            set
            {
                this.pollQuestion = value;
                this.afpolledit.PollQuestion = value;
            }
        }

        public bool Subscribe
        {
            get => this.chkSubscribe.Checked;
            set => this.chkSubscribe.Checked = value;
        }

        public string PollType
        {
            get
            => this.afpolledit.PollType;
            set
            {
                this.pollType = value;
                this.afpolledit.PollType = value;
            }
        }

        public string PollOptions
        {
            get
            => this.afpolledit.PollOptions;
            set
            {
                this.pollOptions = value;
                this.afpolledit.PollOptions = value;
            }
        }

        public bool ShowModOptions { get; set; }

        public int StatusId
        {
            get
            => this.aftopicstatus.Status;
            set
            {
                this.statusId = value;
                this.aftopicstatus.Status = value;
            }
        }

        public string PostBackScript { get; } = string.Empty;

        public int ContentId { get; set; } = -1;

        public int AuthorId { get; set; } = -1;

        public bool AllowHTML { get; set; }

        public bool RequireCaptcha { get; set; } = true;

        public List<DotNetNuke.Modules.ActiveForums.Entities.TopicPropertyInfo> TopicProperties { get; set; }

        #endregion
        #region Protected Controls
        protected TextBox txtSubject = new TextBox();
        protected TextBox txtSummary = new TextBox();
        protected Label lblSubject = new Label();

        // Protected editorActiveEditor As ActiveEditorControls.ActiveEditor
        protected TextBox editorTextBox;
        protected UI.UserControls.TextEditor editorDNN;
        protected PlaceHolder plhEditor = new PlaceHolder();
        protected ImageButton btnPost = new ImageButton();
        protected Button btnSubmit = new Button();
        protected ImageButton btnCancel = new ImageButton();
        protected ImageButton btnPreview = new ImageButton();
        protected PlaceHolder plhControl = new PlaceHolder();
        protected Control txtEditor = null;
        protected af_posticonlist afposticons = new af_posticonlist();
        protected af_polledit afpolledit = new af_polledit();
        protected af_topicstatus aftopicstatus = new af_topicstatus();
        protected CheckBox chkLocked = new CheckBox();
        protected CheckBox chkPinned = new CheckBox();
        protected CheckBox chkAnnounce = new CheckBox();
        protected CheckBox chkSubscribe = new CheckBox();
        protected CheckBox chkApproved = new CheckBox();
        protected System.Web.UI.WebControls.RequiredFieldValidator reqSubject = new System.Web.UI.WebControls.RequiredFieldValidator();
        protected Label reqCustomBody = new Label();
        protected DatePicker calStartDate = new DatePicker();
        protected DatePicker calEndDate = new DatePicker();
        protected af_attach ctlAttach = new af_attach();

        protected PlaceHolder plhUpload;

        // Protected WithEvents tsTags As DotNetNuke.Modules.ActiveForums.Controls.TextSuggest
        protected PlaceHolder plhTopicReview = new PlaceHolder();
        protected TextBox txtTopicPriority = new TextBox();

        // Support for Anonymous
        protected TextBox txtUsername = new TextBox();
        protected System.Web.UI.WebControls.RequiredFieldValidator reqUsername = new System.Web.UI.WebControls.RequiredFieldValidator();
        protected UI.WebControls.CaptchaControl ctlCaptcha = new UI.WebControls.CaptchaControl();
        protected System.Web.UI.WebControls.RequiredFieldValidator reqCaptcha = new System.Web.UI.WebControls.RequiredFieldValidator();

        private string bodyTemplate = string.Empty;

        #endregion

        // Defines the Click event.
        public event EventHandler BubbleClick;

        protected void OnBubbleClick(EventArgs e)
        {
            if (this.BubbleClick != null)
            {
                this.BubbleClick(this, e);
            }
        }

        private string ParseForm(string template)
        {
            template = Globals.ForumsControlsRegisterAMTag + template;
            template = "<%@ register src=\"~/DesktopModules/ActiveForums/controls/af_posticonlist.ascx\" tagprefix=\"af\" tagname=\"posticons\" %>" + template;
            template = template.Replace("[AF:INPUT:SUBJECT]", "<asp:textbox id=\"txtSubject\" cssclass=\"aftextbox dcf-topic-edit-subject\" runat=\"server\" />");
            template = template.Replace("[AF:REQ:SUBJECT]", "<asp:requiredfieldvalidator id=\"reqSubject\" validationgroup=\"afform\" ControlToValidate=\"txtSubject\" runat=\"server\" />");
            template = template.Replace("[AF:REQ:BODY]", "<asp:label id=\"reqCustomBody\" visible=\"false\" runat=\"server\" />");
            if (template.Contains("[AF:BODY:TEMPLATE]"))
            {
                this.bodyTemplate = TemplateUtils.GetTemplateSection(template, "[AF:BODY:TEMPLATE]", "[/AF:BODY:TEMPLATE]");
                this.bodyTemplate = this.bodyTemplate.Replace("[AF:BODY:TEMPLATE]", string.Empty);
                this.bodyTemplate = this.bodyTemplate.Replace("[/AF:BODY:TEMPLATE]", string.Empty);
                template = TemplateUtils.ReplaceSubSection(template, string.Empty, "[AF:BODY:TEMPLATE]", "[/AF:BODY:TEMPLATE]");
            }

            if (template.Contains("[TOOLBAR"))
            {
                template = template.Replace("[TOOLBAR]", Utilities.BuildToolbar(this.PortalId, this.ForumModuleId, this.ForumTabId, this.ModuleId, this.TabId, this.ForumUser, this.Request.Url, this.Request.RawUrl, HttpContext.Current?.Response?.Cookies["language"]?.Value));
            }

            template = template.Replace("[AF:INPUT:SUMMARY]", "<asp:textbox id=\"txtSummary\" cssclass=\"dcf-topic-edit-summary\" runat=\"server\" />");
            template = template.Replace("[AF:INPUT:BODY]", "<asp:placeholder id=\"plhEditor\" runat=\"server\" />");
            template = template.Replace("[AF:LABEL:SUBJECT]", "<asp:label id=\"lblSubject\" runat=\"server\" />");
            if (!this.Request.IsAuthenticated & (this.canCreate || this.canReply))
            {
                if (template.Contains("[AF:UI:ANON]"))
                {
                    template = Globals.DnnControlsRegisterTag + template;
                    template = template.Replace("[AF:INPUT:USERNAME]", "<asp:textbox id=\"txtUsername\" cssclass=\"aftextbox\" runat=\"server\" />");
                    template = template.Replace("[AF:REQ:USERNAME]", "<asp:requiredfieldvalidator id=\"reqUsername\" validationgroup=\"afform\" ControlToValidate=\"txtUsername\" runat=\"server\" />");
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

            if (this.EditorMode != EditorModes.NewTopic || this.EditorMode != EditorModes.EditTopic)
            {
                template = template.Replace("[AF:UI:SECTION:TOPICREVIEW]", "<table class=\"afsection\" cellpadding=\"0\" cellspacing=\"0\"><tr><td class=\"afsectionhd\" style=\"border-left:solid 1px #b3b3b3;\">[RESX:TopicReview]</td><td class=\"afsectionhd\" align=\"right\" style=\"border-right:solid 1px #b3b3b3;\">" +
                DotNetNuke.Modules.ActiveForums.Injector.InjectCollapsibleClosed(target: "sectionTopicReview", title: string.Empty) +
                "</td></tr><tr><td colspan=\"2\" class=\"afsectiondsp\" id=\"sectionTopicReview\" style=\"display:none;\"><div class=\"affieldsetnote\">[RESX:TopicReview:Note]</div>");
                this.topicReviewTemplate = TemplateUtils.GetTemplateSection(template, "[AF:CONTROL:TOPICREVIEW]", "[/AF:CONTROL:TOPICREVIEW]");
                template = TemplateUtils.ReplaceSubSection(template, "<asp:placeholder id=\"plhTopicReview\" runat=\"server\" />", "[AF:CONTROL:TOPICREVIEW]", "[/AF:CONTROL:TOPICREVIEW]");
                template = template.Replace("[/AF:UI:SECTION:TOPICREVIEW]", "</td></tr></table>");
            }

            if (DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(this.ForumInfo.Security.TagRoleIds, this.ForumUser.UserRoleIds))
            {
                template = template.Replace("[AF:UI:SECTION:TAGS]", "<table class=\"afsection\" cellpadding=\"0\" cellspacing=\"0\"><tr><td class=\"afsectionhd\" style=\"border-left:solid 1px #b3b3b3;\">[RESX:Tags]</td><td class=\"afsectionhd\" align=\"right\" style=\"border-right:solid 1px #b3b3b3;\">" +
                    DotNetNuke.Modules.ActiveForums.Injector.InjectCollapsibleClosed(target: "sectionTags", title: string.Empty) +
                    "</td></tr><tr><td colspan=\"2\" class=\"afsectiondsp\" id=\"sectionTags\" style=\"display:none;\"><div class=\"affieldsetnote\">[RESX:Tags:Note]</div>");
                template = template.Replace("[AF:UI:FIELDSET:TAGS]", "<fieldset class=\"affieldset\"><legend>[RESX:Tags]</legend><div class=\"affieldsetnote\">[RESX:Tags:Note]</div>");
                string sTagOut = "<input type=\"text\" id=\"txtTags\" name=\"txtTags\" style=\"width:98%;\" value=\"[TAGS]\" class=\"NormalTextBox\"  />";
                sTagOut = sTagOut.Replace("[TAGS]", this.Tags);
                template = template.Replace("[AF:CONTROL:TAGS]", sTagOut);
                template = template.Replace("[/AF:UI:FIELDSET:TAGS]", "</fieldset>");
                template = template.Replace("[/AF:UI:SECTION:TAGS]", "</td></tr></table>");
            }

            // Properties
            if ((this.EditorMode == EditorModes.EditTopic || this.EditorMode == EditorModes.NewTopic) && this.ForumInfo.Properties != null && this.ForumInfo.Properties.Count > 0)
            {
                string pTemplate = TemplateUtils.GetTemplateSection(template, "[AF:PROPERTIES]", "[/AF:PROPERTIES]");
                string propList = string.Empty;
                foreach (DotNetNuke.Modules.ActiveForums.Entities.PropertyInfo p in this.ForumInfo.Properties)
                {
                    string pValue = string.Empty;
                    if (this.TopicProperties != null && this.TopicProperties.Count > 0)
                    {
                        foreach (DotNetNuke.Modules.ActiveForums.Entities.TopicPropertyInfo tp in this.TopicProperties)
                        {
                            if (tp.PropertyId == p.PropertyId)
                            {
                                pValue = tp.Value;
                            }
                        }
                    }

                    string tmp = pTemplate;

                    if (p.IsRequired)
                    {
                        tmp = tmp.Replace("[AF:PROPERTY:LABEL]", "<span class=\"afprop-required\">[RESX:" + p.Name + "]</span>");
                        tmp = tmp.Replace("[AF:PROPERTY:REQUIRED]", "<span class=\"afrequired\"></span>");
                    }
                    else
                    {
                        tmp = tmp.Replace("[AF:PROPERTY:LABEL]", "<span class=\"afprop-normal\">[RESX:" + p.Name + "]</span>");
                        tmp = tmp.Replace("[AF:PROPERTY:REQUIRED]", string.Empty);
                    }

                    if (p.DataType == "text")
                    {
                        tmp = tmp.Replace("[AF:PROPERTY:CONTROL]", "<input type=\"text\" id=\"afprop-" + p.PropertyId.ToString() + "\" class=\"NormalTextBox afprop-input\" name=\"afprop-" + p.PropertyId + "\" value=\"" + pValue + "\" />");
                    }
                    else if (p.DataType == "yesno")
                    {
                        string sYesSelected = string.Empty;
                        string sNoSelected = " checked=\"checked\"";
                        if (pValue.ToLowerInvariant() == "yes")
                        {
                            sYesSelected = " checked=\"checked\"";
                            sNoSelected = string.Empty;
                        }

                        tmp = tmp.Replace("[AF:PROPERTY:CONTROL]", "[RESX:Yes]:<input type=\"radio\" id=\"afprop-" + p.PropertyId.ToString() + "-yes\" groupname=\"afprop-" + p.PropertyId.ToString() + "\" class=\"NormalTextBox afprop-radio\" name=\"afprop-" + p.PropertyId + "\" value=\"Yes\" " + sYesSelected + " /> [RESX:No]:<input type=\"radio\" id=\"afprop-" + p.PropertyId.ToString() + "-no\" groupname=\"afprop-" + p.PropertyId.ToString() + "\" class=\"NormalTextBox afprop-radio\" name=\"afprop-" + p.PropertyId + "\" value=\"No\" " + sNoSelected + " />");
                    }
                    else if (p.DataType.Contains("list"))
                    {
                        string sList = string.Empty;
                        var lists = new DotNetNuke.Common.Lists.ListController();
                        if (p.DataType.Contains("list|"))
                        {
                            sList = "<select id=\"afprop-" + p.PropertyId.ToString() + "\" class=\"NormalTextBox afprop-select\" name=\"afprop-" + p.PropertyId.ToString() + "\">";

                            string lName = p.DataType.Substring(p.DataType.IndexOf("|") + 1);
                            var lc = lists.GetListEntryInfoItems(lName, string.Empty);
                            foreach (DotNetNuke.Common.Lists.ListEntryInfo l in lc)
                            {
                                if (pValue == l.Value)
                                {
                                    sList += "<option value=\"" + l.Value + "\" selected=\"selected\">" + l.Text + "</option>";
                                }
                                else
                                {
                                    sList += "<option value=\"" + l.Value + "\">" + l.Text + "</option>";
                                }
                            }

                            sList += "</select>";
                        }
                        else if (p.DataType.Contains("list-multi|"))
                        {
                            sList = "<div class=\"afprop-chklist\">";
                            sList += "<ul>";
                            string lName = p.DataType.Substring(p.DataType.IndexOf("|") + 1);
                            var lc = lists.GetListEntryInfoItems(lName, string.Empty);
                            string[] pValues = null;
                            if (!string.IsNullOrEmpty(pValue))
                            {
                                pValues = pValue.Split(',');
                            }

                            foreach (DotNetNuke.Common.Lists.ListEntryInfo l in lc)
                            {
                                bool isSelected = false;
                                if (pValues != null)
                                {
                                    foreach (string pv in pValues)
                                    {
                                        if (pv == l.Value)
                                        {
                                            isSelected = true;
                                        }
                                    }
                                }

                                sList += "<li>";
                                if (isSelected)
                                {
                                    sList += "<input type=\"checkbox\"  name=\"afprop-" + p.PropertyId.ToString() + "\" value=\"" + l.Value + "\" checked=\"checked\" />";
                                }
                                else
                                {
                                    sList += "<input type=\"checkbox\"  name=\"afprop-" + p.PropertyId.ToString() + "\" value=\"" + l.Value + "\" />";
                                }

                                sList += "<span>" + l.Text + "</span></li>";
                            }

                            sList += "</ul></div>";
                        }

                        tmp = tmp.Replace("[AF:PROPERTY:CONTROL]", sList);
                    }

                    propList += tmp;
                }

                template = TemplateUtils.ReplaceSubSection(template, propList, "[AF:PROPERTIES]", "[/AF:PROPERTIES]");
                /* tokens [AF:UI:SECTION:PROPERTIES][/AF:UI:SECTION:PROPERTIES] can now surround entire properties section to support removing entire section; if using properties, just remove the tokens*/
                template = template.Replace("[AF:UI:SECTION:PROPERTIES]", string.Empty);
                template = template.Replace("[/AF:UI:SECTION:PROPERTIES]", string.Empty);
            }
            else
            {
                /* tokens [AF:UI:SECTION:POSTICONS][/AF:UI:SECTION:POSTICONS] can now surround entire properties section to support removing entire section if not using */
                template = TemplateUtils.ReplaceSubSection(template, string.Empty, "[AF:UI:SECTION:PROPERTIES]", "[/AF:UI:SECTION:PROPERTIES]");
                /* leave this for backward compatibility in cases where template doesn't yet have the [AF:UI:SECTION:PROPERTIES][/AF:UI:SECTION:PROPERTIES] tokens */
                template = TemplateUtils.ReplaceSubSection(template, string.Empty, "[AF:PROPERTIES]", "[/AF:PROPERTIES]");
            }

            if (template.Contains("[AF:UI:SECTION:SUMMARY]"))
            {
                template = template.Replace("[AF:UI:SECTION:SUMMARY]", "<table style=\"width:99%\" class=\"afsection\" cellpadding=\"0\" cellspacing=\"0\">" +
                    "<tr>" +
                    "<td class=\"afsectionhd\" style=\"border-left:solid 1px #b3b3b3;\">[RESX:Summary]</td>" +
                    "<td class=\"afsectionhd\" align=\"right\" style=\"border-right:solid 1px #b3b3b3;\">" +
                    DotNetNuke.Modules.ActiveForums.Injector.InjectCollapsibleClosed(target: "sectionSummary", title: Utilities.GetSharedResource("[RESX:SummaryForSEO]")) +
                    "</td>" +
                    "</tr>" +
                    "<tr>" +
                    "<td colspan=\"2\" class=\"afsectiondsp\" id=\"sectionSummary\" style=\"display:none;\">");

                template = template.Replace("[/AF:UI:SECTION:SUMMARY]", "</td></tr></table>");
            }

            if ((this.EditorMode == EditorModes.EditTopic || this.EditorMode == EditorModes.NewTopic) && DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(this.ForumInfo.Security.CategorizeRoleIds, this.ForumUser.UserRoleIds))
            {
                template = template.Replace("[AF:UI:SECTION:CATEGORIES]", "<table class=\"afsection\" cellpadding=\"0\" cellspacing=\"0\"><tr><td class=\"afsectionhd\" style=\"border-left:solid 1px #b3b3b3;\">[RESX:Categories]</td><td class=\"afsectionhd\" align=\"right\" style=\"border-right:solid 1px #b3b3b3;\">" +
                    DotNetNuke.Modules.ActiveForums.Injector.InjectCollapsibleClosed(target: "sectionCategories", title: string.Empty) +
                    "</td></tr><tr><td colspan=\"2\" class=\"afsectiondsp\" id=\"sectionCategories\" style=\"display:none;\"><div class=\"affieldsetnote\">[RESX:Categories:Note]</div>");
                template = template.Replace("[AF:UI:FIELDSET:CATEGORIES]", "<fieldset class=\"affieldset\"><legend>[RESX:Categories]</legend><div class=\"affieldsetnote\">[RESX:Categories:Note]</div>");
                string sCatOut;
                var cc = new CategoriesList(this.PortalId, this.ForumModuleId, this.ForumInfo.ForumID, this.ForumInfo.ForumGroupId);
                if (this.TopicId > 0)
                {
                    cc.SelectedValues = this.Categories;
                }

                sCatOut = cc.RenderEdit();
                template = template.Replace("[AF:CONTROL:CATEGORIES]", sCatOut);
                template = template.Replace("[/AF:UI:FIELDSET:CATEGORIES]", "</fieldset>");
                template = template.Replace("[/AF:UI:SECTION:CATEGORIES]", "</td></tr></table>");
            }

            if ((this.EditorMode == EditorModes.EditTopic || this.EditorMode == EditorModes.NewTopic) && DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(this.ForumInfo.Security.PollRoleIds, this.ForumUser.UserRoleIds))
            {
                template = "<%@ register src=\"~/DesktopModules/ActiveForums/controls/af_polledit.ascx\" tagprefix=\"af\" tagname=\"polledit\" %>" + template;
                template = template.Replace("[AF:UI:SECTION:POLL]", "<table class=\"afsection\" cellpadding=\"0\" cellspacing=\"0\"><tr><td class=\"afsectionhd\" style=\"border-left:solid 1px #b3b3b3;\">[RESX:Polls]</td>" +
                    "<td class=\"afsectionhd\" align=\"right\" style=\"border-right:solid 1px #b3b3b3;\" class=\"afarrow\">" +
                    DotNetNuke.Modules.ActiveForums.Injector.InjectCollapsibleClosed(target: "sectionPoll", title: string.Empty) +
                    "</td></tr><tr><td colspan=\"2\" class=\"afsectiondsp\" id=\"sectionPoll\" style=\"display:none;\"><div class=\"affieldsetnote\">[RESX:Poll:Note]</div>");
                template = template.Replace("[/AF:UI:SECTION:POLL]", "</td></tr></table>");
                template = template.Replace("[AF:UI:FIELDSET:POLL]", "<fieldset class=\"affieldset\"><legend>[RESX:Polls]</legend><div class=\"affieldsetnote\">[RESX:Poll:Note]</div>");
                template = template.Replace("[AF:CONTROL:POLL]", "<af:polledit id=\"afpolledit\" runat=\"server\" />");
                template = template.Replace("[/AF:UI:FIELDSET:POLL]", "</fieldset>");
                template = template.Replace("[AF:CONTROLS:SECTIONTOGGLE]", string.Empty);
            }
            else
            {
                template = TemplateUtils.ReplaceSubSection(template, subTemplate: string.Empty, "[AF:UI:FIELDSET:POLL]", "[/AF:UI:FIELDSET:POLL]");
                template = template.Replace("[AF:CONTROL:POLL]", string.Empty);
            }

            if (this.EditorMode == EditorModes.ReplyWithBody)
            {
                template = template.Replace("[AF:UI:MESSAGEREPLY]", string.Empty);
                template = template.Replace("[/AF:UI:MESSAGEREPLY]", string.Empty);
                template = template.Replace("[AF:LABEL:BODYREPLY]", this.Body);
            }
            else
            {
                template = TemplateUtils.ReplaceSubSection(template, string.Empty, "[AF:UI:MESSAGEREPLY]", "[/AF:UI:MESSAGEREPLY]");
            }

            if (template.Contains("[AF:UI:FIELDSET:OPTIONS]"))
            {
                template = template.Replace("[AF:UI:FIELDSET:OPTIONS]", "<fieldset class=\"affieldset\"><legend>[RESX:AdditionalOptions]</legend>");
                template = template.Replace("[/AF:UI:FIELDSET:OPTIONS]", "</fieldset>");
            }

            string sOptions = this.GetOptions();
            template = template.Replace("[AF:CONTROL:OPTIONS]", sOptions);
            if (template.Contains("[AF:UI:SECTION:OPTIONS]"))
            {
                if (sOptions == string.Empty)
                {
                    template = template.Replace("[AF:UI:SECTION:OPTIONS]", string.Empty);
                    template = template.Replace("[/AF:UI:SECTION:OPTIONS]", string.Empty);
                }
                else
                {
                    template = template.Replace("[AF:UI:SECTION:OPTIONS]", "<table class=\"afsection\" cellpadding=\"0\" cellspacing=\"0\">" +
                        "<tr>" + "<td class=\"afsectionhd\" style=\"border-left:solid 1px #b3b3b3;\">[RESX:AdditionalOptions]</td>" +
                    "<td class=\"afsectionhd\" align=\"right\" style=\"border-right:solid 1px #b3b3b3;\" class=\"afarrow\">" +
                    DotNetNuke.Modules.ActiveForums.Injector.InjectCollapsibleClosed(target: "sectionOptions", title: string.Empty) +
                    "</td></tr>" +
                    "<tr><td colspan=\"2\" class=\"afsectiondsp\" id=\"sectionOptions\" style=\"display:none;\"><div class=\"affieldsetnote\">[RESX:Options:Note]</div>");
                    template = template.Replace("[/AF:UI:SECTION:OPTIONS]", "</td></tr></table>");
                }
            }

            if (template.Contains("[AF:CONTROL:STATUS]"))
            {
                if (this.EditorMode == EditorModes.EditTopic || this.EditorMode == EditorModes.NewTopic)
                {
                    template = "<%@ register src=\"~/DesktopModules/ActiveForums/controls/af_topicstatus.ascx\" tagprefix=\"af\" tagname=\"topicstatus\" %>" + template;
                    template = template.Replace("[AF:CONTROL:STATUS]", "<af:topicstatus id=\"aftopicstatus\" AutoPostBack=\"False\" ForumId=\"" + this.ForumInfo.ForumID + "\" runat=\"server\" />");
                }
            }

            template = template.Replace("[AF:LINK:FORUMNAME]", "<a href=\"" + this.NavigateUrl(this.TabId, string.Empty, ParamKeys.ViewType + "=" + Views.Topics + "&" + ParamKeys.ForumId + "=" + this.ForumInfo.ForumID.ToString()) + "\">" + this.ForumInfo.ForumName + "</a>");
            template = template.Replace("[AF:LINK:FORUMGROUP]", "<a href=\"" + this.NavigateUrl(this.TabId, string.Empty, ParamKeys.GroupId + "=" + this.ForumInfo.ForumGroupId.ToString()) + "\">" + this.ForumInfo.GroupName + "</a>");
            template = template.Replace("[AF:LINK:FORUMMAIN]", "<a href=\"" + this.NavigateUrl(this.TabId) + "\">[RESX:FORUMS]</a>");
            template = !(this.TopicId == -1) ? template.Replace("[AF:LINK:TOPICNAME]", "<a href=\"" + this.NavigateUrl(this.TabId, string.Empty, ParamKeys.TopicId + "=" + this.TopicId + "&" + ParamKeys.ViewType + "=" + Views.Topic + "&" + ParamKeys.ForumId + "=" + this.ForumInfo.ForumID.ToString()) + "\">" + this.TopicSubject + "</a>") : template.Replace("[AF:LINK:TOPICNAME]", string.Empty);
            template = template.Replace("[AF:UI:FIELDSET:ACTIONS]", "<fieldset class=\"affieldset\"><legend>[RESX:Actions]</legend>");
            template = template.Replace("[/AF:UI:FIELDSET:ACTIONS]", "</fieldset>");
            template = template.Replace("[AF:BUTTON:SUBMIT]", "<am:imagebutton id=\"btnPost\" Text=\"[RESX:Submit]\" runat=\"server\" />");
            template = template.Replace("[AF:BUTTON:CANCEL]", "<am:imagebutton id=\"btnCancel\" Text=\"[RESX:Cancel]\" runat=\"server\" />");
            template = template.Replace("[AF:BUTTON:PREVIEW]", this.Request.IsAuthenticated ? "<am:imagebutton id=\"btnPreview\" PostBack=\"False\"  Text=\"[RESX:Preview]\" runat=\"server\" />" : string.Empty);

            if (template.Contains("[AF:CONTROL:POSTICONS]") && this.ForumInfo.FeatureSettings.AllowPostIcon)
            {
                template = template.Replace("[AF:UI:FIELDSET:POSTICONS]", "<fieldset class=\"affieldset\"><legend>[RESX:PostIcons]</legend><div class=\"affieldsetnote\">[RESX:PostIcons:Note]</div>");
                template = template.Replace("[AF:CONTROL:POSTICONS]", "<af:posticons id=\"afposticons\" runat=\"server\" Theme=\"" + this.MainSettings.Theme + "\" />");
                template = template.Replace("[/AF:UI:FIELDSET:POSTICONS]", "</fieldset>");
                /* tokens [AF:UI:SECTION:POSTICONS][/AF:UI:SECTION:POSTICONS] can now surround post icons to support removing entire section; if using post icons, just remove the tokens*/
                template = template.Replace("[AF:UI:SECTION:POSTICONS]", string.Empty);
                template = template.Replace("[/AF:UI:SECTION:POSTICONS]", string.Empty);
            }
            else
            {
                /* tokens [AF:UI:SECTION:POSTICONS][/AF:UI:SECTION:POSTICONS] can now surround post icons to remove entire section */
                template = TemplateUtils.ReplaceSubSection(template, subTemplate: string.Empty, "[AF:UI:SECTION:POSTICONS]", "[/AF:UI:SECTION:POSTICONS]");
                /* leave these 3 lines for backward compatibility in cases where template doesn't yet have the [AF:UI:SECTION:POSTICONS][/AF:UI:SECTION:POSTICONS] tokens */
                template = template.Replace("[AF:UI:FIELDSET:POSTICONS]", string.Empty);
                template = template.Replace("[AF:CONTROL:POSTICONS]", string.Empty);
                template = template.Replace("[/AF:UI:FIELDSET:POSTICONS]", string.Empty);
            }

            if (template.Contains("[AF:CONTROL:EMOTICONS]") && this.ForumInfo.FeatureSettings.AllowEmoticons)
            {
                template = template.Replace("[AF:CONTROL:EMOTICONS]", "<fieldset class=\"affieldset\"><legend>[RESX:Smilies]</legend>" + DotNetNuke.Modules.ActiveForums.Controllers.EmoticonController.LoadEmoticons(this.ForumModuleId, this.Page.ResolveUrl(this.MainSettings.ThemeLocation), this.EditorType) + "</fieldset>");
            }
            else
            {
                template = template.Replace("[AF:CONTROL:EMOTICONS]", string.Empty);
            }

            if (template.Contains("[AF:CONTROL:UPLOAD]"))
            {
                if (this.canAttach && this.ForumInfo.FeatureSettings.AllowAttach)
                {
                    template = "<%@ register src=\"~/DesktopModules/ActiveForums/controls/af_attach.ascx\" tagprefix=\"af\" tagname=\"attach\" %>" + template;
                    template = template.Replace("[AF:UI:FIELDSET:ATTACH]", "<fieldset class=\"affieldset\"><legend>[RESX:Attachments]</legend><div class=\"affieldsetnote\">[RESX:Attacments:Note]</div>");
                    template = template.Replace("[AF:UI:SECTION:ATTACH]", "<table class=\"afsection\" cellpadding=\"0\" cellspacing=\"0\">" +
                        "<tr><td class=\"afsectionhd\" style=\"border-left:solid 1px #b3b3b3;\">[RESX:Attachments]</td><td class=\"afsectionhd\" align=\"right\" style=\"border-right:solid 1px #b3b3b3;\">" +
                        DotNetNuke.Modules.ActiveForums.Injector.InjectCollapsibleClosed(target: "sectionAttach", title: string.Empty) +
                        "</td></tr><tr><td colspan=\"2\" class=\"afsectiondsp\" id=\"sectionAttach\" style=\"display:none;\"><div class=\"affieldsetnote\">[RESX:Attachments:Note]</div>");
                    template = template.Replace("[AF:CONTROL:UPLOAD]", "<af:attach id=\"ctlAttach\" runat=\"server\" />");
                    template = template.Replace("[/AF:UI:FIELDSET:ATTACH]", "</fieldset>");
                    template = template.Replace("[/AF:UI:SECTION:ATTACH]", "</td></tr></table>");
                }
                else
                {
                    template = template.Replace("[AF:UI:FIELDSET:ATTACH]", string.Empty);
                    template = template.Replace("[AF:CONTROL:UPLOAD]", string.Empty);
                    template = template.Replace("[/AF:UI:FIELDSET:ATTACH]", string.Empty);
                    template = template.Replace("[AF:UI:SECTION:ATTACH]", string.Empty);
                    template = template.Replace("[/AF:UI:SECTION:ATTACH]", string.Empty);
                }
            }

            // If str.Contains("[AF:CONTROL:FORUMTREE]") Then
            //    str = str.Replace("[AF:CONTROL:FORUMTREE]", "<af:forumtree id=""ctlForumTree"" runat=""server"" showcheckboxes=""true"" ModuleId=""" & ModuleId & """ />")
            //    sHeader &= "<%@ register src=""~/DesktopModules/ActiveForums/controls/af_forumtree.ascx"" tagprefix=""af"" tagname=""forumtree"" %>"
            // End If
            template = Utilities.LocalizeControl(template);
            template = Utilities.StripTokens(template);
            return template;
        }

        private string GetOptions()
        {
            var sb = new StringBuilder();
            bool bHasOptions = false;
            sb.Append("<table cellpadding=\"2\" cellspacing=\"0\">");
            if (this.ForumInfo.FeatureSettings.IsModerated && this.canModerate && this.ShowModOptions)
            {
                sb.Append("<tr><td>[RESX:Approved]:</td>");
                sb.Append("<td><asp:checkbox id=\"chkApproved\" Text=\"[RESX:Approved:Note]\" TextAlign=\"right\" cssclass=\"afcheckbox\" runat=\"server\" /></td></tr>");
                bHasOptions = true;
            }

            if ((this.canLock || this.canModEdit) & (this.EditorMode == EditorModes.NewTopic || this.EditorMode == EditorModes.EditTopic))
            {
                sb.Append("<tr><td>[RESX:Locked]:</td>");
                sb.Append("<td><asp:checkbox id=\"chkLocked\" Text=\"[RESX:Locked:Note]\" TextAlign=\"right\" cssclass=\"afcheckbox\" runat=\"server\" /></td></tr>");
                bHasOptions = true;
            }

            if ((this.canPin || this.canModEdit) & (this.EditorMode == EditorModes.NewTopic || this.EditorMode == EditorModes.EditTopic))
            {
                sb.Append("<tr><td>[RESX:Pinned]:</td>");
                sb.Append("<td><asp:checkbox id=\"chkPinned\" Text=\"[RESX:Pinned:Note]\" TextAlign=\"right\" cssclass=\"afcheckbox\" runat=\"server\" /></td></tr>");
                bHasOptions = true;
            }

            if (this.canSubscribe && !this.UserPrefTopicSubscribe) /* if user has preference set for auto subscribe, no need to show them the subscribe option */
            {
                if (this.TopicId > 0)
                {
                    var subControl = new ToggleSubscribe(this.ModuleId, this.ForumInfo.ForumID, this.TopicId, 1);
                    subControl.Checked = new DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController().Subscribed(this.PortalId, this.ForumModuleId, this.UserId, this.ForumInfo.ForumID, this.TopicId);
                    subControl.Text = "[RESX:Subscribe]";
                    sb.Append("<tr><td colspan=\"2\">" + subControl.Render() + "</td></tr>");
                }
                else
                {
                    sb.Append("<tr><td colspan=\"2\"><asp:checkbox id=\"chkSubscribe\" Text=\"[RESX:Subscribe]\" TextAlign=\"right\" cssclass=\"afcheckbox\" runat=\"server\" /></td></tr>");
                }

                bHasOptions = true;
            }

            if ((this.EditorMode == EditorModes.NewTopic || this.EditorMode == EditorModes.EditTopic) && DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(this.ForumInfo.Security.PrioritizeRoleIds, this.ForumUser.UserRoleIds))
            {
                sb.Append("<tr><td>[RESX:TopicPriority]:</td>");
                sb.Append("<td><asp:textbox id=\"txtTopicPriority\" cssclass=\"aftextbox\" width=\"75\" onkeypress=\"return onlyNumbers(event);\" runat=\"server\" /></td></tr>");
                bHasOptions = true;
            }

            sb.Append("</table>");
            if (bHasOptions == false)
            {
                sb = new StringBuilder();
            }

            if (this.canAnnounce && (this.EditorMode == EditorModes.NewTopic || this.EditorMode == EditorModes.EditTopic))
            {
                sb.Append("<table cellpadding=\"2\" cellspacing=\"0\">");
                sb.Append("<tr><td valign=\"top\" colspan=\"3\">[RESX:Announce]:</td></tr><tr><td></td>");
                sb.Append("<td>[RESX:Announce:StartDate]:</td><td><am:datepicker id=\"calStartDate\" runat=\"server\" /></td></tr><tr><td></td><td>[RESX:Announce:EndDate]:</td><td><am:datepicker id=\"calEndDate\" runat=\"server\" /></td></tr>");
                sb.Append("</table>");
                bHasOptions = true;
            }

            if (bHasOptions)
            {
                return sb.ToString();
            }

            return string.Empty;
        }

        private void LoadForm()
        {
            this.AppRelativeVirtualPath = "~/submitform.ascx";
            var ctl = new Control { AppRelativeTemplateSourceDirectory = "~/", ID = "ctlTemp" };
            ctl = this.ParseControl(this.ParseForm(this.Template));
            this.Controls.Add(ctl);
            while (!(ctl.Controls.Count == 0))
            {
                this.Controls.Add(ctl.Controls[0]);
            }

            this.calStartDate.NullDate = Utilities.NullDate().ToString();
            this.calEndDate.NullDate = Utilities.NullDate().ToString();

            // calStartDate.DateFormat = MainSettings.DateFormatString
            // calEndDate.DateFormat = MainSettings.DateFormatString
            if (!(this.AnnounceStart == Utilities.NullDate()))
            {
                this.calStartDate.SelectedDate = Utilities.GetUserFormattedDateTime(this.AnnounceStart, this.PortalId, this.UserId);
            }

            if (!(this.AnnounceEnd == Utilities.NullDate()))
            {
                this.calEndDate.SelectedDate = Utilities.GetUserFormattedDateTime(this.AnnounceEnd, this.PortalId, this.UserId);
            }

            this.plhEditor = new PlaceHolder();
            this.plhEditor = (PlaceHolder)this.FindControl("plhEditor");

            // 'plhEditor.Controls.Clear()
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
            switch (this.EditorType)
            {
                case EditorTypes.TEXTBOX:
                    {
                        var editor = new TextBox();
                        this.txtEditor = editor;
                        editor.ID = "txtBody";
                        editor.Width = editorWidth;
                        editor.Height = editorHeight;
                        editor.TextMode = TextBoxMode.MultiLine;
                        editor.Rows = 4;
                        this.plhEditor.Controls.Add(editor);
                        this.clientId = editor.ClientID;
                        break;
                    }

                case EditorTypes.HTMLEDITORPROVIDER:
                    {
                        // TODO: figure out why the editor no longer has any WYSIWYG functionality
                        var editor = new UI.UserControls.TextEditor();
                        this.txtEditor = editor;
                        editor = (UI.UserControls.TextEditor)this.LoadControl("~/controls/TextEditor.ascx");
                        editor.ID = "txtBody";
                        editor.ChooseMode = false;
                        editor.DefaultMode = "R";
                        editor.ChooseRender = false;
                        editor.Mode = "RICH";
                        editor.Width = editorWidth;
                        editor.HtmlEncode = false; // Turn Encoding off or passed already Encoded HTML.
                        editor.Height = editorHeight;
                        this.plhEditor.Controls.Add(editor);
                        this.clientId = editor.ClientID;
                        break;
                    }
            }

            this.plhTopicReview = new PlaceHolder();
            this.plhTopicReview = (PlaceHolder)this.FindControl("plhTopicReview");
            if (this.plhTopicReview != null)
            {
                var ctlTopicView = new TopicView
                {
                    ModuleConfiguration = this.ModuleConfiguration,
                    TopicTemplate = this.topicReviewTemplate,
                    OptPageSize = int.MaxValue,
                    OptDefaultSort = "DESC",
                    ForumInfo = this.ForumInfo,
                };
                this.plhTopicReview.Controls.Add(ctlTopicView);
            }
        }

        public void btnPost_Click(object sender, EventArgs e)
        {
            var captcha = (UI.WebControls.CaptchaControl)this.FindControl("ctlCaptcha");
            if (captcha != null)
            {
                if (!captcha.IsValid && this.RequireCaptcha && !this.Request.IsAuthenticated)
                {
                    return;
                }
            }

            var rd = (RadioButtonList)this.afposticons.FindControl("rblMessageIcons1");
            var stat = (DropDownList)this.aftopicstatus.FindControl("drpStatus");

            var pollQuest = (TextBox)this.afpolledit.FindControl("txtPollQuestion");
            var pType = (RadioButtonList)this.afpolledit.FindControl("rdPollType");
            var pOpt = (TextBox)this.afpolledit.FindControl("txtPollOptions");
            if (rd != null)
            {
                this.TopicIcon = rd.SelectedItem.Value;
            }

            if (stat != null)
            {
                this.StatusId = Convert.ToInt32(stat.SelectedItem.Value);
            }

            if (pollQuest != null)
            {
                this.PollQuestion = pollQuest.Text;
                this.PollType = pType.SelectedItem.Value;
                this.PollOptions = pOpt.Text;
            }

            if (!string.IsNullOrEmpty(this.Body))
            {
                this.Body = this.Body.Replace("&nbsp;", " ");
                string tmpBody = Utilities.StripHTMLTag(this.Body);
                if (string.IsNullOrEmpty(tmpBody))
                {
                    this.reqCustomBody.Visible = true;
                }

                if (this.Body.Trim() == string.Empty)
                {
                    this.reqCustomBody.Visible = true;
                }
            }
            else
            {
                this.reqCustomBody.Visible = true;
            }

            if (!this.Request.IsAuthenticated && Utilities.InputIsValid(this.txtUsername.Text.Trim()) == false)
            {
                this.reqUsername.Visible = true;
            }

            this.OnBubbleClick(e);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            var stringWriter = new System.IO.StringWriter();
            var htmlWriter = new HtmlTextWriter(stringWriter);
            base.Render(htmlWriter);
            string html = stringWriter.ToString();
            html = Utilities.LocalizeControl(html);
            writer.Write(html);
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.canModerate = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(this.ForumInfo.Security.ModerateRoleIds, this.ForumUser.UserRoleIds);
            this.canEdit = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(this.ForumInfo.Security.EditRoleIds, this.ForumUser.UserRoleIds);
            this.canModEdit = this.canModerate && this.canEdit;
            this.canReply = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(this.ForumInfo.Security.ReplyRoleIds, this.ForumUser.UserRoleIds);
            this.canCreate = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(this.ForumInfo.Security.CreateRoleIds, this.ForumUser.UserRoleIds);
            this.canAttach = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(this.ForumInfo.Security.AttachRoleIds, this.ForumUser.UserRoleIds);
            this.canTrust = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(this.ForumInfo.Security.TrustRoleIds, this.ForumUser.UserRoleIds);
            this.canLock = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(this.ForumInfo.Security.LockRoleIds, this.ForumUser.UserRoleIds);
            this.canPin = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(this.ForumInfo.Security.PinRoleIds, this.ForumUser.UserRoleIds);
            this.canAnnounce = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(this.ForumInfo.Security.AnnounceRoleIds, this.ForumUser.UserRoleIds);
            this.canSubscribe = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(this.ForumInfo.Security.SubscribeRoleIds, this.ForumUser.UserRoleIds);

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.LoadForm();
            this.LinkControls(this.Controls);

            // If Not tsTags Is Nothing Then
            //    tsTags.Delimiter = ","
            //    tsTags.CharsTillLoad = 2
            //    tsTags.SelectedText = _tags
            //    tsTags.SelectedValue = _tags
            // End If

            // not sure why this gets set twice.
            this.txtSubject.CssClass = "aftextbox dcf-topic-edit-subject";
            string myTheme = this.MainSettings.Theme;
            string myThemePath = this.Page.ResolveUrl("~/DesktopModules/ActiveForums/themes/" + myTheme);
            this.txtSubject.MaxLength = 255;
            this.txtSummary.MaxLength = 2000;
            this.txtSubject.Text = this.subject;
            this.txtSummary.Text = this.summary;
            this.txtUsername.Text = this.authorName;
            this.lblSubject.Text = this.subject;
            this.lblSubject.CssClass = "aftextbox";

            // not sure why this gets set twice.
            this.txtSummary.CssClass = "aftextbox dcf-topic-edit-summary";
            this.chkLocked.Checked = this.locked;
            this.chkPinned.Checked = this.pinned;
            this.chkApproved.Checked = this.@checked;

            this.txtTopicPriority.Text = this.topicPriority.ToString();
            if (this.AnnounceEnd > Utilities.NullDate())
            {
                this.calEndDate.SelectedDate = Utilities.GetUserFormattedDateTime(this.announceEnd, this.PortalId, this.UserId);
            }

            if (this.AnnounceStart > Utilities.NullDate())
            {
                this.calStartDate.SelectedDate = Utilities.GetUserFormattedDateTime(this.announceStart, this.PortalId, this.UserId);
            }

            this.btnPost.ImageLocation = this.PostButton.ImageLocation;
            this.btnPost.ImageUrl = this.PostButton.ImageUrl;
            this.btnPost.EnableClientValidation = false;
            this.btnPost.ValidationGroup = "afform";
            this.btnPost.ClientSideScript = this.PostButton.ClientSideScript;
            this.btnPost.Height = this.PostButton.Height;
            this.btnPost.Width = this.PostButton.Width;
            this.btnPost.PostBack = this.PostButton.PostBack;
            this.btnPost.CssClass = "amimagebutton";

            this.btnCancel.ImageLocation = this.CancelButton.ImageLocation;
            this.btnCancel.ImageUrl = this.CancelButton.ImageUrl;
            this.btnCancel.PostBack = this.CancelButton.PostBack;
            this.btnCancel.ClientSideScript = this.CancelButton.ClientSideScript;
            this.btnCancel.Confirm = this.CancelButton.Confirm;
            this.btnCancel.ConfirmMessage = this.CancelButton.ConfirmMessage;
            this.btnCancel.Height = this.CancelButton.Height;
            this.btnCancel.Width = this.CancelButton.Width;
            this.btnCancel.CssClass = "amimagebutton";

            this.btnPreview.Height = this.PostButton.Height;
            this.btnPreview.Width = this.PostButton.Width;
            this.btnPreview.CssClass = "amimagebutton";

            this.afposticons.PostIcon = this.topicIcon;

            if (this.aftopicstatus != null)
            {
                this.aftopicstatus.Status = this.statusId;
            }

            this.afpolledit.PollQuestion = this.pollQuestion;
            this.afpolledit.PollType = this.pollType;
            this.afpolledit.PollOptions = this.pollOptions;
            this.afpolledit.ModuleConfiguration = this.ModuleConfiguration;
            this.afpolledit.ForumInfo = this.ForumInfo;

            this.reqSubject.ErrorMessage = string.Format("<img src=\"{0}/images/warning.png\" align=\"absmiddle\" />", myThemePath);
            this.reqSubject.EnableClientScript = false;
            this.reqUsername.ErrorMessage = string.Format("<img src=\"{0}/images/warning.png\" align=\"absmiddle\" />", myThemePath);
            this.reqUsername.EnableClientScript = false;
            this.reqCustomBody.Text = string.Format("<img src=\"{0}/images/warning.png\" align=\"absmiddle\" />", myThemePath);
            this.btnPreview.ImageUrl = myThemePath + "/images/preview32.png";
            this.btnPreview.ClientSideScript = "togglePreview(this);";
            this.btnPreview.ObjectId = "ancPreview";
            this.btnPreview.ImageLocation = this.PostButton.ImageLocation;
            this.btnPreview.Height = this.PostButton.Height;
            this.btnPreview.CssClass = "amimagebutton";

            if (this.EditorMode == EditorModes.NewTopic || this.EditorMode == EditorModes.Reply || this.EditorMode == EditorModes.ReplyWithBody)
            {
                this.body = this.bodyTemplate;
            }

            switch (this.EditorType)
            {
                // Case EditorTypes.ACTIVEEDITOR
                //    Dim txtEditor As New ActiveEditorControls.ActiveEditor
                //    txtEditor = CType(plhEditor.Controls.Item(0), ActiveEditorControls.ActiveEditor)
                //    txtEditor.Text = _Body
                case EditorTypes.HTMLEDITORPROVIDER:
                    ((UI.UserControls.TextEditor)this.plhEditor.FindControl("txtBody")).Text = this.body;
                    break;
                case EditorTypes.TEXTBOX:
                    ((TextBox)this.txtEditor).Text = this.body;
                    break;
            }

            this.EmotScript();
            this.insertHTMLScript();
            var sb = new StringBuilder();
            sb.Append("function CheckBody(sender, args){");
            sb.Append("if (args.Value == ''){");
            sb.Append(" args.IsValid = false;");
            sb.Append(" return;};");
            sb.Append(" args.IsValid = true;};");
            this.Page.ClientScript.RegisterClientScriptBlock(this.Page.GetType(), "bodyval", sb.ToString(), true);

            // PostBack script for Submit button (btnPost)
            string amPostbackScript = @"
                <script>
                    let dcnSubmitted = false;

                    function amPostback() {
                        if (dcnSubmitted) return; // Prevent double-click
                        dcnSubmitted = true;      // Submit button has been clicked
                        " + this.Page.ClientScript.GetPostBackEventReference(this.btnPost, string.Empty) + // __doPostBack('btnPost', '');
                        @"
                    }
                </script>";
            this.Page.ClientScript.RegisterClientScriptBlock(this.Page.GetType(), "ampost", amPostbackScript, false);

            this.btnPost.Click += this.btnPost_Click;
            this.btnSubmit.Click += this.btnSubmit_Click;

            // This field is used to communicate attachment data between af_attach and af_post, etc.
            this.ctlAttach.AttachmentsClientId = this.AttachmentsClientId;
        }

        private void LinkControls(ControlCollection ctrls)
        {
            foreach (Control ctrl in ctrls)
            {
                switch (ctrl.ID)
                {
                    case "txtSubject":
                        this.txtSubject = (TextBox)ctrl;
                        break;
                    case "txtUsername":
                        this.txtUsername = (TextBox)ctrl;
                        break;
                    case "lblSubject":
                        this.lblSubject = (Label)ctrl;
                        break;
                    case "txtSummary":
                        this.txtSummary = (TextBox)ctrl;
                        break;
                    case "btnPost":
                        this.btnPost = (ImageButton)ctrl;
                        break;
                    case "btnCancel":
                        this.btnCancel = (ImageButton)ctrl;
                        break;
                    case "plhEditor":
                        switch (this.EditorType)
                        {
                            // Case EditorTypes.ACTIVEEDITOR
                            //    Dim txtEditor As New ActiveEditorControls.ActiveEditor
                            //    txtEditor = CType(plhEditor.Controls.Item(0), ActiveEditorControls.ActiveEditor)
                            case EditorTypes.HTMLEDITORPROVIDER:
                                var txtEditor = new UI.UserControls.TextEditor();
                                break;
                            case EditorTypes.TEXTBOX:
                                var txtEditor1 = (TextBox)this.plhEditor.Controls[0];
                                break;
                        }

                        break;
                    case "afposticons":
                        this.afposticons = (DotNetNuke.Modules.ActiveForums.af_posticonlist)ctrl;
                        break;
                    case "aftopicstatus":
                        this.aftopicstatus = (DotNetNuke.Modules.ActiveForums.af_topicstatus)ctrl;
                        break;
                    case "chkLocked":
                        this.chkLocked = (CheckBox)ctrl;
                        break;
                    case "chkPinned":
                        this.chkPinned = (CheckBox)ctrl;
                        break;
                    case "txtTopicPriority":
                        this.txtTopicPriority = (TextBox)ctrl;
                        break;
                    case "chkApproved":
                        this.chkApproved = (CheckBox)ctrl;
                        break;
                    case "reqSubject":
                        this.reqSubject = (System.Web.UI.WebControls.RequiredFieldValidator)ctrl;
                        break;
                    case "reqCustomBody":
                        this.reqCustomBody = (Label)ctrl;
                        break;
                    case "calStartDate":
                        this.calStartDate = (DotNetNuke.Modules.ActiveForums.Controls.DatePicker)ctrl;
                        break;
                    case "calEndDate":
                        this.calEndDate = (DotNetNuke.Modules.ActiveForums.Controls.DatePicker)ctrl;
                        break;
                    case "btnPreview":
                        this.btnPreview = (DotNetNuke.Modules.ActiveForums.Controls.ImageButton)ctrl;

                        // Case "tsTags"
                        //    tsTags = CType(ctrl, DotNetNuke.Modules.ActiveForums.Controls.TextSuggest)
                        break;
                    case "afpolledit":
                        this.afpolledit = (DotNetNuke.Modules.ActiveForums.af_polledit)ctrl;
                        break;
                    case "ctlAttach":
                        this.ctlAttach = (DotNetNuke.Modules.ActiveForums.Controls.af_attach)ctrl;
                        this.ctlAttach.ModuleConfiguration = this.ModuleConfiguration;
                        this.ctlAttach.ModuleId = this.ModuleId;
                        this.ctlAttach.ForumInfo = this.ForumInfo;
                        break;
                    case "chkSubscribe":
                        this.chkSubscribe = (CheckBox)ctrl;
                        break;
                    case "ctlCaptcha":
                        this.ctlCaptcha = (DotNetNuke.UI.WebControls.CaptchaControl)ctrl;
                        if (this.RequireCaptcha && !this.Request.IsAuthenticated)
                        {
                            this.ctlCaptcha.Visible = true;
                        }
                        else
                        {
                            this.ctlCaptcha.Visible = false;
                        }

                        break;
                }

                if (ctrl.Controls.Count > 0)
                {
                    this.LinkControls(ctrl.Controls);
                }
            }
        }

        private void btnSubmit_Click(object sender, System.EventArgs e)
        {
        }

        private void insertHTMLScript()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.Append("var afeditor = '" + this.clientId + "';");
            this.Page.ClientScript.RegisterClientScriptBlock(this.Page.GetType(), "editorHTML", sb.ToString(), true);
        }

        private void EmotScript()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.Append("var afeditor = '" + this.clientId + "';");
            this.Page.ClientScript.RegisterClientScriptBlock(this.Page.GetType(), "emot", sb.ToString(), true);
        }
    }
}
