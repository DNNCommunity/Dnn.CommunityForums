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

namespace DotNetNuke.Modules.ActiveForums.Services.ProcessQueue
{
    using System;

    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.Scheduling;

    public class Scheduler : DotNetNuke.Services.Scheduling.SchedulerClient
    {
        public Scheduler(ScheduleHistoryItem scheduleHistoryItem)
        {
            this.ScheduleHistoryItem = scheduleHistoryItem;
        }

        public override void DoWork()
        {
            try
            {
                var intQueueCount = ProcessQueue();
                this.ScheduleHistoryItem.Succeeded = true;
                this.ScheduleHistoryItem.AddLogNote(string.Concat("Processed ", intQueueCount, " items"));
            }
            catch (Exception ex)
            {
                this.ScheduleHistoryItem.Succeeded = false;
                this.ScheduleHistoryItem.AddLogNote(string.Concat("Process Queue Failed. ", ex));
                this.Errored(ref ex);
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        private static int ProcessQueue()
        {
            var intQueueCount = 0;
            try
            {
                new DotNetNuke.Modules.ActiveForums.Controllers.ProcessQueueController().GetBatch().ForEach(item =>
                {
                    intQueueCount += 1;
                    bool completed = false;
                    switch (item.ProcessType)
                    {
                        case ProcessType.ApprovedTopicCreated:
                            completed = DotNetNuke.Modules.ActiveForums.Controllers.TopicController.ProcessApprovedTopicAfterAction(portalId: item.PortalId, tabId: item.TabId, moduleId: item.ModuleId, forumGroupId: item.ForumGroupId, forumId: item.ForumId, topicId: item.TopicId, replyId: item.ReplyId, contentId: item.ContentId, userId: item.UserId, authorId: item.AuthorId, requestUrl: item.RequestUrl);
                            break;
                        case ProcessType.ApprovedReplyCreated:
                            completed = DotNetNuke.Modules.ActiveForums.Controllers.ReplyController.ProcessApprovedReplyAfterAction(portalId: item.PortalId, tabId: item.TabId, moduleId: item.ModuleId, forumGroupId: item.ForumGroupId, forumId: item.ForumId, topicId: item.TopicId, replyId: item.ReplyId, contentId: item.ContentId, userId: item.UserId, authorId: item.AuthorId, requestUrl: item.RequestUrl);
                            break;
                        case ProcessType.UnapprovedTopicCreated:
                            completed = DotNetNuke.Modules.ActiveForums.Controllers.TopicController.ProcessUnapprovedTopicAfterAction(portalId: item.PortalId, tabId: item.TabId, moduleId: item.ModuleId, forumGroupId: item.ForumGroupId, forumId: item.ForumId, topicId: item.TopicId, replyId: item.ReplyId, contentId: item.ContentId, userId: item.UserId, authorId: item.AuthorId, requestUrl: item.RequestUrl);
                            break;
                        case ProcessType.UnapprovedReplyCreated:
                            completed = DotNetNuke.Modules.ActiveForums.Controllers.ReplyController.ProcessUnapprovedReplyAfterAction(portalId: item.PortalId, tabId: item.TabId, moduleId: item.ModuleId, forumGroupId: item.ForumGroupId, forumId: item.ForumId, topicId: item.TopicId, replyId: item.ReplyId, contentId: item.ContentId, userId: item.UserId, authorId: item.AuthorId, requestUrl: item.RequestUrl);
                            break;
                        case ProcessType.PostLiked:
                            completed = DotNetNuke.Modules.ActiveForums.Controllers.LikeController.ProcessPostLiked(portalId: item.PortalId, tabId: item.TabId, moduleId: item.ModuleId, forumGroupId: item.ForumGroupId, forumId: item.ForumId, topicId: item.TopicId, replyId: item.ReplyId, contentId: item.ContentId, userId: item.UserId, authorId: item.AuthorId, requestUrl: item.RequestUrl);
                            break;
                        case ProcessType.TopicPinned:
                            completed = DotNetNuke.Modules.ActiveForums.Controllers.TopicController.ProcessTopicPinned(portalId: item.PortalId, tabId: item.TabId, moduleId: item.ModuleId, forumGroupId: item.ForumGroupId, forumId: item.ForumId, topicId: item.TopicId, replyId: item.ReplyId, contentId: item.ContentId, userId: item.UserId, authorId: item.AuthorId, requestUrl: item.RequestUrl);
                            break;
                        case ProcessType.UpdateForumLastUpdated:
                            completed = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.UpdateForumLastUpdates(item.ForumId);
                            break;
                        case ProcessType.UpdateForumTopicPointers:
                            completed = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.RecalculateTopicPointers(item.ForumId);
                            break;
                        case ProcessType.BadgeAssigned:
                            completed = new DotNetNuke.Modules.ActiveForums.Controllers.UserBadgeController(item.PortalId, item.ModuleId).AssignUserBadgeAfterAction(item.PortalId, item.UserId, item.BadgeId, item.DateCreated, item.RequestUrl);
                            break;
                        case ProcessType.UserMentioned:
                            completed = new DotNetNuke.Modules.ActiveForums.Controllers.UserMentionController().UserMentionAfterAction(item.PortalId, item.ModuleId, item.TabId, item.ForumGroupId, item.ForumId, item.TopicId, item.ReplyId, item.ContentId, item.AuthorId, item.UserId, item.RequestUrl);
                            break;
                        default:
                            DotNetNuke.Services.Exceptions.Exceptions.LogException(new NotImplementedException("invalid ProcessType"));
                            break;
                    }

                    if (completed)
                    {
                        try
                        {
                            new DotNetNuke.Modules.ActiveForums.Controllers.ProcessQueueController().DeleteById(item.Id);
                        }
                        catch (Exception ex)
                        {
                            DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                        }
                    }
                    else
                    {
                        if (DateTime.UtcNow.Subtract(item.DateCreated).TotalDays > 7)
                        {
                            var log = new DotNetNuke.Services.Log.EventLog.LogInfo { LogTypeKey = DotNetNuke.Abstractions.Logging.EventLogType.ADMIN_ALERT.ToString() };
                            log.LogProperties.Add(new LogDetailInfo("Module", Globals.ModuleFriendlyName));
                            var message = string.Format(Utilities.GetSharedResource("[RESX:UnableToProcessItem]"), item.Id);
                            log.AddProperty("Message", message);
                            DotNetNuke.Services.Log.EventLog.LogController.Instance.AddLog(log);
                            new DotNetNuke.Modules.ActiveForums.Controllers.ProcessQueueController().DeleteById(item.Id);
                        }
                        else
                        {
                            intQueueCount = intQueueCount - 1;
                        }
                    }
                });
                return intQueueCount;
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                return -1;
            }
        }
    }
}
