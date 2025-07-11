﻿// Copyright (c) by DNN Community
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

namespace DotNetNuke.Modules.ActiveForums.Controls
{
    using System;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    [DefaultProperty("Text"), ToolboxData("<{0}:Link runat=server></{0}:Link>")]
    public class Link : WebControl
    {
        private string text;
        private string navigateUrl;
        private string enabledRoles;
        private string userPermSet;
        private bool authRequired;
        private int pageId;
        private string @params;
        private string title;

        public string NavigateURL
        {
            get
            {
                return this.navigateUrl;
            }

            set
            {
                this.navigateUrl = value;
            }
        }

        public string EnabledRoles
        {
            get
            {
                return this.enabledRoles;
            }

            set
            {
                this.enabledRoles = value;
            }
        }

        public string UserPermSet
        {
            get
            {
                return this.userPermSet;
            }

            set
            {
                this.userPermSet = value;
            }
        }

        public bool AuthRequired
        {
            get
            {
                return this.authRequired;
            }

            set
            {
                this.authRequired = value;
            }
        }

        [Bindable(true), Category("Appearance"), DefaultValue(""), Localizable(true)]
        public string Text
        {
            get
            {
                return this.text;
            }

            set
            {
                this.text = value;
            }
        }

        public int PageId
        {
            get
            {
                return this.pageId;
            }

            set
            {
                this.pageId = value;
            }
        }

        public string Params
        {
            get
            {
                return this.@params;
            }

            set
            {
                this.@params = value;
            }
        }

        public string Title
        {
            get
            {
                return this.title;
            }

            set
            {
                this.title = value;
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (!string.IsNullOrEmpty(this.EnabledRoles))
            {
                if (string.IsNullOrEmpty(this.UserPermSet))
                {
                    this.Visible = false;
                }
                else
                {
                    this.Visible = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasAccess(this.EnabledRoles, this.UserPermSet);
                }
            }

            if (this.AuthRequired && !HttpContext.Current.Request.IsAuthenticated)
            {
                this.Visible = false;
            }

            if (string.IsNullOrEmpty(this.NavigateURL) && string.IsNullOrEmpty(this.Params))
            {
                this.NavigateURL = Utilities.NavigateURL(this.PageId);
            }
            else if (string.IsNullOrEmpty(this.NavigateURL) && !string.IsNullOrEmpty(this.Params))
            {
                this.NavigateURL = Utilities.NavigateURL(this.PageId, string.Empty, this.Params.Split(','));
            }

            string sTitle = " title=";
            if (!string.IsNullOrEmpty(this.Title))
            {
                sTitle += "\"" + this.Title + "\"";
            }
            else
            {
                sTitle = string.Empty;
            }

            string sClass = string.Empty;
            if (!string.IsNullOrEmpty(this.CssClass))
            {
                sClass = " class=\"" + this.CssClass + "\"";
            }

            if (this.Visible)
            {
                if (this.Enabled)
                {
                    writer.Write("<a href=\"" + this.NavigateURL + "\"" + sTitle + sClass + ">");
                }

                writer.Write(this.Text);
                if (this.Enabled)
                {
                    writer.Write("</a>");
                }
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.EnableViewState = false;
        }
    }
}
