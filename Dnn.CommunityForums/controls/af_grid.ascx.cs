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
    using System.Data;
    using System.Text;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    using DotNetNuke.Modules.ActiveForums.Controls;

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
        private DataTable dtResults;
        private int pageSize = 20;
        private int rowIndex;
        private Control ctl;
        private DataRow currentRow;

        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            string template = TemplateCache.GetCachedTemplate(this.ForumModuleId, "TopicResults", -1);

            try
            {
                template = Globals.ForumsControlsRegisterAMTag + template;
                template = Utilities.LocalizeControl(template);
                this.ctl = this.ParseControl(template);
                this.LinkControls(this.ctl.Controls);
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

            this.drpTimeFrame.SelectedIndexChanged += this.DrpTimeFrameSelectedIndexChanged;
            this.btnMarkRead.ServerClick += this.BtnMarkReadClick;

            this.rptTopics.ItemCreated += this.TopicRepeaterOnItemCreated;
            this.rptTopics.ItemDataBound += this.TopicRepeaterOnItemDataBound;

            var sortDirection = this.Request.Params[ParamKeys.Sort] ?? SortOptions.Descending;

            this.BindPosts(sortDirection);
            this.FilteredTopics.Controls.Clear();
            this.FilteredTopics.Controls.Add(this.ctl);
        }

        private void DrpTimeFrameSelectedIndexChanged(object sender, EventArgs e)
        {
            var timeframe = Utilities.SafeConvertInt(this.drpTimeFrame.SelectedItem.Value, 1440);
            this.Response.Redirect(Utilities.NavigateURL(this.TabId, string.Empty, new[] { ParamKeys.ViewType + $"={Views.Grid}", $"{ParamKeys.GridType}=" + this.Request.Params[$"{ParamKeys.GridType}"], $"{ParamKeys.TimeSpan}={timeframe}" }), false);
            this.Context.ApplicationInstance.CompleteRequest();
        }

        private void BtnMarkReadClick(object sender, EventArgs e)
        {
            if (this.UserId >= 0)
            {
                DataProvider.Instance().Utility_MarkAllRead(this.ForumModuleId, this.UserId, 0);
            }

            this.Response.Redirect(Utilities.NavigateURL(this.TabId, string.Empty, new[] { ParamKeys.ViewType + $"={Views.Grid}", $"{ParamKeys.GridType}={GridTypes.NotRead}" }), false);
            this.Context.ApplicationInstance.CompleteRequest();
        }

        private void TopicRepeaterOnItemDataBound(object sender, RepeaterItemEventArgs repeaterItemEventArgs)
        {
            if (repeaterItemEventArgs.Item.ItemType == ListItemType.Item || repeaterItemEventArgs.Item.ItemType == ListItemType.AlternatingItem)
            {
                try
                {
                    string itemTemplate = ((LiteralControl)repeaterItemEventArgs.Item.Controls[0]).Text;
                    int topicId = Utilities.SafeConvertInt(((System.Data.DataRowView)repeaterItemEventArgs.Item.DataItem)["TopicId"].ToString(), 1);
                    var topic = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(this.ForumModuleId).GetById(topicId);
                    if (topic != null)
                    {
                        itemTemplate = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.ReplaceTopicTokens(new StringBuilder(itemTemplate), topic, this.PortalSettings, this.MainSettings, new Services.URLNavigator().NavigationManager(), this.ForumUser, this.Request.Url, this.Request.RawUrl).ToString();
                        ((LiteralControl)repeaterItemEventArgs.Item.Controls[0]).Text = itemTemplate;
                    }
                }
                catch (Exception ex)
                {
                    Exceptions.LogException(ex);
                }
            }
        }

        private void TopicRepeaterOnItemCreated(object sender, RepeaterItemEventArgs repeaterItemEventArgs)
        {
            this.RepeaterOnItemCreated(sender, repeaterItemEventArgs);
        }

        private void RepeaterOnItemCreated(object sender, RepeaterItemEventArgs repeaterItemEventArgs)
        {
            this.currentRow = repeaterItemEventArgs.Item.DataItem is DataRowView dataRowView ? dataRowView.Row : null;
        }

        #endregion

        #region Private Methods

        private void BindPosts(string sort = "ASC")
        {
            this.pageSize = (this.UserId > 0) ? this.UserDefaultPageSize : this.MainSettings.PageSize;

            if (this.pageSize < 5)
            {
                this.pageSize = 10;
            }

            this.rowIndex = (this.PageId - 1) * this.pageSize;

            var db = new Data.Common();
            var forumIds = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForumsForUser(this.ForumUser.UserRoles, this.PortalId, this.ForumModuleId, "CanRead");

            if (this.Request.Params[ParamKeys.GridType] != null)
            {
                var gview = Utilities.XSSFilter(this.Request.Params[ParamKeys.GridType]).ToLowerInvariant();
                var timeFrame = Utilities.SafeConvertInt(this.Request.Params[ParamKeys.TimeSpan], 1440);
                switch (gview)
                {
                    case GridTypes.NotRead:

                        if (this.UserId != -1)
                        {
                            this.lblHeader.Text = this.GetSharedResource("[RESX:NotRead]");
                            this.dtResults = db.UI_NotReadView(this.PortalId, this.ForumModuleId, this.UserId, this.rowIndex, this.pageSize, sort, forumIds).Tables[0];
                            if (this.dtResults.Rows.Count > 0)
                            {
                                this.rowCount = this.dtResults.Rows[0].GetInt("RecordCount");
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
                        this.dtResults = db.UI_Announcements(this.PortalId, this.ForumModuleId, this.UserId, this.rowIndex, this.pageSize, sort, forumIds).Tables[0];
                        if (this.dtResults.Rows.Count > 0)
                        {
                            this.rowCount = this.dtResults.Rows[0].GetInt("RecordCount");
                        }

                        break;

                    case GridTypes.Unresolved:

                        this.lblHeader.Text = this.GetSharedResource("[RESX:Unresolved]");
                        this.dtResults = db.UI_Unresolved(this.PortalId, this.ForumModuleId, this.UserId, this.rowIndex, this.pageSize, sort, forumIds).Tables[0];
                        if (this.dtResults.Rows.Count > 0)
                        {
                            this.rowCount = this.dtResults.Rows[0].GetInt("RecordCount");
                        }

                        break;
                    case GridTypes.Unanswered:

                        this.lblHeader.Text = this.GetSharedResource("[RESX:Unanswered]");
                        this.dtResults = db.UI_UnansweredView(this.PortalId, this.ForumModuleId, this.UserId, this.rowIndex, this.pageSize, sort, forumIds).Tables[0];
                        if (this.dtResults.Rows.Count > 0)
                        {
                            this.rowCount = this.dtResults.Rows[0].GetInt("RecordCount");
                        }

                        break;
                    case GridTypes.Tags:

                        var tagId = -1;
                        if (this.Request.QueryString[ParamKeys.Tags] != null && SimulateIsNumeric.IsNumeric(this.Request.QueryString[ParamKeys.Tags]))
                        {
                            tagId = int.Parse(this.Request.QueryString[ParamKeys.Tags]);
                        }

                        this.lblHeader.Text = this.GetSharedResource("[RESX:Tags]");
                        this.dtResults = db.UI_TagsView(this.PortalId, this.ForumModuleId, this.UserId, this.rowIndex, this.pageSize, sort, forumIds, tagId).Tables[0];
                        if (this.dtResults.Rows.Count > 0)
                        {
                            this.rowCount = this.dtResults.Rows[0].GetInt("RecordCount");
                        }

                        break;

                    case GridTypes.MyTopics:

                        if (this.UserId != -1)
                        {
                            this.lblHeader.Text = this.GetSharedResource("[RESX:MyTopics]");
                            this.dtResults = db.UI_MyTopicsView(this.PortalId, this.ForumModuleId, this.UserId, this.rowIndex, this.pageSize, sort, forumIds).Tables[0];
                            if (this.dtResults.Rows.Count > 0)
                            {
                                this.rowCount = this.dtResults.Rows[0].GetInt("RecordCount");
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

                        if (timeFrame < 15 | timeFrame > 80640)
                        {
                            timeFrame = 1440;
                        }

                        this.drpTimeFrame.Visible = true;
                        this.drpTimeFrame.SelectedIndex = this.drpTimeFrame.Items.IndexOf(this.drpTimeFrame.Items.FindByValue(timeFrame.ToString()));
                        this.dtResults = db.UI_ActiveView(this.PortalId, this.ForumModuleId, this.UserId, this.rowIndex, this.pageSize, sort, timeFrame, forumIds).Tables[0];
                        if (this.dtResults.Rows.Count > 0)
                        {
                            this.rowCount = Convert.ToInt32(this.dtResults.Rows[0]["RecordCount"]);
                        }

                        break;

                    case GridTypes.MostLiked:

                        this.lblHeader.Text = this.GetSharedResource("[RESX:MostLiked]");
                        if (timeFrame < 15 | timeFrame > 80640)
                        {
                            timeFrame = 1440;
                        }

                        this.drpTimeFrame.Visible = true;
                        this.drpTimeFrame.SelectedIndex = this.drpTimeFrame.Items.IndexOf(this.drpTimeFrame.Items.FindByValue(timeFrame.ToString()));
                        this.dtResults = db.UI_MostLiked(this.PortalId, this.ForumModuleId, this.UserId, this.rowIndex, this.pageSize, sort, timeFrame, forumIds).Tables[0];
                        if (this.dtResults.Rows.Count > 0)
                        {
                            this.rowCount = this.dtResults.Rows[0].GetInt("RecordCount");
                        }

                        break;

                    case GridTypes.MostReplies:

                        this.lblHeader.Text = this.GetSharedResource("[RESX:MostReplies]");

                        if (timeFrame < 15 | timeFrame > 80640)
                        {
                            timeFrame = 1440;
                        }

                        this.drpTimeFrame.Visible = true;
                        this.drpTimeFrame.SelectedIndex = this.drpTimeFrame.Items.IndexOf(this.drpTimeFrame.Items.FindByValue(timeFrame.ToString()));
                        this.dtResults = db.UI_MostReplies(this.PortalId, this.ForumModuleId, this.UserId, this.rowIndex, this.pageSize, sort, timeFrame, forumIds).Tables[0];
                        if (this.dtResults.Rows.Count > 0)
                        {
                            this.rowCount = Convert.ToInt32(this.dtResults.Rows[0]["RecordCount"]);
                        }

                        break;

                    default:
                        this.Response.Redirect(Utilities.NavigateURL(this.TabId), false);
                        this.Context.ApplicationInstance.CompleteRequest();
                        break;
                }

                if (this.MainSettings.UseSkinBreadCrumb)
                {
                    Environment.UpdateBreadCrumb(this.Page.Controls, $"<a href=\"{Utilities.NavigateURL(this.TabId, string.Empty, new[] { ParamKeys.ViewType + $"={Views.Grid}", $"{ParamKeys.GridType}={gview}" })}\">{this.lblHeader.Text}</a>");
                }

                var tempVar = this.BasePage;
                Environment.UpdateMeta(ref tempVar, "[VALUE] - " + this.lblHeader.Text, "[VALUE]", "[VALUE]");
            }

            if (this.dtResults != null && this.dtResults.Rows.Count > 0)
            {
                this.litRecordCount.Text = string.Format(this.GetSharedResource("[RESX:SearchRecords]"), this.rowIndex + 1, this.rowIndex + this.dtResults.Rows.Count, this.rowCount);

                this.pnlMessage.Visible = false;

                try
                {
                    this.rptTopics.Visible = true;
                    this.rptTopics.DataSource = this.dtResults;
                    this.rptTopics.DataBind();
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
            if (this.Request.Params[ParamKeys.Sort] != null)
            {
                @params = new[] { $"{ParamKeys.GridType}={this.Request.Params[ParamKeys.GridType]}", $"{ParamKeys.Sort}={this.Request.Params[ParamKeys.Sort]}", "afcol=" + this.Request.Params["afcol"], };
            }
            else if (this.Request.Params[ParamKeys.TimeSpan] != null)
            {
                @params = new[] { $"{ParamKeys.GridType}={this.Request.Params[ParamKeys.GridType]}", $"{ParamKeys.TimeSpan}={this.Request.Params[ParamKeys.TimeSpan]}", };
            }
            else
            {
                @params = new[] { $"{ParamKeys.GridType}={this.Request.Params[ParamKeys.GridType]}", };
            }

            pager.PageCount = intPages;
            pager.CurrentPage = this.PageId;
            pager.TabID = this.TabId;
            pager.ForumID = this.ForumId;
            pager.PageText = Utilities.GetSharedResource("[RESX:Page]");
            pager.OfText = Utilities.GetSharedResource("[RESX:PageOf]");
            pager.View = Views.Grid;

            pager.PageMode = Modules.ActiveForums.Controls.PagerNav.Mode.Links;

            if (this.MainSettings.URLRewriteEnabled)
            {
                if (!string.IsNullOrEmpty(this.MainSettings.PrefixURLBase))
                {
                    pager.BaseURL = "/" + this.MainSettings.PrefixURLBase;
                }

                if (!string.IsNullOrEmpty(this.MainSettings.PrefixURLOther))
                {
                    pager.BaseURL += "/" + this.MainSettings.PrefixURLOther;
                }

                pager.BaseURL += "/" + this.Request.Params[ParamKeys.GridType] + "/";
            }

            pager.Params = @params;
        }

        #endregion

        public string GetArrowPath()
        {
            string theme = this.Page.ResolveUrl(this.MainSettings.ThemeLocation + "/images/miniarrow_down.png");
            return theme;
        }

        #region "Deprecated Methods"
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
        public string GetMiniPager() => MiniPager.GetMiniPager(this.currentRow, this.TabId, this.SocialGroupId, this.pageSize);
        #endregion "Deprecated Methods"
    }
}
