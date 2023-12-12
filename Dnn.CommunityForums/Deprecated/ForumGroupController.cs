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

using System;
using DotNetNuke.Common.Utilities;
using System.Collections;
using System.Data;
using DotNetNuke.Modules.ActiveForums.API;
using System.Reflection;
using DotNetNuke.Modules.ActiveForums.Entities;
using DotNetNuke.Security.Permissions;


namespace DotNetNuke.Modules.ActiveForums
{
    [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.ForumGroupController")]
    internal partial class ForumGroupController : DotNetNuke.Modules.ActiveForums.Controllers.ForumGroupController
    {
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.ForumGroupController")]
        public new DotNetNuke.Modules.ActiveForums.ForumGroupInfo Groups_Get(int moduleID, int forumGroupID)
        {
            return (DotNetNuke.Modules.ActiveForums.ForumGroupInfo)base.GetForumGroup(moduleID, forumGroupID);
        }
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.ForumGroupController")]
        public new DotNetNuke.Modules.ActiveForums.ForumGroupInfo GetForumGroup(int moduleId, int forumGroupId)
        {
            return (DotNetNuke.Modules.ActiveForums.ForumGroupInfo)base.GetForumGroup(moduleId, forumGroupId);
        }
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public ArrayList Groups_List(int moduleId, bool fillSettings = false)
        {
            var groupArr = CBO.FillCollection(DataProvider.Instance().Groups_List(moduleId), typeof(ForumGroupInfo));
            if (fillSettings == false)
                return groupArr;
            int i;
            for (i = 0; i < groupArr.Count; i++)
            {
                DotNetNuke.Modules.ActiveForums.ForumGroupInfo gi = groupArr[i] as ForumGroupInfo;
                if (gi == null)
                    continue;
                gi.GroupSettings = DataCache.GetSettings(moduleId, gi.GroupSettingsKey, string.Format(CacheKeys.ForumGroupSettings, moduleId, gi.ForumGroupId), false);
                groupArr[i] = gi;
            }
            return groupArr;
        }
    }
}
