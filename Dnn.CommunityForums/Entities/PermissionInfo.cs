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

using System.Diagnostics.Eventing.Reader;
using System.Linq;

namespace DotNetNuke.Modules.ActiveForums.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Web.Caching;

    using DotNetNuke.ComponentModel.DataAnnotations;

    [TableName("activeforums_Permissions")]
    [PrimaryKey("PermissionsId", AutoIncrement = true)]
    [Cacheable("activeforums_Permissions", CacheItemPriority.Low)]
    [Scope("ModuleId")]
    public class PermissionInfo
    {
        [IgnoreColumn]
        private string cacheKeyTemplate => CacheKeys.PermissionsInfo;

        public int PermissionsId { get; set; }

        public int ModuleId { get; set; }

        [ColumnName("CanView")]
        public string View { get; set; }

        [IgnoreColumn]
        public HashSet<int> ViewRoleIds => DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsFromPermSet(this.View);

        [ColumnName("CanRead")]
        public string Read { get; set; }

        [IgnoreColumn]
        public HashSet<int> ReadRoleIds => DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsFromPermSet(this.Read);

        [ColumnName("CanCreate")]
        public string Create { get; set; }

        [IgnoreColumn]
        public HashSet<int> CreateRoleIds => DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsFromPermSet(this.Create);

        [ColumnName("CanReply")]
        public string Reply { get; set; }

        [IgnoreColumn]
        public HashSet<int> ReplyRoleIds => DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsFromPermSet(this.Reply);

        [ColumnName("CanEdit")]
        public string Edit { get; set; }

        [IgnoreColumn]
        public HashSet<int> EditRoleIds => DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsFromPermSet(this.Edit);

        [ColumnName("CanDelete")]
        public string Delete { get; set; }

        [IgnoreColumn]
        public HashSet<int> DeleteRoleIds => DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsFromPermSet(this.Delete);

        [ColumnName("CanLock")]
        public string Lock { get; set; }

        [IgnoreColumn]
        public HashSet<int> LockRoleIds => DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsFromPermSet(this.Lock);

        [ColumnName("CanPin")]
        public string Pin { get; set; }

        [IgnoreColumn]
        public HashSet<int> PinRoleIds => DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsFromPermSet(this.Pin);

        [ColumnName("CanAttach")]
        public string Attach { get; set; }

        [IgnoreColumn]
        public HashSet<int> AttachRoleIds => DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsFromPermSet(this.Attach);

        [ColumnName("CanPoll")]
        public string Poll { get; set; }

        [IgnoreColumn]
        public HashSet<int> PollRoleIds => DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsFromPermSet(this.Poll);

        [ColumnName("CanBlock")]
        public string Block { get; set; }

        [IgnoreColumn]
        public HashSet<int> BlockRoleIds => DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsFromPermSet(this.Block);

        [ColumnName("CanTrust")]
        public string Trust { get; set; }

        [IgnoreColumn]
        public HashSet<int> TrustRoleIds => DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsFromPermSet(this.Trust);

        [ColumnName("CanSubscribe")]
        public string Subscribe { get; set; }

        [IgnoreColumn]
        public HashSet<int> SubscribeRoleIds => DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsFromPermSet(this.Subscribe);

        [ColumnName("CanAnnounce")]
        public string Announce { get; set; }

        [IgnoreColumn]
        public HashSet<int> AnnounceRoleIds => DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsFromPermSet(this.Announce);

        [ColumnName("CanTag")]
        public string Tag { get; set; }

        [IgnoreColumn]
        public HashSet<int> TagRoleIds => DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsFromPermSet(this.Tag);

        [ColumnName("CanCategorize")]
        public string Categorize { get; set; }

        [IgnoreColumn]
        public HashSet<int> CategorizeRoleIds => DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsFromPermSet(this.Categorize);

        [ColumnName("CanPrioritize")]
        public string Prioritize { get; set; }

        [IgnoreColumn]
        public HashSet<int> PrioritizeRoleIds => DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsFromPermSet(this.Prioritize);

        [ColumnName("CanModerate")]
        public string Moderate { get; set; }

        [IgnoreColumn]
        public HashSet<int> ModerateRoleIds => DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsFromPermSet(this.Moderate);

        [ColumnName("CanMove")]
        public string Move { get; set; }

        [IgnoreColumn]
        public HashSet<int> MoveRoleIds => DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsFromPermSet(this.Move);

        [ColumnName("CanSplit")]
        public string Split { get; set; }

        [IgnoreColumn]
        public HashSet<int> SplitRoleIds => DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsFromPermSet(this.Split);

        [ColumnName("CanBan")]
        public string Ban { get; set; }

        [IgnoreColumn]
        public HashSet<int> BanRoleIds => DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsFromPermSet(this.Ban);

        [IgnoreColumn]
        public ObjectType Type { get; set; }

        [IgnoreColumn]
        public string ObjectId { get; set; }

        [IgnoreColumn]
        public string ObjectName { get; set; }

        [Obsolete("Deprecated in Community Forums. Scheduled for removal in 10.00.00. Not Used.")]
        [IgnoreColumn]
        public int UserTrustLevel { get; set; }

        public bool EqualPermissions(PermissionInfo other)
        {
            return !(other is null) &&
                   this.EqualPermissionMembers(this.Announce, other.Announce) &&
                   this.EqualPermissionMembers(this.Attach, other.Attach) &&
                   this.EqualPermissionMembers(this.Ban, other.Ban) &&
                   // EqualPermissionMembers(this.Block, other.Block) &&
                   this.EqualPermissionMembers(this.Categorize, other.Categorize) &&
                   this.EqualPermissionMembers(this.Create, other.Create) &&
                   this.EqualPermissionMembers(this.Delete, other.Delete) &&
                   this.EqualPermissionMembers(this.Edit, other.Edit) &&
                   this.EqualPermissionMembers(this.Lock, other.Lock) &&
                   this.EqualPermissionMembers(this.Moderate, other.Moderate) &&
                   this.EqualPermissionMembers(this.Moderate, other.Moderate) &&
                   this.EqualPermissionMembers(this.Move, other.Move) &&
                   this.EqualPermissionMembers(this.Pin, other.Pin) &&
                   this.EqualPermissionMembers(this.Poll, other.Poll) &&
                   this.EqualPermissionMembers(this.Prioritize, other.Prioritize) &&
                   this.EqualPermissionMembers(this.Read, other.Read) &&
                   this.EqualPermissionMembers(this.Reply, other.Reply) &&
                   this.EqualPermissionMembers(this.Split, other.Split) &&
                   this.EqualPermissionMembers(this.Subscribe, other.Subscribe) &&
                   this.EqualPermissionMembers(this.Tag, other.Tag) &&
                   this.EqualPermissionMembers(this.Trust, other.Trust) &&
                   this.EqualPermissionMembers(this.View, other.View);
        }

        bool EqualPermissionMembers(HashSet<int> thisPermissions, HashSet<int> otherPermissions)
        {
            return thisPermissions.SetEquals(otherPermissions);
        }

        bool EqualPermissionMembers(string thisPermissions, string otherPermissions)
        {
            if (string.IsNullOrEmpty(otherPermissions) && string.IsNullOrEmpty(thisPermissions))
            {
                return true;
            }

            if (string.IsNullOrEmpty(otherPermissions) || string.IsNullOrEmpty(thisPermissions))
            {
                return true;
            }

            var thisPermsRoles = new HashSet<string>();
            var thisPermsUsers = new HashSet<string>();
            var thisPermsGroups = new HashSet<string>();
            var thisPerms = thisPermissions.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (thisPerms.Length > 0 && !string.IsNullOrEmpty(thisPerms[0]))
            {
                thisPermsRoles = thisPerms[0].Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToHashSet();
            }

            if (thisPerms.Length > 1 && !string.IsNullOrEmpty(thisPerms[1]))
            {
                thisPermsUsers = thisPerms[1].Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToHashSet();
            }

            if (thisPerms.Length > 2 && !string.IsNullOrEmpty(thisPerms[2]))
            {
                thisPermsGroups = thisPerms[2].Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToHashSet();
            }

            var otherPermsRoles = new HashSet<string>();
            var otherPermsUsers = new HashSet<string>();
            var otherPermsGroups = new HashSet<string>();
            var otherPerms = otherPermissions.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (otherPerms.Length > 0 && !string.IsNullOrEmpty(otherPerms[0]))
            {
                otherPermsRoles = otherPerms[0].Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToHashSet();
            }

            if (otherPerms.Length > 1 && !string.IsNullOrEmpty(otherPerms[1]))
            {
                otherPermsUsers = otherPerms[1].Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToHashSet();
            }

            if (otherPerms.Length > 2 && !string.IsNullOrEmpty(otherPerms[2]))
            {
                otherPermsGroups = otherPerms[2].Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToHashSet();
            }

            return thisPermsRoles.SetEquals(otherPermsRoles) && thisPermsUsers.SetEquals(otherPermsUsers) && thisPermsGroups.SetEquals(otherPermsGroups);
        }

        internal string GetCacheKey() => string.Format(this.cacheKeyTemplate, this.ModuleId, this.PermissionsId);
    }

}
