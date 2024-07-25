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
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Web;
    using System.Web.UI.WebControls;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Modules.ActiveForums.Controls;

    public partial class af_search : ForumBase
    {
        #region Private Members

        private string _searchText;
        private string _tags;
        private int? _searchType;
        private int? _authorUserId;
        private string _authorUsername;
        private int? _searchColumns;
        private string _forums;
        private int? _searchDays;
        private int? _resultType;
        private int? _searchId;
        private int? _sort;

        private List<string> _parameters;

        private int _rowCount;

        private int _pageSize = 20;
        private int _rowIndex;

        private int? _searchAge;
        private int? _searchDuration;

        private DataRow _currentRow;

        #endregion

        #region Properties

        private string SearchText
        {
            get
            {
                if (this._searchText == null)
                {
                    this._searchText = this.Request.Params[SearchParamKeys.Query] + string.Empty;
                    this._searchText = Utilities.XSSFilter(this._searchText);
                    this._searchText = Utilities.StripHTMLTag(this._searchText);
                    this._searchText = Utilities.CheckSqlString(this._searchText);
                    this._searchText = this.SearchText.Replace("\"", string.Empty);
                    this._searchText = this._searchText.Trim();
                }

                return this._searchText;
            }
        }

        private string Tags
        {
            get
            {
                if (this._tags == null)
                {
                    this._tags = this.Request.Params[SearchParamKeys.Tag] + string.Empty;

                    // The tag links are generated with "aftg"
                    if (this._tags == string.Empty)
                    {
                        this._tags = this.Request.Params[ParamKeys.Tags] + string.Empty;
                    }

                    this._tags = Utilities.XSSFilter(this._tags);
                    this._tags = Utilities.StripHTMLTag(this._tags);
                    this._tags = Utilities.CheckSqlString(this._tags);
                    this._tags = this._tags.Trim();
                }

                return this._tags;
            }
        }

        private int SearchType
        {
            get
            {
                if (!this._searchType.HasValue)
                {
                    int parsedSearchType;
                    this._searchType = int.TryParse(this.Request.Params[SearchParamKeys.SearchType], out parsedSearchType) ? parsedSearchType : 0;
                }

                return this._searchType.Value;
            }
        }

        private string AuthorUsername
        {
            get
            {
                if (this._authorUsername == null)
                {
                    this._authorUsername = this.Request.Params[SearchParamKeys.Author] + string.Empty;
                    this._authorUsername = Utilities.XSSFilter(this._authorUsername);
                    this._authorUsername = Utilities.StripHTMLTag(this._authorUsername);
                    this._authorUsername = Utilities.CheckSqlString(this._authorUsername);
                    this._authorUsername = this._authorUsername.Trim();
                }

                return this._authorUsername;
            }
        }

        private int AuthorUserId
        {
            get
            {
                if (!this._authorUserId.HasValue)
                {
                    int parsedValue;
                    this._authorUserId = int.TryParse(this.Request.Params[SearchParamKeys.User], out parsedValue) ? parsedValue : 0;
                }

                return this._authorUserId.Value;
            }
        }

        private int SearchColumns
        {
            get
            {
                if (!this._searchColumns.HasValue)
                {
                    int parsedSearchColumns;
                    this._searchColumns = int.TryParse(this.Request.Params[SearchParamKeys.Columns], out parsedSearchColumns) ? parsedSearchColumns : 0;
                }

                return this._searchColumns.Value;
            }
        }

        private string Forums
        {
            get
            {
                if (this._forums == null)
                {
                    this._forums = this.Request.Params[SearchParamKeys.Forums] + string.Empty;
                    this._forums = Utilities.XSSFilter(this._forums);
                    this._forums = Utilities.StripHTMLTag(this._forums);
                    this._forums = Utilities.CheckSqlString(this._forums);
                    this._forums = this._forums.Trim();
                }

                return this._forums;
            }
        }

        private int SearchDays
        {
            get
            {
                if (!this._searchDays.HasValue)
                {
                    int parsedValue;
                    this._searchDays = int.TryParse(this.Request.Params[SearchParamKeys.TimeSpan], out parsedValue) ? parsedValue : 0;
                }

                return this._searchDays.Value;
            }
        }

        private int ResultType
        {
            get
            {
                if (!this._resultType.HasValue)
                {
                    int parsedValue;
                    this._resultType = int.TryParse(this.Request.Params[SearchParamKeys.ResultType], out parsedValue) ? parsedValue : 0;
                }

                return this._resultType.Value;
            }
        }

        private int SearchId
        {
            get
            {
                if (!this._searchId.HasValue)
                {
                    int parsedValue;
                    this._searchId = int.TryParse(this.Request.Params[SearchParamKeys.Search], out parsedValue) ? parsedValue : 0;
                }

                return this._searchId.Value;
            }
        }

        private int Sort
        {
            get
            {
                if (!this._sort.HasValue)
                {
                    int parsedValue;
                    this._sort = int.TryParse(this.Request.Params[SearchParamKeys.Sort], out parsedValue) ? parsedValue : 0;
                }

                return this._sort.Value;
            }
        }

        private List<string> Parameters
        {
            get
            {
                if (this._parameters == null)
                {
                    this._parameters = new List<string>();

                    if (!string.IsNullOrWhiteSpace(this.SearchText))
                    {
                        this._parameters.Add($"{SearchParamKeys.Query}=" + System.Web.HttpUtility.UrlEncode(this.SearchText));
                    }

                    if (!string.IsNullOrWhiteSpace(this.Tags))
                    {
                        this._parameters.Add($"{SearchParamKeys.Tag}=" + System.Web.HttpUtility.UrlEncode(this.Tags));
                    }

                    if (this.SearchId > 0)
                    {
                        this._parameters.Add($"{SearchParamKeys.Search}=" + this.SearchId);
                    }

                    if (this.SearchType > 0)
                    {
                        this._parameters.Add($"{SearchParamKeys.SearchType}=" + this.SearchType);
                    }

                    if (this.ResultType > 0)
                    {
                        this._parameters.Add($"{SearchParamKeys.ResultType}=" + this.ResultType);
                    }

                    if (this.SearchColumns > 0)
                    {
                        this._parameters.Add($"{SearchParamKeys.Columns}=" + this.SearchColumns);
                    }

                    if (this.SearchDays > 0)
                    {
                        this._parameters.Add($"{SearchParamKeys.TimeSpan}=" + this.SearchDays);
                    }

                    if (this.AuthorUserId > 0)
                    {
                        this._parameters.Add($"{SearchParamKeys.User}=" + this.AuthorUserId);
                    }

                    if (this.Sort > 0)
                    {
                        this._parameters.Add($"{SearchParamKeys.Sort}=" + this.Sort);
                    }

                    if (!string.IsNullOrWhiteSpace(this.AuthorUsername))
                    {
                        this._parameters.Add($"{SearchParamKeys.Author}=" + System.Web.HttpUtility.UrlEncode(this.AuthorUsername));
                    }

                    if (!string.IsNullOrWhiteSpace(this.Forums))
                    {
                        this._parameters.Add($"{SearchParamKeys.Forums}=" + System.Web.HttpUtility.UrlEncode(this.Forums));
                    }
                }

                return this._parameters;
            }
        }

        #endregion

        #region Event Handlers

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.rptPosts.ItemCreated += this.RepeaterOnItemCreated;
            this.rptTopics.ItemCreated += this.RepeaterOnItemCreated;

            try
            {

                if (this.Request.QueryString[Literals.GroupId] != null && SimulateIsNumeric.IsNumeric(this.Request.QueryString[Literals.GroupId]))
                {
                    this.SocialGroupId = Convert.ToInt32(this.Request.QueryString[Literals.GroupId]);
                }

                this.litSearchTitle.Text = this.GetSharedResource("[RESX:SearchTitle]");
                List<Keyword> keywords;

                // Note: Filter out any keywords that are not at least 3 characters in length

                if (this.SearchType == 2 && !string.IsNullOrWhiteSpace(this.SearchText) && this.SearchText.Trim().Length >= 3) //Exact Match
                {
                    keywords = new List<Keyword> { new Keyword { Value = "\"" + this.SearchText.Trim() + "\"" } };
                }
                else
                {
                    keywords = this.SearchText.Split(' ').Where(kw => !string.IsNullOrWhiteSpace(kw) && kw.Trim().Length >= 3).Select(kw => new Keyword { Value = kw }).ToList();
                }

                if (keywords.Count > 0)
                {
                    this.phKeywords.Visible = true;
                    this.rptKeywords.DataSource = keywords;
                    this.rptKeywords.DataBind();
                }

                if (!string.IsNullOrWhiteSpace(this.AuthorUsername))
                {
                    this.phUsername.Visible = true;
                    this.litUserName.Text = this.Server.HtmlEncode(this.AuthorUsername);
                }

                if (!string.IsNullOrWhiteSpace(this.Tags))
                {
                    this.phTag.Visible = true;
                    this.litTag.Text = this.Server.HtmlEncode(this.Tags);
                }

                this.BindPosts();

                // Update Meta Data
                var tempVar = this.BasePage;
                Environment.UpdateMeta(ref tempVar, "[VALUE] - " + this.GetSharedResource("[RESX:Search]") + " - " + this.SearchText, "[VALUE]", "[VALUE]");
            }
            catch (Exception ex)
            {
                this.Controls.Clear();
                this.RenderMessage("[RESX:ERROR]", "[RESX:ERROR:Search]", ex.Message, ex);
            }
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

        private void BindPosts()
        {
            this._pageSize = (this.UserId > 0) ? this.UserDefaultPageSize : this.MainSettings.PageSize;

            if (this._pageSize < 5)
            {
                this._pageSize = 10;
            }

            this._rowIndex = (this.PageId - 1) * this._pageSize;

            // If we don't have a search string, tag or user id, there is nothing we can do so exit
            if (this.SearchText == string.Empty && this.Tags == string.Empty && this.AuthorUsername == String.Empty && this.AuthorUserId <= 0)
            {
                return;
            }

            // Build the list of forums to search
            // An intersection of the forums allows vs forums requested.

            var parseId = 0;

            var sForumsAllowed = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForumsForUser(this.ForumUser.UserRoles, this.PortalId, this.ModuleId, "CanRead", true); // Make sure and pass strict = true here
            var forumsAllowed = sForumsAllowed.Split(new[] { ':', ';' }).Where(f => int.TryParse(f, out parseId)).Select(f => parseId).ToList();
            var forumsRequested = this.Forums.Split(new[] { ':', ';' }).Where(f => int.TryParse(f, out parseId)).Select(f => parseId).ToList();

            var forumsToSearch = string.Empty;

            // If forums requested is empty or contains and entry less than or equal to zero, return all available forums
            if (!forumsRequested.Any() || forumsRequested.Any(o => o <= 0))
            {
                forumsToSearch = forumsAllowed.Aggregate(forumsToSearch, (current, f) => current + (current.Length == 0 ? string.Empty : ":") + f);
            }
            else
            {
                forumsToSearch = forumsRequested.Where(forumsAllowed.Contains).Aggregate(forumsToSearch, (current, f) => current + (current.Length == 0 ? String.Empty : ":") + f);
            }

            const int maxCacheHours = 1;

            var ds = DataProvider.Instance().Search(this.PortalId, this.ModuleId, this.UserId, this.SearchId, this._rowIndex, this._pageSize, this.SearchText, this.SearchType, this.SearchColumns, this.SearchDays, this.AuthorUserId, this.AuthorUsername, forumsToSearch, this.Tags, this.ResultType, this.Sort, maxCacheHours, this.MainSettings.FullText);

            var dtSummary = (ds != null) ? ds.Tables[0] : null;

            this._searchId = (dtSummary != null) ? Convert.ToInt32(dtSummary.Rows[0][0]) : 0;
            this._rowCount = (dtSummary != null) ? Convert.ToInt32(dtSummary.Rows[0][1]) : 0;
            this._searchDuration = (dtSummary != null) ? Convert.ToInt32(dtSummary.Rows[0][2]) : 0;
            this._searchAge = (dtSummary != null) ? Convert.ToInt32(dtSummary.Rows[0][3]) : 0;

            var totalSeconds = new TimeSpan(0, 0, 0, 0, this._searchDuration.Value).TotalSeconds;
            var ageInMinutes = new TimeSpan(0, 0, 0, 0, this._searchAge.Value).TotalMinutes;

            this.litSearchDuration.Text = string.Format(this.GetSharedResource("[RESX:SearchDuration]"), totalSeconds);

            if (ageInMinutes > 0.25)
            {
                this.litSearchAge.Text = string.Format(this.GetSharedResource("[RESX:SearchAge]"), ageInMinutes);
            }

            this._parameters = null; // We reset this so we make sure to get an updated version

            var dtResults = (ds != null) ? ds.Tables[1] : null;
            if (dtResults != null && dtResults.Rows.Count > 0)
            {
                this.litRecordCount.Text = string.Format(this.GetSharedResource("[RESX:SearchRecords]"), this._rowIndex + 1, this._rowIndex + dtResults.Rows.Count, this._rowCount);

                var rptResults = this.ResultType == 0 ? this.rptTopics : this.rptPosts;

                this.pnlMessage.Visible = false;

                try
                {
                    rptResults.Visible = true;
                    rptResults.DataSource = dtResults;
                    rptResults.DataBind();
                    this.BuildPager(this.PagerTop);
                    this.BuildPager(this.PagerBottom);
                }
                catch (Exception ex)
                {
                    this.litMessage.Text = ex.Message;
                    this.pnlMessage.Visible = true;
                    rptResults.Visible = false;
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
            var intPages = Convert.ToInt32(Math.Ceiling(this._rowCount / (double)this._pageSize));

            pager.PageCount = intPages;
            pager.CurrentPage = this.PageId;
            pager.TabID = this.TabId;
            pager.ForumID = this.ForumId;
            pager.PageText = Utilities.GetSharedResource("[RESX:Page]");
            pager.OfText = Utilities.GetSharedResource("[RESX:PageOf]");
            pager.View = "search";
            pager.PageMode = PagerNav.Mode.Links;

            pager.Params = this.Parameters.ToArray();
        }

        #endregion

        #region Public Methods

        public string GetSearchUrl()
        {
            var @params = new List<string> { ParamKeys.ViewType + "=searchadvanced" };
            @params.AddRange(this.Parameters);

            if (this.SocialGroupId > 0)
            {
                @params.Add("GroupId=" + this.SocialGroupId.ToString());
            }

            return this.NavigateUrl(this.TabId, string.Empty, @params.ToArray());
        }

        public string GetForumUrl()
        {
            if (this._currentRow == null)
            {
                return null;
            }

            var forumId = this._currentRow["ForumID"].ToString();

            var @params = new List<string> { ParamKeys.ForumId + "=" + forumId, ParamKeys.ViewType + "=" + Views.Topics };

            if (this.SocialGroupId > 0)
            {
                @params.Add("GroupId=" + this.SocialGroupId.ToString());
            }

            return this.NavigateUrl(this.TabId, string.Empty, @params.ToArray());
        }

        public string GetThreadUrl()
        {
            if (this._currentRow == null)
            {
                return null;
            }

            var forumId = this._currentRow["ForumID"].ToString();
            var topicId = this._currentRow["TopicId"].ToString();

            var @params = new List<string> { ParamKeys.ForumId + "=" + forumId, ParamKeys.ViewType + "=" + Views.Topic, ParamKeys.TopicId + "=" + topicId };

            if (this.SocialGroupId > 0)
            {
                @params = new List<string> { ParamKeys.TopicId + "=" + topicId };
            }

            @params.Add("GroupId=" + this.SocialGroupId.ToString());

            return this.NavigateUrl(this.TabId, string.Empty, @params.ToArray());
        }

        // Jumps to post for post view, or last reply for topics view
        public string GetPostUrl()
        {
            if (this._currentRow == null)
            {
                return null;
            }

            var forumId = this._currentRow["ForumID"].ToString();
            var topicId = this._currentRow["TopicId"].ToString();
            var contentId = this._currentRow["ContentId"].ToString();

            var @params = new List<string> { ParamKeys.ForumId + "=" + forumId, ParamKeys.ViewType + "=" + Views.Topic, ParamKeys.TopicId + "=" + topicId, ParamKeys.ContentJumpId + "=" + contentId };

            if (this.SocialGroupId > 0)
            {
                @params.Add("GroupId=" + this.SocialGroupId.ToString());
            }

            return this.NavigateUrl(this.TabId, string.Empty, @params.ToArray());
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

        public string GetPostSnippet()
        {
            var post = this._currentRow["Body"].ToString();
            post = Utilities.StripHTMLTag(post);
            post = post.Replace(System.Environment.NewLine, " ");

            if (post.Length > 255)
            {
                post = post.Substring(0, 255).Trim() + "...";
            }

            return post;
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
        public DataRow Get_currentRow() => this._currentRow;

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public int GetSocialGroupId() => this.SocialGroupId;

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public string GetMiniPager() => MiniPager.GetMiniPager(this._currentRow, this.TabId, this.SocialGroupId, this._pageSize);

        #endregion
        public class Keyword
        {
            public string Value { get; set; }

            public string HtmlEncodedValue
            {
                get { return string.IsNullOrWhiteSpace(this.Value) ? this.Value : HttpUtility.HtmlEncode(this.Value); }
            }
        }

    }
}
