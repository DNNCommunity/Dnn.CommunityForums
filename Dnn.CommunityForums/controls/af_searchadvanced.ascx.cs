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
    using System.Text.RegularExpressions;
    using System.Web.UI.WebControls;

    using DotNetNuke.Modules.ActiveForums.Data;
    using DotNetNuke.Web.Client.ClientResourceManagement;

    public partial class af_searchadvanced : ForumBase
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
        private int? _sort;

        #endregion

        #region Properties

        private string SearchText
        {
            get
            {
                if(this._searchText == null)
                {
                    this._searchText = this.Request.Params[SearchParamKeys.Query] + string.Empty;
                    this._searchText = Utilities.XSSFilter(this._searchText);
                    this._searchText = Utilities.StripHTMLTag(this._searchText);
                    this._searchText = Utilities.CheckSqlString(this._searchText);
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
                if(!this._searchType.HasValue)
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

        #endregion

        #region Event Handlers

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            ClientResourceManager.RegisterScript(this.Page, Globals.ModulePath + "scripts/jquery-forumSelector.js");

            try
            {

                if (this.Request.QueryString[Literals.GroupId] != null && SimulateIsNumeric.IsNumeric(this.Request.QueryString[Literals.GroupId]))
                {
                    this.SocialGroupId = Convert.ToInt32(this.Request.QueryString[Literals.GroupId]);
                }

                this.btnSearch.Click += this.btnSearch_Click;
                this.btnSearch2.Click += this.btnSearch_Click;

                if (this.Page.IsPostBack)
                {
                    return;
                }

                // Bind the intial values for the forum

                // Options

                this.litInputError.Text = this.GetSharedResource("[RESX:SearchInputError]");

                this.litOptions.Text = this.GetSharedResource("[RESX:SearchOptions]");

                this.lblSearch.Text = this.GetSharedResource("[RESX:SearchKeywords]");
                this.txtSearch.Text = this.SearchText;

                ListItem selectedItem = this.drpSearchColumns.Items.FindByValue(this.SearchColumns.ToString());
                if (selectedItem != null)
                {
                    selectedItem.Selected = true;
                }

                selectedItem = this.drpSearchType.Items.FindByValue(this.SearchType.ToString());
                if (selectedItem != null)
                {
                    selectedItem.Selected = true;
                }

                this.lblUserName.Text = this.GetSharedResource("[RESX:SearchByUser]");
                this.txtUserName.Text = this.AuthorUsername;

                this.lblTags.Text = this.GetSharedResource("[RESX:SearchByTag]");
                this.txtTags.Text = this.Tags;

                // Additional Options

                this.litAdditionalOptions.Text = this.GetSharedResource("[RESX:SearchOptionsAdditional]");

                this.lblForums.Text = this.GetSharedResource("[RESX:SearchInForums]");
                this.BindForumList();

                this.lblSearchDays.Text = this.GetSharedResource("[RESX:SearchTimeFrame]");
                this.BindSearchRange();

                this.lblResultType.Text = this.GetSharedResource("[RESX:SearchResultType]");
                selectedItem = this.drpResultType.Items.FindByValue(this.ResultType.ToString());
                if (selectedItem != null)
                {
                    selectedItem.Selected = true;
                }

                this.lblSortType.Text = this.GetSharedResource("[RESX:SearchSort]");
                selectedItem = this.drpSort.Items.FindByValue(this.Sort.ToString());
                if (selectedItem != null)
                {
                    selectedItem.Selected = true;
                }

                // Buttons

                this.btnSearch.Text =  this.GetSharedResource("[RESX:Search]");
                this.btnSearch2.Text = this.GetSharedResource("[RESX:Search]");

                this.btnReset.InnerText = this.GetSharedResource("[RESX:Reset]");
                this.btnReset2.InnerText = this.GetSharedResource("[RESX:Reset]");

                // Update Meta Data
                var basePage = this.BasePage;
                Environment.UpdateMeta(ref basePage, "[VALUE] - " + this.GetSharedResource("[RESX:SearchAdvanced]"), "[VALUE]", "[VALUE]");
            }
            catch (Exception ex)
            {
                this.Controls.Clear();
                this.RenderMessage("[RESX:ERROR]", "[RESX:ERROR:Search]", ex.Message, ex);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            var searchText = this.txtSearch.Text;
            var authorUsername = this.txtUserName.Text;
            var tags = this.txtTags.Text;

            if (string.IsNullOrWhiteSpace(searchText) && string.IsNullOrWhiteSpace(authorUsername) && string.IsNullOrWhiteSpace(tags))
            {
                return;
            }

            // Query
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                searchText = Utilities.XSSFilter(searchText);
                searchText = Utilities.StripHTMLTag(searchText);
                searchText = Utilities.CheckSqlString(searchText);
                searchText = searchText.Trim();
            }

            // Author Name
            if (!string.IsNullOrWhiteSpace(authorUsername))
            {
                authorUsername = this.txtUserName.Text;
                authorUsername = Utilities.CheckSqlString(authorUsername);
                authorUsername = Utilities.XSSFilter(authorUsername);
                authorUsername = authorUsername.Trim();
            }

            // Tags
            if (!string.IsNullOrWhiteSpace(tags))
            {
                tags = Utilities.XSSFilter(tags);
                tags = Utilities.StripHTMLTag(tags);
                tags = Utilities.CheckSqlString(tags);
                tags = tags.Trim();
            }

            // Search Type, Search Column & Search Days
            var searchType = Convert.ToInt32(this.drpSearchType.SelectedItem.Value);
            var searchColumns = Convert.ToInt32(this.drpSearchColumns.SelectedItem.Value);
            var searchDays = Convert.ToInt32(this.drpSearchDays.SelectedItem.Value);
            var resultType = Convert.ToInt32(this.drpResultType.SelectedItem.Value);
            var sort = Convert.ToInt32(this.drpSort.SelectedValue);

            // Selected Forums
            var forums = string.Empty;

            // If the "All" item is selected, we don't need pass the forums parameter
            if(!this.lbForums.Items[0].Selected)
            {
                var forumId = 0;

                foreach (var item in this.lbForums.Items.Cast<ListItem>().Where(item => item.Selected && int.TryParse(Regex.Replace(item.Value, @"F(\d+)G\d+", "$1"), out forumId)))
                {
                    if(forums != string.Empty)
                    {
                        forums += ":";
                    }

                    forums += forumId;
                }
            }

            var @params = new List<string> { $"{ParamKeys.ViewType}={Views.Search}" };

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                @params.Add($"{SearchParamKeys.Query}={System.Web.HttpUtility.UrlEncode(searchText)}");
            }

            if (!string.IsNullOrWhiteSpace(tags))
            {
                @params.Add($"{SearchParamKeys.Tag}={System.Web.HttpUtility.UrlEncode(tags)}");
            }

            if (searchType > 0)
            {
                @params.Add($"{SearchParamKeys.SearchType}={searchType}");
            }

            if (searchColumns > 0)
            {
                @params.Add($"{SearchParamKeys.Columns}={searchColumns}");
            }

            if(searchDays > 0)
            {
                @params.Add($"{SearchParamKeys.TimeSpan}={searchDays}");
            }

            if(resultType > 0)
            {
                @params.Add($"{SearchParamKeys.ResultType}={resultType}");
            }

            if(sort > 0)
            {
                @params.Add($"{SearchParamKeys.Sort}={sort}");
            }

            if(!string.IsNullOrWhiteSpace(authorUsername))
            {
                @params.Add($"{SearchParamKeys.Author}={System.Web.HttpUtility.UrlEncode(authorUsername)}");
            }

            if (!string.IsNullOrWhiteSpace(forums))
            {
                @params.Add($"{SearchParamKeys.Forums}={System.Web.HttpUtility.UrlEncode(forums)}");
            }

            if (this.SocialGroupId > 0)
            {
                @params.Add($"{Literals.GroupId}={this.SocialGroupId}");
            }

            this.Response.Redirect(this.NavigateUrl(this.TabId, string.Empty, @params.ToArray()));
        }

        #endregion

        #region Private Methods

        private void BindSearchRange()
        {
            var sHours = this.GetSharedResource("SearchRangeHours.Text");
            var sDays = this.GetSharedResource("SearchRangeDays.Text");
            var sAll = this.GetSharedResource("SearchRangeAll.Text");

            this.drpSearchDays.Items.Clear();
            this.drpSearchDays.Items.Add(new ListItem(sAll, "0"));
            this.drpSearchDays.Items.Add(new ListItem(string.Format(sHours, "12"), "12"));
            this.drpSearchDays.Items.Add(new ListItem(string.Format(sHours, "24"), "24"));
            this.drpSearchDays.Items.Add(new ListItem(string.Format(sDays, "3"), "72"));
            this.drpSearchDays.Items.Add(new ListItem(string.Format(sDays, "5"), "124"));
            this.drpSearchDays.Items.Add(new ListItem(string.Format(sDays, "10"), "240"));
            this.drpSearchDays.Items.Add(new ListItem(string.Format(sDays, "15"), "360"));
            this.drpSearchDays.Items.Add(new ListItem(string.Format(sDays, "30"), "720"));
            this.drpSearchDays.Items.Add(new ListItem(string.Format(sDays, "45"), "1080"));
            this.drpSearchDays.Items.Add(new ListItem(string.Format(sDays, "60"), "1440"));
            this.drpSearchDays.Items.Add(new ListItem(string.Format(sDays, "90"), "2160"));
            this.drpSearchDays.Items.Add(new ListItem(string.Format(sDays, "120"), "2880"));
            this.drpSearchDays.Items.Add(new ListItem(string.Format(sDays, "240"), "5760"));
            this.drpSearchDays.Items.Add(new ListItem(string.Format(sDays, "365"), "8640"));

            var selectItem = this.drpSearchDays.Items.FindByValue(this.SearchDays.ToString());
            if (selectItem != null)
            {
                selectItem.Selected = true;
            }
        }

        private void BindForumList()
        {
            this.lbForums.Items.Clear();

            int parseId;
            var forumsToSearch = this.Forums.Split(':').Where(o => int.TryParse(o, out parseId) && parseId > 0).Select(int.Parse).ToList();

            // Add the "All Forums" item
            this.lbForums.Items.Add(new ListItem("All Forums", "0") { Selected = forumsToSearch.Count == 0 && this.ForumId <= 0 });

            var forums = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetForums(this.ForumModuleId);
            DotNetNuke.Modules.ActiveForums.Controllers.ForumController.IterateForumsList(forums, this.ForumUser,
                fi => {
                this.lbForums.Items.Add(new ListItem(fi.GroupName, $"G{fi.ForumGroupId}"));
                },
                fi => {
                this.lbForums.Items.Add(new ListItem($"{fi.ForumName}", $"F{fi.ForumID}G{fi.ForumGroupId}") { Selected = forumsToSearch.Contains(fi.ForumID) || this.ForumId == fi.ForumID });
                },
                fi => {
                this.lbForums.Items.Add(new ListItem($"--{fi.ForumName}", $"F{fi.ForumID}G{fi.ForumGroupId}") { Selected = forumsToSearch.Contains(fi.ForumID) || this.ForumId == fi.ForumID } );
                }
                );
        }

        #endregion
    }
}
