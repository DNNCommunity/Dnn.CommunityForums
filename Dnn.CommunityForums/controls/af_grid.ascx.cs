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
    using System.Data;
    using System.Web;
    using System.Web.UI.WebControls;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Modules.ActiveForums.Controls;

    public partial class af_grid : ForumBase
    {
        #region Private Members

        private int rowCount;
        private DataTable _dtResults;
        private int _pageSize = 20;
        private int _rowIndex;
        private DataRow _currentRow;
        private string _currentTheme = "_default";

        #endregion

        #region Event Handlers

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            foreach (ListItem timeFrameItem in this.drpTimeFrame.Items)
            {
                timeFrameItem.Text = this.GetSharedResource($"ActiveTopics-{timeFrameItem.Value}min.Text");
            }

            this._currentTheme = this.MainSettings.Theme;

            this.drpTimeFrame.SelectedIndexChanged += this.DrpTimeFrameSelectedIndexChanged;
            this.btnMarkRead.ServerClick += this.BtnMarkReadClick;

            this.rptTopics.ItemCreated += this.RepeaterOnItemCreated;

            var sortDirection = this.Request.Params[ParamKeys.Sort] ?? SortOptions.Descending;

            this.BindPosts(sortDirection);
        }

        private void DrpTimeFrameSelectedIndexChanged(object sender, EventArgs e)
        {
            var timeframe = Utilities.SafeConvertInt(this.drpTimeFrame.SelectedItem.Value, 1440);
            this.Response.Redirect(Utilities.NavigateURL(this.TabId, string.Empty, new[] { ParamKeys.ViewType + $"={Views.Grid}", $"{ParamKeys.GridType}=" + this.Request.Params[$"{ParamKeys.GridType}"], $"{ParamKeys.TimeSpan}={timeframe}" }));
        }

        private void BtnMarkReadClick(object sender, EventArgs e)
        {
            if(this.UserId >= 0)
            {
                DataProvider.Instance().Utility_MarkAllRead(this.ForumModuleId, this.UserId, 0);
            }

            this.Response.Redirect(Utilities.NavigateURL(this.TabId, string.Empty, new[] { ParamKeys.ViewType + $"={Views.Grid}", $"{ParamKeys.GridType}={GridTypes.NotRead}" }));
        }

        private void RepeaterOnItemCreated(object sender, RepeaterItemEventArgs repeaterItemEventArgs)
        {
            var dataRowView = repeaterItemEventArgs.Item.DataItem as DataRowView;

            if (dataRowView == null)
            {
                return;
            }

            this._currentRow = dataRowView.Row;
        }

        #endregion

        #region Private Methods

        private void BindPosts(string sort = "ASC")
        {
            this._pageSize = this.MainSettings.PageSize;

            if (this.UserId > 0)
            {
                this._pageSize = this.UserDefaultPageSize;
            }

            if (this._pageSize < 5)
            {
                this._pageSize = 10;
            }

            this._rowIndex = (this.PageId == 1) ? 0 : ((this.PageId * this._pageSize) - this._pageSize);

            var db = new Data.Common();
            var forumIds = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForumsForUser(this.ForumUser.UserRoles, this.PortalId, this.ForumModuleId, "CanRead");

            var sCrumb = "<a href=\"" + Utilities.NavigateURL(this.TabId, "", new[] { ParamKeys.ViewType + $"={Views.Grid}", $"{ParamKeys.GridType}=xxx" }) + "\">yyyy</a>";
            sCrumb = sCrumb.Replace("xxx", "{0}").Replace("yyyy", "{1}");

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
                            this._dtResults = db.UI_NotReadView(this.PortalId, this.ForumModuleId, this.UserId, this._rowIndex, this._pageSize, sort, forumIds).Tables[0];
                            if (this._dtResults.Rows.Count > 0)
                            {
                                this.rowCount = this._dtResults.Rows[0].GetInt("RecordCount");
                                this.btnMarkRead.Visible = true;
                                this.btnMarkRead.InnerText = this.GetSharedResource("[RESX:MarkAllRead]");
                            }
                        }
                        else
                            this.Response.Redirect(Utilities.NavigateURL(this.TabId), true);
                        break;

                    case GridTypes.Announcements:

                        this.lblHeader.Text = this.GetSharedResource("[RESX:Announcements]");
                        this._dtResults = db.UI_Announcements(this.PortalId, this.ForumModuleId, this.UserId, this._rowIndex, this._pageSize, sort, forumIds).Tables[0];
                        if (this._dtResults.Rows.Count > 0)
                        {
                            this.rowCount = this._dtResults.Rows[0].GetInt("RecordCount");
                        }

                        break;

                    case GridTypes.Unresolved:

                        this.lblHeader.Text = this.GetSharedResource("[RESX:Unresolved]");
                        this._dtResults = db.UI_Unresolved(this.PortalId, this.ForumModuleId, this.UserId, this._rowIndex, this._pageSize, sort, forumIds).Tables[0];
                        if (this._dtResults.Rows.Count > 0)
                        {
                            this.rowCount = this._dtResults.Rows[0].GetInt("RecordCount");
                        }

                        break;
                    case GridTypes.Unanswered:

                        this.lblHeader.Text = this.GetSharedResource("[RESX:Unanswered]");
                        this._dtResults = db.UI_UnansweredView(this.PortalId, this.ForumModuleId, this.UserId, this._rowIndex, this._pageSize, sort, forumIds).Tables[0];
                        if (this._dtResults.Rows.Count > 0)
                        {
                            this.rowCount = this._dtResults.Rows[0].GetInt("RecordCount");
                        }

                        break;
                    case GridTypes.Tags:

                        var tagId = -1;
                        if (this.Request.QueryString[ParamKeys.Tags] != null && SimulateIsNumeric.IsNumeric(this.Request.QueryString[ParamKeys.Tags]))
                        {
                            tagId = int.Parse(this.Request.QueryString[ParamKeys.Tags]);
                        }

                        this.lblHeader.Text = this.GetSharedResource("[RESX:Tags]");
                        this._dtResults = db.UI_TagsView(this.PortalId, this.ForumModuleId, this.UserId, this._rowIndex, this._pageSize, sort, forumIds, tagId).Tables[0];
                        if (this._dtResults.Rows.Count > 0)
                        {
                            this.rowCount = this._dtResults.Rows[0].GetInt("RecordCount");
                        }

                        break;

                    case GridTypes.MyTopics:

                        if (this.UserId != -1)
                        {
                            this.lblHeader.Text = this.GetSharedResource("[RESX:MyTopics]");
                            this._dtResults = db.UI_MyTopicsView(this.PortalId, this.ForumModuleId, this.UserId, this._rowIndex, this._pageSize, sort, forumIds).Tables[0];
                            if (this._dtResults.Rows.Count > 0)
                            {
                                this.rowCount = this._dtResults.Rows[0].GetInt("RecordCount");
                            }
                        }
                        else
                            this.Response.Redirect(Utilities.NavigateURL(this.TabId), true);

                        break;

                    case GridTypes.ActiveTopics:

                        this.lblHeader.Text = this.GetSharedResource("[RESX:ActiveTopics]");

                        if (timeFrame < 15 | timeFrame > 80640)
                        {
                            timeFrame = 1440;
                        }

                        this.drpTimeFrame.Visible = true;
                        this.drpTimeFrame.SelectedIndex = this.drpTimeFrame.Items.IndexOf(this.drpTimeFrame.Items.FindByValue(timeFrame.ToString()));
                        this._dtResults = db.UI_ActiveView(this.PortalId, this.ForumModuleId, this.UserId, this._rowIndex, this._pageSize, sort, timeFrame, forumIds).Tables[0];
                        if (this._dtResults.Rows.Count > 0)
                        {
                            this.rowCount = Convert.ToInt32(this._dtResults.Rows[0]["RecordCount"]);
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
                        this._dtResults = db.UI_MostLiked(this.PortalId, this.ForumModuleId, this.UserId, this._rowIndex, this._pageSize, sort, timeFrame, forumIds).Tables[0];
                        if (this._dtResults.Rows.Count > 0)
                        {
                            this.rowCount = this._dtResults.Rows[0].GetInt("RecordCount");
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
                        this._dtResults = db.UI_MostReplies(this.PortalId, this.ForumModuleId, this.UserId, this._rowIndex, this._pageSize, sort, timeFrame, forumIds).Tables[0];
                        if (this._dtResults.Rows.Count > 0)
                        {
                            this.rowCount = Convert.ToInt32(this._dtResults.Rows[0]["RecordCount"]);
                        }

                        break;

                    default:
                        this.Response.Redirect(Utilities.NavigateURL(this.TabId), true);
                        break;
                }

                sCrumb = string.Format(sCrumb, gview, this.lblHeader.Text);

                if (this.MainSettings.UseSkinBreadCrumb)
                {
                    Environment.UpdateBreadCrumb(this.Page.Controls, sCrumb);
                }

                var tempVar = this.BasePage;
                Environment.UpdateMeta(ref tempVar, "[VALUE] - " + this.lblHeader.Text, "[VALUE]", "[VALUE]");
            }

            if (this._dtResults != null && this._dtResults.Rows.Count > 0)
            {
                this.litRecordCount.Text = string.Format(this.GetSharedResource("[RESX:SearchRecords]"), this._rowIndex + 1, this._rowIndex + this._dtResults.Rows.Count, this.rowCount);

                this.pnlMessage.Visible = false;

                try
                {
                    this.rptTopics.Visible = true;
                    this.rptTopics.DataSource = this._dtResults;
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

        private void BuildPager(PagerNav pager)
        {
            if (pager == null)
            {
                return;
            }

            var intPages = Convert.ToInt32(Math.Ceiling(this.rowCount/(double) this._pageSize));

            string[] @params;
            if (this.Request.Params[ParamKeys.Sort] != null)
                @params = new[] {
                    $"{ParamKeys.GridType}={this.Request.Params[ParamKeys.GridType]}",
                    $"{ParamKeys.Sort}={this.Request.Params[ParamKeys.Sort]}",
                    "afcol=" + this.Request.Params["afcol"]
                };
            else if (this.Request.Params[ParamKeys.TimeSpan] != null)
                @params = new[] {
                    $"{ParamKeys.GridType}={this.Request.Params[ParamKeys.GridType]}",
                    $"{ParamKeys.TimeSpan}={this.Request.Params[ParamKeys.TimeSpan]}"
                };
            else
                @params = new[] {
                    $"{ParamKeys.GridType}={this.Request.Params[ParamKeys.GridType]}"
                };

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

        #region Public Methods

        public string GetForumUrl()
        {
            if (this._currentRow == null)
            {
                return null;
            }

            int ForumId = Utilities.SafeConvertInt(this._currentRow["ForumId"].ToString());
            int ForumGroupId = Utilities.SafeConvertInt(this._currentRow["ForumGroupId"].ToString());
            DotNetNuke.Modules.ActiveForums.Entities.ForumInfo ForumInfo = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(ForumId, this.ForumModuleId);
            return new ControlUtils().BuildUrl(this.TabId, this.ForumModuleId, ForumInfo.ForumGroup.PrefixURL, ForumInfo.PrefixURL, ForumInfo.ForumGroupId, ForumInfo.ForumID, -1, -1, string.Empty, 1, -1, this.SocialGroupId);
        }

        public string GetThreadUrl()
        {
            if (this._currentRow == null)
            {
                return null;
            }

            int ForumId = Utilities.SafeConvertInt(this._currentRow["ForumId"].ToString());
            int ForumGroupId = Utilities.SafeConvertInt(this._currentRow["ForumGroupId"].ToString());
            int TopicId = Utilities.SafeConvertInt(this._currentRow["TopicId"].ToString());
            DotNetNuke.Modules.ActiveForums.Entities.ForumInfo ForumInfo = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(ForumId, this.ForumModuleId);
            return new ControlUtils().BuildUrl(this.TabId, this.ForumModuleId, ForumInfo.ForumGroup.PrefixURL, ForumInfo.PrefixURL, ForumGroupId, ForumId, TopicId, this._currentRow["TopicUrl"].ToString(), -1, -1, string.Empty, 1, -1, this.SocialGroupId);

        }

        public string GetLastRead()
        {
            if (this._currentRow == null)
            {
                return null;
            }

            int ForumId = Utilities.SafeConvertInt(this._currentRow["ForumId"].ToString());
            int ForumGroupId = Utilities.SafeConvertInt(this._currentRow["ForumGroupId"].ToString());
            int TopicId = Utilities.SafeConvertInt(this._currentRow["TopicId"].ToString());
            int userLastRead = Utilities.SafeConvertInt(this._currentRow["UserLastTopicRead"].ToString());
            DotNetNuke.Modules.ActiveForums.Entities.ForumInfo ForumInfo = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(ForumId, this.ForumModuleId);
            return new ControlUtils().BuildUrl(this.TabId, this.ForumModuleId, ForumInfo.ForumGroup.PrefixURL, ForumInfo.PrefixURL, ForumGroupId, ForumId, TopicId, this._currentRow["TopicUrl"].ToString(), -1, -1, string.Empty, 1, userLastRead, this.SocialGroupId);

        }

        public string GetArrowPath()
        {
            string theme = this.Page.ResolveUrl(this.MainSettings.ThemeLocation + "/images/miniarrow_down.png");
            return theme;
        }

        public string GetPostTime()
        {
            return (this._currentRow == null) ? null : Utilities.GetUserFriendlyDateTimeString(Convert.ToDateTime(this._currentRow["DateCreated"]), this.ForumModuleId, this.UserInfo);
        }

        public string GetAuthor()
        {
            if (this._currentRow == null)
            {
                return null;
            }

            var userId = Convert.ToInt32(this._currentRow["AuthorId"]);
            var userName = this._currentRow["AuthorUserName"].ToString();
            var firstName = this._currentRow["AuthorFirstName"].ToString();
            var lastName = this._currentRow["AuthorLastName"].ToString();
            var displayName = this._currentRow["AuthorDisplayName"].ToString();
            return UserProfiles.GetDisplayName(this.PortalSettings, this.ForumModuleId, true, false, this.ForumUser.IsAdmin, userId, userName, firstName, lastName, displayName);
        }

        public string GetLastPostAuthor()
        {
            if (this._currentRow == null)
            {
                return null;
            }

            var userId = Convert.ToInt32(this._currentRow["LastReplyAuthorId"]);
            var userName = this._currentRow["LastReplyUserName"].ToString();
            var firstName = this._currentRow["LastReplyFirstName"].ToString();
            var lastName = this._currentRow["LastReplyLastName"].ToString();
            var displayName = this._currentRow["LastReplyDisplayName"].ToString();
            return UserProfiles.GetDisplayName(this.PortalSettings, this.ForumModuleId, true, false, this.ForumUser.IsAdmin, userId, userName, firstName, lastName, displayName);
        }

        public string GetLastPostTime()
        {
            return (this._currentRow == null) ? null : Utilities.GetUserFriendlyDateTimeString(Convert.ToDateTime(this._currentRow["LastReplyDate"]), this.ForumModuleId, this.UserInfo);
        }

        public string GetIcon()
        {
            return DotNetNuke.Modules.ActiveForums.Controllers.TopicController.GetTopicIcon(
                Utilities.SafeConvertInt(this._currentRow["TopicId"].ToString()),
                this.ThemePath,
                Utilities.SafeConvertInt(this._currentRow["UserLastTopicRead"]),
                Utilities.SafeConvertInt(this._currentRow["UserLastReplyRead"]));
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public string GetMiniPager() => MiniPager.GetMiniPager(this._currentRow, this.TabId, this.SocialGroupId, this._pageSize);

        #endregion
    }
}
