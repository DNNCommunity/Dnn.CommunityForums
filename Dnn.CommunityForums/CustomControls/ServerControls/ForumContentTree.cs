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
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    [DefaultProperty("Text"), ToolboxData("<{0}:ForumContentTree runat=server></{0}:ForumContentTree>")]
    public class ForumContentTree : WebControl
    {
        public DisplayTemplate ItemTemplate { get; set; }

        public DisplayTemplate HeaderTemplate { get; set; }

        public DisplayTemplate FooterTemplate { get; set; }

        public int PortalId { get; set; } = -1;

        public int ModuleId { get; set; } = -1;

        public int TabId { get; set; } = -1;

        public int ForumId { get; set; } = -1;

        public int ForumGroupId { get; set; } = -1;

        public int ParentForumId { get; set; } = -1;

        public bool IncludeClasses { get; set; } = true;

        private DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo forumUser = null;

        protected override void Render(HtmlTextWriter writer)
        {
            this.forumUser = (User)new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ModuleId).GetUserFromHttpContext(this.PortalId, this.ModuleId);
            CategoryTreeView fd = new CategoryTreeView();
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
