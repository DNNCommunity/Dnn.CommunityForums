//
// Community Forums
// Copyright (c) 2013-2021
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
using System;
using DotNetNuke.ComponentModel.DataAnnotations;
using System.Web.Caching;

namespace DotNetNuke.Modules.ActiveForums
{
    public enum SecureActions : int
    {
        View,
        Read,
        Create,
        Reply,
        Edit,
        Delete,
        Lock,
        Pin,
        Attach,
        Poll,
        Block,
        Trust,
        Subscribe,
        Announce,
        Tag,
        Categorize,
        Prioritize,
        ModApprove,
        ModMove,
        ModSplit,
        ModDelete,
        ModUser,
        ModEdit,
        ModLock,
        ModPin
    }
    public enum ObjectType : int
    {
        RoleId,
        UserId,
        GroupId
    }
    [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
    public enum SecureType : int
    {
        ForumGroup,
        Forum
    }
}
namespace DotNetNuke.Modules.ActiveForums.Entities
{
    [TableName("activeforums_Permissions")]
    [PrimaryKey("PermissionsId", AutoIncrement = true)]
    [Cacheable("activeforums_Permissions", CacheItemPriority.Low)]
    public class PermissionInfo
    {
        public int PermissionsId { get; set; }
        [ColumnName("CanView")] public string View { get; set; }
        [ColumnName("CanRead")] public string Read { get; set; }
        [ColumnName("CanCreate")] public string Create { get; set; }
        [ColumnName("CanReply")] public string Reply { get; set; }
        [ColumnName("CanEdit")] public string Edit { get; set; }
        [ColumnName("CanDelete")] public string Delete { get; set; }
        [ColumnName("CanLock")] public string Lock { get; set; }
        [ColumnName("CanPin")] public string Pin { get; set; }
        [ColumnName("CanAttach")] public string Attach { get; set; }
        [ColumnName("CanPoll")] public string Poll { get; set; }
        [ColumnName("CanBlock")] public string Block { get; set; }
        [ColumnName("CanTrust")] public string Trust { get; set; }
        [ColumnName("CanSubscribe")] public string Subscribe { get; set; }
        [ColumnName("CanAnnounce")] public string Announce { get; set; }
        [ColumnName("CanTag")] public string Tag { get; set; }
        [ColumnName("CanCategorize")] public string Categorize { get; set; }
        [ColumnName("CanPrioritize")] public string Prioritize { get; set; }
        [ColumnName("CanModApprove")] public string ModApprove { get; set; }
        [ColumnName("CanModMove")] public string ModMove { get; set; }
        [ColumnName("CanModSplit")] public string ModSplit { get; set; }
        [ColumnName("CanModDelete")] public string ModDelete { get; set; }
        [ColumnName("CanModUser")] public string ModUser { get; set; }
        [ColumnName("CanModEdit")] public string ModEdit { get; set; }
        [ColumnName("CanModLock")] public string ModLock { get; set; }
        [ColumnName("CanModPin")] public string ModPin { get; set; }
        [Obsolete("Deprecated in Community Forums. Scheduled for removal in 10.00.00. Not Used.")][IgnoreColumn()] public int UserTrustLevel { get; set; }
        [IgnoreColumn()] public ObjectType Type { get; set; }
        [IgnoreColumn()] public string ObjectId { get; set; }
        [IgnoreColumn()] public string ObjectName { get; set; }

    }
}
namespace DotNetNuke.Modules.ActiveForums
{
    [Obsolete("Deprecated in Community Forums. Scheduled for removal in 09.00.00. Use DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo.")]
    public class PermissionInfo : DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo
    {
    }
}