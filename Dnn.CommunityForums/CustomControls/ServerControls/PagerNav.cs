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
namespace DotNetNuke.Modules.ActiveForums.Controls
{
    using System;
    using System.ComponentModel;
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    [DefaultProperty("Text"), ToolboxData("<{0}:pagernav runat=server></{0}:pagernav>")]
    public class PagerNav : WebControl
    {
        public enum Mode
        {
            Links,
            CallBack,
        }

        public bool UseShortUrls { get; set; }

        public Mode PageMode { get; set; }

        public string BaseURL { get; set; }

        [Bindable(true), Category("Appearance"), DefaultValue("")]
        public int PageCount { get; set; }

        [Bindable(true), Category("Appearance"), DefaultValue("")]
        public int ForumID { get; set; }

        [Bindable(true), Category("Appearance"), DefaultValue("0")]
        public int TopicId { get; set; }

        [Bindable(true), Category("Appearance"), DefaultValue("")]
        public int TabID { get; set; }

        [Bindable(true), Category("Appearance"), DefaultValue("")]
        public string Text { get; set; }

        [Bindable(true), Category("Appearance"), DefaultValue("")]
        public string View { get; set; }

        [Bindable(true), Category("Appearance"), DefaultValue("Page:")]
        public string PageText { get; set; }

        [Bindable(true), Category("Appearance"), DefaultValue("of")]
        public string OfText { get; set; }

        [Bindable(true), Category("Appearance"), DefaultValue("")]
        public int CurrentPage { get; set; }

        [Bindable(true), Category("Appearance"), DefaultValue("")]
        public string[] Params { get; set; }

        [Bindable(true), Category("Appearance"), DefaultValue("")]
        public string Optional2 { get; set; }

