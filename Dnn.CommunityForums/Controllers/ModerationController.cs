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

using DotNetNuke.Services.Social.Notifications;
using System.Collections.Generic;
using System;
using DotNetNuke.Entities.Users;

namespace DotNetNuke.Modules.ActiveForums.Controllers
{
    public class ModerationController 
    {
        internal static bool SendModerationNotification(int PortalId, int TabId, int ModuleId, int ForumGroupId, int ForumId, int TopicId, int ReplyId, int AuthorId, string RequestUrl)
        {
            try
            {
                int authorId;
                string subject;
                string body;
                List<DotNetNuke.Entities.Users.UserInfo> mods = Utilities.GetListOfModerators(PortalId, ModuleId, ForumId);
                NotificationType notificationType = NotificationsController.Instance.GetNotificationType("AF-ForumModeration");
                if (ReplyId < 0)
                {
                    DotNetNuke.Modules.ActiveForums.Entities.TopicInfo topic = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(TopicId);
                    subject = Utilities.GetSharedResource("NotificationSubjectTopic");
                    subject = subject.Replace("[DisplayName]", topic.Content.AuthorName);
                    subject = subject.Replace("[TopicSubject]", topic.Content.Subject);

                    body = Utilities.GetSharedResource("NotificationBodyTopic");
                    body = body.Replace("[DisplayName]", topic.Content.AuthorName);
                    body = body.Replace("[TopicSubject]", topic.Content.Subject);
                    body = body.Replace("[Post]", topic.Content.Body);
                    authorId = topic.Content.AuthorId;
                }
                else
                {
                    DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo reply = new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController().GetById(ReplyId);
                    subject = Utilities.GetSharedResource("NotificationSubjectReply");
                    subject = subject.Replace("[DisplayName]", reply.Content.AuthorName);
                    subject = subject.Replace("[TopicSubject]", reply.Topic.Content.Subject);
                    body = Utilities.GetSharedResource("NotificationBodyReply");
                    body = body.Replace("[DisplayName]", reply.Content.AuthorName);
                    body = body.Replace("[TopicSubject]", reply.Content.Subject);
                    authorId = reply.Content.AuthorId;
                }
                string modLink = Utilities.NavigateURL(TabId, string.Empty, new[] { $"{ParamKeys.ViewType}={Views.ModerateTopics}", $"{ParamKeys.ForumId}={ForumId}" });
                body = body.Replace("[MODLINK]", modLink);

                string notificationKey = string.Format("{0}:{1}:{2}:{3}:{4}", TabId, ModuleId, ForumId, TopicId, ReplyId);

                Notification notification = new Notification
                {
                    NotificationTypeID = notificationType.NotificationTypeId,
                    Subject = subject,
                    Body = body,
                    IncludeDismissAction = false,
                    SenderUserID = authorId,
                    Context = notificationKey
                };

                NotificationsController.Instance.SendNotification(notification, PortalId, null, mods);
                return true;
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                return false;
            }
        }
    }
}

