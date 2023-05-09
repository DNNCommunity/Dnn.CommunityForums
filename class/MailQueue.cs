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
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN doworkafaffdaafdfdfffdfdffd WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
//
using System;
using System.Collections.Generic;
using DotNetNuke.Services.Scheduling;
namespace DotNetNuke.Modules.ActiveForums.Queue
{
    [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Use DotNetNuke.Modules.ActiveForums.Entities.Message.")]
    public class Message : DotNetNuke.Modules.ActiveForums.Entities.Message
    {
        [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Use DotNetNuke.Modules.ActiveForums.Controllers.MessageController.Send().")]
        public bool SendMail()
        { return DotNetNuke.Modules.ActiveForums.Controllers.MessageController.Send(this); }
    }
    [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Use DotNetNuke.Modules.ActiveForums.Controllers.MailQueue().")]
    public class Controller : DotNetNuke.Modules.ActiveForums.Controllers.EmailNotificationQueueController
    {
        [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Use DotNetNuke.Modules.ActiveForums.Controllers.MailQueue.Add(int portalId, int moduleId, string EmailFrom, string EmailTo, string EmailSubject, string EmailBody, string EmailBodyPlainText, string EmailCC, string EmailBCC).")]
        public static void Add(string emailFrom, string emailTo, string emailSubject, string emailBody, string emailBodyPlainText, string emailCC, string emailBcc)
        {
            DotNetNuke.Modules.ActiveForums.Controllers.EmailNotificationQueueController.Add(-1, -1, emailFrom, emailTo, emailSubject, emailBody, emailBodyPlainText, emailCC, emailBcc);
        }
        [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Use DotNetNuke.Modules.ActiveForums.Controllers.MailQueue.Add(int portalId, int moduleId, string EmailFrom, string EmailTo, string EmailSubject, string EmailBody, string EmailBodyPlainText, string EmailCC, string EmailBCC).")]
        public static void Add(int portalId, string emailFrom, string emailTo, string emailSubject, string emailBody, string emailBodyPlainText, string emailCC, string emailBcc)
        {
            DotNetNuke.Modules.ActiveForums.Controllers.EmailNotificationQueueController.Add(-1, -1, emailFrom, emailTo, emailSubject, emailBody, emailBodyPlainText, emailCC, emailBcc);
        }
    }
    [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Use DotNetNuke.Modules.ActiveForums.Services.MailQueue.Scheduler().")]
    public class Scheduler : DotNetNuke.Modules.ActiveForums.Services.MailQueue.Scheduler
    {
        [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Use DotNetNuke.Modules.ActiveForums.Services.MailQueue.Scheduler().")]
        public Scheduler(ScheduleHistoryItem objScheduleHistoryItem) : base(objScheduleHistoryItem) { }
        [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Use DotNetNuke.Modules.ActiveForums.Services.MailQueue.Scheduler().")]
        public override void DoWork() => base.DoWork();
    }
}