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
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web.UI.WebControls;

    using DotNetNuke.Web.Client.ClientResourceManagement;

    public partial class af_searchadvanced : ForumBase
    {
        #region Private Members

        private string searchText;
        private string tags;
        private DotNetNuke.Modules.ActiveForums.Enums.SearchResultType? searchResultType;
        private DotNetNuke.Modules.ActiveForums.Enums.SearchSortType? searchSortType;
        private string authorUsername;
        private string forums;
        private int? searchDays;

        #endregion

        #region Properties

        private string SearchText => (string)(this.searchText ?? (this.searchText = Utilities.CheckSqlString(Utilities.StripHTMLTag(Utilities.XSSFilter(this.Request.Params[SearchParamKeys.Query] + string.Empty))).Replace("\"", string.Empty).Trim()));

        private string AuthorUsername => (string)(this.authorUsername ?? (this.authorUsername = Utilities.CheckSqlString(Utilities.StripHTMLTag(Utilities.XSSFilter(this.Request.Params[SearchParamKeys.Author] + string.Empty))).Trim()));

        private DotNetNuke.Modules.ActiveForums.Enums.SearchResultType SearchResultType => (DotNetNuke.Modules.ActiveForums.Enums.SearchResultType)(this.searchResultType ?? (this.searchResultType = (DotNetNuke.Modules.ActiveForums.Enums.SearchResultType)Utilities.SafeConvertInt(this.Request.Params[SearchParamKeys.ResultType], (int)DotNetNuke.Modules.ActiveForums.Enums.SearchResultType.SearchByTopics)));

        private DotNetNuke.Modules.ActiveForums.Enums.SearchSortType SearchSortType => (DotNetNuke.Modules.ActiveForums.Enums.SearchSortType)(this.searchSortType ?? (this.searchSortType = (DotNetNuke.Modules.ActiveForums.Enums.SearchSortType)Utilities.SafeConvertInt(this.Request.Params[SearchParamKeys.Sort], (int)DotNetNuke.Modules.ActiveForums.Enums.SearchSortType.SearchSortTypeRelevance)));

        private string Forums => (string)(this.forums ?? (this.forums = Utilities.CheckSqlString(Utilities.StripHTMLTag(Utilities.XSSFilter(this.Request.Params[SearchParamKeys.Forums] + string.Empty))).Trim()));

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
        #endregion

        #region Event Handlers

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            ClientResourceManager.RegisterScript(this.Page, Globals.ModulePath + "scripts/jquery-forumSelector.js");

            try
            {
                if (this.Request.QueryString[Literals.GroupId] != null && Utilities.IsNumeric(this.Request.QueryString[Literals.GroupId]))
                {
                    this.SocialGroupId = Convert.ToInt32(this.Request.QueryString[Literals.GroupId]);
                }

                this.btnSearch.Click += this.btnSearch_Click;

                if (this.Page.IsPostBack)
                {
                    return;
                }

                this.txtSearch.Text = this.SearchText;
                this.txtUserName.Text = this.AuthorUsername;
                this.txtTags.Text = this.Tags;

                this.BindForumList();
                this.BindSearchRange();

                Utilities.BindEnum(pDDL: this.drpResultType, enumType: typeof(Enums.SearchResultType), pColValue: ((int)this.SearchResultType).ToString(), addEmptyValue: false, localize: true, excludeIndex: -1);
                Utilities.BindEnum(pDDL: this.drpSort, enumType: typeof(Enums.SearchSortType), pColValue: ((int)this.SearchSortType).ToString(), addEmptyValue: false, localize: true, excludeIndex: -1);

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
            var searchText = Utilities.CheckSqlString(Utilities.StripHTMLTag(Utilities.XSSFilter(this.txtSearch.Text))).Replace("\"", string.Empty).Trim();
            var authorUsername = Utilities.CheckSqlString(Utilities.StripHTMLTag(Utilities.XSSFilter(this.txtUserName.Text))).Trim();
            var tags = Utilities.CheckSqlString(Utilities.StripHTMLTag(Utilities.XSSFilter(this.txtTags.Text))).Trim();
            if (string.IsNullOrWhiteSpace(searchText) && string.IsNullOrWhiteSpace(authorUsername) && string.IsNullOrWhiteSpace(tags))
            {
                return;
            }

            var searchDays = Convert.ToInt32(this.drpSearchDays.SelectedItem.Value);
            var resultType = Convert.ToInt32(this.drpResultType.SelectedItem.Value);
            var sortType = Convert.ToInt32(this.drpSort.SelectedValue);

            // Selected Forums
            var forums = string.Empty;

            // If the "All" item is selected, we don't need pass the forums parameter
            if (!this.lbForums.Items[0].Selected)
            {
                var forumId = 0;

                foreach (var item in this.lbForums.Items.Cast<ListItem>().Where(item => item.Selected && int.TryParse(Regex.Replace(item.Value, @"F(\d+)G\d+", "$1"), out forumId)))
                {
                    if (forums != string.Empty)
                    {
                        forums += ":";
                    }

                    forums += forumId;
                }
            }

            var @params = new List<string> { $"{ParamKeys.ViewType}={Views.Search}" };

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                @params.Add($"{SearchParamKeys.Query}={System.Net.WebUtility.UrlEncode(searchText)}");
            }

            if (!string.IsNullOrWhiteSpace(tags))
            {
                @params.Add($"{SearchParamKeys.Tag}={System.Net.WebUtility.UrlEncode(tags)}");
            }

            if (searchDays > 0)
            {
                @params.Add($"{SearchParamKeys.TimeSpan}={searchDays}");
            }

            if (resultType > 0)
            {
                @params.Add($"{SearchParamKeys.ResultType}={resultType}");
            }

            if (sortType > 0)
            {
                @params.Add($"{SearchParamKeys.Sort}={sortType}");
            }

            if (!string.IsNullOrWhiteSpace(authorUsername))
            {
                @params.Add($"{SearchParamKeys.Author}={System.Net.WebUtility.UrlEncode(authorUsername)}");
            }

            if (!string.IsNullOrWhiteSpace(forums))
            {
                @params.Add($"{SearchParamKeys.Forums}={System.Net.WebUtility.UrlEncode(forums)}");
            }

            if (this.SocialGroupId > 0)
            {
                @params.Add($"{Literals.GroupId}={this.SocialGroupId}");
            }

            this.Response.Redirect(this.NavigateUrl(this.TabId, string.Empty, @params.ToArray()), false);
            this.Context.ApplicationInstance.CompleteRequest();
        }

        #endregion

        #region Private Methods

        private void BindSearchRange()
        {
            var sHours = this.GetSharedResource("[RESX:SearchRangeHours]");
            var sDays = this.GetSharedResource("[RESX:SearchRangeDays]");
            var sAll = this.GetSharedResource("[RESX:SearchRangeAll]");

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
            this.lbForums.Items.Add(new ListItem("[RESX:SearchRangeAll] [RESX:FORUMS]", "0") { Selected = forumsToSearch.Count == 0 && this.ForumId <= 0 });

            var forums = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetForums(this.ForumModuleId);
            DotNetNuke.Modules.ActiveForums.Controllers.ForumController.IterateForumsList(
                forums: forums,
                forumUserInfo: this.ForumUser,
                groupAction: fi =>
                {
                    this.lbForums.Items.Add(new ListItem(fi.GroupName, $"G{fi.ForumGroupId}"));
                },
                forumAction: fi =>
                {
                    this.lbForums.Items.Add(new ListItem($"{fi.ForumName}", $"F{fi.ForumID}G{fi.ForumGroupId}") { Selected = forumsToSearch.Contains(fi.ForumID) || this.ForumId == fi.ForumID });
                },
                subForumAction: fi =>
                {
                    this.lbForums.Items.Add(new ListItem($"--{fi.ForumName}", $"F{fi.ForumID}G{fi.ForumGroupId}") { Selected = forumsToSearch.Contains(fi.ForumID) || this.ForumId == fi.ForumID });
                },
                includeHiddenForums: false,
                includeInactiveForums: false);
        }

        #endregion
    }
}
