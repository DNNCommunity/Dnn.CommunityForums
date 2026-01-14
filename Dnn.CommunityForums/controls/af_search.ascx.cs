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
    using System.Web.UI.WebControls;

    using DotNetNuke.Modules.ActiveForums.Controls;
    using DotNetNuke.Modules.ActiveForums.Extensions;
    using DotNetNuke.Modules.ActiveForums.Services.Search;

    public partial class af_search : ForumBase
    {
        protected System.Web.UI.WebControls.PlaceHolder plhMessage = new PlaceHolder();
        protected System.Web.UI.WebControls.PlaceHolder phKeywords = new PlaceHolder();
        protected System.Web.UI.WebControls.PlaceHolder phTag = new PlaceHolder();
        protected System.Web.UI.WebControls.PlaceHolder phUsername = new PlaceHolder();
        protected System.Web.UI.WebControls.Repeater rptTopics = new Repeater();
        protected System.Web.UI.WebControls.Repeater rptPosts = new Repeater();
        protected System.Web.UI.WebControls.Panel pnlMessage = new Panel();
        protected System.Web.UI.WebControls.Literal litTag = new Literal();
        protected System.Web.UI.WebControls.Literal litUserName = new Literal();
        protected System.Web.UI.WebControls.Literal litMessage = new Literal();
        protected System.Web.UI.WebControls.Literal litSearchDuration = new Literal();
        protected System.Web.UI.WebControls.Literal litSearchAge = new Literal();
        protected System.Web.UI.WebControls.Literal litRecordCount = new Literal();
        protected System.Web.UI.WebControls.Literal litKeywords = new Literal();
        protected DotNetNuke.Modules.ActiveForums.Controls.PagerNav PagerTop = new PagerNav();
        protected DotNetNuke.Modules.ActiveForums.Controls.PagerNav PagerBottom = new PagerNav();

        private string searchText;
        private string tags;
        private int? authorUserId;
        private string authorUsername;
        private string forums;
        private int? searchDays;
        private DotNetNuke.Modules.ActiveForums.Enums.SearchSortType? searchSortType;
        private DotNetNuke.Modules.ActiveForums.Enums.SearchResultType? searchResultType;
        private int? searchId;

        private DotNetNuke.Modules.ActiveForums.ViewModels.SearchResults searchResults;

        private List<string> parameters;

        private int pageSize = 20;
        private int rowIndex;

        private Control ctl;
        private DataRow currentRow;

        #region Event Handlers
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            string template = DotNetNuke.Modules.ActiveForums.Controllers.TemplateController.Template_Get(this.ForumModuleId, Enums.TemplateType.SearchResults, SettingsBase.GetModuleSettings(this.ForumModuleId).DefaultFeatureSettings.TemplateFileNameSuffix);

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
            this.rptPosts.ItemCreated += this.PostRepeaterOnItemCreated;
            this.rptPosts.ItemDataBound += this.PostRepeaterOnItemDataBound;
            this.rptTopics.ItemCreated += this.TopicRepeaterOnItemCreated;
            this.rptTopics.ItemDataBound += this.TopicRepeaterOnItemDataBound;

            try
            {
                if (this.Request.QueryString[Literals.GroupId] != null && Utilities.IsNumeric(this.Request.QueryString[Literals.GroupId]))
                {
                    this.SocialGroupId = Convert.ToInt32(this.Request.QueryString[Literals.GroupId]);
                }

                string keywords;

                // Note: Filter out any keywords that are not at least 3 characters in length
                keywords = string.Join(" ", this.SearchText.Split(' ').Where(kw => !string.IsNullOrWhiteSpace(kw) && kw.Trim().Length >= 3).ToArray()).Trim();

                if (!string.IsNullOrEmpty(keywords))
                {
                    this.phKeywords.Visible = true;
                    this.litKeywords.Visible = true;
                    this.litKeywords.Text = keywords;
                }
                else
                {
                    this.ctl.Controls.Remove(this.phKeywords);
                }

                if (!string.IsNullOrWhiteSpace(this.AuthorUsername))
                {
                    this.phUsername.Visible = true;
                    this.litUserName.Visible = true;
                    this.litUserName.Text = this.Server.HtmlEncode(this.AuthorUsername);
                }
                else
                {
                    this.ctl.Controls.Remove(this.phUsername);
                }

                if (!string.IsNullOrWhiteSpace(this.Tags))
                {
                    this.phTag.Visible = true;
                    this.litTag.Visible = true;
                    this.litTag.Text = this.Server.HtmlEncode(this.Tags);
                }
                else
                {
                    this.ctl.Controls.Remove(this.phTag);
                }

                this.BindPosts();
                this.Search.Controls.Clear();
                this.Search.Controls.Add(this.ctl);

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

        private void TopicRepeaterOnItemDataBound(object sender, RepeaterItemEventArgs repeaterItemEventArgs)
        {
            if (repeaterItemEventArgs.Item.ItemType == ListItemType.Item || repeaterItemEventArgs.Item.ItemType == ListItemType.AlternatingItem)
            {
                try
                {
                    string itemTemplate = ((LiteralControl)repeaterItemEventArgs.Item.Controls[0]).Text;
                    var topic = ((DotNetNuke.Modules.ActiveForums.Entities.IPostInfo)repeaterItemEventArgs.Item.DataItem).Topic;
                    if (topic != null)
                    {
                        itemTemplate = Utilities.DecodeBrackets(DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.ReplaceTopicTokens(new StringBuilder(itemTemplate), topic, this.PortalSettings, this.ModuleSettings, new Services.URLNavigator().NavigationManager(), this.ForumUser, this.Request.Url, this.Request.RawUrl).ToString());
                        ((LiteralControl)repeaterItemEventArgs.Item.Controls[0]).Text = itemTemplate;
                    }
                }
                catch (Exception ex)
                {
                    Exceptions.LogException(ex);
                }
            }
        }

        private void PostRepeaterOnItemDataBound(object sender, RepeaterItemEventArgs repeaterItemEventArgs)
        {
            if (repeaterItemEventArgs.Item.ItemType == ListItemType.Item || repeaterItemEventArgs.Item.ItemType == ListItemType.AlternatingItem)
            {
                try
                {
                    var post = (DotNetNuke.Modules.ActiveForums.Entities.IPostInfo)repeaterItemEventArgs.Item.DataItem;
                    if (post != null)
                    {
                        foreach (Control control in repeaterItemEventArgs.Item.Controls)
                        {
                            string itemTemplate = string.Empty;
                            try
                            {
                                if (control.GetType().FullName == "System.Web.UI.LiteralControl")
                                {
                                    itemTemplate = ((System.Web.UI.LiteralControl)control).Text;
                                }
                                else if (control.GetType().FullName == "System.Web.UI.HtmlControls.HtmlGenericControl")
                                {
                                    itemTemplate = ((System.Web.UI.HtmlControls.HtmlGenericControl)control).InnerText;
                                }
                                else
                                {
                                    Exceptions.LogException(new KeyNotFoundException($"Unexpected control type: {control.GetType().FullName}"));
                                }
                            }
                            catch (Exception ex)
                            {
                                Exceptions.LogException(ex);
                            }

                            if (!string.IsNullOrEmpty(itemTemplate) && itemTemplate.Contains("["))
                            {

                                itemTemplate = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.ReplacePostTokens(new StringBuilder(itemTemplate), post, this.PortalSettings, this.ModuleSettings, new Services.URLNavigator().NavigationManager(), this.ForumUser, this.Request.Url, this.Request.RawUrl).ToString();
                                if (control.GetType().FullName == "System.Web.UI.LiteralControl")
                                {
                                    ((System.Web.UI.LiteralControl)control).Text = itemTemplate;
                                }
                                else if (control.GetType().FullName == "System.Web.UI.HtmlControls.HtmlGenericControl")
                                {
                                    ((System.Web.UI.HtmlControls.HtmlGenericControl)control).InnerText = itemTemplate;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Exceptions.LogException(ex);
                }
            }
        }

        private void PostRepeaterOnItemCreated(object sender, RepeaterItemEventArgs repeaterItemEventArgs)
        {
            this.RepeaterOnItemCreated(sender, repeaterItemEventArgs);
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
        #region Properties

        private int SearchId => (int)(this.searchId ?? (this.searchId = Utilities.SafeConvertInt(this.Request.Params[SearchParamKeys.Search], 0)));

        private string SearchText => (string)(this.searchText ?? (this.searchText = Utilities.CheckSqlString(Utilities.StripHTMLTag(Utilities.XSSFilter(this.Request.Params[SearchParamKeys.Query] + string.Empty))).Replace("\"", string.Empty).Trim()));

        private string AuthorUsername => (string)(this.authorUsername ?? (this.authorUsername = Utilities.CheckSqlString(Utilities.StripHTMLTag(Utilities.XSSFilter(this.Request.Params[SearchParamKeys.Author] + string.Empty))).Trim()));

        private string Forums => (string)(this.forums ?? (this.forums = Utilities.CheckSqlString(Utilities.StripHTMLTag(Utilities.XSSFilter(this.Request.Params[SearchParamKeys.Forums] + string.Empty))).Trim()));

        private int AuthorUserId => (int)(this.authorUserId ?? (this.authorUserId = Utilities.SafeConvertInt(this.Request.Params[SearchParamKeys.User], 0)));

        private DotNetNuke.Modules.ActiveForums.Enums.SearchResultType SearchResultType => (DotNetNuke.Modules.ActiveForums.Enums.SearchResultType)(this.searchResultType ?? (this.searchResultType = (DotNetNuke.Modules.ActiveForums.Enums.SearchResultType)Utilities.SafeConvertInt(this.Request.Params[SearchParamKeys.ResultType], (int)DotNetNuke.Modules.ActiveForums.Enums.SearchResultType.SearchByTopics)));

        private DotNetNuke.Modules.ActiveForums.Enums.SearchSortType SearchSortType => (DotNetNuke.Modules.ActiveForums.Enums.SearchSortType)(this.searchSortType ?? (this.searchSortType = (DotNetNuke.Modules.ActiveForums.Enums.SearchSortType)Utilities.SafeConvertInt(this.Request.Params[SearchParamKeys.Sort], (int)DotNetNuke.Modules.ActiveForums.Enums.SearchSortType.SearchSortTypeRelevance)));

        private int SearchDays => (int)(this.searchDays ?? (this.searchDays = Utilities.SafeConvertInt(this.Request.Params[SearchParamKeys.TimeSpan], 0)));

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

                    this.tags = Utilities.CheckSqlString(Utilities.StripHTMLTag(Utilities.XSSFilter(this.tags))).Trim();
                }

                return this.tags;
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
                        this.parameters.Add($"{SearchParamKeys.Query}={System.Net.WebUtility.UrlEncode(this.SearchText)}");
                    }

                    if (!string.IsNullOrWhiteSpace(this.Tags))
                    {
                        this.parameters.Add($"{SearchParamKeys.Tag}={System.Net.WebUtility.UrlEncode(this.Tags)}");
                    }

                    if (this.SearchId > 0)
                    {
                        this.parameters.Add($"{SearchParamKeys.Search}=" + this.SearchId);
                    }

                    if ((int)this.SearchResultType > 0)
                    {
                        this.parameters.Add($"{SearchParamKeys.ResultType}={(int)this.SearchResultType}");
                    }

                    if (this.SearchDays > 0)
                    {
                        this.parameters.Add($"{SearchParamKeys.TimeSpan}={this.SearchDays}");
                    }

                    if (this.AuthorUserId > 0)
                    {
                        this.parameters.Add($"{SearchParamKeys.User}={this.AuthorUserId}");
                    }

                    if (this.SearchSortType > 0)
                    {
                        this.parameters.Add($"{SearchParamKeys.Sort}={(int)this.SearchSortType}");
                    }

                    if (!string.IsNullOrWhiteSpace(this.AuthorUsername))
                    {
                        this.parameters.Add($"{SearchParamKeys.Author}={System.Net.WebUtility.UrlEncode(this.AuthorUsername)}");
                    }

                    if (!string.IsNullOrWhiteSpace(this.Forums))
                    {
                        this.parameters.Add($"{SearchParamKeys.Forums}={System.Net.WebUtility.UrlEncode(this.Forums)}");
                    }
                }

                return this.parameters;
            }
        }

        #endregion

        #region Private Methods

        private void BindPosts()
        {
            // If we don't have a search string, tag or user id, there is nothing we can do so exit
            if (string.IsNullOrEmpty(this.SearchText) && string.IsNullOrEmpty(this.Tags) && string.IsNullOrEmpty(this.AuthorUsername) && this.AuthorUserId <= 0)
            {
                return;
            }

            this.pageSize = (this.UserId > 0) ? this.UserDefaultPageSize : this.ModuleSettings.PageSize;
            if (this.pageSize < 5)
            {
                this.pageSize = 10;
            }

            this.rowIndex = (this.PageId - 1) * this.pageSize;

            // Build the list of forums to search
            // An intersection of the forums allows vs forums requested.
            var parseId = 0;

            var sForumsAllowed = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForumsForUser(this.ModuleId, this.ForumUser, DotNetNuke.Modules.ActiveForums.SecureActions.Read).FromHashSetToDelimitedString<int>(";");
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
                forumsToSearch = forumsRequested.Where(forumsAllowed.Contains).Aggregate(forumsToSearch, (current, f) => current + (current.Length == 0 ? string.Empty : ":") + f);
            }

            const int maxCacheHours = 1;
            this.searchResults = SearchController.Provider.Search(
                portalId: this.PortalId,
                moduleId: this.ModuleId,
                userId: this.UserId,
                searchId: this.SearchId,
                rowIndex: this.rowIndex,
                pageSize: this.pageSize,
                searchText: this.SearchText,
                searchDays: this.SearchDays,
                authorUserId: this.AuthorUserId,
                authorUsername: this.AuthorUsername,
                forumsToSearch: forumsToSearch,
                tags: this.Tags,
                resultType: this.SearchResultType,
                sort: this.SearchSortType,
                maxCacheHours: maxCacheHours,
                fullText: this.ModuleSettings.FullText);

            this.searchId = (this.searchResults != null) ? this.searchResults.SearchId : 0;

            var totalSeconds = new TimeSpan(0, 0, 0, 0, this.searchResults.SearchDuration).TotalSeconds;
            var ageInMinutes = new TimeSpan(0, 0, 0, 0, this.searchResults.SearchAge).TotalMinutes;

            this.litSearchDuration.Text = string.Format(this.GetSharedResource("[RESX:SearchDuration]"), totalSeconds);

            if (ageInMinutes > 0.25)
            {
                this.litSearchAge.Text = string.Format(this.GetSharedResource("[RESX:SearchAge]"), ageInMinutes);
            }

            this.parameters = null; // We reset this so we make sure to get an updated version

            if (this.searchResults?.Results?.Count > 0)
            {
                this.litRecordCount.Text = string.Format(this.GetSharedResource("[RESX:SearchRecords]"), this.rowIndex + 1, this.rowIndex + this.searchResults?.Results?.Count, this.searchResults?.Results?.Count);

                var rptResults = this.SearchResultType.Equals(DotNetNuke.Modules.ActiveForums.Enums.SearchResultType.SearchByTopics) ? this.rptTopics : this.rptPosts;

                this.pnlMessage.Visible = false;

                try
                {
                    rptResults.Visible = true;
                    rptResults.DataSource = this.searchResults?.Results;
                    rptResults.DataBind();
                    this.BuildPager(this.PagerTop, this.searchResults.Results.Count);
                    this.BuildPager(this.PagerBottom, this.searchResults.Results.Count);
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

        private void LinkControls(ControlCollection ctrls)
        {
            foreach (Control ctrl in ctrls)
            {
                switch (ctrl.ID)
                {
                    case "litKeywords":
                        this.litKeywords = (Literal)ctrl;
                        break;
                    case "litTag":
                        this.litTag = (Literal)ctrl;
                        break;
                    case "litUserName":
                        this.litUserName = (Literal)ctrl;
                        break;
                    case "litMessage":
                        this.litMessage = (Literal)ctrl;
                        break;
                    case "litSearchDuration":
                        this.litSearchDuration = (Literal)ctrl;
                        break;
                    case "litSearchAge":
                        this.litSearchAge = (Literal)ctrl;
                        break;
                    case "litRecordCount":
                        this.litRecordCount = (Literal)ctrl;
                        break;
                    case "rptTopics":
                        this.rptTopics = (Repeater)ctrl;
                        break;
                    case "rptPosts":
                        this.rptPosts = (Repeater)ctrl;
                        break;
                    case "phKeywords":
                        this.phKeywords = (PlaceHolder)ctrl;
                        break;
                    case "phTag":
                        this.phTag = (PlaceHolder)ctrl;
                        break;
                    case "phUsername":
                        this.phUsername = (PlaceHolder)ctrl;
                        break;
                    case "plhMessage":
                        this.plhMessage = (PlaceHolder)ctrl;
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

        private void BuildPager(PagerNav pager, int rowCount)
        {
            var intPages = Convert.ToInt32(Math.Ceiling(rowCount / (double)this.pageSize));

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
            var @params = new List<string> { $"{ParamKeys.ViewType}=searchadvanced" };
            @params.AddRange(this.Parameters);

            if (this.SocialGroupId > 0)
            {
                @params.Add($"GroupId={this.SocialGroupId}");
            }

            return this.NavigateUrl(this.TabId, string.Empty, @params.ToArray());
        }

        #endregion

        #region "Deprecated"
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]

        public string GetPostSnippet() => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public string GetForumUrl() => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")] 

        public string GetThreadUrl() => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")] 

        public string GetPostUrl() => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")] 

        public string GetIcon() => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")] 

        public string GetLastPostAuthor() => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")] 

        public string GetLastPostTime() => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public DataRow Get_currentRow() => this.currentRow;

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")] 
        public int GetSocialGroupId() => this.SocialGroupId;

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")] 
        public string GetMiniPager() => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")] 
        public class Keyword
        {
            [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")] 
            public string Value { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")] 
            public string HtmlEncodedValue => throw new NotImplementedException();
        }

        #endregion
    }
}
