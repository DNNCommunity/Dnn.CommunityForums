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

namespace DotNetNuke.Modules.ActiveForums.Services.Badges
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Collections;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Modules.ActiveForums.Enums;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.Scheduling;

    public class BadgeAwardQueue : DotNetNuke.Services.Scheduling.SchedulerClient
    {
        public BadgeAwardQueue(ScheduleHistoryItem scheduleHistoryItem)
        {
            this.ScheduleHistoryItem = scheduleHistoryItem;
        }

        public override void DoWork()
        {
            try
            {
                foreach (DotNetNuke.Abstractions.Portals.IPortalInfo portal in DotNetNuke.Entities.Portals.PortalController.Instance.GetPortals())
                {
                    foreach (ModuleInfo module in DotNetNuke.Entities.Modules.ModuleController.Instance.GetModules(portal.PortalId))
                    {
                        if (!module.IsDeleted && module.DesktopModule.ModuleName.Trim().ToLowerInvariant().Equals(Globals.ModuleName.ToLowerInvariant()))
                        {
                            var badgeCount = ProcessBadgeAwards(module.PortalID, module.ModuleID);
                            this.ScheduleHistoryItem.Succeeded = true;
                            this.ScheduleHistoryItem.AddLogNote($"Processed {badgeCount} badge awards for {module.ModuleTitle} on portal {portal.PortalName}. ");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.ScheduleHistoryItem.Succeeded = false;
                this.ScheduleHistoryItem.AddLogNote(string.Concat("Badge Award Queue Failed. ", ex));
                this.Errored(ref ex);
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        private static int ProcessBadgeAwards(int portalId, int moduleId)
        {
            var badgeCount = 0;
            try
            {
                var badges = new DotNetNuke.Modules.ActiveForums.Controllers.BadgeController().Get(moduleId).Where(b => b.BadgeMetric != BadgeMetric.BadgeMetricNone);
                GetBatch(portalId, moduleId).ForEach(forumUser =>
                {
                    foreach (var badge in badges)
                    {
                        var awarded = ProcessBadgesForUser(portalId: portalId, moduleId: moduleId, forumUser: forumUser, badge: badge);
                        if (awarded)
                        {
                            badgeCount += 1;
                        }
                    }
                });
                return badgeCount;
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                return -1;
            }
        }

        private static IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo> GetBatch(int portalId, int moduleId)
        {
            try
            {
                return new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(moduleId: moduleId).Get().Where(u => (u.PortalId == portalId && !u.UserInfo.IsDeleted));
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                return null;
            }
        }

        private static bool ProcessBadgesForUser(int portalId, int moduleId, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo forumUser, DotNetNuke.Modules.ActiveForums.Entities.BadgeInfo badge)
        {
            try
            {
                if (forumUser == null)
                {
                    return false;
                }

                if (!forumUser.Badges.Any(b => b.BadgeId == badge.BadgeId))
                {
                    var awardBadge = false;
                    switch (badge.BadgeMetric)
                    {
                        case BadgeMetric.BadgeMetricNewUser:
                            if (DateTime.UtcNow.Subtract(forumUser.DateCreated.Value).TotalDays < 30)
                            {
                                awardBadge = true;
                            }

                            break;
                        case BadgeMetric.BadgeMetricLikesReceived:
                            if (forumUser.GetLikeCountForUser() >= badge.Threshold)
                            {
                                awardBadge = true;
                            }

                            break;
                        case BadgeMetric.BadgeMetricTopicsCreated:
                            if (forumUser.TopicCount >= badge.Threshold)
                            {
                                awardBadge = true;
                            }

                            break;
                        case BadgeMetric.BadgeMetricRepliesCreated:
                            if (forumUser.ReplyCount >= badge.Threshold)
                            {
                                awardBadge = true;
                            }

                            break;
                        case BadgeMetric.BadgeMetricTopicsRead:
                            if (forumUser.GetTopicReadCount() >= badge.Threshold)
                            {
                                awardBadge = true;
                            }

                            break;
                    }

                    if (awardBadge)
                    {
                        DotNetNuke.Modules.ActiveForums.Controllers.UserBadgeController.AssignUserBadge(portalId, moduleId, forumUser.UserId, badge.BadgeId, string.Empty);
                        return true;
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }

            return false;
        }
    }
}
