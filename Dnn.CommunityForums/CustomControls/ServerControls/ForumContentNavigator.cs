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

    [DefaultProperty("Text"), ToolboxData("<{0}:ForumContentNavigator runat=server></{0}:ForumContentNavigator>")]
    public class ForumContentNavigator : WebControl
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

        private int portalId = -1;

        public int PortalId
        {
            get
            {
                return this.portalId;
            }

            set
            {
                this.portalId = value;
            }
        }

        private int moduleId = -1;

        public int ModuleId
        {
            get
            {
                return this.moduleId;
            }

            set
            {
                this.moduleId = value;
            }
        }

        private int tabId = -1;

        public int TabId
        {
            get
            {
                return this.tabId;
            }

            set
            {
                this.tabId = value;
            }
        }

        private int forumId = -1;

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

        private int forumGroupId = -1;

        public int ForumGroupId
        {
            get
            {
                return this.forumGroupId;
            }

            set
            {
                this.forumGroupId = value;
            }
        }

        private int parentForumId = -1;

        public int ParentForumId
        {
            get
            {
                return this.parentForumId;
            }

            set
            {
                this.parentForumId = value;
            }
        }

        private bool includeClasses = true;

        public bool IncludeClasses
        {
            get
            {
                return this.includeClasses;
            }

            set
            {
                this.includeClasses = value;
            }
        }

        private User forumUser = null;

        protected override void Render(HtmlTextWriter writer)
        {
            UserController uc = new UserController();
            this.forumUser = uc.GetUser(this.PortalId, this.ModuleId);
            Controls.ForumContent fd = new Controls.ForumContent();
            fd.ModuleId = this.ModuleId;
            fd.TabId = this.TabId;
            fd.PortalId = this.PortalId;
            fd.ForumUser = this.forumUser;
            fd.ForumId = this.ForumId;
            fd.ForumGroupId = this.ForumGroupId;
            fd.ParentForumId = this.ParentForumId;
            fd.IncludeClasses = this.IncludeClasses;
            if (HttpContext.Current.Request.QueryString[ParamKeys.TopicId] != null)
            {
                fd.TopicId = int.Parse(HttpContext.Current.Request.QueryString[ParamKeys.TopicId]);
            }

            if (this.ItemTemplate != null)
            {
                fd.ItemTemplate = this.ItemTemplate.Text;
            }

            if (this.HeaderTemplate != null)
            {
                fd.HeaderTemplate = this.HeaderTemplate.Text;
            }

            if (this.FooterTemplate != null)
            {
                fd.FooterTemplate = this.FooterTemplate.Text;
            }

            writer.Write(fd.Render());
        }

    }

}
