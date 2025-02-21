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
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.Providers;
    using DotNetNuke.Modules.ActiveForums.Controls;
    using DotNetNuke.Modules.ActiveForums.Extensions;
    using DotNetNuke.Services.FileSystem;

    public partial class af_post : ForumBase
    {
        protected SubmitForm ctlForm = new SubmitForm();
        private bool isApproved;
        public string PreviewText = string.Empty;
        private bool isEdit;
        private string themePath = string.Empty;
        private bool userIsTrusted;
        private int contentId = -1;
        private int authorId = -1;
        private bool allowHTML;
        private EditorTypes editorType = EditorTypes.TEXTBOX;
        private bool canModEdit;
        private bool canModApprove;
        private bool canEdit;
        private bool canTrust;
        private bool canLock;
        private bool canPin;

        public string Spinner { get; set; }

        public string EditorClientId { get; set; }

        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();

            var oLink = new System.Web.UI.HtmlControls.HtmlGenericControl("link");
            oLink.Attributes["rel"] = "stylesheet";
            oLink.Attributes["type"] = "text/css";
            oLink.Attributes["href"] = this.Page.ResolveUrl(Globals.ModulePath + "scripts/calendar.css");

            var oCSS = this.Page.FindControl("CSS");
            if (oCSS != null)
            {
                oCSS.Controls.Add(oLink);
            }

            this.authorId = this.UserId;
            this.canModApprove = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this.ForumInfo.Security.Moderate, this.ForumUser.UserRoles);
            this.canEdit = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this.ForumInfo.Security.Edit, this.ForumUser.UserRoles);
            this.canModEdit = (this.canModApprove && this.canEdit);

            this.canTrust = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this.ForumInfo.Security.Trust, this.ForumUser.UserRoles);
            this.canLock = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this.ForumInfo.Security.Lock, this.ForumUser.UserRoles);
            this.canPin = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this.ForumInfo.Security.Pin, this.ForumUser.UserRoles);

            if (this.ForumInfo == null)
            {
                if (!this.canEdit && (this.Request.Params[ParamKeys.Action].ToLowerInvariant() == PostActions.TopicEdit || this.Request.Params[ParamKeys.Action].ToLowerInvariant() == PostActions.ReplyEdit))
                {
                    this.Response.Redirect(this.NavigateUrl(this.TabId), false);
                    this.Context.ApplicationInstance.CompleteRequest();
                }
            }

            if (this.CanCreate == false && this.CanReply == false)
            {
                this.Response.Redirect(this.NavigateUrl(this.TabId, string.Empty, "ctl=login") + "?returnurl=" + this.Server.UrlEncode(this.Request.RawUrl), false);
                this.Context.ApplicationInstance.CompleteRequest();
            }

            if (this.UserId <= 0)
            {
                this.ForumUser.TopicCount = 0;
                this.ForumUser.ReplyCount = 0;
                this.ForumUser.RewardPoints = 0;
                this.ForumUser.TrustLevel = -1;
            }

            this.userIsTrusted = Utilities.IsTrusted((int)this.ForumInfo.FeatureSettings.DefaultTrustValue, this.ForumUser.TrustLevel, this.canTrust, this.ForumInfo.FeatureSettings.AutoTrustLevel, this.ForumUser.PostCount);
            this.themePath = this.Page.ResolveUrl(this.MainSettings.ThemeLocation);
            this.Spinner = this.Page.ResolveUrl(this.themePath + "/images/loading.gif");
            this.isApproved = !this.ForumInfo.FeatureSettings.IsModerated || this.userIsTrusted || this.canModApprove;

            this.ctlForm.ID = "ctlForm";
            this.ctlForm.PostButton.ImageUrl = this.themePath + "/images/save32.png";
            this.ctlForm.PostButton.ImageLocation = "TOP";
            this.ctlForm.PostButton.Height = Unit.Pixel(50);
            this.ctlForm.PostButton.Width = Unit.Pixel(50);

            this.ctlForm.PostButton.ClientSideScript = "amPostback();";
            this.ctlForm.PostButton.PostBack = false;

            this.ctlForm.AttachmentsClientId = this.hidAttachments.ClientID;

            // TODO: Make sure this check happens on submit
            // if (_canAttach && _fi.AllowAttach) {}
            this.ctlForm.CancelButton.ImageUrl = this.themePath + "/images/cancel32.png";
            this.ctlForm.CancelButton.ImageLocation = "TOP";
            this.ctlForm.CancelButton.PostBack = false;
            this.ctlForm.CancelButton.ClientSideScript = "javascript:history.go(-1);";
            this.ctlForm.CancelButton.Confirm = true;
            this.ctlForm.CancelButton.Height = Unit.Pixel(50);
            this.ctlForm.CancelButton.Width = Unit.Pixel(50);
            this.ctlForm.CancelButton.ConfirmMessage = this.GetSharedResource("[RESX:ConfirmCancel]");
            this.ctlForm.ModuleConfiguration = this.ModuleConfiguration;
            if (this.ForumInfo.FeatureSettings.AllowHTML)
            {
                this.allowHTML = this.IsHtmlPermitted(this.ForumInfo.FeatureSettings.EditorPermittedUsers, this.userIsTrusted, this.canModEdit);
            }

            this.ctlForm.AllowHTML = this.allowHTML;
            if (this.allowHTML)
            {
                if (this.Request.Browser.IsMobileDevice)
                {
                    this.editorType = (EditorTypes)this.ForumInfo.FeatureSettings.EditorMobile;
                }
                else
                {
                    this.editorType = this.ForumInfo.FeatureSettings.EditorType;
                }
            }
            else
            {
                this.editorType = EditorTypes.TEXTBOX;
            }

            this.ctlForm.EditorType = this.editorType;
            this.ctlForm.ForumInfo = this.ForumInfo;
            this.ctlForm.RequireCaptcha = true;
            switch (this.editorType)
            {
                case EditorTypes.TEXTBOX:
                    this.Page.ClientScript.RegisterClientScriptInclude("afeditor", this.Page.ResolveUrl(Globals.ModulePath + "scripts/text_editor.js"));
                    break;
                case EditorTypes.ACTIVEEDITOR:
                    this.Page.ClientScript.RegisterClientScriptInclude("afeditor", this.Page.ResolveUrl(Globals.ModulePath + "scripts/active_editor.js"));
                    break;
                default:
                    {
                        var prov = ProviderConfiguration.GetProviderConfiguration("htmlEditor");

                        if (prov.DefaultProvider.Contains("CKHtmlEditorProvider") || prov.DefaultProvider.Contains("DNNConnect.CKE"))
                        {
                            this.Page.ClientScript.RegisterClientScriptInclude("afeditor", this.Page.ResolveUrl(Globals.ModulePath + "scripts/ck_editor.js"));
                        }
                        else if (prov.DefaultProvider.Contains("FckHtmlEditorProvider"))
                        {
                            this.Page.ClientScript.RegisterClientScriptInclude("afeditor", this.Page.ResolveUrl(Globals.ModulePath + "scripts/fck_editor.js"));
                        }
                        else
                        {
                            this.Page.ClientScript.RegisterClientScriptInclude("afeditor", this.Page.ResolveUrl(Globals.ModulePath + "scripts/other_editor.js"));
                        }
                    }

                    break;
            }

            if (this.Request.Params[ParamKeys.Action] != null)
            {
                switch (this.Request.Params[ParamKeys.Action].ToLowerInvariant())
                {
                    case PostActions.TopicEdit:
                        if (this.canModEdit || (this.canEdit && this.Request.IsAuthenticated))
                        {
                            this.isEdit = true;
                            this.PrepareTopic();
                            this.LoadTopic();
                        }

                        break;
                    case PostActions.ReplyEdit:
                        if (this.canModEdit || (this.canEdit && this.Request.IsAuthenticated))
                        {
                            this.isEdit = true;
                            this.PrepareReply();
                            this.LoadReply();
                        }

                        break;
                    case PostActions.Reply:
                        if (this.CanReply)
                        {
                            this.PrepareReply();
                        }

                        break;
                    default:
                        if (this.CanCreate)
                        {
                            this.PrepareTopic();
                        }

                        break;
                }
            }
            else
            {
                if (this.QuoteId == 0 && this.ReplyId == 0 && this.TopicId == -1 && this.CanCreate)
                {
                    this.PrepareTopic();
                }
                else if ((this.QuoteId > 0 | this.ReplyId > 0 | this.TopicId > 0) && this.CanReply)
                {
                    this.PrepareReply();
                }
            }

            if (this.isEdit && !this.Request.IsAuthenticated)
            {
                this.Response.Redirect(this.NavigateUrl(this.TabId), false);
                this.Context.ApplicationInstance.CompleteRequest();
            }

            this.PrepareAttachments(this.contentId);

            this.ctlForm.ContentId = this.contentId;
            this.ctlForm.AuthorId = this.authorId;
            this.plhContent.Controls.Add(this.ctlForm);

            this.EditorClientId = this.ctlForm.ClientID;

            this.ctlForm.BubbleClick += this.ctlForm_Click;
            this.cbPreview.CallbackEvent += this.cbPreview_Callback;
        }

        protected void ContactByFaxOnlyCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            // if someone activates this checkbox send him home :-)
            this.Response.Redirect("about:blank", false);
            this.Context.ApplicationInstance.CompleteRequest();
        }

        private void ctlForm_Click(object sender, EventArgs e)
        {
            this.Page.Validate();
            if (this.ContactByFaxOnlyCheckBox.Checked)
            {
                // if someone activates this checkbox send him home :-)
                this.Response.Redirect("about:blank", false);
                this.Context.ApplicationInstance.CompleteRequest();
            }

            if (!Utilities.HasFloodIntervalPassed(floodInterval: this.MainSettings.FloodInterval, forumUser: this.ForumUser, forumInfo: this.ForumInfo))
            {
                this.plhMessage.Controls.Add(new InfoMessage { Message = "<div class=\"afmessage\">" + string.Format(this.GetSharedResource("[RESX:Error:FloodControl]"), this.MainSettings.FloodInterval) + "</div>" });
                return;
            }

            if (!this.Page.IsValid || !Utilities.InputIsValid(this.ctlForm.Body.Trim()) || !Utilities.InputIsValid(this.ctlForm.Subject))
            {
                return;
            }

            if (this.TopicId == -1 || (this.TopicId > 0 && this.Request.Params[ParamKeys.Action] == PostActions.TopicEdit))
            {
                if (this.ValidateProperties())
                {
                    this.SaveTopic();
                }
            }
            else
            {
                this.SaveReply();
            }
        }

        private bool ValidateProperties()
        {
            if (this.ForumInfo.Properties != null && this.ForumInfo.Properties.Count > 0)
            {
                foreach (var p in this.ForumInfo.Properties)
                {
                    if (p.IsRequired)
                    {
                        if (this.Request.Form["afprop-" + p.PropertyId] == null)
                        {
                            return false;
                        }

                        if ((this.Request.Form["afprop-" + p.PropertyId] != null) && string.IsNullOrEmpty(this.Request.Form["afprop-" + p.PropertyId].Trim()))
                        {
                            return false;
                        }
                    }

                    if (!string.IsNullOrEmpty(p.ValidationExpression) && !string.IsNullOrEmpty(this.Request.Form["afprop-" + p.PropertyId].Trim()))
                    {
                        var isMatch = Regex.IsMatch(this.Request.Form["afprop-" + p.PropertyId].Trim(), p.ValidationExpression, RegexOptions.IgnoreCase);
                        if (!isMatch)
                        {
                            return false;
                        }
                    }
                }

                return true;
            }

            return true;
        }

        private void cbPreview_Callback(object sender, CallBackEventArgs e)
        {
            switch (e.Parameters[0].ToLower())
            {
                case "preview":
                    var message = e.Parameters[1];

                    var topicTemplateID = this.ForumInfo.FeatureSettings.TopicTemplateId;
                    message = Utilities.CleanString(this.PortalId, message, this.allowHTML, this.editorType, this.ForumInfo.FeatureSettings.UseFilter, this.ForumInfo.FeatureSettings.AllowScript, this.ForumModuleId, this.ImagePath, this.ForumInfo.FeatureSettings.AllowEmoticons);
                    message = Utilities.ManageImagePath(message, HttpContext.Current.Request.Url);
                    var user = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ForumModuleId).GetByUserId(this.PortalId, this.UserId);
                    message = TemplateUtils.PreviewTopic(topicTemplateID, this.ForumInfo, user, message, this.ImagePath, DateTime.UtcNow, this.ForumUser.CurrentUserType, this.UserId, this.TimeZoneOffset);
                    this.hidPreviewText.Value = message;
                    break;
            }

            this.hidPreviewText.RenderControl(e.Output);
        }

        #endregion

        #region Private Methods

        private void LoadTopic()
        {
            this.ctlForm.EditorMode = Modules.ActiveForums.Controls.SubmitForm.EditorModes.EditTopic;
            var ti = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(this.ForumModuleId).GetById(this.TopicId);
            if (ti == null)
            {
                this.Response.Redirect(this.NavigateUrl(this.TabId), false);
                this.Context.ApplicationInstance.CompleteRequest();
            }
            else if ((ti.Content.AuthorId != this.UserId && this.canModEdit == false) | (ti.Content.AuthorId == this.UserId && this.canEdit == false) | (this.canEdit == false && this.canModEdit))
            {
                this.Response.Redirect(this.NavigateUrl(this.TabId), false);
                this.Context.ApplicationInstance.CompleteRequest();
            }
            else if (!this.canModEdit && ti.Content.AuthorId == this.UserId && this.canEdit && this.MainSettings.EditInterval > 0 & SimulateDateDiff.DateDiff(SimulateDateDiff.DateInterval.Minute, ti.Content.DateCreated, DateTime.UtcNow) > this.MainSettings.EditInterval)
            {
                var im = new InfoMessage
                {
                    Message = "<div class=\"afmessage\">" + string.Format(this.GetSharedResource("[RESX:Message:EditIntervalReached]"), this.MainSettings.EditInterval) + "</div>",
                };
                this.plhMessage.Controls.Add(im);
                this.plhContent.Controls.Clear();
            }
            else
            {
                // User has acccess
                var sBody = System.Net.WebUtility.HtmlDecode(ti.Content.Body);
                var sSubject = System.Net.WebUtility.HtmlDecode(ti.Content.Subject);
                sBody = Utilities.PrepareForEdit(this.PortalId, this.ForumModuleId, this.ImagePath, sBody, this.allowHTML, this.editorType);
                sSubject = Utilities.PrepareForEdit(this.PortalId, this.ForumModuleId, this.ImagePath, sSubject, false, EditorTypes.TEXTBOX);
                this.ctlForm.Subject = sSubject;
                this.ctlForm.Summary = System.Net.WebUtility.HtmlDecode(ti.Content.Summary);
                this.ctlForm.Body = sBody;
                this.ctlForm.AnnounceEnd = ti.AnnounceEnd ?? Utilities.NullDate();
                this.ctlForm.AnnounceStart =  ti.AnnounceStart ?? Utilities.NullDate();
                this.ctlForm.Locked = ti.IsLocked;
                this.ctlForm.Pinned = ti.IsPinned;
                this.ctlForm.TopicIcon = ti.TopicIcon;
                this.ctlForm.Tags = ti.Tags;
                this.ctlForm.Categories = ti.SelectedCategoriesAsString;
                this.ctlForm.IsApproved = ti.IsApproved;
                this.ctlForm.StatusId = ti.StatusId;
                this.ctlForm.TopicPriority = ti.Priority;

                // if (ti.Author.AuthorId > 0)
                //    ctlForm.Subscribe = Subscriptions.IsSubscribed(PortalId, ForumModuleId, ForumId, TopicId, SubscriptionTypes.Instant, ti.Author.AuthorId);
                this.contentId = ti.ContentId;
                this.authorId = ti.Author.AuthorId;

                if (!string.IsNullOrEmpty(ti.TopicData))
                {
                    this.ctlForm.TopicProperties = DotNetNuke.Modules.ActiveForums.Controllers.TopicPropertyController.Deserialize(ti.TopicData);
                }

                if (ti.TopicType == TopicTypes.Poll)
                {
                    // Get Poll
                    var ds = DataProvider.Instance().Poll_Get(ti.TopicId);
                    if (ds.Tables.Count > 0)
                    {
                        var pollRow = ds.Tables[0].Rows[0];
                        this.ctlForm.PollQuestion = pollRow["Question"].ToString();
                        this.ctlForm.PollType = pollRow["PollType"].ToString();

                        foreach (DataRow dr in ds.Tables[1].Rows)
                        {
                            this.ctlForm.PollOptions += dr["OptionName"] + System.Environment.NewLine;
                        }
                    }
                }

                if (ti.Content.AuthorId != this.UserId && this.canModApprove)
                {
                    this.ctlForm.ShowModOptions = true;
                }
            }

            if (this.authorId != this.UserId)
            {
                this.ctlForm.Template = "<div class=\"dcf-mod-edit-wrap\"><span>[RESX:Moderator-Editor]</span>" + this.ctlForm.Template + "</div>";
            }
        }

        private void LoadReply()
        {
            // Edit a Reply
            this.ctlForm.EditorMode = Modules.ActiveForums.Controls.SubmitForm.EditorModes.EditReply;

            DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo ri = new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController(this.ForumModuleId).GetById(this.PostId);

            if (ri == null)
            {
                this.Response.Redirect(this.NavigateUrl(this.TabId), false);
                this.Context.ApplicationInstance.CompleteRequest();
            }
            else if ((ri.Content.AuthorId != this.UserId && this.canModEdit == false) | (ri.Content.AuthorId == this.UserId && this.canEdit == false) | (this.canEdit == false && this.canModEdit))
            {
                this.Response.Redirect(this.NavigateUrl(this.TabId), false);
                this.Context.ApplicationInstance.CompleteRequest();
            }
            else if (!this.canModEdit && ri.Content.AuthorId == this.UserId && this.canEdit && this.MainSettings.EditInterval > 0 & SimulateDateDiff.DateDiff(SimulateDateDiff.DateInterval.Minute, ri.Content.DateCreated, DateTime.UtcNow) > this.MainSettings.EditInterval)
            {
                var im = new Controls.InfoMessage
                {
                    Message = "<div class=\"afmessage\">" + string.Format(this.GetSharedResource("[RESX:Message:EditIntervalReached]"), this.MainSettings.EditInterval.ToString()) + "</div>",
                };
                this.plhMessage.Controls.Add(im);
                this.plhContent.Controls.Clear();
            }
            else
            {
                var sBody = System.Net.WebUtility.HtmlDecode(ri.Content.Body);
                var sSubject = System.Net.WebUtility.HtmlDecode(ri.Content.Subject);
                sBody = Utilities.PrepareForEdit(this.PortalId, this.ForumModuleId, this.ImagePath, sBody, this.allowHTML, this.editorType);
                sSubject = Utilities.PrepareForEdit(this.PortalId, this.ForumModuleId, this.ImagePath, sSubject, false, EditorTypes.TEXTBOX);
                this.ctlForm.Subject = sSubject;
                this.ctlForm.Body = sBody;
                this.ctlForm.IsApproved = ri.IsApproved;
                this.contentId = ri.ContentId;
                this.authorId = ri.Author.AuthorId;
                if (ri.Content.AuthorId != this.UserId && this.canModApprove)
                {
                    this.ctlForm.ShowModOptions = true;
                }
            }

            if (this.authorId != this.UserId)
            {
                this.ctlForm.Template = "<div class=\"dcf-mod-edit-wrap\"><span>[RESX:Moderator-Editor]</span>" + this.ctlForm.Template + "</div>";
            }
        }

        private void PrepareTopic()
        {
            string template = TemplateCache.GetCachedTemplate(this.ForumModuleId, "TopicEditor", this.ForumInfo.FeatureSettings.TopicFormId);
            if (this.isEdit)
            {
                template = template.Replace("[RESX:CreateNewTopic]", "[RESX:EditingExistingTopic]");
            }

            if (this.MainSettings.UseSkinBreadCrumb)
            {
                var sCrumb = "<a href=\"" + this.NavigateUrl(this.TabId, string.Empty, ParamKeys.GroupId + "=" + this.ForumInfo.ForumGroupId.ToString()) + "\">" + this.ForumInfo.GroupName + "</a>|";
                sCrumb += "<a href=\"" + this.NavigateUrl(this.TabId, string.Empty, ParamKeys.ForumId + "=" + this.ForumInfo.ForumID.ToString()) + "\">" + this.ForumInfo.ForumName + "</a>";
                if (Environment.UpdateBreadCrumb(this.Page.Controls, sCrumb))
                {
                    template = template.Replace("<div class=\"afcrumb\">[AF:LINK:FORUMMAIN] > [AF:LINK:FORUMGROUP] > [AF:LINK:FORUMNAME]</div>", string.Empty);
                }
            }

            this.ctlForm.EditorMode = Modules.ActiveForums.Controls.SubmitForm.EditorModes.NewTopic;

            if (this.canModApprove)
            {
                this.ctlForm.ShowModOptions = true;
            }

            this.ctlForm.Template = template;
            this.ctlForm.IsApproved = this.isApproved;
        }

        /// <summary>
        /// Prepares the post form for creating a reply.
        /// </summary>
        private void PrepareReply()
        {
            this.ctlForm.EditorMode = Modules.ActiveForums.Controls.SubmitForm.EditorModes.Reply;

            string template = TemplateCache.GetCachedTemplate(this.ForumModuleId, "ReplyEditor", this.ForumInfo.FeatureSettings.ReplyFormId);

            #region "Backward compatilbility -- remove in v10.00.00"
            template = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.MapLegacyAuthorTokenSynonyms(new StringBuilder(template), this.PortalSettings, this.MainSettings, this.ForumUser.UserInfo?.Profile?.PreferredLocale).ToString();
            template = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.MapLegacyPostTokenSynonyms(new StringBuilder(template), this.PortalSettings, this.ForumUser.UserInfo?.Profile?.PreferredLocale).ToString();
            #endregion "Backward compatilbility -- remove in v10.00.00"

            if (this.isEdit)
            {
                template = template.Replace("[RESX:ReplyToTopic]", "[RESX:EditingExistingReply]");
            }

            if (this.MainSettings.UseSkinBreadCrumb)
            {
                template = template.Replace("<div class=\"afcrumb\">[AF:LINK:FORUMMAIN] > [AF:LINK:FORUMGROUP] > [AF:LINK:FORUMNAME]</div>", string.Empty);
            }

            this.ctlForm.Template = template;
            if (!(this.TopicId > 0))
            {
                // Can't Find Topic
                var im = new InfoMessage { Message = this.GetSharedResource("[RESX:Message:LoadTopicFailed]") };
                this.plhContent.Controls.Add(im);
            }
            else if (!this.CanReply)
            {
                // No permission to reply
                var im = new InfoMessage { Message = this.GetSharedResource("[RESX:Message:AccessDenied]") };
                this.plhContent.Controls.Add(im);
            }
            else
            {
                var ti = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(this.ForumModuleId).GetById(this.TopicId);

                if (ti == null)
                {
                    this.Response.Redirect(this.NavigateUrl(this.TabId), false);
                    this.Context.ApplicationInstance.CompleteRequest();
                }

                this.ctlForm.Subject = Utilities.GetSharedResource("[RESX:SubjectPrefix]") + " " + System.Net.WebUtility.HtmlDecode(ti.Content.Subject);
                this.ctlForm.TopicSubject = System.Net.WebUtility.HtmlDecode(ti.Content.Subject);
                var body = string.Empty;

                if (ti.IsLocked && (this.ForumUser.CurrentUserType == CurrentUserTypes.Anon || this.ForumUser.CurrentUserType == CurrentUserTypes.Auth))
                {
                    this.Response.Redirect(this.NavigateUrl(this.TabId), false);
                    this.Context.ApplicationInstance.CompleteRequest();
                }

                if (this.Request.Params[ParamKeys.QuoteId] != null | this.Request.Params[ParamKeys.ReplyId] != null | this.Request.Params[ParamKeys.PostId] != null)
                {
                    // Setup form for Quote or Reply with body display
                    var isQuote = false;
                    var postId = 0;
                    var sPostedBy = Utilities.GetSharedResource("[RESX:PostedBy]") + " {0} {1} {2}";
                    if (this.Request.Params[ParamKeys.QuoteId] != null)
                    {
                        isQuote = true;
                        if (SimulateIsNumeric.IsNumeric(this.Request.Params[ParamKeys.QuoteId]))
                        {
                            postId = Convert.ToInt32(this.Request.Params[ParamKeys.QuoteId]);
                        }
                    }
                    else if (this.Request.Params[ParamKeys.ReplyId] != null)
                    {
                        if (SimulateIsNumeric.IsNumeric(this.Request.Params[ParamKeys.ReplyId]))
                        {
                            postId = Convert.ToInt32(this.Request.Params[ParamKeys.ReplyId]);
                        }
                    }
                    else if (this.Request.Params[ParamKeys.PostId] != null)
                    {
                        if (SimulateIsNumeric.IsNumeric(this.Request.Params[ParamKeys.PostId]))
                        {
                            postId = Convert.ToInt32(this.Request.Params[ParamKeys.PostId]);
                        }
                    }

                    if (postId != 0)
                    {
                        var userDisplay = this.MainSettings.UserNameDisplay;
                        if (this.editorType == EditorTypes.TEXTBOX)
                        {
                            userDisplay = "none";
                        }

                        DotNetNuke.Modules.ActiveForums.Entities.ContentInfo ci;
                        if (postId == TopicId)
                        {
                            ci = ti.Content;
                        }
                        else
                        {
                            var ri = new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController(this.ForumModuleId).GetById(postId);
                            ci = ri.Content;
                        }

                        if (ci != null)
                        {
                            body = ci.Body;
                        }

                        var post = postId == this.TopicId ? ti : (DotNetNuke.Modules.ActiveForums.Entities.IPostInfo)new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController(this.ForumModuleId).GetById(postId);
                        sPostedBy = string.Format(sPostedBy, DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.GetDisplayName(this.PortalSettings, this.MainSettings, false, false, post.Author.AuthorId, post.Author.Username, post.Author.FirstName, post.Author.LastName, post.Author.DisplayName), Utilities.GetSharedResource("On.Text"), Utilities.GetUserFormattedDateTime(post.Content.DateCreated, this.PortalId, this.UserId));
                    }

                    if (this.allowHTML && this.editorType != EditorTypes.TEXTBOX)
                    {
                        if (body.ToUpper().Contains("<CODE") | body.ToUpper().Contains("[CODE]"))
                        {
                            var objCode = new CodeParser();
                            body = CodeParser.ParseCode(System.Net.WebUtility.HtmlDecode(body));
                        }
                    }
                    else
                    {
                        body = Utilities.PrepareForEdit(this.PortalId, this.ForumModuleId, this.ImagePath, body, this.allowHTML, this.editorType);
                    }

                    if (isQuote)
                    {
                        this.ctlForm.EditorMode = SubmitForm.EditorModes.Quote;
                        if (this.allowHTML && this.editorType != EditorTypes.TEXTBOX)
                        {
                            body = "<blockquote>" + System.Environment.NewLine + sPostedBy + System.Environment.NewLine + "<br />" + System.Environment.NewLine + body + System.Environment.NewLine + "</blockquote><br /><br />";
                        }
                        else
                        {
                            body = "[quote]" + System.Environment.NewLine + sPostedBy + System.Environment.NewLine + body + System.Environment.NewLine + "[/quote]" + System.Environment.NewLine;
                        }
                    }
                    else
                    {
                        this.ctlForm.EditorMode = SubmitForm.EditorModes.ReplyWithBody;
                        body = sPostedBy + "<br />" + body;
                    }

                    this.ctlForm.Body = body;
                }
            }

            if (this.ctlForm.EditorMode != SubmitForm.EditorModes.EditReply && this.canModApprove)
            {
                this.ctlForm.ShowModOptions = false;
            }
        }

        private void SaveTopic()
        {
            var subject = this.ctlForm.Subject;
            var body = this.ctlForm.Body;
            subject = Utilities.CleanString(this.PortalId, Utilities.XSSFilter(subject, true), false, EditorTypes.TEXTBOX, this.ForumInfo.FeatureSettings.UseFilter, false, this.ForumModuleId, this.themePath, false);
            body = Utilities.CleanString(this.PortalId, body, this.allowHTML, this.editorType, this.ForumInfo.FeatureSettings.UseFilter, this.ForumInfo.FeatureSettings.AllowScript, this.ForumModuleId, this.themePath, this.ForumInfo.FeatureSettings.AllowEmoticons);
            var summary = this.ctlForm.Summary;
            int authorId;
            string authorName;
            if (this.Request.IsAuthenticated)
            {
                authorId = this.UserInfo.UserID;
                switch (this.MainSettings.UserNameDisplay.ToUpperInvariant())
                {
                    case "USERNAME":
                        authorName = this.UserInfo.Username.Trim(' ');
                        break;
                    case "FULLNAME":
                        authorName = Convert.ToString(this.UserInfo.FirstName + " " + this.UserInfo.LastName).Trim(' ');
                        break;
                    case "FIRSTNAME":
                        authorName = this.UserInfo.FirstName.Trim(' ');
                        break;
                    case "LASTNAME":
                        authorName = this.UserInfo.LastName.Trim(' ');
                        break;
                    case "DISPLAYNAME":
                        authorName = this.UserInfo.DisplayName.Trim(' ');
                        break;
                    default:
                        authorName = this.UserInfo.DisplayName;
                        break;
                }
            }
            else
            {
                authorId = -1;
                authorName = Utilities.CleanString(this.PortalId, this.ctlForm.AuthorName, false, EditorTypes.TEXTBOX, true, false, this.ForumModuleId, this.themePath, false);
                if (authorName.Trim() == string.Empty)
                {
                    return;
                }
            }

            DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti;

            if (this.TopicId > 0)
            {
                ti = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(this.ForumModuleId).GetById(this.TopicId);
                authorId = ti.Author.AuthorId;
            }
            else
            {
                ti = new DotNetNuke.Modules.ActiveForums.Entities.TopicInfo();
                ti.Content = new DotNetNuke.Modules.ActiveForums.Entities.ContentInfo();
                ti.ForumId = this.ForumInfo.ForumID;
                ti.Forum = this.ForumInfo;
            }

            ti.AnnounceEnd = this.ctlForm.AnnounceEnd;
            ti.AnnounceStart = this.ctlForm.AnnounceStart;
            ti.Priority = this.ctlForm.TopicPriority;

            if (!this.isEdit)
            {
                ti.Content.AuthorId = authorId;
                ti.Content.AuthorName = authorName;
                ti.Content.IPAddress = this.Request.UserHostAddress;
            }

            if (Regex.IsMatch(body, "<CODE([^>]*)>", RegexOptions.IgnoreCase))
            {
                foreach (Match m in Regex.Matches(body, "<CODE([^>]*)>(.*?)</CODE>", RegexOptions.IgnoreCase))
                {
                    body = body.Replace(m.Value, m.Value.Replace("<br>", System.Environment.NewLine));
                }
            }

            ti.TopicUrl = DotNetNuke.Modules.ActiveForums.Controllers.UrlController.BuildTopicUrlSegment(portalId: this.PortalId, moduleId: this.ForumModuleId, topicId: this.TopicId, subject: subject, forumInfo: this.ForumInfo);

            ti.Content.Body = body;
            ti.Content.Subject = subject;
            ti.Content.Summary = summary;
            ti.IsAnnounce = ti.AnnounceEnd != Utilities.NullDate() && ti.AnnounceStart != Utilities.NullDate();

            if (this.canModApprove && this.ForumInfo.FeatureSettings.IsModerated)
            {
                ti.IsApproved = this.ctlForm.IsApproved;
            }
            else
            {
                ti.IsApproved = this.isApproved;
            }

            ti.IsArchived = false;
            ti.IsDeleted = false;
            ti.IsLocked = this.canLock && this.ctlForm.Locked;
            ti.IsPinned = this.canPin && this.ctlForm.Pinned;
            ti.StatusId = this.ctlForm.StatusId;
            ti.TopicIcon = this.ctlForm.TopicIcon;
            ti.TopicType = 0;
            if (this.ForumInfo.Properties != null && this.ForumInfo.Properties.Count > 0)
            {
                var tData = new StringBuilder();
                tData.Append("<topicdata>");
                tData.Append("<properties>");
                foreach (var p in this.ForumInfo.Properties)
                {
                    var pkey = "afprop-" + p.PropertyId.ToString();

                    tData.Append("<property id=\"" + p.PropertyId.ToString() + "\">");
                    tData.Append("<name><![CDATA[");
                    tData.Append(p.Name);
                    tData.Append("]]></name>");
                    if (this.Request.Form[pkey] != null)
                    {
                        tData.Append("<value><![CDATA[");
                        tData.Append(Utilities.XSSFilter(this.Request.Form[pkey]));
                        tData.Append("]]></value>");
                    }
                    else
                    {
                        tData.Append("<value></value>");
                    }

                    tData.Append("</property>");
                }

                tData.Append("</properties>");
                tData.Append("</topicdata>");
                ti.TopicData = tData.ToString();
            }

            this.TopicId = DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Save(ti);
            DotNetNuke.Modules.ActiveForums.Controllers.TopicController.SaveToForum(this.ForumModuleId, this.ForumId, this.TopicId);
            ti = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(this.ForumModuleId).GetById(this.TopicId);

            this.SaveAttachments(ti.ContentId);
            if (DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this.ForumInfo.Security.Tag, this.ForumUser.UserRoles))
            {
                new DotNetNuke.Modules.ActiveForums.Controllers.TopicTagController().DeleteForTopicId(this.TopicId);
                var tagForm = string.Empty;
                if (this.Request.Form["txtTags"] != null)
                {
                    tagForm = this.Request.Form["txtTags"];
                }

                if (tagForm != string.Empty)
                {
                    var tags = tagForm.Split(',');
                    foreach (var tag in tags)
                    {
                        var sTag = Utilities.CleanString(this.PortalId, tag.Trim(), false, EditorTypes.TEXTBOX, false, false, this.ForumModuleId, string.Empty, false);
                        DataProvider.Instance().Tags_Save(this.PortalId, this.ForumModuleId, -1, sTag, 0, 1, 0, this.TopicId, false, -1, -1);
                    }
                }
            }

            if (DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this.ForumInfo.Security.Categorize, this.ForumUser.UserRoles))
            {
                if (this.Request.Form["amaf-catselect"] != null)
                {
                    var cats = this.Request.Form["amaf-catselect"].Split(';');
                    DataProvider.Instance().Tags_DeleteTopicToCategory(this.PortalId, this.ForumModuleId, -1, this.TopicId);
                    foreach (var c in cats)
                    {
                        if (string.IsNullOrEmpty(c) || !SimulateIsNumeric.IsNumeric(c))
                        {
                            continue;
                        }

                        var cid = Convert.ToInt32(c);
                        if (cid > 0)
                        {
                            DataProvider.Instance().Tags_AddTopicToCategory(this.PortalId, this.ForumModuleId, cid, this.TopicId);
                        }
                    }
                }
            }

            if (!String.IsNullOrEmpty(this.ctlForm.PollQuestion) && !String.IsNullOrEmpty(this.ctlForm.PollOptions))
            {
                // var sPollQ = ctlForm.PollQuestion.Trim();
                // sPollQ = Utilities.CleanString(PortalId, sPollQ, false, EditorTypes.TEXTBOX, true, false, ForumModuleId, string.Empty, false);
                var pollId = DataProvider.Instance().Poll_Save(-1, this.TopicId, this.UserId, this.ctlForm.PollQuestion.Trim(), this.ctlForm.PollType);
                if (pollId > 0)
                {
                    var options = this.ctlForm.PollOptions.Split(new[] { System.Environment.NewLine }, StringSplitOptions.None);

                    foreach (string opt in options)
                    {
                        if (opt.Trim() != string.Empty)
                        {
                            var value = Utilities.CleanString(this.PortalId, opt, false, EditorTypes.TEXTBOX, true, false, this.ForumModuleId, string.Empty, false);
                            DataProvider.Instance().Poll_Option_Save(-1, pollId, value.Trim(), this.TopicId);
                        }
                    }

                    ti.TopicType = TopicTypes.Poll;
                    DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Save(ti);
                }
            }

            if ((this.UserPrefTopicSubscribe && authorId == this.UserId) || this.ctlForm.Subscribe)
            {
                new DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController().Subscribe(this.PortalId, this.ForumModuleId, this.UserId, this.ForumId, ti.TopicId);
            }

            try
            {
                DataCache.ContentCacheClearForForum(this.ModuleId, this.ForumId);
                DataCache.ContentCacheClearForTopic(this.ModuleId, ti.TopicId);

                if (!ti.IsApproved)
                {
                    DotNetNuke.Modules.ActiveForums.Controllers.TopicController.QueueUnapprovedTopicAfterAction(portalId: this.PortalId, tabId: this.TabId, moduleId: this.ForumModuleId, forumGroupId: this.ForumInfo.ForumGroupId, forumId: this.ForumId, topicId: this.TopicId, replyId: 0, contentId: ti.ContentId, authorId: ti.Content.AuthorId, userId: this.ForumUser.UserId);
                    string[] @params = { ParamKeys.ForumId + "=" + this.ForumId, ParamKeys.ViewType + "=confirmaction", ParamKeys.ConfirmActionId + "=" + ConfirmActions.MessagePending };
                    this.Response.Redirect(this.NavigateUrl(this.ForumTabId, string.Empty, @params), false);
                    this.Context.ApplicationInstance.CompleteRequest();
                }
                else
                {
                    if (!this.isEdit)
                    {
                        DotNetNuke.Modules.ActiveForums.Controllers.TopicController.QueueApprovedTopicAfterAction(portalId: this.PortalId, tabId: this.TabId, moduleId: this.ForumModuleId, forumGroupId: this.ForumInfo.ForumGroupId, forumId: this.ForumId, topicId: this.TopicId, replyId: 0, contentId: ti.ContentId, authorId: ti.Content.AuthorId, userId: this.ForumUser.UserId);
                    }

                    ControlUtils ctlUtils = new ControlUtils();
                    string sUrl = ctlUtils.BuildUrl(this.PortalId, this.TabId, this.ForumModuleId, this.ForumInfo.ForumGroup.PrefixURL, this.ForumInfo.PrefixURL, this.ForumInfo.ForumGroupId, this.ForumInfo.ForumID, this.TopicId, ti.TopicUrl, -1, -1, string.Empty, 1, -1, this.SocialGroupId);
                    if (sUrl.Contains("~/"))
                    {
                        sUrl = Utilities.NavigateURL(this.ForumTabId, string.Empty, ParamKeys.TopicId + "=" + this.TopicId);
                    }

                    this.Response.Redirect(sUrl, false);
                    this.Context.ApplicationInstance.CompleteRequest();
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        private void SaveReply()
        {
            var subject = this.ctlForm.Subject;
            var body = this.ctlForm.Body;
            subject = Utilities.CleanString(this.PortalId, subject, false, EditorTypes.TEXTBOX, this.ForumInfo.FeatureSettings.UseFilter, false, this.ForumModuleId, this.themePath, false);
            body = Utilities.CleanString(this.PortalId, body, this.allowHTML, this.editorType, this.ForumInfo.FeatureSettings.UseFilter, this.ForumInfo.FeatureSettings.AllowScript, this.ForumModuleId, this.themePath, this.ForumInfo.FeatureSettings.AllowEmoticons);

            // This HTML decode is used to make Quote functionality work properly even when it appears in Text Box instead of Editor
            if (this.Request.Params[ParamKeys.QuoteId] != null)
            {
                body = System.Net.WebUtility.HtmlDecode(body);
            }

            int authorId;
            string authorName;
            if (this.Request.IsAuthenticated)
            {
                authorId = this.UserInfo.UserID;
                switch (this.MainSettings.UserNameDisplay.ToUpperInvariant())
                {
                    case "USERNAME":
                        authorName = this.UserInfo.Username.Trim(' ');
                        break;
                    case "FULLNAME":
                        authorName = Convert.ToString(this.UserInfo.FirstName + " " + this.UserInfo.LastName).Trim(' ');
                        break;
                    case "FIRSTNAME":
                        authorName = this.UserInfo.FirstName.Trim(' ');
                        break;
                    case "LASTNAME":
                        authorName = this.UserInfo.LastName.Trim(' ');
                        break;
                    case "DISPLAYNAME":
                        authorName = this.UserInfo.DisplayName.Trim(' ');
                        break;
                    default:
                        authorName = this.UserInfo.DisplayName;
                        break;
                }
            }
            else
            {
                authorId = -1;
                authorName = Utilities.CleanString(this.PortalId, this.ctlForm.AuthorName, false, EditorTypes.TEXTBOX, true, false, this.ForumModuleId, this.themePath, false);
                if (authorName.Trim() == string.Empty)
                {
                    return;
                }
            }

            DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo ri;

            var sc = new DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController();
            var rc = new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController(this.ForumModuleId);

            if (this.PostId > 0)
            {
                ri = rc.GetById(this.PostId);
            }
            else
            {
                ri = new DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo();
                ri.Content = new DotNetNuke.Modules.ActiveForums.Entities.ContentInfo();
            }

            if (!this.isEdit)
            {
                ri.Content.AuthorId = authorId;
                ri.Content.AuthorName = authorName;
                ri.Content.IPAddress = this.Request.UserHostAddress;
            }

            if (Regex.IsMatch(body, "<CODE([^>]*)>", RegexOptions.IgnoreCase))
            {
                foreach (Match m in Regex.Matches(body, "<CODE([^>]*)>(.*?)</CODE>", RegexOptions.IgnoreCase))
                {
                    body = body.Replace(m.Value, m.Value.Replace("<br>", System.Environment.NewLine));
                }
            }

            ri.Content.Body = body;
            ri.Content.Subject = subject;
            ri.Content.Summary = string.Empty;

            if (this.canModApprove && ri.Content.AuthorId != this.UserId)
            {
                ri.IsApproved = this.ctlForm.IsApproved;
            }
            else
            {
                ri.IsApproved = this.isApproved;
            }

            ri.IsDeleted = false;
            ri.StatusId = this.ctlForm.StatusId;
            ri.TopicId = this.TopicId;
            if (this.UserPrefTopicSubscribe)
            {
                sc.Subscribe(this.PortalId, this.ForumModuleId, this.UserId, this.ForumId, ri.TopicId);
            }

            var tmpReplyId = rc.Reply_Save(this.PortalId, this.ForumModuleId, ri);
            ri = new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController(this.ForumModuleId).GetById(tmpReplyId);
            this.SaveAttachments(ri.ContentId);
            try
            {
                if (this.ctlForm.Subscribe && authorId == this.UserId)
                {
                    if (!sc.Subscribed(this.PortalId, this.ForumModuleId, authorId, this.ForumId, this.TopicId))
                    {
                        // TODO: move to new DAL2 subscription controller
                        new SubscriptionController().Subscription_Update(this.PortalId, this.ForumModuleId, this.ForumId, this.TopicId, 1, authorId, this.ForumUser.UserRoles);
                    }
                }
                else if (this.isEdit)
                {
                    var isSub = new DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController().Subscribed(this.PortalId, this.ForumModuleId, authorId, this.ForumId, this.TopicId);
                    if (isSub && !this.ctlForm.Subscribe)
                    {
                        // TODO: move to new DAL2 subscription controller
                        new SubscriptionController().Subscription_Update(this.PortalId, this.ForumModuleId, this.ForumId, this.TopicId, 1, authorId, this.ForumUser.UserRoles);
                    }
                }

                if (!ri.IsApproved)
                {
                    DotNetNuke.Modules.ActiveForums.Controllers.ReplyController.QueueUnapprovedReplyAfterAction(portalId: this.PortalId, tabId: this.TabId, moduleId: this.ForumModuleId, forumGroupId: this.ForumInfo.ForumGroupId, forumId: this.ForumId, topicId: this.TopicId, replyId: tmpReplyId, contentId: ri.ContentId, authorId: ri.Content.AuthorId, userId: this.ForumUser.UserId);

                    string[] @params = { ParamKeys.ForumId + "=" + this.ForumId, ParamKeys.TopicId + "=" + this.TopicId, ParamKeys.ViewType + "=confirmaction", ParamKeys.ConfirmActionId + "=" + ConfirmActions.MessagePending };
                    this.Response.Redirect(Utilities.NavigateURL(this.TabId, string.Empty, @params), false);
                    this.Context.ApplicationInstance.CompleteRequest();
                }
                else
                {
                    if (!this.isEdit)
                    {
                        DotNetNuke.Modules.ActiveForums.Controllers.ReplyController.QueueApprovedReplyAfterAction(portalId: this.PortalId, tabId: this.TabId, moduleId: this.ModuleId, forumGroupId: this.ForumInfo.ForumGroupId, forumId: this.ForumId, topicId: this.TopicId, replyId: tmpReplyId, contentId: ri.ContentId, authorId: ri.Content.AuthorId, userId: this.ForumUser.UserId);
                    }

                    var fullURL = new ControlUtils().BuildUrl(this.PortalId, this.TabId, this.ForumModuleId, this.ForumInfo.ForumGroup.PrefixURL, this.ForumInfo.PrefixURL, this.ForumInfo.ForumGroupId, this.ForumInfo.ForumID, this.TopicId, ri.Topic.TopicUrl, -1, -1, string.Empty, 1, tmpReplyId, this.SocialGroupId);
                    if (fullURL.Contains("~/"))
                    {
                        fullURL = Utilities.NavigateURL(this.TabId, string.Empty, new[] { ParamKeys.TopicId + "=" + this.TopicId, ParamKeys.ContentJumpId + "=" + tmpReplyId });
                    }

                    if (fullURL.EndsWith("/"))
                    {
                        fullURL += Utilities.UseFriendlyURLs(this.ForumModuleId) ? String.Concat("#", tmpReplyId) : String.Concat("?", ParamKeys.ContentJumpId, "=", tmpReplyId);
                    }

                    this.Response.Redirect(fullURL, false);
                    this.Context.ApplicationInstance.CompleteRequest();
                }
            }
            catch (Exception)
            {
            }
        }

        // Note attachments are currently saved into the authors file directory
        private void SaveAttachments(int contentId)
        {
            var fileManager = FileManager.Instance;
            var folderManager = FolderManager.Instance;
            var adb = new Data.AttachController();

            var userFolder = folderManager.GetUserFolder(this.UserInfo);

            const string uploadFolderName = "activeforums_Upload";
            const string attachmentFolderName = "activeforums_Attach";
            const string fileNameTemplate = "__{0}__{1}__{2}";

            var attachmentFolder = folderManager.GetFolder(this.PortalId, attachmentFolderName) ?? folderManager.AddFolder(this.PortalId, attachmentFolderName);

            // Read the attachment list sent in the hidden field as json
            var attachmentsJson = this.hidAttachments.Value;
            var serializer = new DataContractJsonSerializer(typeof(List<ClientAttachment>));
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(attachmentsJson));
            var attachmentsNew = (List<ClientAttachment>)serializer.ReadObject(ms);
            ms.Close();

            // Read the list of existing attachments for the content.  Must do this before saving any of the new attachments!
            // Ignore any legacy inline attachments
            var attachmentsOld = adb.ListForContent(contentId).Where(o => !o.AllowDownload.HasValue || o.AllowDownload.Value);

            // Save all of the new attachments
            foreach (var attachment in attachmentsNew)
            {
                // Don't need to do anything with existing attachments
                if (attachment.AttachmentId.HasValue && attachment.AttachmentId.Value > 0)
                {
                    continue;
                }

                IFileInfo file = null;

                var fileId = attachment.FileId.GetValueOrDefault();
                if (fileId > 0 && userFolder != null)
                {
                    // Make sure that the file exists and it actually belongs to the user who is trying to attach it
                    file = fileManager.GetFile(fileId);
                    if (file == null || file.FolderId != userFolder.FolderID)
                    {
                        continue;
                    }
                }
                else if (!string.IsNullOrWhiteSpace(attachment.UploadId) && !string.IsNullOrWhiteSpace(attachment.FileName))
                {
                    if (!Regex.IsMatch(attachment.UploadId, @"^[\w\-. ]+$")) // Check for shenanigans.
                    {
                        continue;
                    }

                    var uploadFilePath = PathUtils.Instance.GetPhysicalPath(this.PortalId, uploadFolderName + "/" + attachment.UploadId);

                    if (!File.Exists(uploadFilePath))
                    {
                        continue;
                    }

                    // Store the files with a filename format that prevents overwrites.
                    var index = 0;
                    var fileName = string.Format(fileNameTemplate, contentId, index, Regex.Replace(attachment.FileName, @"[^\w\-. ]+", string.Empty));
                    while (fileManager.FileExists(attachmentFolder, fileName))
                    {
                        index++;
                        fileName = string.Format(fileNameTemplate, contentId, index, Regex.Replace(attachment.FileName, @"[^\w\-. ]+", string.Empty));
                    }

                    // Copy the file into the attachment folder with the correct name.
                    using (var fileStream = new FileStream(uploadFilePath, FileMode.Open, FileAccess.Read))
                    {
                        file = fileManager.AddFile(attachmentFolder, fileName, fileStream);
                    }

                    File.Delete(uploadFilePath);
                }

                if (file == null)
                {
                    continue;
                }

                adb.Save(contentId, this.UserId, file.FileName, file.ContentType, file.Size, file.FileId);
            }

            // Remove any attachments that are no longer in the list of attachments
            var attachmentsToRemove = attachmentsOld.Where(a1 => attachmentsNew.All(a2 => a2.AttachmentId != a1.AttachmentId));
            foreach (var attachment in attachmentsToRemove)
            {
                adb.Delete(attachment.AttachmentId);

                var file = attachment.FileId.HasValue ? fileManager.GetFile(attachment.FileId.Value) : fileManager.GetFile(attachmentFolder, attachment.FileName);

                // Only delete the file if it exists in the attachment folder
                if (file != null && file.FolderId == attachmentFolder.FolderID)
                {
                    fileManager.DeleteFile(file);
                }
            }
        }

        private void PrepareAttachments(int? contentId = null)
        {
            // Handle the case where we don't yet have a topic id (new posts)
            if (!contentId.HasValue || contentId.Value <= 0)
            {
                this.hidAttachments.Value = "[]"; // JSON for an empty array
                return;
            }

            var adb = new Data.AttachController();
            var attachments = adb.ListForContent(contentId.Value);

            var clientAttachments = attachments.Select(attachment => new ClientAttachment
            {
                AttachmentId = attachment.AttachmentId,
                ContentType = attachment.ContentType,
                FileId = attachment.FileId,
                FileName = Regex.Replace(attachment.FileName.TextOrEmpty(), @"^__\d+__\d+__", string.Empty), // Remove our unique file prefix before sending to the client.
                FileSize = attachment.FileSize,
            }).ToList();

            var serializer = new DataContractJsonSerializer(typeof(List<ClientAttachment>));

            using (var ms = new MemoryStream())
            {
                serializer.WriteObject(ms, clientAttachments);
                ms.Seek(0, 0);
                using (var sr = new StreamReader(ms, Encoding.UTF8))
                {
                    this.hidAttachments.Value = sr.ReadToEnd();
                }
            }
        }

        #endregion
    }
}
