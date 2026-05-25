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
    using System.Linq;
    using System.Text;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    using DotNetNuke.Modules.ActiveForums.Controls;
    using DotNetNuke.Web.UI.WebControls;

    public partial class af_grid : ForumBase
    {
        protected System.Web.UI.WebControls.Label lblHeader;
        protected System.Web.UI.HtmlControls.HtmlButton btnMarkRead;
        protected System.Web.UI.WebControls.DropDownList drpTimeFrame;
        protected System.Web.UI.WebControls.Panel pnlMessage;
        protected System.Web.UI.WebControls.Literal litMessage;
        protected System.Web.UI.WebControls.Literal litRecordCount;
        protected System.Web.UI.WebControls.Repeater rptTopics;
        protected DotNetNuke.Modules.ActiveForums.Controls.PagerNav PagerTop;
        protected DotNetNuke.Modules.ActiveForums.Controls.PagerNav PagerBottom;

        private int rowCount;
        private IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.IPostInfo> posts;
        private int pageSize = 20;
        private int rowIndex;
        private Control ctl;

        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            string template = DotNetNuke.Modules.ActiveForums.Controllers.TemplateController.Template_Get(this.ForumModuleId, Enums.TemplateType.TopicResults, SettingsBase.GetModuleSettings(this.ForumModuleId).DefaultFeatureSettings.TemplateFileNameSuffix, this.ForumUser);

            try
            {
                template = Globals.ForumsControlsRegisterAMTag + template;
                template = Utilities.LocalizeControl(template);
                this.ctl = this.ParseControl(template);
                this.LinkControls(this.ctl.Controls);

                this.FilteredTopics.Controls.Clear();
                this.FilteredTopics.Controls.Add(this.ctl);

                this.drpTimeFrame.SelectedIndexChanged += this.DrpTimeFrameSelectedIndexChanged;
                this.btnMarkRead.ServerClick += this.BtnMarkReadClick;
                this.rptTopics.ItemDataBound += this.TopicRepeaterOnItemDataBound;
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                throw;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            foreach (ListItem timeFrameItem in this.drpTimeFrame.Items)
            {
                timeFrameItem.Text = this.GetSharedResource($"ActiveTopics-{timeFrameItem.Value}min.Text");
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            this.BindPosts();
        }

        private void DrpTimeFrameSelectedIndexChanged(object sender, EventArgs e)
        {
            var timeframe = Utilities.SafeConvertInt(this.drpTimeFrame.SelectedItem.Value, 1440);
            string url = string.Empty;
            if (this.Request.Params[ParamKeys.GridType].Equals(GridTypes.Tags))
            {
                var tag = new DotNetNuke.Modules.ActiveForums.Controllers.TagController().GetById(Utilities.SafeConvertInt(this.Request.QueryString[ParamKeys.Tags], -1));
                if (tag != null)
                {
                    url = new ControlUtils().BuildUrl(portalId: this.PortalId, tabId: this.TabId, moduleId: this.ModuleId, groupPrefix: string.Empty, forumPrefix: string.Empty, forumGroupId: this.ForumGroupId, forumID: this.ForumId, topicId: this.TopicId, topicURL: string.Empty, tagId: Utilities.SafeConvertInt(this.Request.QueryString[ParamKeys.Tags], -1), categoryId: -1, otherPrefix: tag.TagName, pageId: 1, contentId: -1, socialGroupId: -1);
                }
            }

            if (string.IsNullOrEmpty(url))
            {
                url = new ControlUtils().BuildUrl(portalId: this.PortalId, tabId: this.TabId, moduleId: this.ModuleId, groupPrefix: string.Empty, forumPrefix: string.Empty, forumGroupId: this.ForumGroupId, forumID: this.ForumId, topicId: this.TopicId, topicURL: string.Empty, tagId: -1, categoryId: -1, otherPrefix: this.Request.Params[ParamKeys.GridType], pageId: 1, contentId: -1, socialGroupId: -1);
            }

            if (this.ModuleSettings.URLRewriteEnabled)
            {
                url += $"{ParamKeys.TimeSpan}/{timeframe}";
            }
            else
            {
                url += url.Contains("?") ? $"&{ParamKeys.TimeSpan}={timeframe}" : $"?{ParamKeys.TimeSpan}={timeframe}";
            }

            this.Response.Redirect(url);
            this.Context.ApplicationInstance.CompleteRequest();
        }

        private void BtnMarkReadClick(object sender, EventArgs e)
        {
            if (this.UserId >= 0)
            {
                DataProvider.Instance().Utility_MarkAllRead(this.ForumModuleId, this.UserId, 0);
            }

            this.Response.Redirect(Utilities.NavigateURL(this.TabId, string.Empty, new[] { $"{ParamKeys.ViewType}={Views.Grid}", $"{ParamKeys.GridType}={GridTypes.NotRead}" }), false);
            this.Context.ApplicationInstance.CompleteRequest();
        }

        private void TopicRepeaterOnItemDataBound(object sender, RepeaterItemEventArgs repeaterItemEventArgs)
        {
            if (repeaterItemEventArgs.Item.ItemType == ListItemType.Item || repeaterItemEventArgs.Item.ItemType == ListItemType.AlternatingItem)
            {
                try
                {
                    string itemTemplate = ((LiteralControl)repeaterItemEventArgs.Item.Controls[0]).Text;
                    var topic = (DotNetNuke.Modules.ActiveForums.Entities.IPostInfo)repeaterItemEventArgs.Item.DataItem;

                    if (topic != null)
                    {
                        itemTemplate = Utilities.DecodeBrackets(DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.ReplacePostTokens(new StringBuilder(itemTemplate), topic, this.PortalSettings, this.ModuleSettings, new Services.URLNavigator().NavigationManager(), this.ForumUser, this.Request.Url, this.Request.RawUrl).ToString());
                        ((LiteralControl)repeaterItemEventArgs.Item.Controls[0]).Text = itemTemplate;
                    }
                }
                catch (Exception ex)
                {
                    Exceptions.LogException(ex);
                }
            }
        }

        #endregion

        #region Private Methods

        private void BindPosts()
        {
            this.pageSize = (this.UserId > 0) ? this.UserDefaultPageSize : this.ModuleSettings.PageSize;

            if (this.pageSize < 5)
            {
                this.pageSize = 10;
            }

            this.posts = null;
            this.rowIndex = (this.PageId - 1) * this.pageSize;

            var forumIds = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForumsForUser(this.ForumModuleId, this.ForumUser, DotNetNuke.Modules.ActiveForums.SecureActions.Read);

            if (this.Request.Params[ParamKeys.GridType] != null)
            {
                var timeFrame = Utilities.SafeConvertInt(this.Request.Params[ParamKeys.TimeSpan], 1440);
                var gview = Utilities.XSSFilter(this.Request.Params[ParamKeys.GridType]).ToLowerInvariant();
                switch (gview)
                {
                    case GridTypes.NotRead:

                        if (this.UserId > 0)
                        {
                            this.lblHeader.Text = this.GetSharedResource("[RESX:NotRead]");
                            if (timeFrame < 15 || timeFrame > int.MaxValue)
                            {
                                timeFrame = 1440;
                            }

                            this.drpTimeFrame.Visible = true;
                            this.drpTimeFrame.SelectedIndex = this.drpTimeFrame.Items.IndexOf(this.drpTimeFrame.Items.FindByValue(timeFrame.ToString()));
                            this.rowCount = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(this.ForumModuleId).GetMyUnreadTopicsCount(forumIds, timeFrame, this.UserId);
                            if (this.rowCount > 0)
                            {
                                this.posts = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(this.ForumModuleId).GetMyUnreadTopics(forumIds: forumIds, timeFrameMinutes: timeFrame, pageId: this.PageId, pageSize: this.pageSize, authorId: this.UserId);
                                this.btnMarkRead.Visible = true;
                                this.btnMarkRead.InnerText = this.GetSharedResource("[RESX:MarkAllRead]");
                            }
                        }
                        else
                        {
                            this.Response.Redirect(Utilities.NavigateURL(this.TabId), false);
                            this.Context.ApplicationInstance.CompleteRequest();
                        }

                        break;

                    case GridTypes.Announcements:

                        this.lblHeader.Text = this.GetSharedResource("[RESX:Announcements]");
                        this.rowCount = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(this.ForumModuleId).GetAnnouncementsCount(forumIds);
                        if (this.rowCount > 0)
                        {
                            this.posts = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(this.ForumModuleId).GetAnnouncements(forumIds: forumIds, pageId: this.PageId, pageSize: this.pageSize);
                        }

                        break;

                    case GridTypes.Unresolved:

                        this.lblHeader.Text = this.GetSharedResource("[RESX:Unresolved]");
                        if (timeFrame < 15 || timeFrame > int.MaxValue)
                        {
                            timeFrame = 1440;
                        }

                        this.drpTimeFrame.Visible = true;
                        this.drpTimeFrame.SelectedIndex = this.drpTimeFrame.Items.IndexOf(this.drpTimeFrame.Items.FindByValue(timeFrame.ToString()));
                        this.rowCount = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(this.ForumModuleId).GetUnresolvedCount(forumIds, timeFrame);
                        if (this.rowCount > 0)
                        {
                            this.posts = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(this.ForumModuleId).GetUnresolved(forumIds: forumIds, timeFrameMinutes: timeFrame, pageId: this.PageId, pageSize: this.pageSize);
                        }

                        break;

                    case GridTypes.Unanswered:

                        this.lblHeader.Text = this.GetSharedResource("[RESX:Unanswered]");
                        if (timeFrame < 15 || timeFrame > int.MaxValue)
                        {
                            timeFrame = 1440;
                        }

                        this.drpTimeFrame.Visible = true;
                        this.drpTimeFrame.SelectedIndex = this.drpTimeFrame.Items.IndexOf(this.drpTimeFrame.Items.FindByValue(timeFrame.ToString()));
                        this.rowCount = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(this.ForumModuleId).GetUnansweredCount(forumIds, timeFrame);
                        if (this.rowCount > 0)
                        {
                            this.posts = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(this.ForumModuleId).GetUnanswered(forumIds: forumIds, timeFrameMinutes: timeFrame, pageId: this.PageId, pageSize: this.pageSize);
                        }

                        break;

                    case GridTypes.Tags:

                        var tagId = -1;
                        if (this.Request.QueryString[ParamKeys.Tags] != null && Utilities.IsNumeric(this.Request.QueryString[ParamKeys.Tags]))
                        {
                            tagId = int.Parse(this.Request.QueryString[ParamKeys.Tags]);
                            if (tagId > 0)
                            {
                                var tag = new DotNetNuke.Modules.ActiveForums.Controllers.TagController().GetById(tagId);
                                if (tag != null)
                                {
                                    this.lblHeader.Text = $"{this.GetSharedResource("[RESX:Tags]")}: {tag.TagName.Trim()}";
                                    if (timeFrame < 15 || timeFrame > int.MaxValue)
                                    {
                                        timeFrame = 1440;
                                    }

                                    this.drpTimeFrame.Visible = true;
                                    this.drpTimeFrame.SelectedIndex = this.drpTimeFrame.Items.IndexOf(this.drpTimeFrame.Items.FindByValue(timeFrame.ToString()));
                                    this.rowCount = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(this.ForumModuleId).GetTaggedTopicsCount(forumIds: forumIds, tagId: tagId, timeFrameMinutes: timeFrame);
                                    if (this.rowCount > 0)
                                    {
                                        this.posts = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(this.ForumModuleId).GetTaggedTopics(forumIds: forumIds, tagId: tagId, timeFrameMinutes: timeFrame, pageId: this.PageId, pageSize: this.pageSize);
                                    }
                                }
                            }
                        }

                        break;

                    case GridTypes.MyTopics:

                        if (this.UserId > 0)
                        {
                            this.lblHeader.Text = this.GetSharedResource("[RESX:MyTopics]");
                            if (timeFrame < 15 || timeFrame > int.MaxValue)
                            {
                                timeFrame = 1440;
                            }

                            this.drpTimeFrame.Visible = true;
                            this.drpTimeFrame.SelectedIndex = this.drpTimeFrame.Items.IndexOf(this.drpTimeFrame.Items.FindByValue(timeFrame.ToString()));
                            this.rowCount = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(this.ForumModuleId).GetMyTopicsCount(forumIds, timeFrame, this.UserId);
                            if (this.rowCount > 0)
                            {
                                this.posts = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(this.ForumModuleId).GetMyTopics(forumIds: forumIds, timeFrameMinutes: timeFrame, pageId: this.PageId, pageSize: this.pageSize, authorId: this.UserId);
                            }
                        }
                        else
                        {
                            this.Response.Redirect(Utilities.NavigateURL(this.TabId), false);
                            this.Context.ApplicationInstance.CompleteRequest();
                        }

                        break;

                    case GridTypes.ActiveTopics:

                        this.lblHeader.Text = this.GetSharedResource("[RESX:ActiveTopics]");
                        if (timeFrame < 15 || timeFrame > int.MaxValue)
                        {
                            timeFrame = 1440;
                        }

                        this.drpTimeFrame.Visible = true;
                        this.drpTimeFrame.SelectedIndex = this.drpTimeFrame.Items.IndexOf(this.drpTimeFrame.Items.FindByValue(timeFrame.ToString()));
                        this.rowCount = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(this.ForumModuleId).GetActiveTopicsCount(forumIds, timeFrame);
                        if (this.rowCount > 0)
                        {
                            this.posts = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(this.ForumModuleId).GetActiveTopics(forumIds: forumIds, timeFrameMinutes: timeFrame, pageId: this.PageId, pageSize: this.pageSize);
                        }

                        break;

                    case GridTypes.MostLiked:

                        this.lblHeader.Text = this.GetSharedResource("[RESX:MostLiked]");
                        if (timeFrame < 15 || timeFrame > int.MaxValue)
                        {
                            timeFrame = 1440;
                        }

                        this.drpTimeFrame.Visible = true;
                        this.drpTimeFrame.SelectedIndex = this.drpTimeFrame.Items.IndexOf(this.drpTimeFrame.Items.FindByValue(timeFrame.ToString()));
                        this.rowCount = DotNetNuke.Modules.ActiveForums.Controllers.ContentController.GetMostLikesCount(this.ForumModuleId, forumIds, timeFrame);
                        if (this.rowCount > 0)
                        {
                            this.posts = DotNetNuke.Modules.ActiveForums.Controllers.ContentController.GetMostLikes(this.ForumModuleId, forumIds, timeFrame, this.PageId, this.pageSize);
                        }

                        break;

                    case GridTypes.MostReplies:

                        this.lblHeader.Text = this.GetSharedResource("[RESX:MostReplies]");
                        if (timeFrame < 15 || timeFrame > int.MaxValue)
                        {
                            timeFrame = 1440;
                        }

                        this.drpTimeFrame.Visible = true;
                        this.drpTimeFrame.SelectedIndex = this.drpTimeFrame.Items.IndexOf(this.drpTimeFrame.Items.FindByValue(timeFrame.ToString()));
                        this.rowCount = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(this.ForumModuleId).GetMostRepliesCount(forumIds, timeFrame);
                        if (this.rowCount > 0)
                        {
                            this.posts = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(this.ForumModuleId).GetMostReplies(forumIds, timeFrame, this.PageId, this.pageSize);
                        }

                        break;

                    default:
                        this.Response.Redirect(Utilities.NavigateURL(this.TabId), false);
                        this.Context.ApplicationInstance.CompleteRequest();
                        break;
                }

                if (this.ModuleSettings.UseSkinBreadCrumb)
                {
                    Environment.UpdateBreadCrumb(this.Page.Controls, $"<a href=\"{Utilities.NavigateURL(this.TabId, string.Empty, new[] { ParamKeys.ViewType + $"={Views.Grid}", $"{ParamKeys.GridType}={gview}" })}\">{this.lblHeader.Text}</a>");
                }

                var tempVar = this.BasePage;
                Environment.UpdateMeta(ref tempVar, "[VALUE] - " + this.lblHeader.Text, "[VALUE]", "[VALUE]");
            }

            if (this.posts != null && this.posts.Any())
            {
                this.pnlMessage.Visible = false;

                try
                {
                    this.rptTopics.DataSource = this.posts;
                    this.rptTopics.DataBind();
                    this.litRecordCount.Text = string.Format(this.GetSharedResource("[RESX:SearchRecords]"), this.rowIndex + 1, this.rowIndex + this.posts.Count(), this.rowCount);
                    this.rptTopics.Visible = true;
                    this.BuildPager(this.PagerTop);
                    this.BuildPager(this.PagerBottom);
                }
                catch (Exception ex)
                {
                    this.litMessage.Text = ex.Message;
                    this.pnlMessage.Visible = true;
                    this.rptTopics.Visible = false;
                }
            }
            else
            {
                this.litMessage.Text = this.GetSharedResource("[RESX:SearchNoResults]");
                this.pnlMessage.Visible = true;
            }
        }

        private void LinkControls(ControlCollection ctrls)
        {
            foreach (Control ctrl in ctrls)
            {
                switch (ctrl.ID)
                {
                    case "lblHeader":
                        this.lblHeader = (Label)ctrl;
                        break;
                    case "btnMarkRead":
                        this.btnMarkRead = (HtmlButton)ctrl;
                        break;
                    case "drpTimeFrame":
                        this.drpTimeFrame = (DropDownList)ctrl;
                        break;
                    case "litMessage":
                        this.litMessage = (Literal)ctrl;
                        break;
                    case "litRecordCount":
                        this.litRecordCount = (Literal)ctrl;
                        break;
                    case "rptTopics":
                        this.rptTopics = (Repeater)ctrl;
                        break;
                    case "pnlMessage":
                        this.pnlMessage = (Panel)ctrl;
                        break;
                    case "PagerTop":
                        this.PagerTop = (PagerNav)ctrl;
                        break;
                    case "PagerBottom":
                        this.PagerBottom = (PagerNav)ctrl;
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

        private void BuildPager(PagerNav pager)
        {
            if (pager == null)
            {
                return;
            }

            var intPages = Convert.ToInt32(Math.Ceiling(this.rowCount / (double)this.pageSize));

            string[] @params;
            if (this.Request.Params[ParamKeys.TimeSpan] != null)
            {
                if (this.Request.Params[ParamKeys.GridType].Equals(GridTypes.Tags))
                {
                    @params = new[] { $"{ParamKeys.GridType}={this.Request.Params[ParamKeys.GridType]}", $"{ParamKeys.Tags}={this.Request.QueryString[ParamKeys.Tags]}", $"{ParamKeys.TimeSpan}={this.Request.Params[ParamKeys.TimeSpan]}", };
                }
                else
                {
                    @params = new[] { $"{ParamKeys.GridType}={this.Request.Params[ParamKeys.GridType]}", $"{ParamKeys.TimeSpan}={this.Request.Params[ParamKeys.TimeSpan]}", };
                }
            }
            else
            {
                if (this.Request.Params[ParamKeys.GridType].Equals(GridTypes.Tags))
                {
                    @params = new[] { $"{ParamKeys.GridType}={this.Request.Params[ParamKeys.GridType]}", $"{ParamKeys.Tags}={this.Request.QueryString[ParamKeys.Tags]}", };
                }
                else
                {
                    @params = new[] { $"{ParamKeys.GridType}={this.Request.Params[ParamKeys.GridType]}", };
                }
            }

            pager.PageCount = intPages;
            pager.CurrentPage = this.PageId;
            pager.ModuleID = this.ForumModuleId;
            pager.TabID = this.TabId;
            pager.ForumID = this.ForumId;
            pager.PageText = Utilities.GetSharedResource("[RESX:Page]");
            pager.OfText = Utilities.GetSharedResource("[RESX:PageOf]");
            pager.View = Views.Grid;

            pager.PageMode = Modules.ActiveForums.Controls.PagerNav.Mode.Links;

            if (this.ModuleSettings.URLRewriteEnabled)
            {
                if (!string.IsNullOrEmpty(this.ModuleSettings.PrefixURLBase))
                {
                    pager.BaseURL = "/" + this.ModuleSettings.PrefixURLBase;
                }

                if (!string.IsNullOrEmpty(this.ModuleSettings.PrefixURLOther))
                {
                    pager.BaseURL += "/" + this.ModuleSettings.PrefixURLOther;
                }

                if (this.Request.Params[ParamKeys.GridType].Equals(GridTypes.Tags) && !string.IsNullOrEmpty(this.ModuleSettings.PrefixURLTag))
                {
                    var tag = new DotNetNuke.Modules.ActiveForums.Controllers.TagController().GetById(Utilities.SafeConvertInt(this.Request.QueryString[ParamKeys.Tags], -1));
                    if (tag != null)
                    {
                        pager.BaseURL += $"/{this.ModuleSettings.PrefixURLTag}/{tag.TagName}/{ParamKeys.TimeSpan}/{this.Request.Params[ParamKeys.TimeSpan]}";
                    }
                }

                pager.BaseURL += "/" + this.Request.Params[ParamKeys.GridType] + "/";
            }

            pager.Params = @params;
        }

        #endregion

        #region "Deprecated Methods"
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public string GetArrowPath() => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public string GetForumUrl() => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public string GetThreadUrl() => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public string GetLastRead() => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public string GetPostTime() => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public string GetAuthor() => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public string GetLastPostAuthor() => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public string GetLastPostTime() => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public string GetIcon() => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public string GetMiniPager() => throw new NotImplementedException();
        #endregion "Deprecated Methods"
    }
}
