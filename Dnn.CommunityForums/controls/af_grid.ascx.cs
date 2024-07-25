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
        
        private int _rowCount;
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

            foreach (ListItem timeFrameItem in drpTimeFrame.Items)
            {
                timeFrameItem.Text = GetSharedResource($"ActiveTopics-{timeFrameItem.Value}min.Text");
            }

            _currentTheme = MainSettings.Theme;

            drpTimeFrame.SelectedIndexChanged += DrpTimeFrameSelectedIndexChanged;
            btnMarkRead.ServerClick += BtnMarkReadClick;

            rptTopics.ItemCreated += RepeaterOnItemCreated;

            var sortDirection = Request.Params[ParamKeys.Sort] ?? SortOptions.Descending;

            BindPosts(sortDirection);
        }

        private void DrpTimeFrameSelectedIndexChanged(object sender, EventArgs e)
        {
            var timeframe = Utilities.SafeConvertInt(drpTimeFrame.SelectedItem.Value, 1440);
            Response.Redirect(Utilities.NavigateURL(TabId, string.Empty, new[] { ParamKeys.ViewType + $"={Views.Grid}", $"{ParamKeys.GridType}=" + Request.Params[$"{ParamKeys.GridType}"], $"{ParamKeys.TimeSpan}={timeframe}" }));
        }

        private void BtnMarkReadClick(object sender, EventArgs e)
        {
            if(UserId >= 0)
            {
                DataProvider.Instance().Utility_MarkAllRead(ForumModuleId, UserId, 0);
            }

            Response.Redirect(Utilities.NavigateURL(TabId, string.Empty, new[] { ParamKeys.ViewType + $"={Views.Grid}", $"{ParamKeys.GridType}={GridTypes.NotRead}" }));
        }

        private void RepeaterOnItemCreated(object sender, RepeaterItemEventArgs repeaterItemEventArgs)
        {
            var dataRowView = repeaterItemEventArgs.Item.DataItem as DataRowView;

            if (dataRowView == null)
            {
                return;
            }

            _currentRow = dataRowView.Row;
        }

        #endregion
        
        #region Private Methods
        
        private void BindPosts(string sort = "ASC")
        {
            _pageSize = MainSettings.PageSize;
            
            if (UserId > 0)
            {
                _pageSize = UserDefaultPageSize;
            }

            if (_pageSize < 5)
            {
                _pageSize = 10;
            }

            _rowIndex = (PageId == 1) ? 0 : ((PageId * _pageSize) - _pageSize);

            var db = new Data.Common();
            var forumIds = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForumsForUser(ForumUser.UserRoles, PortalId, ForumModuleId, "CanRead");
            
            var sCrumb = "<a href=\"" + Utilities.NavigateURL(TabId, "", new[] { ParamKeys.ViewType + $"={Views.Grid}", $"{ParamKeys.GridType}=xxx" }) + "\">yyyy</a>";
            sCrumb = sCrumb.Replace("xxx", "{0}").Replace("yyyy", "{1}");
            
            if (Request.Params[ParamKeys.GridType] != null)
            {
                var gview = Utilities.XSSFilter(Request.Params[ParamKeys.GridType]).ToLowerInvariant(); 
                var timeFrame = Utilities.SafeConvertInt(Request.Params[ParamKeys.TimeSpan], 1440);
                switch (gview)
                {
                    case GridTypes.NotRead:

                        if (UserId != -1)
                        {
                            lblHeader.Text = GetSharedResource("[RESX:NotRead]");
                            _dtResults = db.UI_NotReadView(PortalId, ForumModuleId, UserId, _rowIndex, _pageSize, sort, forumIds).Tables[0];
                            if (_dtResults.Rows.Count > 0)
                            {
                                _rowCount = _dtResults.Rows[0].GetInt("RecordCount");
                                btnMarkRead.Visible = true;
                                btnMarkRead.InnerText = GetSharedResource("[RESX:MarkAllRead]");
                            }
                        }
                        else
                            Response.Redirect(Utilities.NavigateURL(TabId), true);
                        break;

                    case GridTypes.Announcements:

                        lblHeader.Text = GetSharedResource("[RESX:Announcements]");
                        _dtResults = db.UI_Announcements(PortalId, ForumModuleId, UserId, _rowIndex, _pageSize, sort, forumIds).Tables[0];
                        if (_dtResults.Rows.Count > 0)
                        {
                            _rowCount = _dtResults.Rows[0].GetInt("RecordCount");
                        }

                        break;

                    case GridTypes.Unresolved:

                        lblHeader.Text = GetSharedResource("[RESX:Unresolved]");
                        _dtResults = db.UI_Unresolved(PortalId, ForumModuleId, UserId, _rowIndex, _pageSize, sort, forumIds).Tables[0];
                        if (_dtResults.Rows.Count > 0)
                        {
                            _rowCount = _dtResults.Rows[0].GetInt("RecordCount");
                        }

                        break;
                    case GridTypes.Unanswered:

                        lblHeader.Text = GetSharedResource("[RESX:Unanswered]");
                        _dtResults = db.UI_UnansweredView(PortalId, ForumModuleId, UserId, _rowIndex, _pageSize, sort, forumIds).Tables[0];
                        if (_dtResults.Rows.Count > 0)
                        {
                            _rowCount = _dtResults.Rows[0].GetInt("RecordCount");
                        }

                        break;
                    case GridTypes.Tags:

                        var tagId = -1;
                        if (Request.QueryString[ParamKeys.Tags] != null && SimulateIsNumeric.IsNumeric(Request.QueryString[ParamKeys.Tags]))
                        {
                            tagId = int.Parse(Request.QueryString[ParamKeys.Tags]);
                        }

                        lblHeader.Text = GetSharedResource("[RESX:Tags]");
                        _dtResults = db.UI_TagsView(PortalId, ForumModuleId, UserId, _rowIndex, _pageSize, sort, forumIds, tagId).Tables[0];
                        if (_dtResults.Rows.Count > 0)
                        {
                            _rowCount = _dtResults.Rows[0].GetInt("RecordCount");
                        }

                        break;

                    case GridTypes.MyTopics:

                        if (UserId != -1)
                        {
                            lblHeader.Text = GetSharedResource("[RESX:MyTopics]");
                            _dtResults = db.UI_MyTopicsView(PortalId, ForumModuleId, UserId, _rowIndex, _pageSize, sort, forumIds).Tables[0];
                            if (_dtResults.Rows.Count > 0)
                            {
                                _rowCount = _dtResults.Rows[0].GetInt("RecordCount");
                            }
                        }
                        else
                            Response.Redirect(Utilities.NavigateURL(TabId), true);

                        break;

                    case GridTypes.ActiveTopics:

                        lblHeader.Text = GetSharedResource("[RESX:ActiveTopics]");

                        if (timeFrame < 15 | timeFrame > 80640)
                        {
                            timeFrame = 1440;
                        }

                        drpTimeFrame.Visible = true;
                        drpTimeFrame.SelectedIndex = drpTimeFrame.Items.IndexOf(drpTimeFrame.Items.FindByValue(timeFrame.ToString()));
                        _dtResults = db.UI_ActiveView(PortalId, ForumModuleId, UserId, _rowIndex, _pageSize, sort, timeFrame, forumIds).Tables[0];
                        if (_dtResults.Rows.Count > 0)
                        {
                            _rowCount = Convert.ToInt32(_dtResults.Rows[0]["RecordCount"]);
                        }

                        break;

                    case GridTypes.MostLiked:

                        lblHeader.Text = GetSharedResource("[RESX:MostLiked]");
                        if (timeFrame < 15 | timeFrame > 80640)
                        {
                            timeFrame = 1440;
                        }

                        drpTimeFrame.Visible = true;
                        drpTimeFrame.SelectedIndex = drpTimeFrame.Items.IndexOf(drpTimeFrame.Items.FindByValue(timeFrame.ToString()));
                        _dtResults = db.UI_MostLiked(PortalId, ForumModuleId, UserId, _rowIndex, _pageSize, sort, timeFrame, forumIds).Tables[0];
                        if (_dtResults.Rows.Count > 0)
                        {
                            _rowCount = _dtResults.Rows[0].GetInt("RecordCount");
                        }

                        break;

                    case GridTypes.MostReplies:

                        lblHeader.Text = GetSharedResource("[RESX:MostReplies]");

                        if (timeFrame < 15 | timeFrame > 80640)
                        {
                            timeFrame = 1440;
                        }

                        drpTimeFrame.Visible = true;
                        drpTimeFrame.SelectedIndex = drpTimeFrame.Items.IndexOf(drpTimeFrame.Items.FindByValue(timeFrame.ToString()));
                        _dtResults = db.UI_MostReplies(PortalId, ForumModuleId, UserId, _rowIndex, _pageSize, sort, timeFrame, forumIds).Tables[0];
                        if (_dtResults.Rows.Count > 0)
                        {
                            _rowCount = Convert.ToInt32(_dtResults.Rows[0]["RecordCount"]);
                        }

                        break;

                    default:
                        Response.Redirect(Utilities.NavigateURL(TabId), true);
                        break;
                }

                sCrumb = string.Format(sCrumb, gview, lblHeader.Text);
               
                if (MainSettings.UseSkinBreadCrumb)
                {
                    Environment.UpdateBreadCrumb(Page.Controls, sCrumb);
                }

                var tempVar = BasePage;
                Environment.UpdateMeta(ref tempVar, "[VALUE] - " + lblHeader.Text, "[VALUE]", "[VALUE]");
            }

            if (_dtResults != null && _dtResults.Rows.Count > 0)
            {
                litRecordCount.Text = string.Format(GetSharedResource("[RESX:SearchRecords]"), _rowIndex + 1, _rowIndex + _dtResults.Rows.Count, _rowCount);

                pnlMessage.Visible = false;

                try
                {
                    rptTopics.Visible = true;
                    rptTopics.DataSource = _dtResults;
                    rptTopics.DataBind();
                    BuildPager(PagerTop);
                    BuildPager(PagerBottom);
                }
                catch (Exception ex)
                {
                    litMessage.Text = ex.Message;
                    pnlMessage.Visible = true;
                    rptTopics.Visible = false;
                }
            }
            else
            {
                litMessage.Text = GetSharedResource("[RESX:SearchNoResults]");
                pnlMessage.Visible = true;
            }

        }

        private void BuildPager(PagerNav pager)
        {
            if (pager == null)
            {
                return;
            }

            var intPages = Convert.ToInt32(Math.Ceiling(_rowCount/(double) _pageSize));

            string[] @params;
            if (Request.Params[ParamKeys.Sort] != null)
                @params = new[] {
                    $"{ParamKeys.GridType}={Request.Params[ParamKeys.GridType]}",
                    $"{ParamKeys.Sort}={Request.Params[ParamKeys.Sort]}",
                    "afcol=" + Request.Params["afcol"]
                };
            else if (Request.Params[ParamKeys.TimeSpan] != null)
                @params = new[] {
                    $"{ParamKeys.GridType}={Request.Params[ParamKeys.GridType]}",
                    $"{ParamKeys.TimeSpan}={Request.Params[ParamKeys.TimeSpan]}"
                };
            else
                @params = new[] {
                    $"{ParamKeys.GridType}={Request.Params[ParamKeys.GridType]}" 
                };

            pager.PageCount = intPages;
            pager.CurrentPage = PageId;
            pager.TabID = TabId;
            pager.ForumID = ForumId;
            pager.PageText = Utilities.GetSharedResource("[RESX:Page]");
            pager.OfText = Utilities.GetSharedResource("[RESX:PageOf]");
            pager.View = Views.Grid;

            pager.PageMode = Modules.ActiveForums.Controls.PagerNav.Mode.Links;

            if (MainSettings.URLRewriteEnabled)
            {
                if (!(string.IsNullOrEmpty(MainSettings.PrefixURLBase)))
                {
                    pager.BaseURL = "/" + MainSettings.PrefixURLBase;
                }

                if (!(string.IsNullOrEmpty(MainSettings.PrefixURLOther)))
                {
                    pager.BaseURL += "/" + MainSettings.PrefixURLOther;
                }

                pager.BaseURL += "/" + Request.Params[ParamKeys.GridType] + "/";
            }

            pager.Params = @params;
        }

        #endregion
        
        #region Public Methods

        public string GetForumUrl()
        {
            if (_currentRow == null)
            {
                return null;
            }

            int ForumId = Utilities.SafeConvertInt(_currentRow["ForumId"].ToString());
            int ForumGroupId = Utilities.SafeConvertInt(_currentRow["ForumGroupId"].ToString());
            DotNetNuke.Modules.ActiveForums.Entities.ForumInfo ForumInfo = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(ForumId, ForumModuleId);
            return new ControlUtils().BuildUrl(TabId, ForumModuleId, ForumInfo.ForumGroup.PrefixURL, ForumInfo.PrefixURL, ForumInfo.ForumGroupId, ForumInfo.ForumID, -1, -1, string.Empty, 1, -1, SocialGroupId);
        }

        public string GetThreadUrl()
        {
            if (_currentRow == null)
            {
                return null;
            }

            int ForumId = Utilities.SafeConvertInt(_currentRow["ForumId"].ToString());
            int ForumGroupId = Utilities.SafeConvertInt(_currentRow["ForumGroupId"].ToString());
            int TopicId = Utilities.SafeConvertInt(_currentRow["TopicId"].ToString());
            DotNetNuke.Modules.ActiveForums.Entities.ForumInfo ForumInfo = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(ForumId, ForumModuleId);
            return new ControlUtils().BuildUrl(TabId, ForumModuleId, ForumInfo.ForumGroup.PrefixURL, ForumInfo.PrefixURL, ForumGroupId, ForumId, TopicId, _currentRow["TopicUrl"].ToString(), -1, -1, string.Empty, 1, -1, SocialGroupId);

        }

        public string GetLastRead()
        {
            if (_currentRow == null)
            {
                return null;
            }

            int ForumId = Utilities.SafeConvertInt(_currentRow["ForumId"].ToString());
            int ForumGroupId = Utilities.SafeConvertInt(_currentRow["ForumGroupId"].ToString());
            int TopicId = Utilities.SafeConvertInt(_currentRow["TopicId"].ToString());
            int userLastRead = Utilities.SafeConvertInt(_currentRow["UserLastTopicRead"].ToString());
            DotNetNuke.Modules.ActiveForums.Entities.ForumInfo ForumInfo = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(ForumId, ForumModuleId);
            return new ControlUtils().BuildUrl(TabId, ForumModuleId, ForumInfo.ForumGroup.PrefixURL, ForumInfo.PrefixURL, ForumGroupId, ForumId, TopicId, _currentRow["TopicUrl"].ToString(), -1, -1, string.Empty, 1, userLastRead, SocialGroupId);
             
        }

        public string GetArrowPath()
        {
            string theme = Page.ResolveUrl(MainSettings.ThemeLocation + "/images/miniarrow_down.png");
            return theme;
        }

        public string GetPostTime()
        {
            return (_currentRow == null) ? null : Utilities.GetUserFriendlyDateTimeString(Convert.ToDateTime(_currentRow["DateCreated"]), ForumModuleId, UserInfo);
        }

        public string GetAuthor()
        {
            if (_currentRow == null)
            {
                return null;
            }

            var userId = Convert.ToInt32(_currentRow["AuthorId"]);
            var userName = _currentRow["AuthorUserName"].ToString();
            var firstName = _currentRow["AuthorFirstName"].ToString();
            var lastName = _currentRow["AuthorLastName"].ToString();
            var displayName = _currentRow["AuthorDisplayName"].ToString();
            return UserProfiles.GetDisplayName(PortalSettings, ForumModuleId, true, false, ForumUser.IsAdmin, userId, userName, firstName, lastName, displayName);
        }

        public string GetLastPostAuthor()
        {
            if (_currentRow == null)
            {
                return null;
            }

            var userId = Convert.ToInt32(_currentRow["LastReplyAuthorId"]);
            var userName = _currentRow["LastReplyUserName"].ToString();
            var firstName = _currentRow["LastReplyFirstName"].ToString();
            var lastName = _currentRow["LastReplyLastName"].ToString();
            var displayName = _currentRow["LastReplyDisplayName"].ToString();
            return UserProfiles.GetDisplayName(PortalSettings, ForumModuleId, true, false, ForumUser.IsAdmin, userId, userName, firstName, lastName, displayName);
        }

        public string GetLastPostTime()
        {
            return (_currentRow == null) ? null : Utilities.GetUserFriendlyDateTimeString(Convert.ToDateTime(_currentRow["LastReplyDate"]), ForumModuleId, UserInfo); 
        }

        public string GetIcon()
        {
            return DotNetNuke.Modules.ActiveForums.Controllers.TopicController.GetTopicIcon(
                Utilities.SafeConvertInt(_currentRow["TopicId"].ToString()),
                ThemePath, 
                Utilities.SafeConvertInt(_currentRow["UserLastTopicRead"]),
                Utilities.SafeConvertInt(_currentRow["UserLastReplyRead"]));
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public string GetMiniPager() => MiniPager.GetMiniPager(_currentRow, TabId, SocialGroupId, _pageSize);

        #endregion
    }
}
