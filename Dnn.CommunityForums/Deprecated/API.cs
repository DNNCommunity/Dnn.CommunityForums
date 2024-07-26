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

namespace DotNetNuke.Modules.ActiveForums.API
{
    using System;
    using System.Collections.Generic;

    [Obsolete("Deprecated in Community Forums. Not Used. Scheduled removal in 09.00.00.")]
    public class Content
    {
        [Obsolete("Deprecated in Community Forums. Not Used. Scheduled removal in 09.00.00.")]
        public int Topic_QuickCreate(int portalId, int moduleId, int forumId, string subject, string body, int userId, string displayName, bool isApproved, string iPAddress)
        {
            try
            {
                var tc = new TopicsController();
                return tc.Topic_QuickCreate(portalId, moduleId, forumId, subject, body, userId, displayName, isApproved, iPAddress);
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        [Obsolete("Deprecated in Community Forums. Not Used. Scheduled removal in 09.00.00.")]
        public int Reply_QuickCreate(int portalId, int moduleId, int forumId, int topicId, int replyToId, string subject, string body, int userId, string displayName, bool isApproved, string iPAddress)
        {
            try
            {
                var rc = new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController();
                return rc.Reply_QuickCreate(portalId, moduleId, forumId, topicId, replyToId, subject, body, userId, displayName, isApproved, iPAddress);
            }
            catch (Exception ex)
            {
                return -1;
            }
        }
    }

    [Obsolete("Deprecated in Community Forums. Not Used. Scheduled removal in 09.00.00.")]
    public class ForumGroups
    {
    }

    [Obsolete("Deprecated in Community Forums. Not Used. Scheduled removal in 09.00.00.")]
    public class Forums
    {
        [Obsolete("Deprecated in Community Forums. Not Used. Scheduled removal in 09.00.00.")]
        public int Forums_Save(int portalId, DotNetNuke.Modules.ActiveForums.Forum fi, bool isNew, bool useGroup) => new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().Forums_Save(portalId, fi, isNew, useGroup, useGroup);
    }

    [Obsolete("Deprecated in Community Forums. Not Used. Scheduled removal in 09.00.00.")]
    public class Rewards
    {
    }
}
