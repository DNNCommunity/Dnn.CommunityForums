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
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    [ToolboxData("<{0}:TopicSort runat=server></{0}:TopicSort>")]
    public class TopicSort : WebControl
    {
        private string defaultSort = "ASC";
        private int forumId = -1;
        private int topicId = -1;
        protected DropDownList drpSort;

        public string DefaultSort
        {
            get
            {
                return this.defaultSort;
            }

            set
            {
                this.defaultSort = value;
            }
        }

        public int ForumId
        {
            get
            {
                return this.forumId;
            }

            set
            {
                this.forumId = value;
            }
        }

        public int TopicId
        {
            get
            {
                return this.topicId;
            }

            set
            {
                this.topicId = value;
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            this.drpSort.RenderControl(writer);
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.drpSort.SelectedIndexChanged += new System.EventHandler(this.drpSort_SelectedIndexChanged);

            // EnableViewState = False
            this.drpSort = new DropDownList();
            this.drpSort.ID = "drpSort";
            this.drpSort.AutoPostBack = true;
            this.drpSort.CssClass = this.CssClass;
            this.drpSort.Items.Add(new ListItem("[RESX:TopicSortOldest]", "ASC"));
            this.drpSort.Items.Add(new ListItem("[RESX:TopicSortNewest]", "DESC"));
            this.Controls.Add(this.drpSort);
        }

        private void drpSort_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            string sort = this.drpSort.SelectedItem.Value;
            int tabId = int.Parse(HttpContext.Current.Request.QueryString["TabId"]);

            HttpContext.Current.Response.Redirect(Utilities.NavigateURL(tabId, "", new string[] { ParamKeys.ViewType + "=" + Views.Topic, ParamKeys.ForumId + "=" + this.ForumId, ParamKeys.TopicId + "=" + this.TopicId, ParamKeys.Sort + "=" + sort }));
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!this.Page.IsPostBack)
            {

                string sort = this.DefaultSort;
                if (HttpContext.Current.Request.Params[ParamKeys.Sort] != null)
                {
                    sort = HttpContext.Current.Request.Params[ParamKeys.Sort];
                }

                this.drpSort.SelectedIndex = this.drpSort.Items.IndexOf(this.drpSort.Items.FindByValue(sort));
            }
        }
    }

}
