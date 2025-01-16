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

    [ParseChildren(true, ""), ToolboxData("<{0}:ForumRow runat=server></{0}:ForumRow>")]
    public class ForumRow : CompositeControl
    {
        protected Controls.Link hypForumName = new Controls.Link();
        protected PlaceHolder plhLastPost = new PlaceHolder();
        protected Controls.Link hypLastPostSubject = new Controls.Link();
        private int forumId;
        private string forumIcon;
        private ForumRowControl rowTemplate;
        private string viewRoles;
        private string readRoles;
        private string userRoles;
        private bool hidden;

        public override System.Web.UI.ControlCollection Controls
        {
            get
            {
                this.EnsureChildControls();
                return base.Controls;
            }
        }

        [Description("Initial content to render."), DefaultValue(null, ""), Browsable(false), NotifyParentProperty(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty)]
        public ForumRowControl Content
        {
            get
            {
                this.EnsureChildControls();
                return this.rowTemplate;
            }

            set
            {
                this.rowTemplate = value;
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

        public string ForumIcon
        {
            get
            {
                return this.forumIcon;
            }

            set
            {
                this.forumIcon = value;
            }
        }

        public string ReadRoles
        {
            get
            {
                return this.readRoles;
            }

            set
            {
                this.readRoles = value;
            }
        }

        public string ViewRoles
        {
            get
            {
                return this.viewRoles;
            }

            set
            {
                this.viewRoles = value;
            }
        }

        public string UserRoles
        {
            get
            {
                return this.userRoles;
            }

            set
            {
                this.userRoles = value;
            }
        }

        public bool Hidden
        {
            get
            {
                return this.hidden;
            }

            set
            {
                this.hidden = value;
            }
        }

        protected override void CreateChildControls()
        {
            if (this.Content != null)
            {
                this.Controls.Clear();
                this.Controls.Add(this.Content);
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            bool canView = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this.ViewRoles, this.UserRoles);
            bool canRead = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this.ReadRoles, this.UserRoles);

            if (this.Content != null)
            {
                this.hypForumName = (Link)this.Content.FindControl("hypForumName" + this.ForumId);
                this.hypForumName.Enabled = canView;

                this.plhLastPost = (PlaceHolder)this.Content.FindControl("plhLastPost" + this.ForumId);
                if (this.plhLastPost != null)
                {
                    this.plhLastPost.Visible = canView;
                }

                this.hypLastPostSubject = (Link)this.Content.FindControl("hypLastPostSubject" + this.ForumId);
                if (this.hypLastPostSubject != null)
                {
                    this.hypLastPostSubject.Enabled = canView;
                }
            }

            if (canView)
            {
                this.Content.RenderControl(writer);
            }
            else if (this.Hidden == false)
            {
                this.Content.RenderControl(writer);
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (this.Context == null || this.Page == null)
            {
                return;
            }

            if (this.Content != null)
            {
                this.Controls.Add(this.Content);
            }
        }
    }

    [ToolboxItem(false)]
    public class ForumRowControl : Control
    {
    }
}
