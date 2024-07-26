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

        private string searchText;
        private string tags;
        private int? searchType;
        private int? authorUserId;
        private string authorUsername;
        private int? searchColumns;
        private string forums;
        private int? searchDays;
        private int? resultType;
        private int? searchId;
        private int? sort;

        private List<string> parameters;

        private int rowCount;

        private int pageSize = 20;
        private int rowIndex;

        private int? searchAge;
        private int? searchDuration;

        private DataRow currentRow;

        #endregion

        #region Properties

        private string SearchText
        {
            get
            {
                if (this.searchText == null)
                {
                    this.searchText = this.Request.Params[SearchParamKeys.Query] + string.Empty;
                    this.searchText = Utilities.XSSFilter(this.searchText);
                    this.searchText = Utilities.StripHTMLTag(this.searchText);
                    this.searchText = Utilities.CheckSqlString(this.searchText);
                    this.searchText = this.SearchText.Replace("\"", string.Empty);
                    this.searchText = this.searchText.Trim();
                }

                return this.searchText;
            }
        }

        private string Tags
        {
            get
            {
                if (this.tags == null)
                {
                    this.tags = this.Request.Params[SearchParamKeys.Tag] + string.Empty;

                    // The tag links are generated with "aftg"
                    if (this.tags == string.Empty)
                    {
                        this.tags = this.Request.Params[ParamKeys.Tags] + string.Empty;
                    }

                    this.tags = Utilities.XSSFilter(this.tags);
                    this.tags = Utilities.StripHTMLTag(this.tags);
                    this.tags = Utilities.CheckSqlString(this.tags);
                    this.tags = this.tags.Trim();
                }

                return this.tags;
            }
        }

        private int SearchType
        {
            get
            {
                if (!this.searchType.HasValue)
                {
                    int parsedSearchType;
                    this.searchType = int.TryParse(this.Request.Params[SearchParamKeys.SearchType], out parsedSearchType) ? parsedSearchType : 0;
                }

                return this.searchType.Value;
            }
        }

        private string AuthorUsername
        {
            get
            {
                if (this.authorUsername == null)
                {
                    this.authorUsername = this.Request.Params[SearchParamKeys.Author] + string.Empty;
                    this.authorUsername = Utilities.XSSFilter(this.authorUsername);
                    this.authorUsername = Utilities.StripHTMLTag(this.authorUsername);
                    this.authorUsername = Utilities.CheckSqlString(this.authorUsername);
                    this.authorUsername = this.authorUsername.Trim();
                }

                return this.authorUsername;
            }
        }

        private int AuthorUserId
        {
            get
            {
                if (!this.authorUserId.HasValue)
                {
                    int parsedValue;
                    this.authorUserId = int.TryParse(this.Request.Params[SearchParamKeys.User], out parsedValue) ? parsedValue : 0;
                }

                return this.authorUserId.Value;
            }
        }

        private int SearchColumns
        {
            get
            {
                if (!this.searchColumns.HasValue)
                {
                    int parsedSearchColumns;
                    this.searchColumns = int.TryParse(this.Request.Params[SearchParamKeys.Columns], out parsedSearchColumns) ? parsedSearchColumns : 0;
                }

                return this.searchColumns.Value;
            }
        }

        private string Forums
        {
            get
            {
                if (this.forums == null)
                {
                    this.forums = this.Request.Params[SearchParamKeys.Forums] + string.Empty;
                    this.forums = Utilities.XSSFilter(this.forums);
                    this.forums = Utilities.StripHTMLTag(this.forums);
                    this.forums = Utilities.CheckSqlString(this.forums);
                    this.forums = this.forums.Trim();
                }

                return this.forums;
            }
        }

        private int SearchDays
        {
            get
            {
                if (!this.searchDays.HasValue)
                {
                    int parsedValue;
                    this.searchDays = int.TryParse(this.Request.Params[SearchParamKeys.TimeSpan], out parsedValue) ? parsedValue : 0;
                }

                return this.searchDays.Value;
            }
        }

        private int ResultType
        {
            get
            {
                if (!this.resultType.HasValue)
                {
                    int parsedValue;
                    this.resultType = int.TryParse(this.Request.Params[SearchParamKeys.ResultType], out parsedValue) ? parsedValue : 0;
                }

                return this.resultType.Value;
            }
        }

        private int SearchId
        {
            get
            {
                if (!this.searchId.HasValue)
                {
                    int parsedValue;
                    this.searchId = int.TryParse(this.Request.Params[SearchParamKeys.Search], out parsedValue) ? parsedValue : 0;
                }

                return this.searchId.Value;
            }
        }

        private int Sort
        {
            get
            {
                if (!this.sort.HasValue)
                {
                    int parsedValue;
                    this.sort = int.TryParse(this.Request.Params[SearchParamKeys.Sort], out parsedValue) ? parsedValue : 0;
                }

                return this.sort.Value;
            }
        }

        private List<string> Parameters
        {
            get
            {
                if (this.parameters == null)
                {
                    this.parameters = new List<string>();

                    if (!string.IsNullOrWhiteSpace(this.SearchText))
                    {
                        this.parameters.Add($"{SearchParamKeys.Query}=" + System.Web.HttpUtility.UrlEncode(this.SearchText));
                    }

                    if (!string.IsNullOrWhiteSpace(this.Tags))
                    {
                        this.parameters.Add($"{SearchParamKeys.Tag}=" + System.Web.HttpUtility.UrlEncode(this.Tags));
                    }

                    if (this.SearchId > 0)
                    {
                        this.parameters.Add($"{SearchParamKeys.Search}=" + this.SearchId);
                    }

                    if (this.SearchType > 0)
                    {
                        this.parameters.Add($"{SearchParamKeys.SearchType}=" + this.SearchType);
                    }

                    if (this.ResultType > 0)
                    {
                        this.parameters.Add($"{SearchParamKeys.ResultType}=" + this.ResultType);
                    }

                    if (this.SearchColumns > 0)
                    {
                        this.parameters.Add($"{SearchParamKeys.Columns}=" + this.SearchColumns);
                    }

                    if (this.SearchDays > 0)
                    {
                        this.parameters.Add($"{SearchParamKeys.TimeSpan}=" + this.SearchDays);
                    }

                    if (this.AuthorUserId > 0)
                    {
                        this.parameters.Add($"{SearchParamKeys.User}=" + this.AuthorUserId);
                    }

                    if (this.Sort > 0)
                    {
                        this.parameters.Add($"{SearchParamKeys.Sort}=" + this.Sort);
                    }

                    if (!string.IsNullOrWhiteSpace(this.AuthorUsername))
                    {
                        this.parameters.Add($"{SearchParamKeys.Author}=" + System.Web.HttpUtility.UrlEncode(this.AuthorUsername));
                    }

                    if (!string.IsNullOrWhiteSpace(this.Forums))
                    {
                        this.parameters.Add($"{SearchParamKeys.Forums}=" + System.Web.HttpUtility.UrlEncode(this.Forums));
                    }
                }

                return this.parameters;
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
                if (this.SearchType == 2 && !string.IsNullOrWhiteSpace(this.SearchText) && this.SearchText.Trim().Length >= 3) // Exact Match
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

            this.currentRow = dataRowView.Row;
        }

        #endregion

        #region Private Methods

        private void BindPosts()
        {
            this.pageSize = (this.UserId > 0) ? this.UserDefaultPageSize : this.MainSettings.PageSize;

            if (this.pageSize < 5)
            {
                this.pageSize = 10;
            }

            this.rowIndex = (this.PageId - 1) * this.pageSize;

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

            var ds = DataProvider.Instance().Search(this.PortalId, this.ModuleId, this.UserId, this.SearchId, this.rowIndex, this.pageSize, this.SearchText, this.SearchType, this.SearchColumns, this.SearchDays, this.AuthorUserId, this.AuthorUsername, forumsToSearch, this.Tags, this.ResultType, this.Sort, maxCacheHours, this.MainSettings.FullText);

            var dtSummary = (ds != null) ? ds.Tables[0] : null;

            this.searchId = (dtSummary != null) ? Convert.ToInt32(dtSummary.Rows[0][0]) : 0;
            this.rowCount = (dtSummary != null) ? Convert.ToInt32(dtSummary.Rows[0][1]) : 0;
            this.searchDuration = (dtSummary != null) ? Convert.ToInt32(dtSummary.Rows[0][2]) : 0;
            this.searchAge = (dtSummary != null) ? Convert.ToInt32(dtSummary.Rows[0][3]) : 0;

            var totalSeconds = new TimeSpan(0, 0, 0, 0, this.searchDuration.Value).TotalSeconds;
            var ageInMinutes = new TimeSpan(0, 0, 0, 0, this.searchAge.Value).TotalMinutes;

            this.litSearchDuration.Text = string.Format(this.GetSharedResource("[RESX:SearchDuration]"), totalSeconds);

            if (ageInMinutes > 0.25)
            {
                this.litSearchAge.Text = string.Format(this.GetSharedResource("[RESX:SearchAge]"), ageInMinutes);
            }

            this.parameters = null; // We reset this so we make sure to get an updated version

            var dtResults = (ds != null) ? ds.Tables[1] : null;
            if (dtResults != null && dtResults.Rows.Count > 0)
            {
                this.litRecordCount.Text = string.Format(this.GetSharedResource("[RESX:SearchRecords]"), this.rowIndex + 1, this.rowIndex + dtResults.Rows.Count, this.rowCount);

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
            var intPages = Convert.ToInt32(Math.Ceiling(this.rowCount / (double)this.pageSize));

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
            if (this.currentRow == null)
            {
                return null;
            }

            var forumId = this.currentRow["ForumID"].ToString();

            var @params = new List<string> { ParamKeys.ForumId + "=" + forumId, ParamKeys.ViewType + "=" + Views.Topics };

            if (this.SocialGroupId > 0)
            {
                @params.Add("GroupId=" + this.SocialGroupId.ToString());
            }

            return this.NavigateUrl(this.TabId, string.Empty, @params.ToArray());
        }

        public string GetThreadUrl()
        {
            if (this.currentRow == null)
            {
                return null;
            }

            var forumId = this.currentRow["ForumID"].ToString();
            var topicId = this.currentRow["TopicId"].ToString();

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
            if (this.currentRow == null)
            {
                return null;
            }

            var forumId = this.currentRow["ForumID"].ToString();
            var topicId = this.currentRow["TopicId"].ToString();
            var contentId = this.currentRow["ContentId"].ToString();

            var @params = new List<string> { ParamKeys.ForumId + "=" + forumId, ParamKeys.ViewType + "=" + Views.Topic, ParamKeys.TopicId + "=" + topicId, ParamKeys.ContentJumpId + "=" + contentId };

            if (this.SocialGroupId > 0)
            {
                @params.Add("GroupId=" + this.SocialGroupId.ToString());
            }

            return this.NavigateUrl(this.TabId, string.Empty, @params.ToArray());
        }

        public string GetPostTime()
        {
            return (this.currentRow == null) ? null : Utilities.GetUserFriendlyDateTimeString(Convert.ToDateTime(this.currentRow["DateCreated"]), this.ForumModuleId, this.UserInfo);
        }

        public string GetAuthor()
        {
            if (this.currentRow == null)
            {
                return null;
            }

            var userId = Convert.ToInt32(this.currentRow["AuthorId"]);
            var userName = this.currentRow["AuthorUserName"].ToString();
            var firstName = this.currentRow["AuthorFirstName"].ToString();
            var lastName = this.currentRow["AuthorLastName"].ToString();
            var displayName = this.currentRow["AuthorDisplayName"].ToString();

            return UserProfiles.GetDisplayName(this.PortalSettings, this.ForumModuleId, true, false, this.ForumUser.IsAdmin, userId, userName, firstName, lastName, displayName);
        }

        public string GetLastPostAuthor()
        {
            if (this.currentRow == null)
            {
                return null;
            }

            var userId = Convert.ToInt32(this.currentRow["LastReplyAuthorId"]);
            var userName = this.currentRow["LastReplyUserName"].ToString();
            var firstName = this.currentRow["LastReplyFirstName"].ToString();
            var lastName = this.currentRow["LastReplyLastName"].ToString();
            var displayName = this.currentRow["LastReplyDisplayName"].ToString();

            return UserProfiles.GetDisplayName(this.PortalSettings, this.ForumModuleId, true, false, this.ForumUser.IsAdmin, userId, userName, firstName, lastName, displayName);
        }

        public string GetLastPostTime()
        {
            return (this.currentRow == null) ? null : Utilities.GetUserFriendlyDateTimeString(Convert.ToDateTime(this.currentRow["LastReplyDate"]), this.ForumModuleId, this.UserInfo);
        }

        public string GetPostSnippet()
        {
            var post = this.currentRow["Body"].ToString();
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
                Utilities.SafeConvertInt(this.currentRow["TopicId"].ToString()),
                this.ThemePath,
                Utilities.SafeConvertInt(this.currentRow["UserLastTopicRead"]),
                Utilities.SafeConvertInt(this.currentRow["UserLastReplyRead"]));
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public DataRow Get_currentRow() => this.currentRow;

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public int GetSocialGroupId() => this.SocialGroupId;

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public string GetMiniPager() => MiniPager.GetMiniPager(this.currentRow, this.TabId, this.SocialGroupId, this.pageSize);

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
