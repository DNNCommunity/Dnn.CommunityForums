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

    [DefaultProperty("Text"), ToolboxData("<{0}:ForumNavigator runat=server></{0}:ForumNavigator>")]
    public class ForumNavigator : WebControl
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

        private User forumUser = null;

        protected override void Render(HtmlTextWriter writer)
        {
            UserController uc = new UserController();
            this.forumUser = uc.GetUser(this.PortalId, this.ModuleId);
            Controls.ForumDirectory fd = new Controls.ForumDirectory();
            fd.ModuleId = this.ModuleId;
            fd.TabId = this.TabId;
            fd.PortalId = this.PortalId;
            fd.ForumUser = this.forumUser;
            if (this.ItemTemplate != null)
            {
                fd.Template = this.ItemTemplate.Text;
            }

            writer.Write(fd.Render());
        }

    }

}
