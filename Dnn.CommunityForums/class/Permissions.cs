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

namespace DotNetNuke.Modules.ActiveForums
{
    using System;
    using System.Collections;

    public enum SecureActions : int
    {
        // NOTE: These need to be maintained in same order as they are in Control Panel security grid
        View,
        Read,
        Create,
        Reply,
        Edit,
        Delete,
        Move,
        Lock,
        Pin,
        Split,
        Attach,
        Poll,
        Block,
        Trust,
        Subscribe,
        Announce,
        Tag,
        Categorize,
        Prioritize,
        Moderate,
        Ban,
    }

    [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
    public enum ObjectType : int
    {
        RoleId,
        UserId,
        GroupId,
    }

    [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
    public enum SecureType : int
    {
        ForumGroup,
        Forum,
    }

    [Obsolete("Deprecated in Community Forums. Scheduled for removal in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo.")]
    public partial class PermissionInfo : DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo { }

    [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
    public class PermissionCollection : CollectionBase, ICollection, IList
    {
        private PermissionInfo item;

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public void CopyTo(Array array, int index) => this.List.CopyTo(array, index);

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public bool IsSynchronized => this.List.IsSynchronized;

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public object SyncRoot => this.List.SyncRoot;

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public int Add(PermissionInfo value) => this.List.Add(value);

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public bool Contains(PermissionInfo value) => this.List.Contains(value);

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public int IndexOf(PermissionInfo value) => this.List.IndexOf(value);

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public void Insert(int index, PermissionInfo value) => this.List.Insert(index, value);

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public bool IsFixedSize => this.List.IsFixedSize;

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public bool IsReadOnly => this.List.IsReadOnly;

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public PermissionInfo this[int index] { get => this.item; set => this.item = value; }

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public void Remove(PermissionInfo value) => this.List.Remove(value);
    }

    [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.")]
    public class Permissions
    {
        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.")]
        public static bool HasPerm(string authorizedRoles, int userId, int portalId) => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.")]
        public static bool HasPerm(string authorizedRoles, string userPermSet) => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.")]
        public static string RemovePermFromSet(string objectId, int objectType, string permissionSet) => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.")]
        public static string AddPermToSet(string objectId, int objectType, string permissionSet) => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.")]
        public static string GetRoleIds(string[] roles, int portalId) => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.")]
        public static bool HasRequiredPerm(string[] authorizedRoles, string[] userRoles) => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.")]
        public static bool HasAccess(string authorizedRoles, string userRoles) => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Scheduled for removal in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo.")]
        public partial class PermissionInfo : DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo { }
    }
}
