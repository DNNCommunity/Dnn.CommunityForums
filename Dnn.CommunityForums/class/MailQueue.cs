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

namespace DotNetNuke.Modules.ActiveForums.Queue
{
    using System;

    using DotNetNuke.Services.Scheduling;

    [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Use DotNetNuke.Modules.ActiveForums.Entities.Message.")]
    public class Message
    {
        [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Not used.")]
        public bool SendMail() => throw new NotImplementedException();
    }

    [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Using DotNetNuke.Modules.ActiveForums.Controllers.MailQueue().")]
    public class Controller
    {
        [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Using DotNetNuke.Modules.ActiveForums.Controllers.MailQueue().")]
        public static void Add(string emailFrom, string emailTo, string emailSubject, string emailBody, string emailBodyPlainText, string emailCC, string emailBcc) => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Using DotNetNuke.Modules.ActiveForums.Controllers.MailQueue().")]
        public static void Add(int portalId, string emailFrom, string emailTo, string emailSubject, string emailBody, string emailBodyPlainText, string emailCC, string emailBcc) => throw new NotImplementedException();
    }

    [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Using DotNetNuke.Modules.ActiveForums.Services.MailQueue.Scheduler().")]
    public class Scheduler : DotNetNuke.Modules.ActiveForums.Services.EmailNotificationQueue.Scheduler
    {
        [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Using DotNetNuke.Modules.ActiveForums.Services.MailQueue.Scheduler().")]
        public Scheduler(ScheduleHistoryItem objScheduleHistoryItem) : base(objScheduleHistoryItem) { }

        [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Using DotNetNuke.Modules.ActiveForums.Services.MailQueue.Scheduler().")]
        public override void DoWork() => base.DoWork();
    }
}
