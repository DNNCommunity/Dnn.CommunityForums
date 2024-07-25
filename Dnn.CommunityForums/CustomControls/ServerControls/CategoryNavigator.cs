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

    [DefaultProperty("Text"), ToolboxData("<{0}:CategoryNavigator runat=server></{0}:CategoryNavigator>")]
    public class CategoryNavigator : WebControl
    {
        private DisplayTemplate _itemTemplate;

        public DisplayTemplate ItemTemplate
        {
            get
            {
                return this._itemTemplate;
            }

            set
            {
                this._itemTemplate = value;
            }
        }

        private DisplayTemplate _headerTemplate;

        public DisplayTemplate HeaderTemplate
        {
            get
            {
                return this._headerTemplate;
            }

            set
            {
                this._headerTemplate = value;
            }
        }

        private DisplayTemplate _footerTemplate;

        public DisplayTemplate FooterTemplate
        {
            get
            {
                return this._footerTemplate;
            }

            set
            {
                this._footerTemplate = value;
            }
        }

        private int _renderMode = 0;

        public int RenderMode
        {
            get
            {
                return this._renderMode;
            }

            set
            {
                this._renderMode = value;
            }
        }

        private int _portalId = -1;

        public int PortalId
        {
            get
            {
                return this._portalId;
            }

            set
            {
                this._portalId = value;
            }
        }

        private int _moduleId = -1;

        public int ModuleId
        {
            get
            {
                return this._moduleId;
            }

            set
            {
                this._moduleId = value;
            }
        }

        private int _tabId = -1;

        public int TabId
        {
            get
            {
                return this._tabId;
            }

            set
            {
                this._tabId = value;
            }
        }

        private int _forumId = -1;

        public int ForumId
        {
            get
            {
                return this._forumId;
            }

            set
            {
                this._forumId = value;
            }
        }

        private int _forumGroupId = -1;

        public int ForumGroupId
        {
            get
            {
                return this._forumGroupId;
            }

            set
            {
                this._forumGroupId = value;
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            Controls.CategoriesList tb = new Controls.CategoriesList(this.PortalId, this.ModuleId, this.ForumId, this.ForumGroupId);
            tb.TabId = this.TabId;
            tb.Template = this.ItemTemplate.Text;
            tb.HeaderTemplate = this.HeaderTemplate.Text;
            tb.FooterTemplate = this.FooterTemplate.Text;
            tb.CSSClass = this.CssClass;
            if (HttpContext.Current.Request.QueryString[ParamKeys.Category] != null && SimulateIsNumeric.IsNumeric(HttpContext.Current.Request.QueryString[ParamKeys.Category]))
            {
                tb.SelectedCategory = int.Parse(HttpContext.Current.Request.QueryString[ParamKeys.Category]);
            }

            if (this.RenderMode == 0)
            {
                writer.Write(tb.RenderView());
            }
            else
            {
                writer.Write(tb.RenderEdit());
            }

        }

    }
}
