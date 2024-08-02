// Copyright (c) 2013-2024 by DNN Community
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
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    [DefaultProperty("Text"), ToolboxData("<{0}:TopicsNavigator runat=server></{0}:TopicsNavigator>")]
    public class TopicsNavigator : ForumBase
    {
        private DisplayTemplate itemTemplate;

        public DisplayTemplate ItemTemplate
        {
            get
            {
                return this.itemTemplate;
            }

            set
            {
                this.itemTemplate = value;
            }
        }

        private DisplayTemplate headerTemplate;

        public DisplayTemplate HeaderTemplate
        {
            get
            {
                return this.headerTemplate;
            }

            set
            {
                this.headerTemplate = value;
            }
        }

        private DisplayTemplate footerTemplate;

        public DisplayTemplate FooterTemplate
        {
            get
            {
                return this.footerTemplate;
            }

            set
            {
                this.footerTemplate = value;
            }
        }

        private bool maintainPage = false;

        public bool MaintainPage
        {
            get
            {
                return this.maintainPage;
            }

            set
            {
                this.maintainPage = value;
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            Controls.TopicBrowser tb = new Controls.TopicBrowser();

            tb.PortalId = this.PortalId;
            tb.ModuleId = this.ForumModuleId;
            tb.TabId = this.ForumTabId;
            if (tb.TabId <= 0)
            {
                tb.TabId = int.Parse(this.Request.QueryString["TabID"]);
            }

            tb.ForumGroupId = this.ForumGroupId;
            tb.ForumId = this.ForumId;

            if (this.ForumId > 0)
            {
                if (DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasAccess(this.ForumInfo.Security.View, this.ForumUser.UserRoles))
                {
                    tb.ForumIds = this.ForumId.ToString();
                }
                else
                {
                    writer.Write(string.Empty);
                    return;
                }
            }
            else
            {
                tb.ForumIds = this.UserForumsList;
            }

            if (this.Request.QueryString["atg"] != null && SimulateIsNumeric.IsNumeric(this.Request.QueryString["atg"]))
            {
                tb.TagId = int.Parse(this.Request.QueryString["atg"]);
            }

            if (this.Request.QueryString[ParamKeys.Category] != null && SimulateIsNumeric.IsNumeric(this.Request.QueryString[ParamKeys.Category]))
            {
                tb.CategoryId = int.Parse(this.Request.QueryString[ParamKeys.Category]);
            }

            tb.ForumUser = this.ForumUser;
            tb.PageIndex = this.PageId;
            tb.PageSize = this.MainSettings.PageSize;
            tb.Template = this.ItemTemplate.Text;
            tb.HeaderTemplate = this.HeaderTemplate.Text;
            tb.FooterTemplate = this.FooterTemplate.Text;
            tb.ImagePath = this.ImagePath;
            tb.TopicId = this.TopicId;
            writer.Write(tb.Render());
        }
    }
}
