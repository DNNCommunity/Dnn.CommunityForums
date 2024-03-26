//
// Community Forums
// Copyright (c) 2013-2021
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
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Modules.ActiveForums.Controls;

namespace DotNetNuke.Modules.ActiveForums
{
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
                if(_searchText == null)
                {
                    _searchText = Request.Params[SearchParamKeys.Query] + string.Empty;
                    _searchText = Utilities.XSSFilter(_searchText);
                    _searchText = Utilities.StripHTMLTag(_searchText);
                    _searchText = Utilities.CheckSqlString(_searchText);
                    _searchText = SearchText.Replace("\"", string.Empty);
                    _searchText = _searchText.Trim();
                }

                return _searchText;
            }
        }

        private string Tags
        {
            get
            {
                if (_tags == null)
                {
                    _tags = Request.Params[SearchParamKeys.Tag] + string.Empty;

                    // The tag links are generated with "aftg"
                    if (_tags == string.Empty)
                        _tags = Request.Params[ParamKeys.Tags] + string.Empty;

                    _tags = Utilities.XSSFilter(_tags);
                    _tags = Utilities.StripHTMLTag(_tags);
                    _tags = Utilities.CheckSqlString(_tags);
                    _tags = _tags.Trim();
                }

                return _tags;
            }
        }

        private int SearchType
        {
            get
            {
                if(!_searchType.HasValue)
                {
                    int parsedSearchType;
                    _searchType = int.TryParse(Request.Params[SearchParamKeys.SearchType], out parsedSearchType) ? parsedSearchType : 0;
                }

                return _searchType.Value;
            }
        }

        private string AuthorUsername
        {
            get
            {
                if (_authorUsername == null)
                {
                    _authorUsername = Request.Params[SearchParamKeys.Author] + string.Empty;
                    _authorUsername = Utilities.XSSFilter(_authorUsername);
                    _authorUsername = Utilities.StripHTMLTag(_authorUsername);
                    _authorUsername = Utilities.CheckSqlString(_authorUsername);
                    _authorUsername = _authorUsername.Trim();
                }

                return _authorUsername;
            }
        }

        private int AuthorUserId
        {
            get
            {
                if (!_authorUserId.HasValue)
                {
                    int parsedValue;
                    _authorUserId = int.TryParse(Request.Params[SearchParamKeys.User], out parsedValue) ? parsedValue : 0;
                }

                return _authorUserId.Value;
            }
        }

        private int SearchColumns
        {
            get
            {
                if (!_searchColumns.HasValue)
                {
                    int parsedSearchColumns;
                    _searchColumns = int.TryParse(Request.Params[SearchParamKeys.Columns], out parsedSearchColumns) ? parsedSearchColumns : 0;
                }

                return _searchColumns.Value;
            }
        }

        private string Forums
        {
            get
            {
                if (_forums == null)
                {
                    _forums = Request.Params[SearchParamKeys.Forums] + string.Empty;
                    _forums = Utilities.XSSFilter(_forums);
                    _forums = Utilities.StripHTMLTag(_forums);
                    _forums = Utilities.CheckSqlString(_forums);
                    _forums = _forums.Trim();
                }

                return _forums;
            }
        }

        private int SearchDays
        {
            get
            {
                if (!_searchDays.HasValue)
                {
                    int parsedValue;
                    _searchDays = int.TryParse(Request.Params[SearchParamKeys.TimeSpan], out parsedValue) ? parsedValue : 0;
                }

                return _searchDays.Value;
            }
        }

        private int ResultType
        {
            get
            {
                if (!_resultType.HasValue)
                {
                    int parsedValue;
                    _resultType = int.TryParse(Request.Params[SearchParamKeys.ResultType], out parsedValue) ? parsedValue : 0;
                }

                return _resultType.Value;
            }
        }

        private int SearchId
        {
            get
            {
                if (!_searchId.HasValue)
                {
                    int parsedValue;
                    _searchId = int.TryParse(Request.Params[SearchParamKeys.Search], out parsedValue) ? parsedValue : 0;
                }

                return _searchId.Value;
            }
        }

        private int Sort
        {
            get
            {
                if (!_sort.HasValue)
                {
                    int parsedValue;
                    _sort = int.TryParse(Request.Params[SearchParamKeys.Sort], out parsedValue) ? parsedValue : 0;
                }

                return _sort.Value;
            }
        }

        private List<string> Parameters
        {
            get
            {
                if(_parameters == null)
                {
                    _parameters = new List<string>();

                    if (!string.IsNullOrWhiteSpace(SearchText))
                        _parameters.Add($"{SearchParamKeys.Query}=" + System.Web.HttpUtility.UrlEncode(SearchText));

                    if (!string.IsNullOrWhiteSpace(Tags))
                        _parameters.Add($"{SearchParamKeys.Tag}=" + System.Web.HttpUtility.UrlEncode(Tags));

                    if (SearchId > 0)
                        _parameters.Add($"{SearchParamKeys.Search}=" + SearchId);

                    if (SearchType > 0)
                        _parameters.Add($"{SearchParamKeys.SearchType}=" + SearchType);

                    if (ResultType > 0)
                        _parameters.Add($"{SearchParamKeys.ResultType}=" + ResultType);

                    if (SearchColumns > 0)
                        _parameters.Add($"{SearchParamKeys.Columns}=" + SearchColumns);

                    if (SearchDays > 0)
                        _parameters.Add($"{SearchParamKeys.TimeSpan}=" + SearchDays);

                    if (AuthorUserId > 0)
                        _parameters.Add($"{SearchParamKeys.User}=" + AuthorUserId);

                    if (Sort > 0)
                        _parameters.Add($"{SearchParamKeys.Sort}=" + Sort);

                    if (!string.IsNullOrWhiteSpace(AuthorUsername))
                        _parameters.Add($"{SearchParamKeys.Author}=" + System.Web.HttpUtility.UrlEncode(AuthorUsername));

                    if (!string.IsNullOrWhiteSpace(Forums))
                        _parameters.Add($"{SearchParamKeys.Forums}=" + System.Web.HttpUtility.UrlEncode(Forums));
                }

                return _parameters;
            }
        }

        #endregion

        #region Event Handlers

        protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

            rptPosts.ItemCreated += RepeaterOnItemCreated;
            rptTopics.ItemCreated += RepeaterOnItemCreated;

            try
            {

                if (Request.QueryString[Literals.GroupId] != null && SimulateIsNumeric.IsNumeric(Request.QueryString[Literals.GroupId]))
                {
                    SocialGroupId = Convert.ToInt32(Request.QueryString[Literals.GroupId]);
                }

                litSearchTitle.Text = GetSharedResource("[RESX:SearchTitle]");

                List<string> keywords;

                // Note: Filter out any keywords that are not at least 3 characters in length

                if (SearchType == 2 && !string.IsNullOrWhiteSpace(SearchText) && SearchText.Trim().Length >= 3) //Exact Match
                    keywords = new List<string> { SearchText.Trim() };
                else
                    keywords = SearchText.Split(' ').Where(kw => !string.IsNullOrWhiteSpace(kw) && kw.Trim().Length >= 3).ToList();

                if(keywords.Count > 0)
                {
                    phKeywords.Visible = true;
                    rptKeywords.DataSource = keywords;
                    rptKeywords.DataBind();  
                }

                if (!string.IsNullOrWhiteSpace(AuthorUsername))
                {
                    phUsername.Visible = true;
                    litUserName.Text = Server.HtmlEncode(AuthorUsername);
                }

                if(!string.IsNullOrWhiteSpace(Tags))
                {
                    phTag.Visible = true;
                    litTag.Text = Server.HtmlEncode(Tags);
                }

                BindPosts();

                // Update Meta Data
                var tempVar = BasePage;
                Environment.UpdateMeta(ref tempVar, "[VALUE] - " + GetSharedResource("[RESX:Search]") + " - " + SearchText, "[VALUE]", "[VALUE]");
            }
            catch (Exception ex)
            {
                Controls.Clear();
                RenderMessage("[RESX:ERROR]", "[RESX:ERROR:Search]", ex.Message, ex);
            }
        }

        private void RepeaterOnItemCreated(object sender, RepeaterItemEventArgs repeaterItemEventArgs)
        {
            var dataRowView = repeaterItemEventArgs.Item.DataItem as DataRowView;

            if (dataRowView == null)
                return;

            _currentRow = dataRowView.Row;
        }

        #endregion

        #region Private Methods

        private void BindPosts()
        {
            _pageSize = (UserId > 0) ? UserDefaultPageSize : MainSettings.PageSize;

            if (_pageSize < 5)
                _pageSize = 10;

            _rowIndex = (PageId - 1) * _pageSize;
        
            // If we don't have a search string, tag or user id, there is nothing we can do so exit
            if (SearchText == string.Empty && Tags == string.Empty && AuthorUsername == String.Empty && AuthorUserId <= 0) 
                return;

            // Build the list of forums to search
            // An intersection of the forums allows vs forums requested.

            var parseId = 0;

            var sForumsAllowed = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForumsForUser(ForumUser.UserRoles, PortalId, ModuleId, "CanRead", true); // Make sure and pass strict = true here
            var forumsAllowed = sForumsAllowed.Split(new [] {':',';'}).Where(f => int.TryParse(f, out parseId)).Select(f => parseId).ToList();
            var forumsRequested = Forums.Split(new[] { ':', ';' }).Where(f => int.TryParse(f, out parseId)).Select(f => parseId).ToList();

            var forumsToSearch = string.Empty;
            
            // If forums requested is empty or contains and entry less than or equal to zero, return all available forums
            if(!forumsRequested.Any() || forumsRequested.Any(o => o <= 0))
                forumsToSearch = forumsAllowed.Aggregate(forumsToSearch, (current, f) => current + ((current.Length == 0 ? string.Empty : ":") + f));
            else
                forumsToSearch = forumsRequested.Where(forumsAllowed.Contains).Aggregate(forumsToSearch, (current, f) => current + ((current.Length == 0 ? String.Empty : ":") + f));

            const int maxCacheHours = 1;

            var ds = DataProvider.Instance().Search(PortalId, ModuleId, UserId, SearchId, _rowIndex, _pageSize, SearchText, SearchType, SearchColumns, SearchDays, AuthorUserId, AuthorUsername, forumsToSearch, Tags, ResultType, Sort, maxCacheHours, MainSettings.FullText);

            var dtSummary = (ds != null) ? ds.Tables[0] : null;

            _searchId = (dtSummary != null) ? Convert.ToInt32(dtSummary.Rows[0][0]) : 0;
            _rowCount = (dtSummary != null) ? Convert.ToInt32(dtSummary.Rows[0][1]) : 0;
            _searchDuration = (dtSummary != null) ? Convert.ToInt32(dtSummary.Rows[0][2]) : 0;
            _searchAge = (dtSummary != null) ? Convert.ToInt32(dtSummary.Rows[0][3]) : 0;

            var totalSeconds = new TimeSpan(0, 0, 0, 0, _searchDuration.Value).TotalSeconds;
            var ageInMinutes = new TimeSpan(0, 0, 0, 0, _searchAge.Value).TotalMinutes;

            litSearchDuration.Text = string.Format(GetSharedResource("[RESX:SearchDuration]"), totalSeconds); 

            if(ageInMinutes > 0.25)
                litSearchAge.Text = string.Format(GetSharedResource("[RESX:SearchAge]"), ageInMinutes); 


            _parameters = null; // We reset this so we make sure to get an updated version

            var dtResults = (ds != null) ? ds.Tables[1] : null;
            if (dtResults != null && dtResults.Rows.Count > 0)
            {
                litRecordCount.Text = string.Format(GetSharedResource("[RESX:SearchRecords]"), _rowIndex + 1, _rowIndex + dtResults.Rows.Count, _rowCount);

                var rptResults = ResultType == 0 ? rptTopics : rptPosts;

                pnlMessage.Visible = false;
                
                try
                {
                    rptResults.Visible = true;
                    rptResults.DataSource = dtResults;
                    rptResults.DataBind();
                    BuildPager(PagerTop);
                    BuildPager(PagerBottom);
                }
                catch (Exception ex)
                {
                    litMessage.Text = ex.Message;
                    pnlMessage.Visible = true;
                    rptResults.Visible = false;
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
            var intPages = Convert.ToInt32(Math.Ceiling(_rowCount / (double)_pageSize));

            pager.PageCount = intPages;
            pager.CurrentPage = PageId;
            pager.TabID = TabId;
            pager.ForumID = ForumId;
            pager.PageText = Utilities.GetSharedResource("[RESX:Page]");
            pager.OfText = Utilities.GetSharedResource("[RESX:PageOf]");
            pager.View = "search";
            pager.PageMode = PagerNav.Mode.Links;

            pager.Params = Parameters.ToArray();
        }

        #endregion

        #region Public Methods

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00.  Not Used.")]
        public string GetSearchUrl()
        {
            var @params = new List<string> { ParamKeys.ViewType + "=searchadvanced" };
            @params.AddRange(Parameters);

            if (SocialGroupId > 0)
                @params.Add("GroupId=" + SocialGroupId.ToString());

            return NavigateUrl(TabId, string.Empty, @params.ToArray());
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00.  Not Used.")]
        public string GetForumUrl()
        {
            if (_currentRow == null)
                return null;

            var forumId = _currentRow["ForumID"].ToString();

            var @params = new List<string> { ParamKeys.ForumId + "=" + forumId, ParamKeys.ViewType + "=" + Views.Topics };
            

            if (SocialGroupId > 0)
                @params.Add("GroupId=" + SocialGroupId.ToString());

            return NavigateUrl(TabId, string.Empty, @params.ToArray()); 
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00.  Not Used.")]
        public string GetThreadUrl()
        {
            if (_currentRow == null)
                return null;

            var forumId = _currentRow["ForumID"].ToString();
            var topicId = _currentRow["TopicId"].ToString();

            var @params = new List<string> { ParamKeys.ForumId + "=" + forumId, ParamKeys.ViewType + "=" + Views.Topic, ParamKeys.TopicId + "=" + topicId };
            

            if (SocialGroupId > 0)
                @params = new List<string> { ParamKeys.TopicId + "=" + topicId };
                @params.Add("GroupId=" + SocialGroupId.ToString());

            return NavigateUrl(TabId, string.Empty, @params.ToArray());
        }

        // Jumps to post for post view, or last reply for topics view
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00.  Not Used.")]
        public string GetPostUrl() 
        {
            if (_currentRow == null)
                return null;

            var forumId = _currentRow["ForumID"].ToString();
            var topicId = _currentRow["TopicId"].ToString();
            var contentId = _currentRow["ContentId"].ToString();

            var @params = new List<string> { ParamKeys.ForumId + "=" + forumId, ParamKeys.ViewType + "=" + Views.Topic, ParamKeys.TopicId + "=" + topicId, ParamKeys.ContentJumpId + "=" + contentId };
           

            if (SocialGroupId > 0)
                @params.Add("GroupId=" + SocialGroupId.ToString());

            return NavigateUrl(TabId, string.Empty, @params.ToArray());
        }

        // Todo: Localize today and yesterday
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00.  Not Used.")]
        public string GetPostTime()
        {
            if (_currentRow == null)
                return null;

            var date = GetUserDate(Convert.ToDateTime(_currentRow["DateCreated"]));
            var currentDate = GetUserDate(DateTime.UtcNow);

            var datePart = date.ToString(MainSettings.DateFormatString);

            if (currentDate.Date == date.Date)
                datePart = GetSharedResource("Today");
            else if (currentDate.AddDays(-1).Date == date.Date)
                datePart = GetSharedResource("Yesterday");

            return string.Format(GetSharedResource("SearchPostTime"), datePart, date.ToString(MainSettings.TimeFormatString));
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00.  Not Used.")]
        public string GetAuthor()
        {
            if (_currentRow == null)
                return null;

            var userId = Convert.ToInt32(_currentRow["AuthorId"]);
            var userName = _currentRow["AuthorUserName"].ToString();
            var firstName = _currentRow["AuthorFirstName"].ToString();
            var lastName = _currentRow["AuthorLastName"].ToString();
            var displayName = _currentRow["AuthorDisplayName"].ToString();

            return UserProfiles.GetDisplayName(ModuleId, true, false, ForumUser.IsAdmin, userId, userName, firstName, lastName, displayName);
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00.  Not Used.")]
        public string GetLastPostAuthor()
        {
            if (_currentRow == null)
                return null;

            var userId = Convert.ToInt32(_currentRow["LastReplyAuthorId"]);
            var userName = _currentRow["LastReplyUserName"].ToString();
            var firstName = _currentRow["LastReplyFirstName"].ToString();
            var lastName = _currentRow["LastReplyLastName"].ToString();
            var displayName = _currentRow["LastReplyDisplayName"].ToString();

            return UserProfiles.GetDisplayName(ModuleId, true, false, ForumUser.IsAdmin, userId, userName, firstName, lastName, displayName);
        }

        // Todo: Localize today and yesterday
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00.  Not Used.")]
        public string GetLastPostTime()
        {
            if (_currentRow == null)
                return null;

            var date = GetUserDate(Convert.ToDateTime(_currentRow["LastReplyDate"]));
            var currentDate = GetUserDate(DateTime.UtcNow);

            var datePart = date.ToString(MainSettings.DateFormatString);

            if (currentDate.Date == date.Date)
                datePart = GetSharedResource("Today");
            else if (currentDate.AddDays(-1).Date == date.Date)
                datePart = GetSharedResource("Yesterday");

            return datePart + " @ " + date.ToString(MainSettings.TimeFormatString);
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00.  Not Used.")]
        public string GetPostSnippet()
        {
            var post = _currentRow["Body"].ToString();
            post = Utilities.StripHTMLTag(post);
            post = post.Replace(System.Environment.NewLine, " ");

            if (post.Length > 255)
                post = post.Substring(0, 255).Trim() + "...";

            return post;
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00.  Not Used.")]
        public string GetIcon()
        {
            return DotNetNuke.Modules.ActiveForums.Controllers.TopicController.GetTopicIcon(
                Utilities.SafeConvertInt(_currentRow["TopicId"].ToString()),
                Utilities.SafeConvertBool(_currentRow["IsRead"]),
                ThemePath,
                Utilities.SafeConvertInt(_currentRow["UserLastTopicRead"]),
                Utilities.SafeConvertInt(_currentRow["UserLastReplyRead"]));
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00.  Not Used.")]
        public string GetMiniPager()
        {
            if (_currentRow == null)
                return null;
         
            var replyCount = Convert.ToInt32(_currentRow["ReplyCount"]);
            var pageCount = Convert.ToInt32(Math.Ceiling((double)(replyCount + 1) / _pageSize));
            var forumId = _currentRow["ForumId"].ToString();
            var topicId = _currentRow["TopicId"].ToString();

            // No pager if there is only one page.
            if (pageCount <= 1)
                return null;

             List<string> @params;

            var result = string.Empty;

            if(pageCount <= 5)
            {
                for (var i = 1; i <= pageCount; i++)
                {
                    @params = new List<string> { ParamKeys.ForumId + "=" + forumId, ParamKeys.TopicId + "=" + topicId, ParamKeys.ViewType + "=" + Views.Topic, ParamKeys.PageId + "=" + i };

                    if (SocialGroupId > 0)
                        @params.Add("GroupId=" + SocialGroupId.ToString());

                    result += "<a href=\"" + NavigateUrl(TabId, string.Empty, @params.ToArray()) + "\">" + i + "</a>";
                }

                return result;
            }
            
            // 1 2 3 ... N

            for(var i = 1; i <= 3; i++)
            {
                @params = new List<string> { ParamKeys.ForumId + "=" + forumId, ParamKeys.TopicId + "=" + topicId, ParamKeys.ViewType + "=" + Views.Topic, ParamKeys.PageId + "=" + i };
                if (SocialGroupId > 0)
                    @params.Add("GroupId=" + SocialGroupId.ToString());

                result += "<a href=\"" + NavigateUrl(TabId, string.Empty, @params.ToArray()) + "\">" + i + "</a>";  
            }

            result += "<span>...</span>";

            @params = new List<string>  { ParamKeys.ForumId + "=" + forumId, ParamKeys.TopicId + "=" + topicId, ParamKeys.ViewType + "=" + Views.Topic, ParamKeys.PageId + "=" + pageCount };
            if (SocialGroupId > 0)
                @params.Add("GroupId=" + SocialGroupId.ToString());

            result += "<a href=\"" + NavigateUrl(TabId, string.Empty, @params.ToArray()) + "\">" + pageCount + "</a>";  

            return result;
        }

        #endregion

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Replaced with List<string>")]
        public class Keyword
        {
            [Obsolete("Deprecated in Community Forums. Removed in 10.00.00.  Replaced with List<string>")]
            public string Value { get; set; }
            [Obsolete("Deprecated in Community Forums. Removed in 10.00.00.  Replaced with List<string>")]
            public string HtmlEncodedValue
            {
                get { return string.IsNullOrWhiteSpace(Value) ? Value : HttpUtility.HtmlEncode(Value); }
            }
        }

    }
}
