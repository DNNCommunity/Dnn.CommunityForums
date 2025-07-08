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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Security;

    public class Permissions
    {
        private readonly DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo permissions;

        public Permissions()
        {
            this.permissions = new DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo();
        }

        public Permissions(DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo permissions)
        {
            this.permissions = permissions;
        }

        public int PermissionsId => this.permissions.PermissionsId;

        public HashSet<int> ViewRoleIds => this.permissions.ViewRoleIds;

        public HashSet<int> ReadRoleIds => this.permissions.ReadRoleIds;

        public HashSet<int> CreateRoleIds => this.permissions.CreateRoleIds;

        public HashSet<int> ReplyRoleIds => this.permissions.ReplyRoleIds;

        public HashSet<int> EditRoleIds => this.permissions.EditRoleIds;

        public HashSet<int> DeleteRoleIds => this.permissions.DeleteRoleIds;

        public HashSet<int> LockRoleIds => this.permissions.LockRoleIds;

        public HashSet<int> PinRoleIds => this.permissions.PinRoleIds;

        public HashSet<int> AttachRoleIds => this.permissions.AttachRoleIds;

        public HashSet<int> PollRoleIds => this.permissions.PollRoleIds;

        public HashSet<int> BlockRoleIds => this.permissions.BlockRoleIds;

        public HashSet<int> TrustRoleIds => this.permissions.TrustRoleIds;

        public HashSet<int> SubscribeRoleIds => this.permissions.SubscribeRoleIds;

        public HashSet<int> AnnounceRoleIds => this.permissions.AnnounceRoleIds;

        public HashSet<int> TagRoleIds => this.permissions.TagRoleIds;

        public HashSet<int> CategorizeRoleIds => this.permissions.CategorizeRoleIds;

        public HashSet<int> PrioritizeRoleIds => this.permissions.PrioritizeRoleIds;

        public HashSet<int> ModerateRoleIds => this.permissions.ModerateRoleIds;

        public HashSet<int> MoveRoleIds => this.permissions.MoveRoleIds;

        public HashSet<int> SplitRoleIds => this.permissions.SplitRoleIds;

        public HashSet<int> BanRoleIds => this.permissions.BanRoleIds;

        public bool EqualPermissions(Permissions other)
        {
            return !(other is null) &&
                   this.EqualPermissionMembers(this.AnnounceRoleIds, other.AnnounceRoleIds) &&
                   this.EqualPermissionMembers(this.AttachRoleIds, other.AttachRoleIds) &&
                   this.EqualPermissionMembers(this.BanRoleIds, other.BanRoleIds) &&
                   // EqualPermissionMembers(this.BlockRoleIds, other.BlockRoleIds) &&
                   this.EqualPermissionMembers(this.CategorizeRoleIds, other.CategorizeRoleIds) &&
                   this.EqualPermissionMembers(this.CreateRoleIds, other.CreateRoleIds) &&
                   this.EqualPermissionMembers(this.DeleteRoleIds, other.DeleteRoleIds) &&
                   this.EqualPermissionMembers(this.EditRoleIds, other.EditRoleIds) &&
                   this.EqualPermissionMembers(this.LockRoleIds, other.LockRoleIds) &&
                   this.EqualPermissionMembers(this.ModerateRoleIds, other.ModerateRoleIds) &&
                   this.EqualPermissionMembers(this.ModerateRoleIds, other.ModerateRoleIds) &&
                   this.EqualPermissionMembers(this.MoveRoleIds, other.MoveRoleIds) &&
                   this.EqualPermissionMembers(this.PinRoleIds, other.PinRoleIds) &&
                   this.EqualPermissionMembers(this.PollRoleIds, other.PollRoleIds) &&
                   this.EqualPermissionMembers(this.PrioritizeRoleIds, other.PrioritizeRoleIds) &&
                   this.EqualPermissionMembers(this.ReadRoleIds, other.ReadRoleIds) &&
                   this.EqualPermissionMembers(this.ReplyRoleIds, other.ReplyRoleIds) &&
                   this.EqualPermissionMembers(this.SplitRoleIds, other.SplitRoleIds) &&
                   this.EqualPermissionMembers(this.SubscribeRoleIds, other.SubscribeRoleIds) &&
                   this.EqualPermissionMembers(this.TagRoleIds, other.TagRoleIds) &&
                   this.EqualPermissionMembers(this.TrustRoleIds, other.TrustRoleIds) &&
                   this.EqualPermissionMembers(this.ViewRoleIds, other.ViewRoleIds);
        }

        public bool EqualPermissionMembers(HashSet<int> thisPermissions, HashSet<int> otherPermissions)
        {
            return thisPermissions.SetEquals(otherPermissions);
        }

        /// <summary>
        /// Maps this <see cref="Permissions"/> view model to a <see cref="DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo"/> entity.
        /// </summary>
        /// <returns>A new <see cref="DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo"/> entity with values from this view model.</returns>
        public DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo ToPermissionInfo()
        {
            var entity = new DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo
            {
                PermissionsId = this.PermissionsId,
                View = string.Join(";", this.ViewRoleIds.Distinct().OrderBy(r => r)),
                Read = string.Join(";", this.ReadRoleIds.Distinct().OrderBy(r => r)),
                Create = string.Join(";", this.CreateRoleIds.Distinct().OrderBy(r => r)),
                Reply = string.Join(";", this.ReplyRoleIds.Distinct().OrderBy(r => r)),
                Edit = string.Join(";", this.EditRoleIds.Distinct().OrderBy(r => r)),
                Delete = string.Join(";", this.DeleteRoleIds.Distinct().OrderBy(r => r)),
                Lock = string.Join(";", this.LockRoleIds.Distinct().OrderBy(r => r)),
                Pin = string.Join(";", this.PinRoleIds.Distinct().OrderBy(r => r)),
                Attach = string.Join(";", this.AttachRoleIds.Distinct().OrderBy(r => r)),
                Poll = string.Join(";", this.PollRoleIds.Distinct().OrderBy(r => r)),
                Block = string.Join(";", this.BlockRoleIds.Distinct().OrderBy(r => r)),
                Trust = string.Join(";", this.TrustRoleIds.Distinct().OrderBy(r => r)),
                Subscribe = string.Join(";", this.SubscribeRoleIds.Distinct().OrderBy(r => r)),
                Announce = string.Join(";", this.AnnounceRoleIds.Distinct().OrderBy(r => r)),
                Tag = string.Join(";", this.TagRoleIds.Distinct().OrderBy(r => r)),
                Categorize = string.Join(";", this.CategorizeRoleIds.Distinct().OrderBy(r => r)),
                Prioritize = string.Join(";", this.PrioritizeRoleIds.Distinct().OrderBy(r => r)),
                Moderate = string.Join(";", this.ModerateRoleIds.Distinct().OrderBy(r => r)),
                Move = string.Join(";", this.MoveRoleIds.Distinct().OrderBy(r => r)),
                Split = string.Join(";", this.SplitRoleIds.Distinct().OrderBy(r => r)),
                Ban = string.Join(";", this.BanRoleIds.Distinct().OrderBy(r => r)),
            };

            return entity;
        }
    }
}
