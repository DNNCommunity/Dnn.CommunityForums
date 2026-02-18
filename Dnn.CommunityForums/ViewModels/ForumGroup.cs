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

namespace DotNetNuke.Modules.ActiveForums.ViewModels
{
    using System.Collections.Generic;

    public class ForumGroup
    {
        private readonly DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo forumGroup;

        public ForumGroup(DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo forumGroup)
        {
            this.forumGroup = forumGroup;
            if (this.forumGroup.Security == null)
            {
                this.forumGroup.Security = new DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo();
            }
        }

        public int Id { get => this.forumGroup.ForumGroupId; set => this.forumGroup.ForumGroupId = value; }

        public string Name { get => this.forumGroup.GroupName; set => this.forumGroup.GroupName = value; }

        public int PortalId { get => this.forumGroup.PortalSettings.PortalId; set => this.forumGroup.PortalSettings.PortalId = value; }

        public int ModuleId { get => this.forumGroup.ModuleId; set => this.forumGroup.ModuleId = value; }

        public int PermissionsId { get => this.forumGroup.PermissionsId; set => this.forumGroup.PermissionsId = value; }

        public string SettingsKey { get => this.forumGroup.GroupSettingsKey; set => this.forumGroup.GroupSettingsKey = value; }

        public bool Active { get => this.forumGroup.Active; set => this.forumGroup.Active = value; }

        public bool Hidden { get => this.forumGroup.Hidden; set => this.forumGroup.Hidden = value; }

        public string PrefixURL => this.forumGroup.PrefixURL;

        public DotNetNuke.Modules.ActiveForums.ViewModels.Permissions Security => new DotNetNuke.Modules.ActiveForums.ViewModels.Permissions(this.forumGroup.Security);


        /// <summary>
        /// Maps the current ForumGroup view model to a ForumGroupInfo entity.
        /// </summary>
        /// <returns>A new instance of ForumGroupInfo with properties set from this view model.</returns>
        public DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo ToForumGroupInfo()
        {
            var forumGroupInfo = new DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo();
            forumGroupInfo.ForumGroupId = this.Id;
            forumGroupInfo.GroupName = this.Name;
            forumGroupInfo.ModuleId = this.ModuleId;
            forumGroupInfo.PermissionsId = this.PermissionsId;
            forumGroupInfo.GroupSettingsKey = this.SettingsKey;
            forumGroupInfo.Active = this.Active;
            forumGroupInfo.Hidden = this.Hidden;
            forumGroupInfo.PrefixURL = this.PrefixURL;

            return forumGroupInfo;
        }
    }
}
