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
    using System.Linq;
    using System.Text;

    public class ControlPanel
    {
        public int PortalId { get; set; }

        public int ModuleId { get; set; }

        public ControlPanel(int portalId, int moduleId)
        {
            this.PortalId = portalId;
            this.ModuleId = moduleId;
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public string TemplatesOptions(Enums.TemplateType templateType) => throw new NotImplementedException();

        public string ForumGroupOptions()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<option value=\"-1\">" + Utilities.GetSharedResource("DropDownSelect", true) + "</option>");
            var forums = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetForums(this.ModuleId).OrderBy(f => f.ForumGroup.SortOrder).ThenBy(f => f.SortOrder).ToList();

            int tmpGroupId = -1;
            foreach (var forum in forums)
            {
                if (tmpGroupId != forum.ForumGroupId)
                {
                    sb.Append("<option value=\"GROUP" + forum.ForumGroupId + "\">" + forum.GroupName + "</option>");
                    tmpGroupId = forum.ForumGroupId;
                }

                if (forum.ForumID != 0 && forum.ParentForumId == 0)
                {
                    sb.Append("<option value=\"FORUM" + forum.ForumID + "\"> - " + forum.ForumName + "</option>");
                }
            }

            return sb.ToString();
        }

        public string BindRolesForSecurityGrid(string rootPath)
        {
            StringBuilder sb = new StringBuilder();
            foreach (DotNetNuke.Security.Roles.RoleInfo ri in DotNetNuke.Security.Roles.RoleController.Instance.GetRoles(portalId: this.PortalId))
            {
                sb.Append("<option value=\"" + ri.RoleID + "\">" + ri.RoleName + "</option>");
            }

            return sb.ToString();
        }
    }
}