        public string ClientScript
        {
            get
            {
                return "afPage({0});";
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.EnableViewState = false;
        }

        protected override void Render(HtmlTextWriter output)
        {
            this.Text = this.RenderPager();
            output.Write(this.Text);
        }

        public string RenderPager()
        {
            var sb = new StringBuilder();

            this.PageMode = Mode.Links;

            if (!string.IsNullOrEmpty(this.BaseURL))
            {
                if (!this.BaseURL.EndsWith("/"))
                {
                    this.BaseURL += "/";
                }
            }

            var qs = string.Empty;
            if (HttpContext.Current.Request.QueryString[ParamKeys.TimeSpan] != null)
            {
                qs = $"?{ParamKeys.TimeSpan}={HttpContext.Current.Request.QueryString[ParamKeys.TimeSpan]}";
            }

            if (this.PageCount > 1)
            {
                sb.Append("<table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" class=\"afpager\"><tr>");
                sb.Append("<td class=\"af_pager\">" + this.PageText + " " + this.CurrentPage + " " + this.OfText + " " + this.PageCount + "</td>");
                if (this.CurrentPage != 1)
                {
                    if (this.PageMode == Mode.Links)
                    {
                        if (string.IsNullOrEmpty(this.BaseURL))
                        {
                            sb.Append("<td class=\"af_pagernumber\" style=\"text-align:center;\"><a href=\"" + Utilities.NavigateURL(this.TabID, string.Empty, this.BuildParams(this.View, this.ForumID, 1, this.TopicId)) + "\" title=\"First Page\"> &lt;&lt; </a></td>");
                            sb.Append("<td class=\"af_pagernumber\" style=\"text-align:center;\"><a href=\"" + Utilities.NavigateURL(this.TabID, string.Empty, this.BuildParams(this.View, this.ForumID, this.CurrentPage - 1, this.TopicId)) + "\" title=\"Previous Page\"> &lt; </a></td>");
                        }
                        else
                        {
                            sb.Append("<td class=\"af_pagernumber\" style=\"text-align:center;\"><a href=\"" + this.BaseURL + qs + "\" title=\"First Page\"> &lt;&lt; </a></td>");
                            sb.Append("<td class=\"af_pagernumber\" style=\"text-align:center;\"><a href=\"" + this.BaseURL + (this.CurrentPage - 1) + "/" + qs + "\" title=\"Previous Page\"> &lt; </a></td>");
                        }
                    }
                    else
                    {
                        sb.Append("<td class=\"af_pagernumber\" style=\"text-align:center;\"><a href=\"javascript:" + string.Format(this.ClientScript, "1") + "\" title=\"First Page\"> &lt;&lt; </a></td>");
                        sb.Append("<td class=\"af_pagernumber\" style=\"text-align:center;\"><a href=\"javascript:" + string.Format(this.ClientScript, this.CurrentPage - 1) + "\" title=\"Previous Page\"> &lt; </a></td>");
                    }
                }

                int iStart;
                int iMaxPage;

                if (this.CurrentPage <= 3)
                {
                    iStart = 1;
                    iMaxPage = 5;
                }
                else
                {
                    iStart = this.CurrentPage - 2;
                    iMaxPage = this.CurrentPage + 2;
                }

                if (iMaxPage > this.PageCount)
                {
                    iMaxPage = this.PageCount;
                }

                if (iMaxPage == this.PageCount)
                {
                    iStart = iMaxPage - 4;
                }

                if (iStart <= 0)
                {
                    iStart = 1;
                }

                int i;
                for (i = iStart; i <= iMaxPage; i++)
                {
                    if (i == this.CurrentPage)
                    {
                        sb.Append("<td class=\"af_currentpage\" style=\"text-align:center;\">" + i + "</td>");
                    }
                    else
                    {
                        if (this.PageMode == Mode.Links)
                        {
                            if (string.IsNullOrEmpty(this.BaseURL))
                            {
                                sb.Append("<td class=\"af_pagernumber\" style=\"text-align:center;\"><a href=\"" + Utilities.NavigateURL(this.TabID, string.Empty, this.BuildParams(this.View, this.ForumID, i, this.TopicId)) + "\">" + i + "</a></td>");
                            }
                            else
                            {
                                if (i > 1)
                                {
                                    sb.Append("<td class=\"af_pagernumber\" style=\"text-align:center;\"><a href=\"" + this.BaseURL + i + "/" + qs + "\">" + i + "</a></td>");
                                }
                                else
                                {
                                    sb.Append("<td class=\"af_pagernumber\" style=\"text-align:center;\"><a href=\"" + this.BaseURL + qs + "\">" + i + "</a></td>");
                                }
                            }
                        }
                        else
                        {
                            sb.Append("<td class=\"af_pagernumber\" style=\"text-align:center;\"><a href=\"javascript:" + string.Format(this.ClientScript, i) + "\">" + i + "</a></td>");
                        }

                    }

                    if (i == this.PageCount)
                    {
                        break;
                    }
                }

                if (this.CurrentPage != this.PageCount)
                {
                    if (this.PageMode == Mode.Links)
                    {
                        if (string.IsNullOrEmpty(this.BaseURL))
                        {
                            sb.Append("<td class=\"af_pagernumber\" style=\"text-align:center;\"><a href=\"" + Utilities.NavigateURL(this.TabID, string.Empty, this.BuildParams(this.View, this.ForumID, this.CurrentPage + 1, this.TopicId)) + "\" title=\"Next Page\"> &gt;</a></td>");
                            sb.Append("<td class=\"af_pagernumber\" style=\"text-align:center;\"><a href=\"" + Utilities.NavigateURL(this.TabID, string.Empty, this.BuildParams(this.View, this.ForumID, this.PageCount, this.TopicId)) + "\" title=\"Last Page\"> &gt;&gt;</a></td>");
                        }
                        else
                        {
                            sb.Append("<td class=\"af_pagernumber\" style=\"text-align:center;\"><a href=\"" + this.BaseURL + (this.CurrentPage + 1) + "/" + qs + "\" title=\"Next Page\"> &gt;</a></td>");
                            sb.Append("<td class=\"af_pagernumber\" style=\"text-align:center;\"><a href=\"" + this.BaseURL + this.PageCount + "/" + qs + "\" title=\"Last Page\"> &gt;&gt;</a></td>");
                        }
                    }
                    else
                    {
                        sb.Append("<td class=\"af_pagernumber\" style=\"text-align:center;\"><a href=\"javascript:" + string.Format(this.ClientScript, this.CurrentPage + 1) + "\" title=\"Next Page\"> &gt;</a></td>");
                        sb.Append("<td class=\"af_pagernumber\" style=\"text-align:center;\"><a href=\"javascript:" + string.Format(this.ClientScript, this.PageCount) + "\"> &gt;&gt;</a></td>");
                    }
                }

                sb.Append("</tr></table>");
            }

            return sb.ToString();
        }

        private string[] BuildParams(string view, int forumID, int page, int postID = 0)
        {
            string[] params2;
            if (view.ToLowerInvariant() == Views.Topics.ToLowerInvariant())
            {
                if (page > 1)
                {
                    params2 = new[] { ParamKeys.ViewType + "=" + view, ParamKeys.ForumId + "=" + forumID, ParamKeys.PageId + "=" + page };
                }
                else
                {
                    params2 = new[] { ParamKeys.ViewType + "=" + view, ParamKeys.ForumId + "=" + forumID };
                }

                if (this.UseShortUrls && page > 1)
                {
                    params2 = new[] { ParamKeys.ForumId + "=" + forumID, ParamKeys.PageId + "=" + page };
                }
                else if (this.UseShortUrls && page == 1)
                {
                    params2 = new[] { ParamKeys.ForumId + "=" + forumID };
                }

                if (this.Params != null)
                {
                    var intLength = params2.Length;
                    Array.Resize(ref params2, intLength + this.Params.Length);
                    this.Params.CopyTo(params2, intLength);
                }

            }
            else if (view.ToLowerInvariant() == Views.Topic.ToLowerInvariant())
            {
                if (page > 1)
                {
                    params2 = new[] { ParamKeys.ViewType + "=" + view, ParamKeys.ForumId + "=" + forumID, ParamKeys.TopicId + "=" + postID, ParamKeys.PageId + "=" + page };
                }
                else
                {
                    params2 = new[] { ParamKeys.ViewType + "=" + view, ParamKeys.ForumId + "=" + forumID, ParamKeys.TopicId + "=" + postID };
                }

                if (this.UseShortUrls && page > 1)
                {
                    params2 = new[] { ParamKeys.TopicId + "=" + postID, ParamKeys.PageId + "=" + page };
                }
                else if (this.UseShortUrls && page == 1)
                {
                    params2 = new[] { ParamKeys.TopicId + "=" + postID };
                }

                if (this.Params != null)
                {
                    var intLength = params2.Length;
                    Array.Resize(ref params2, intLength + this.Params.Length);
                    this.Params.CopyTo(params2, intLength);
                }
            }
            else
            {
                if (this.Params != null)
                {
                    params2 = new[] { ParamKeys.ViewType + "=" + view, ParamKeys.PageId + "=" + page };
                    var intLength = params2.Length;
                    Array.Resize(ref params2, intLength + this.Params.Length);
                    this.Params.CopyTo(params2, intLength);
                }
                else
                {
                    params2 = new[] { ParamKeys.ViewType + "=" + view, ParamKeys.PageId + "=" + page };
                }
            }

            return params2;
        }

    }
}
